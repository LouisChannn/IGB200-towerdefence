using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;

    public void TakeDamage(int damage)
    {
        hitPoint -= damage;

        if (hitPoint <= 0)
        {
            EnemySpawner.onEnemyDestroyed.Invoke();
            Destroy(gameObject);
        }
    }


}
