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
            // Create a random paint splatter
            if (paintSplatterPrefabs != null &&
                paintSplatterPrefabs.Length > 0)
            {
                int randomIndex =
                    Random.Range(0, paintSplatterPrefabs.Length);

                GameObject selectedSplatter =
                    paintSplatterPrefabs[randomIndex];

                Instantiate(
                    selectedSplatter,
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