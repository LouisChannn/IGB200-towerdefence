using UnityEngine;
using UnityEditor;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attribute")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 400f;
    [SerializeField] private float bps = 1f; //Bullets Per Second


    

    private Transform target;
    private float timeUntilFire = 0f;


    private void Update ()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();
        if(!CheckIfTargetIsInRange())
        {
            target = null;
        }
        else   
        {
            timeUntilFire += Time.deltaTime;

            if (timeUntilFire > 1f/bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }

    }

    private void Shoot ()
    {
       GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
       Bullet bulletscript = bullet.GetComponent<Bullet>();
       bulletscript.SetTarget(target);
    }

    private void FindTarget ()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }
    
    private bool CheckIfTargetIsInRange ()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget ()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(turretRotationPoint.position, Vector3.up, targetingRange);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
   
}
