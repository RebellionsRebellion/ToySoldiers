using UnityEngine;
using System.Collections.Generic;

public class WeaponsSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The weapon ScriptableObject to use, will be set by inventory at later date")]
    [SerializeField] private WeaponSO currentWeapon;
    [Tooltip("Reference to the players ammo inventory to check for ammo when reloading")]
    [SerializeField] private PlayerAmmoInventory playerAmmoInventory;
    [Tooltip("Reference to input logic")]
    [SerializeField] private PlayerInputController playerInputController;
    [Tooltip("Player camera used for the obstruction check")]
    [SerializeField] private Camera playerCamera;   // player camera used for the obstruction check
    [Tooltip("Test cube to visualise spread")]
    [SerializeField] private Transform firePoint;
    public Transform cube;                          // test cube to visualise spread


    // spread paramaters
    private float CurrentSpreadPosition = 0f;       // the position on the spread curve we are at
    private float currentSpreadAmount = 0f;         // the actual spread amount we are at based on the curve
    private float spreadInterval = 0f;              // amount to increase spread position by per shot
    private AnimationCurve spreadCurve;

    // timing values
    private float lastShotTime = 0;                 // time in seconds since the start of the application when the last shot happened
    private float accumulatedShootingTime = 0f;     // total time spent shooting, used for recovery speed
    private float lastReloadTime = -999f;
    
    // weapon instances
    private WeaponSO currentWeaponInstance;         // the instance of the scriptable object weapon we are using
    private Dictionary<string, WeaponSO> initialisedWeapons = new Dictionary<string, WeaponSO>();   // the weapons we have already held (and initialised)

    private void Start()
    {
        // initialise the currently held weapon
        if(currentWeapon == null)
            return;

        if(initialisedWeapons.ContainsKey(currentWeapon.ClassName))
        {
            currentWeaponInstance = initialisedWeapons[currentWeapon.ClassName];
        }
        else
        {
            InitialiseWeapon();
        }
        
        // Input events
        playerInputController.OnShootAction += FireWeapon;
        playerInputController.OnReloadAction += ReloadWeapon;
    }

    private void InitialiseWeapon()
    {
        currentWeaponInstance = ScriptableObject.CreateInstance<WeaponSO>();
        currentWeaponInstance.CopyFrom(currentWeapon); // copy base data

        // max out the ammo and set the fire mode
        currentWeaponInstance.CurrentAmmoInMag = currentWeaponInstance.MagSize;

        currentWeaponInstance.CurrentFireMode = currentWeaponInstance.FireModes[0];

        // make its spread
        currentWeaponInstance.weaponSpread = new WeaponSpread(
            currentWeaponInstance.BaseSpread,
            currentWeaponInstance.HalfSpread,
            currentWeaponInstance.MaxSpread,
            currentWeaponInstance.MagSize,
            currentWeaponInstance.FireRateRPM
        );

        // add to dict
        initialisedWeapons.Add(currentWeaponInstance.ClassName, currentWeaponInstance);
    }

    private void Update()
    {
        currentWeaponInstance.weaponSpread.UpdateSpreadOverTime();
        if(cube)
            cube.localScale = new Vector3(currentWeaponInstance.weaponSpread.CurrentSpreadAmount, 1f, 1f);
    }

    // called when for example the player clicks, or called every frame if holding down for full auto
    private void FireWeapon()
    {
        // check we arent still reloading
        if(Time.time - lastReloadTime < currentWeaponInstance.ReloadTime)
        {
            // am still reloading
            Debug.Log("Still reloading!");
            return;
        }

        if(currentWeaponInstance.CurrentAmmoInMag <= 0)
        {
            // play empty mag sound here
            Debug.Log("No ammo in mag!");
            return;
        }

        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeaponInstance.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            Debug.Log("Shooting too fast!");
            return;
        }

        Debug.Log("Firing weapon: " + currentWeaponInstance.DisplayName);

        // do the actual shooting here
        if(currentWeaponInstance.IsPhysicsBased)
        {
            DoPhysicsShoot();
        }
        else
        {
            DoRaycastShoot();
        }

        // do the spread calculations
        lastShotTime = Time.time;

        // update spread
        currentWeaponInstance.weaponSpread.OnShotFired(currentWeaponInstance.MagSize);

        currentWeaponInstance.CurrentAmmoInMag--;
    }

    public void ReloadWeapon()
    {
        // reset spread on reload
        currentWeaponInstance.weaponSpread.ResetSpread();
        accumulatedShootingTime = 0f;

        // determine which ammo to use
        int availableAmmo;

        if (currentWeaponInstance.SpecialAmmo)
        {
            availableAmmo = playerAmmoInventory.CurrentSpecialAmmo;
        }
        else
        {
            availableAmmo = playerAmmoInventory.CurrentNormalAmmo;
        }

        if (availableAmmo <= 0)
        {
            // no ammo left at all! play a sound or something
            Debug.Log("No ammo left to reload!");
            return;
        }

        // actually give/take the ammo
        if (availableAmmo >= currentWeaponInstance.MagSize)
        {
            currentWeaponInstance.CurrentAmmoInMag = currentWeaponInstance.MagSize;

            if (currentWeaponInstance.SpecialAmmo)
            {
                playerAmmoInventory.CurrentSpecialAmmo -= currentWeaponInstance.MagSize;
            }
            else
            {
                playerAmmoInventory.CurrentNormalAmmo -= currentWeaponInstance.MagSize;
            }
            Debug.Log("Reloaded full mag");
        }
        else
        {
            currentWeaponInstance.CurrentAmmoInMag = availableAmmo;

            if (currentWeaponInstance.SpecialAmmo)
            {
                playerAmmoInventory.CurrentSpecialAmmo = 0;
            }
            else
            {
                playerAmmoInventory.CurrentNormalAmmo = 0;
            }

            Debug.Log("Reloaded partial mag");
        }

        lastReloadTime = Time.time;
    }

    private bool IsGunObstructed()
    {
        Ray cameraRay;
        RaycastHit cameraHit;

        // shoot a ray from the camera and see what its looking at
        cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // also shoot a ray from the gun in the direction of where the camera ray landed
        if(Physics.Raycast(cameraRay, out cameraHit))
        {
            if(Physics.Linecast(currentWeaponInstance.FirePoint.position, cameraHit.point))
            {
                return true;
            }
            else
            {
                // if that ray collides with anything else then the gun is obstructed so return true
                return false;
            }
        }
        return false;
    }

    private void DoRaycastShoot()
    {
        // do the actual racyast to fire the weapon (potentially we also need a physics based one for rpg etc)

        if(IsGunObstructed())
        {
            // shoot from gun
            // fire ray from gun
            // if hit collider that has tag shootable
            // call take damage on it somhow

            // TODO: Jasper just shoot an infinite length ray here and dont worry about the physics shoot yet, make it damage the health system when you have that in from ollie. Shoot from firePoint
        }
        else
        {
            // shoot from camera
            // same as the other one

            // TODO: Jasper just shoot an infinite length ray here and dont worry about the physics shoot yet, make it damage the health system when you have that in from ollie. Shoot from firePoint
        }
    }

    private void DoPhysicsShoot()
    {
        // do the actual physics based shoot for rockets etc

        // shoot ray from camera. Set initial direction of projectile to point at that
        
        // after that use the projectile physics i did for my ballistic system where visually it looks like its effected by gravity etc but its just all raycasts
    }
}
