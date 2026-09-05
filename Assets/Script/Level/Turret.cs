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
    [SerializeField] private float targetingRange = 5f; // level 1 baseline - never changed at runtime
    [SerializeField] private float rotationSpeed = 400f;
    [SerializeField] private float bps = 1f; // level 1 baseline Bullets Per Second
    [SerializeField] private int baseDamage = 1; // level 1 baseline damage

    [Header("Upgrade")]
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private int baseUpgradeCost = 50;
    [SerializeField] private float upgradeCostMultiplier = 1.5f; // cost growth per level
    [SerializeField] private float rangeMultiplierPerLevel = 1.1f;
    [SerializeField] private float fireRateMultiplierPerLevel = 1.15f;
    [SerializeField] private float damageMultiplierPerLevel = 1.25f;

    private int level = 1;
    private float currentTargetingRange;
    private float currentBps;
    private int currentDamage;

    private Transform target;
    private float timeUntilFire = 0f;

    private void Start()
    {
        RecalculateStats();
    }

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

            if (timeUntilFire > 1f/currentBps)
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
       bulletscript.SetDamage(currentDamage);
    }

    private void FindTarget ()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, currentTargetingRange, (Vector2)transform.position, 0f, enemyMask);
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }
    
    private bool CheckIfTargetIsInRange ()
    {
        return Vector2.Distance(target.position, transform.position) <= currentTargetingRange;
    }

    private void RotateTowardsTarget ()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // =========================
    // UPGRADES
    // =========================

    // Recomputes every stat fresh from the original level-1 baseline each time,
    // rather than repeatedly multiplying a value by itself - avoids floating point
    // drift and means levels can be recalculated safely in any order
    private void RecalculateStats()
    {
        currentTargetingRange = targetingRange * Mathf.Pow(rangeMultiplierPerLevel, level - 1);
        currentBps = bps * Mathf.Pow(fireRateMultiplierPerLevel, level - 1);
        currentDamage = Mathf.RoundToInt(baseDamage * Mathf.Pow(damageMultiplierPerLevel, level - 1));
    }

    public bool IsMaxLevel()
    {
        return level >= maxLevel;
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetUpgradeCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, level - 1));
    }

    // Call this from a UI button, same pattern as BuildManager.SelectTurretToBuild
    public bool Upgrade()
    {
        if (IsMaxLevel())
        {
            Debug.Log("Turret is already at max level!");
            return false;
        }

        int cost = GetUpgradeCost();

        if (!LevelManager.main.SpendColourFuel(cost))
        {
            Debug.Log("Not enough Colour Fuel to upgrade this turret!");
            return false;
        }

        level++;
        RecalculateStats();

        Debug.Log("Turret upgraded to level " + level + "! Cost: " + cost + " Colour Fuel");
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(turretRotationPoint.position, Vector3.up, Application.isPlaying ? currentTargetingRange : targetingRange);
    }
}