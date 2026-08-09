using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [Header("Attributes")]
    [SerializeField] private float speed = 2f;

    private Transform target;
    private int pathIndex = 0;
    private void Start()
    {
        // Set the initial target to the first point in the path defined by the LevelManager
        target = LevelManager.main.path[pathIndex];
    }
    private void Update()
    {
        // Check if the enemy is close enough to the target point
        if(Vector2.Distance(target.position,transform.position) < 0.1f) 
        {
            // If the enemy has reached the target point, move to the next point in the path
            pathIndex++;
            // If the enemy has reached the end of the path, invoke the onEnemyDestroyed event and destroy the enemy game object
            if(pathIndex >= LevelManager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroyed.Invoke(); // Notify the EnemySpawner that an enemy has been destroyed
                Destroy(gameObject);
                return;
            } else 
            {
                // Move to the next target point in the path
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    void FixedUpdate()
    {
        // Calculate the direction towards the target point and move the enemy in that direction
        Vector2 direction = (target.position - transform.position).normalized;
        // Set the velocity of the Rigidbody2D to move the enemy towards the target point
        rb.linearVelocity = direction * speed;
    }

}
