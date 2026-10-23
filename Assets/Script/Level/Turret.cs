using UnityEngine;

public enum TurretUpgradeType
{
    Range,
    Damage,
    FireRate,
    DamageOverTime
}

public class Turret : MonoBehaviour
{
    public const int MaxUpgradePoints = 10;
    private const int UpgradePointCost = 100;
    private const int DotDamagePerRank = 3;

    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [Tooltip("Optional cone spray attack. Turrets without this component use their normal projectile.")]
    [SerializeField] private TurretSprayAttack sprayAttack;

    [Header("Base Attributes")]
    [Tooltip("Unupgraded range. Runtime upgrades are exposed by CurrentRange and leave this baseline unchanged.")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 400f;
    [Tooltip("Unupgraded shots per second. Runtime upgrades are exposed by CurrentFireRate.")]
    [SerializeField] private float bps = 1f;
    [Tooltip("Unupgraded direct-hit damage. Runtime upgrades are exposed by CurrentDamage.")]
    [SerializeField] private int baseDamage = 10;

    [Header("Independent Upgrade Multipliers")]
    [Tooltip("Applied only to the Range rank, relative to the range baseline.")]
    [SerializeField] private float rangeMultiplierPerLevel = 1.1f;
    [Tooltip("Applied only to the Fire Rate rank, relative to the firing-rate baseline.")]
    [SerializeField] private float fireRateMultiplierPerLevel = 1.15f;
    [Tooltip("Applied only to the Damage rank. Direct-hit damage rounds upward to a whole number.")]
    [SerializeField] private float damageMultiplierPerLevel = 1.25f;

    [Header("Damage Over Time")]
    [Tooltip("Allows Damage Over Time upgrades instead of Fire Rate upgrades on this turret.")]
    [SerializeField] private bool usesDamageOverTime = false;
    [Tooltip("Damage per tick at Damage Over Time rank 1. Rank 0 has no damage over time; each later rank adds 3 per tick.")]
    [SerializeField] private int baseDotDamage = 3;
    [SerializeField, Min(0.01f)] private float dotDuration = 3f;
    [SerializeField, Min(0.01f)] private float dotTickInterval = 1f;

    private int rangeUpgradeLevel;
    private int damageUpgradeLevel;
    private int fireRateUpgradeLevel;
    private int dotUpgradeLevel;
    private float currentTargetingRange;
    private float currentBps;
    private int currentDamage;
    private int currentDotDamage;
    private Transform target;
    private float timeUntilFire;
    private TurretUpgradeVisuals upgradeVisuals;

    public int UpgradePointsUsed => rangeUpgradeLevel + damageUpgradeLevel + fireRateUpgradeLevel + dotUpgradeLevel;
    public int UpgradePointsRemaining => Mathf.Max(0, MaxUpgradePoints - UpgradePointsUsed);
    public int TotalUpgradeSpend { get; private set; }
    public bool UsesDamageOverTime => usesDamageOverTime;
    public float CurrentRange => currentTargetingRange;
    public int CurrentDamage => currentDamage;
    public float CurrentFireRate => currentBps;
    public int CurrentDotDamage => currentDotDamage;
    public float DotDuration => dotDuration;
    public float DotTickInterval => dotTickInterval;

    private void Awake()
    {
        RecalculateStats();
        upgradeVisuals = GetComponent<TurretUpgradeVisuals>();
        if (upgradeVisuals == null)
            upgradeVisuals = gameObject.AddComponent<TurretUpgradeVisuals>();
        upgradeVisuals.Initialize(this, firePoint);
    }

    public void SetUpgradePreviewVisible(bool visible)
    {
        if (upgradeVisuals != null)
            upgradeVisuals.SetSelected(visible);
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();
        if (!CheckIfTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;
            if (timeUntilFire > 1f / currentBps)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    private void Shoot()
    {
        if (sprayAttack != null)
        {
            sprayAttack.BeginSpray(CurrentRange, CurrentDamage, CurrentDotDamage, DotDuration, DotTickInterval);
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetTarget(target);
        bulletScript.SetDamage(CurrentDamage);
        bulletScript.SetDamageOverTime(CurrentDotDamage, DotDuration, DotTickInterval);
        bulletScript.SetUpgradeAppearance(damageUpgradeLevel);
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, currentTargetingRange, (Vector2)transform.position, 0f, enemyMask);
        if (hits.Length > 0)
            target = hits[0].transform;
    }

    private bool CheckIfTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= currentTargetingRange;
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void RecalculateStats()
    {
        currentTargetingRange = GetStatValue(TurretUpgradeType.Range);
        currentBps = GetStatValue(TurretUpgradeType.FireRate);
        currentDamage = (int)GetStatValue(TurretUpgradeType.Damage);
        currentDotDamage = usesDamageOverTime ? (int)GetStatValue(TurretUpgradeType.DamageOverTime) : 0;
        if (sprayAttack != null)
            sprayAttack.SetUpgradeAppearance(damageUpgradeLevel, dotUpgradeLevel);
    }

    public int GetUpgradeLevel(TurretUpgradeType type)
    {
        switch (type)
        {
            case TurretUpgradeType.Range: return rangeUpgradeLevel;
            case TurretUpgradeType.Damage: return damageUpgradeLevel;
            case TurretUpgradeType.FireRate: return fireRateUpgradeLevel;
            case TurretUpgradeType.DamageOverTime: return dotUpgradeLevel;
            default: return 0;
        }
    }

    public float GetStatValue(TurretUpgradeType type, bool nextLevel = false)
    {
        int rank = GetUpgradeLevel(type) + (nextLevel ? 1 : 0);
        switch (type)
        {
            case TurretUpgradeType.Range:
                return targetingRange * Mathf.Pow(rangeMultiplierPerLevel, rank);
            case TurretUpgradeType.Damage:
                return Mathf.CeilToInt(baseDamage * Mathf.Pow(damageMultiplierPerLevel, rank));
            case TurretUpgradeType.FireRate:
                return bps * Mathf.Pow(fireRateMultiplierPerLevel, rank);
            case TurretUpgradeType.DamageOverTime:
                return rank == 0 ? 0 : baseDotDamage + (rank - 1) * DotDamagePerRank;
            default:
                return 0f;
        }
    }

    public bool IsUpgradeTypeAvailable(TurretUpgradeType type)
    {
        switch (type)
        {
            case TurretUpgradeType.Range:
            case TurretUpgradeType.Damage:
                return true;
            case TurretUpgradeType.FireRate:
                return !usesDamageOverTime;
            case TurretUpgradeType.DamageOverTime:
                return usesDamageOverTime;
            default:
                return false;
        }
    }

    public bool IsMaxLevel()
    {
        return UpgradePointsUsed >= MaxUpgradePoints;
    }

    public int GetLevel()
    {
        return UpgradePointsUsed;
    }

    public int GetUpgradeCost()
    {
        return UpgradePointCost;
    }

    public bool TryUpgrade(TurretUpgradeType type)
    {
        if (!IsUpgradeTypeAvailable(type) || IsMaxLevel() || Time.timeScale <= 0f || LevelManager.main == null)
            return false;

        int cost = GetUpgradeCost();
        bool chargedFuel = !LevelManager.main.UnlimitedColourFuel;
        if (!LevelManager.main.SpendColourFuel(cost))
            return false;

        switch (type)
        {
            case TurretUpgradeType.Range: rangeUpgradeLevel++; break;
            case TurretUpgradeType.Damage: damageUpgradeLevel++; break;
            case TurretUpgradeType.FireRate: fireRateUpgradeLevel++; break;
            case TurretUpgradeType.DamageOverTime: dotUpgradeLevel++; break;
        }

        if (chargedFuel)
            TotalUpgradeSpend += cost;

        RecalculateStats();
        if (upgradeVisuals != null)
            upgradeVisuals.Refresh(true);
        Debug.Log("Turret " + type + " upgraded to rank " + GetUpgradeLevel(type) + "! Points: " + UpgradePointsUsed + "/" + MaxUpgradePoints + ". Cost: " + GetUpgradeCost() + " Colour Fuel");
        return true;
    }
}
