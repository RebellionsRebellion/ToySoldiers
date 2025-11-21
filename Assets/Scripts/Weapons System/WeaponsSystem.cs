using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class WeaponsSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to input logic")]
    [SerializeField] private PlayerInputController playerInputController;
    [Tooltip("Player camera used for the obstruction check")]
    [SerializeField] private Camera playerCamera;   // player camera used for the obstruction check
    [FormerlySerializedAs("playerLayer")]
    [Tooltip("Layer mask to stop the gun from shooting the player torso")]
    [SerializeField] private LayerMask canShoot;
    [Tooltip("The point that the gun actually shoots from, will be obtained dynamically in the future")]
    [SerializeField] private Transform firePoint;
    
    
    [Tooltip("Test cube to visualise spread")]
    public Transform cube;                          // test cube to visualise spread


    [HideInInspector] public Weapon currentWeapon;
    private PlayerInventory playerInventory => PlayerInventory.Instance;
    
    // tracer
    public GameObject tracerPrefab;   // assign in Inspector
    public float tracerSpeed = 200f;  // only for moving tracers (optional)
    
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
            Vector3 endPos = DoRaycastShoot();
            SpawnTracer(firePoint.position, endPos);
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

    private Vector3 DoRaycastShoot()
    {
        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // debug lines
        Debug.DrawRay(firePoint.position, cameraRay.direction * 100f, Color.red, 10f);

        // raycast from gun along shoot direction
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, cameraRay.direction, out hit, Mathf.Infinity, canShoot))
        {
            if (hit.collider.CompareTag("Enemy"))
                hit.collider.GetComponent<AIController>().TakeDamage(currentWeapon.WeaponData.Damage);
        }

        return firePoint.position + cameraRay.direction * 100f; // 100 units forward
    }



    private void DoPhysicsShoot()
    {
        // do the actual physics based shoot for rockets etc

        // shoot ray from camera. Set initial direction of projectile to point at that
        
        // after that use the projectile physics i did for my ballistic system where visually it looks like its effected by gravity etc but its just all raycasts
    }
    
    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);

        Vector3 direction = (end - start).normalized;

        // move the tracer with a coroutine
        StartCoroutine(MoveTracer(tracer, direction, end));
    }
    
    private IEnumerator MoveTracer(GameObject tracer, Vector3 dir, Vector3 end)
    {
        while (tracer && Vector3.Distance(tracer.transform.position, end) > 0.1f)
        {
            tracer.transform.position += dir * (tracerSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(tracer);
    }
}
