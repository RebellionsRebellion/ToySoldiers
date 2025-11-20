using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class WeaponsSystem : MonoBehaviour
{
    [FormerlySerializedAs("inventory")]
    [Header("References")]
    [Tooltip("Reference to input logic")]
    [SerializeField] private PlayerInputController playerInputController;
    [Tooltip("Player camera used for the obstruction check")]
    [SerializeField] private Camera playerCamera;   // player camera used for the obstruction check
    [Tooltip("Test cube to visualise spread")]
    [SerializeField] private Transform firePoint;
    public Transform cube;                          // test cube to visualise spread


    private Weapon currentWeapon;
    private PlayerInventory playerInventory => PlayerInventory.Instance;
    
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

    private void Start()
    {

        currentWeapon = playerInventory.GetPrimaryWeapon();
        
        // Input events
        playerInputController.OnShootAction += Fire;
        playerInputController.OnReloadAction += Reload;
    }

    private void Update()
    {
        currentWeapon.WeaponSpread.UpdateSpreadOverTime();
        if(cube)
            cube.localScale = new Vector3(currentWeapon.WeaponSpread.CurrentSpreadAmount, 1f, 1f);
    }

    // called when for example the player clicks, or called every frame if holding down for full auto
    private void Fire()
    {
        // check we arent still reloading
        if(Time.time - lastReloadTime < currentWeapon.WeaponData.ReloadTime)
        {
            // am still reloading
            Debug.Log("Still reloading!");
            return;
        }

        if(currentWeapon.CurrentAmmoInMag <= 0)
        {
            // play empty mag sound here
            Debug.Log("No ammo in mag!");
            return;
        }

        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeapon.WeaponData.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            Debug.Log("Shooting too fast!");
            return;
        }

        Debug.Log("Firing weapon: " + currentWeapon.WeaponData.DisplayName);

        // do the actual shooting here
        if(currentWeapon.WeaponData.IsPhysicsBased)
        {
            DoPhysicsShoot();
        }
        else
        {
            DoRaycastShoot();
        }

        // do the spread calculations
        lastShotTime = Time.time;

        
        currentWeapon.Fire();
    }

    public void Reload()
    {
        accumulatedShootingTime = 0f;
        lastReloadTime = Time.time;

        currentWeapon.Reload(playerInventory);
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
            if(currentWeapon.FirePoint && Physics.Linecast(currentWeapon.FirePoint.position, cameraHit.point))
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
