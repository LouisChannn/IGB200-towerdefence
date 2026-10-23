using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 20;

    [Header("Colour Fuel Reward")]
    [SerializeField] private int colourFuelReward = 10;

    [Header("Colour Meter Reward")]
    [SerializeField] private int colourMeterReward = 10;

    private bool isDestroyed = false;
    private float colourFuelRewardMultiplier = 1f;
    private int dotDamage;
    private float dotTimeRemaining;
    private float dotTickCountdown;
    private float dotTickInterval;
    private GameObject[] dotPaintSplatterPrefabs;
    private int maxHitPoints;
    private EnemyCombatFeedback combatFeedback;

    public int CurrentHitPoints => hitPoint;
    public int MaxHitPoints => Mathf.Max(1, maxHitPoints);
    public bool HasDamageOverTime => dotDamage > 0 && dotTimeRemaining > 0f;

    private void Awake()
    {
        maxHitPoints = Mathf.Max(1, hitPoint);
        combatFeedback = GetComponent<EnemyCombatFeedback>();
        if (combatFeedback == null)
            combatFeedback = gameObject.AddComponent<EnemyCombatFeedback>();
    }

    private void Update()
    {
        AdvanceDamageOverTime(Time.deltaTime);
    }

    public void ApplyDamageOverTime(int damagePerTick, float duration, float tickInterval, GameObject[] paintSplatterPrefabs = null)
    {
        if (isDestroyed || hitPoint <= 0 || damagePerTick <= 0 || duration <= 0f || tickInterval <= 0f)
            return;

        bool alreadyAffected = HasDamageOverTime;
        // Repeated spray hits refresh one coating instead of stacking per projectile.
        // Preserve its ticking clock so frequent hits cannot postpone damage forever.
        if (!alreadyAffected || damagePerTick >= dotDamage)
        {
            dotDamage = damagePerTick;
            dotTickInterval = tickInterval;
            dotPaintSplatterPrefabs = paintSplatterPrefabs;
        }
        if (!alreadyAffected)
            dotTickCountdown = dotTickInterval;
        dotTimeRemaining = Mathf.Max(dotTimeRemaining, duration);
    }

    private void AdvanceDamageOverTime(float deltaTime)
    {
        if (isDestroyed || !HasDamageOverTime || deltaTime <= 0f)
            return;

        float activeTime = Mathf.Min(deltaTime, dotTimeRemaining);
        dotTimeRemaining = Mathf.Max(0f, dotTimeRemaining - deltaTime);
        dotTickCountdown -= activeTime;
        while (dotTickCountdown <= 0.0001f && !isDestroyed)
        {
            ApplyDamage(dotDamage, dotPaintSplatterPrefabs, true, true);
            dotTickCountdown += dotTickInterval;
        }

        if (dotTimeRemaining <= 0f || isDestroyed)
            ClearDamageOverTime();
    }

    private void OnDisable()
    {
        ClearDamageOverTime();
    }

    private void ClearDamageOverTime()
    {
        dotDamage = 0;
        dotTimeRemaining = 0f;
        dotTickCountdown = 0f;
        dotTickInterval = 0f;
        dotPaintSplatterPrefabs = null;
    }

    public void AddBonusHealth(int bonusHealth)
    {
        int bonus = Mathf.Max(0, bonusHealth);
        maxHitPoints = Mathf.Max(maxHitPoints, hitPoint) + bonus;
        hitPoint += bonus;
    }

    public void SetColourFuelRewardMultiplier(float multiplier)
    {
        colourFuelRewardMultiplier = Mathf.Max(0f, multiplier);
    }

    // A path exit and a lethal hit can occur in the same frame. Resolve removal once.
    public bool TryResolveEscape()
    {
        if (isDestroyed || hitPoint <= 0)
            return false;

        isDestroyed = true;
        ClearDamageOverTime();
        return true;
    }

    public void TakeDamage(int damage, GameObject[] paintSplatterPrefabs = null, bool awardColourMeter = true)
    {
        ApplyDamage(damage, paintSplatterPrefabs, awardColourMeter, false);
    }

    private void ApplyDamage(int damage, GameObject[] paintSplatterPrefabs, bool awardColourMeter, bool damageOverTime)
    {
        if (isDestroyed || hitPoint <= 0 || damage <= 0)
            return;

        int appliedDamage = Mathf.Min(damage, hitPoint);
        hitPoint -= appliedDamage;
        FloatingDamageNumber.Show(appliedDamage, transform, damageOverTime);
        if (combatFeedback != null)
            combatFeedback.ShowHit(damageOverTime);

        if (hitPoint <= 0)
        {
            isDestroyed = true;

            // Leave paint behind, chosen from whichever bullet landed the killing blow
            if (paintSplatterPrefabs != null && paintSplatterPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, paintSplatterPrefabs.Length);
                Instantiate(paintSplatterPrefabs[randomIndex], transform.position, Quaternion.identity);
            }

            // Enemy destroyed
            EnemySpawner.onEnemyDestroyed.Invoke();

            // Give Colour Fuel
            int fuelReward = colourFuelReward > 0
                ? Mathf.Max(1, Mathf.RoundToInt(colourFuelReward * colourFuelRewardMultiplier))
                : 0;
            LevelManager.main.IncreaseColourFuel(fuelReward);

            // Fill Colour Meter
            if (awardColourMeter)
                LevelManager.main.IncreaseColourMeter(colourMeterReward);

            Destroy(gameObject);
        }
    }
}
