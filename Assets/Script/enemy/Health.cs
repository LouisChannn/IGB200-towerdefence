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

    public void TakeDamage(int damage, GameObject[] paintSplatterPrefabs = null)
    {
        hitPoint -= damage;

        if (hitPoint <= 0 && !isDestroyed)
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
            LevelManager.main.IncreaseColourFuel(colourFuelReward);

            // Fill Colour Meter
            LevelManager.main.IncreaseColourMeter(colourMeterReward);

            Destroy(gameObject);
        }
    }
}