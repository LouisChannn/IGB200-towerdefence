using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;

    [Header("Colour Fuel Reward")]
    [SerializeField] private int colourFuelReward = 10;

    [Header("Colour Meter Reward")]
    [SerializeField] private int colourMeterReward = 10;

    private bool isDestroyed = false;
    private float colourFuelRewardMultiplier = 1f;

    public void AddBonusHealth(int bonusHealth)
    {
        hitPoint += Mathf.Max(0, bonusHealth);
    }

    public void SetColourFuelRewardMultiplier(float multiplier)
    {
        colourFuelRewardMultiplier = Mathf.Max(0f, multiplier);
    }

    public void TakeDamage(int damage, GameObject[] paintSplatterPrefabs = null)
    {
        if (isDestroyed || hitPoint <= 0 || damage <= 0)
            return;

        int appliedDamage = Mathf.Min(damage, hitPoint);
        hitPoint -= appliedDamage;
        FloatingDamageNumber.Show(appliedDamage, transform);

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
            LevelManager.main.IncreaseColourMeter(colourMeterReward);

            Destroy(gameObject);
        }
    }
}
