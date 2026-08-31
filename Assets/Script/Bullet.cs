using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Paint")]
    [SerializeField] private GameObject paintSplatterPrefab;

    private Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
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
        // Only create paint when hitting an enemy
        Health enemyHealth = other.gameObject.GetComponent<Health>();

        if (enemyHealth != null)
        {
            // Create paint at the bullet's position
            if (paintSplatterPrefab != null)
            {
                Instantiate(
                    paintSplatterPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }

            // Damage enemy
            enemyHealth.TakeDamage(bulletDamage);
        }

        // Destroy bullet
        Destroy(gameObject);
    }
}