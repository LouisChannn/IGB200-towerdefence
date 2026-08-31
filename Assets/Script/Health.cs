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
            EnemySpawner.onEnemyDestroyed.Invoke();
            LevelManager.main.IncreasePaintFuel(paintFuelReward);
            isDestroyed = true;
            Destroy(gameObject);
        }
    }


}
