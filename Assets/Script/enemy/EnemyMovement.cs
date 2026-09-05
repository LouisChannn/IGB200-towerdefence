using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float speed = 2f;

    [Header("Enemy Weight")]
    [SerializeField] private int enemyWeight = 1;

    private Transform target;
    private int pathIndex = 0;

    private void Start()
    {
        // Set the first target
        target = LevelManager.main.path[pathIndex];
    }

    private void Update()
    {
        // Check if the enemy has reached the current path point
        if (Vector2.Distance(target.position, transform.position) < 0.1f)
        {
            pathIndex++;

            // Enemy has reached the END of the path
            if (pathIndex >= LevelManager.main.path.Length)
            {
                // Deal damage to the player's HP
                LevelManager.main.TakePlayerDamage(enemyWeight);

                // Notify the EnemySpawner that this enemy is gone
                EnemySpawner.onEnemyDestroyed.Invoke();

                // Destroy the enemy
                Destroy(gameObject);

                return;
            }

            // Move towards the next path point
            target = LevelManager.main.path[pathIndex];
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        // Move towards target
        Vector2 direction =
            (target.position - transform.position).normalized;

        rb.linearVelocity = direction * speed;
    }
}