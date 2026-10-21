using UnityEngine;

public class SmoothHomingBullet : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); // Target died, destroy bullet
            return;
        }

        // Smooth frame-independent movement
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Optional: Rotate bullet to face the enemy smoothly
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
