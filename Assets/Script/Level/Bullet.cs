using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float lifetime = 5f;

    [Header("Homing")]
    [Tooltip("Turn rate (deg/sec) at launch. Lower = wider, smoother curve.")]
    [SerializeField] private float startTurnSpeed = 360f;
    [Tooltip("Extra deg/sec gained per second of flight. Stops the bullet orbiting.")]
    [SerializeField] private float turnRamp = 2000f;
    [Tooltip("Within this distance the bullet locks straight onto the target.")]
    [SerializeField] private float snapDistance = 1.5f;
    [Tooltip("Rotation offset so the sprite's front faces +X. Use -90 if your sprite points up.")]
    [SerializeField] private float spriteAngleOffset = 0f;

    [Header("Paint")]
    [SerializeField] private GameObject[] paintSplatterPrefabs;

    private Transform target;
    private Vector2 moveDir;
    private float age;
    private int dotDamage;
    private float dotDuration;
    private float dotTickInterval;
    private SpriteRenderer sourceArt;
    private SpriteRenderer upgradedArt;
    private bool sourceArtWasEnabled;

    public void SetUpgradeAppearance(int damageRank)
    {
        damageRank = Mathf.Clamp(damageRank, 0, Turret.MaxUpgradePoints);
        if (damageRank == 0)
        {
            if (upgradedArt != null) upgradedArt.enabled = false;
            if (sourceArt != null) sourceArt.enabled = sourceArtWasEnabled;
            return;
        }

        if (upgradedArt != null)
        {
            upgradedArt.transform.localScale = Vector3.one * (1f + damageRank * 0.07f);
            upgradedArt.enabled = true;
            sourceArt.enabled = false;
            return;
        }

        SpriteRenderer source = null;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer.sprite != null)
            {
                source = renderer;
                break;
            }
        }
        if (source == null)
            return;

        // Enlarge the artwork only: projectile collision and movement stay unchanged.
        var art = new GameObject("Upgraded Projectile Art");
        art.layer = gameObject.layer;
        art.transform.SetParent(source.transform, false);
        art.transform.localScale = Vector3.one * (1f + damageRank * 0.07f);
        SpriteRenderer visual = art.AddComponent<SpriteRenderer>();
        visual.sprite = source.sprite;
        visual.sharedMaterial = source.sharedMaterial;
        visual.color = source.color;
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder;
        visual.flipX = source.flipX;
        visual.flipY = source.flipY;
        visual.drawMode = source.drawMode;
        visual.size = source.size;
        visual.maskInteraction = source.maskInteraction;
        sourceArt = source;
        sourceArtWasEnabled = source.enabled;
        upgradedArt = visual;
        source.enabled = false;
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        // Smooths rendering between physics steps (removes jitter)
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // Stops fast bullets tunnelling through enemies
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        moveDir = transform.right;

        // Cleanup if it never hits anything
        Destroy(gameObject, lifetime);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;

        // Face the enemy immediately so there's no U-turn at launch
        if (target)
            moveDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetDamageOverTime(int damagePerTick, float duration, float tickInterval)
    {
        dotDamage = Mathf.Max(0, damagePerTick);
        dotDuration = Mathf.Max(0f, duration);
        dotTickInterval = Mathf.Max(0.05f, tickInterval);
    }

    private void FixedUpdate()
    {
        age += Time.fixedDeltaTime;

        // If the target dies, the bullet just keeps flying straight until lifetime ends
        if (target)
        {
            Vector2 toTarget = (Vector2)target.position - rb.position;
            Vector2 desired = toTarget.normalized;

            if (toTarget.magnitude <= snapDistance)
            {
                // Close enough: lock on so it can't miss or orbit
                moveDir = desired;
            }
            else
            {
                // Turn rate grows over time = gentle curve first, guaranteed hit later
                float turn = startTurnSpeed + turnRamp * age;
                float maxRadians = turn * Mathf.Deg2Rad * Time.fixedDeltaTime;
                moveDir = Vector3.RotateTowards(moveDir, desired, maxRadians, 0f);
            }
        }

        rb.linearVelocity = moveDir * bulletSpeed;

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg + spriteAngleOffset;
        rb.MoveRotation(angle);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Health enemyHealth = other.gameObject.GetComponent<Health>();

        if (enemyHealth != null)
        {
            // Hand this bullet's splatter set along - Health only spawns from it
            // if this hit is the one that actually kills the enemy
            enemyHealth.TakeDamage(bulletDamage, paintSplatterPrefabs);
            if (dotDamage > 0)
                enemyHealth.ApplyDamageOverTime(dotDamage, dotDuration, dotTickInterval, paintSplatterPrefabs);
        }

        Destroy(gameObject);
    }
}
