using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 20f;
    
    private Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void FixedUpdate ()
        {
            if (!target) return;
            
            Vector2 direction = (target.position - transform.position).normalized;

            rb.linearVelocity = direction * bulletSpeed;
        }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Take health from the enemy
        Destroy(gameObject);
    }
}
