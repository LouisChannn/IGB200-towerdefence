using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private int PaintFuelReward = 10;
    private bool isDestroyed = false;

    public void TakeDamage(int damage)
    {
        hitPoint -= damage;

        if (hitPoint <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroyed.Invoke();
            LevelManager.main.IncreasePaintFuel(PaintFuelReward);
            isDestroyed = true;
            Destroy(gameObject);
        }
    }


}
