using UnityEngine;
using System.Collections;

public class AIWeaponSystem : MonoBehaviour
{
    [Tooltip("Layer mask to stop the gun from shooting the player torso")]
    [SerializeField] private LayerMask canShoot;
    [Tooltip("The point that the gun actually shoots from, will be obtained dynamically in the future")]
    [SerializeField] private Transform firePoint;
    
    [HideInInspector] public Weapon currentWeapon;
    
    // tracer
    public GameObject tracerPrefab;   // assign in Inspector
    public float tracerSpeed = 200f;  // only for moving tracers (optional)
    
    // timing values
    private float lastShotTime = 0;                 // time in seconds since the start of the application when the last shot happened
    private float accumulatedShootingTime = 0f;     // total time spent shooting, used for recovery speed
    private float lastReloadTime = -999f;
    
    [HideInInspector] public Transform target;
    private AIInventory aiInventory;

    [Tooltip("Damage multiplier for enemy weapons")]
    [SerializeField] private float damageMult = 0.5f;

    private void Start()
    {
        aiInventory = GetComponent<AIInventory>();
        currentWeapon = aiInventory.GetPrimaryWeapon();
    }
    
    
    public void Fire()
    {
        // check we aren't still reloading
        if(Time.time - lastReloadTime < currentWeapon.WeaponData.ReloadTime)
        {
            // am still reloading
            return;
        }

        if(currentWeapon.CurrentAmmoInMag <= 0)
        {
            // play empty mag sound here
            Reload();
            return;
        }

        // limit it so you can only shoot up to the max fire rate
        float timeBetweenShots = 60f / currentWeapon.WeaponData.FireRateRPM;
        if(Time.time - lastShotTime < timeBetweenShots)
        {
            return;
        }

        
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

        currentWeapon.EnemyReload(aiInventory);
    }

    private Vector3 DoRaycastShoot()
    {
        // Direction to target center
        Vector3 direction = ((target.position + Vector3.up) - firePoint.position).normalized;

        // debug lines
        Debug.DrawRay(firePoint.position, direction * 100f, Color.red, 10f);

        // raycast from gun along shoot direction
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, Mathf.Infinity, canShoot))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerHealth>().TakeDamage(currentWeapon.WeaponData.Damage * damageMult);
            }
        }

        return firePoint.position + direction * 100f; // 100 units forward
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
