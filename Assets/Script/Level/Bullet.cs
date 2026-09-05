using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Paint")]
    [SerializeField] private GameObject[] paintSplatterPrefabs;

    private Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    private void FixedUpdate()
    {
        if (!target) return;

        Vector2 direction =
            (target.position - transform.position).normalized;

        rb.linearVelocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Health enemyHealth = other.gameObject.GetComponent<Health>();

        if (enemyHealth != null)
        {
            // Hand this bullet's splatter set along - Health only spawns from it
            // if this hit is the one that actually kills the enemy
            enemyHealth.TakeDamage(bulletDamage, paintSplatterPrefabs);
        }

        // Destroy bullet
        Destroy(gameObject);
    }
}