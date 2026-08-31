using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private int paintFuelReward = 10;

    private bool isDestroyed = false;

    public void TakeDamage(int damage)
    {
        hitPoint -= damage;

        if (hitPoint <= 0 && !isDestroyed)
        {
            isDestroyed = true;

            // Enemy destroyed
            EnemySpawner.onEnemyDestroyed.Invoke();

            // Give Paint Fuel reward
            LevelManager.main.IncreasePaintFuel(paintFuelReward);

            // Destroy enemy
            Destroy(gameObject);
        }
    }
}