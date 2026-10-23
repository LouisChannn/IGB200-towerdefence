using System.Collections.Generic;
using UnityEngine;

public sealed class TurretSprayAttack : MonoBehaviour
{
    private const int PulseCount = 5;
    private const int PuffPoolSize = 24;
    private const float PuffEmissionInterval = 0.0125f;
    private const float PuffLifetime = 0.18f;
    private const float TimeTolerance = 0.000001f;

    [SerializeField] private Transform muzzle;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField, Min(0.01f)] private float sprayDuration = 0.5f;
    [SerializeField, Range(1f, 179f)] private float coneAngle = 40f;
    [SerializeField] private Sprite[] sprayFrames;
    [SerializeField] private Material sprayMaterial;
    [SerializeField] private int sortingOrder = 16;

    private readonly HashSet<Health> hitEnemies = new HashSet<Health>();
    private Transform visualRoot;
    private SpriteRenderer[] puffRenderers;
    private Puff[] puffs;
    private int nextPuff;
    private int emissionNumber;
    private float elapsed;
    private float activeDuration;
    private float activeRange;
    private int activeTotalDamage;
    private int activeDotDamage;
    private float activeDotDuration;
    private float activeDotTickInterval;
    private int nextPulse;
    private float nextPuffTime;
    private float visualWidthMultiplier = 1f;
    private float visualEmissionInterval = PuffEmissionInterval;
    private Color visualColour = Color.white;

    public bool IsSpraying { get; private set; }

    public void SetUpgradeAppearance(int damageRank, int dotRank)
    {
        damageRank = Mathf.Clamp(damageRank, 0, Turret.MaxUpgradePoints);
        dotRank = Mathf.Clamp(dotRank, 0, Turret.MaxUpgradePoints);
        float strength = damageRank * 0.03f + dotRank * 0.015f;
        visualWidthMultiplier = 1f + Mathf.Min(0.35f, strength);
        visualEmissionInterval = PuffEmissionInterval / (1f + Mathf.Min(0.45f, strength));
        visualColour = dotRank > 0
            ? Color.Lerp(Color.white, new Color(1f, 0.5f, 0.16f), 0.25f + dotRank * 0.04f)
            : Color.white;
    }

    private struct Puff
    {
        public bool active;
        public float age;
        public Vector3 origin;
        public Vector2 direction;
        public float range;
        public float endWidth;
    }

    private void Awake()
    {
        EnsureVisualPool();
    }

    public void BeginSpray(float range, int totalDamage, int dotDamage, float dotDuration, float dotTickInterval)
    {
        if (IsSpraying || !isActiveAndEnabled || muzzle == null || range <= 0f || Time.timeScale <= 0f)
            return;

        EnsureVisualPool();
        activeDuration = Mathf.Max(0.01f, sprayDuration);
        activeRange = range;
        activeTotalDamage = Mathf.Max(0, totalDamage);
        activeDotDamage = Mathf.Max(0, dotDamage);
        activeDotDuration = Mathf.Max(0f, dotDuration);
        activeDotTickInterval = Mathf.Max(0.05f, dotTickInterval);
        elapsed = 0f;
        nextPulse = 0;
        nextPuff = 0;
        emissionNumber = 0;
        nextPuffTime = visualEmissionInterval;
        IsSpraying = true;
        visualRoot.gameObject.SetActive(true);

        ApplyPulse(nextPulse++);
        EmitPuff(0f);
    }

    private void Update()
    {
        AdvanceSpray(Time.deltaTime);
    }

    // Kept separate from Update so timing can be checked with explicit game-time deltas.
    private void AdvanceSpray(float deltaTime)
    {
        if (!IsSpraying || deltaTime <= 0f)
            return;
        if (muzzle == null)
        {
            StopSpray();
            return;
        }

        elapsed = Mathf.Min(activeDuration, elapsed + deltaTime);
        float pulseInterval = activeDuration / PulseCount;
        while (nextPulse < PulseCount && nextPulse * pulseInterval <= elapsed + TimeTolerance)
            ApplyPulse(nextPulse++);

        if (elapsed >= activeDuration)
        {
            // The burst owns its visuals: no puff is left travelling after the half-second ends.
            StopSpray();
            return;
        }

        AdvancePuffs(deltaTime);
        int emissions = 0;
        while (nextPuffTime < activeDuration && nextPuffTime <= elapsed + TimeTolerance && emissions < PuffPoolSize)
        {
            EmitPuff(Mathf.Max(0f, elapsed - nextPuffTime));
            nextPuffTime += visualEmissionInterval;
            emissions++;
        }

        // A stalled frame never produces an unbounded backlog of old visual emissions.
        if (nextPuffTime <= elapsed)
            nextPuffTime = elapsed + visualEmissionInterval;
    }

    private void ApplyPulse(int pulseIndex)
    {
        int pulseDamage = activeTotalDamage / PulseCount +
            (pulseIndex < activeTotalDamage % PulseCount ? 1 : 0);
        Vector2 origin = muzzle.position;
        Vector2 forward = muzzle.up;
        float minimumDot = Mathf.Cos(Mathf.Clamp(coneAngle, 1f, 179f) * 0.5f * Mathf.Deg2Rad);
        float rangeSquared = activeRange * activeRange;
        Collider2D[] candidates = Physics2D.OverlapCircleAll(origin, activeRange, enemyMask);
        hitEnemies.Clear();

        foreach (Collider2D candidate in candidates)
        {
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
                continue;
            Health health = candidate.GetComponentInParent<Health>();
            if (health == null || !health.gameObject.activeInHierarchy || health.CurrentHitPoints <= 0)
                continue;

            Vector2 offset = (Vector2)health.transform.position - origin;
            float distanceSquared = offset.sqrMagnitude;
            if (distanceSquared > rangeSquared ||
                (distanceSquared > 0.000001f && Vector2.Dot(forward, offset.normalized) < minimumDot))
                continue;
            if (!hitEnemies.Add(health))
                continue;

            if (pulseDamage > 0)
                health.TakeDamage(pulseDamage);
            if (activeDotDamage > 0)
                health.ApplyDamageOverTime(activeDotDamage, activeDotDuration, activeDotTickInterval);
        }
    }

    private void EnsureVisualPool()
    {
        if (visualRoot != null)
            return;

        var rootObject = new GameObject("Spray Visuals");
        rootObject.layer = gameObject.layer;
        visualRoot = rootObject.transform;
        visualRoot.SetParent(transform, false);
        puffRenderers = new SpriteRenderer[PuffPoolSize];
        puffs = new Puff[PuffPoolSize];

        for (int i = 0; i < PuffPoolSize; i++)
        {
            var puffObject = new GameObject("Spray Puff " + i);
            puffObject.layer = gameObject.layer;
            puffObject.transform.SetParent(visualRoot, false);
            SpriteRenderer renderer = puffObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            if (sprayMaterial != null)
                renderer.sharedMaterial = sprayMaterial;
            renderer.enabled = false;
            puffRenderers[i] = renderer;
        }

        rootObject.SetActive(false);
    }

    private void EmitPuff(float age)
    {
        if (age >= PuffLifetime || sprayFrames == null || sprayFrames.Length == 0)
            return;

        // Deterministic spread avoids changing the game's shared random sequence.
        float spread = Mathf.Sin(emissionNumber++ * 2.399963f);
        float halfAngle = Mathf.Clamp(coneAngle, 1f, 179f) * 0.5f;
        Vector2 direction = Quaternion.Euler(0f, 0f, spread * halfAngle * 0.6f) * muzzle.up;
        puffs[nextPuff] = new Puff
        {
            active = true,
            age = age,
            origin = muzzle.position,
            direction = direction,
            range = activeRange,
            endWidth = Mathf.Max(0.35f, activeRange * Mathf.Tan(halfAngle * Mathf.Deg2Rad) * 1.1f)
        };
        DrawPuff(nextPuff);
        nextPuff = (nextPuff + 1) % PuffPoolSize;
    }

    private void AdvancePuffs(float deltaTime)
    {
        for (int i = 0; i < puffs.Length; i++)
        {
            if (!puffs[i].active)
                continue;
            puffs[i].age += deltaTime;
            if (puffs[i].age >= PuffLifetime)
            {
                puffs[i].active = false;
                puffRenderers[i].enabled = false;
            }
            else
            {
                DrawPuff(i);
            }
        }
    }

    private void DrawPuff(int index)
    {
        Puff puff = puffs[index];
        SpriteRenderer renderer = puffRenderers[index];
        float progress = Mathf.Clamp01(puff.age / PuffLifetime);
        // The final source frame is transparent; fade the last visible frame instead.
        int visibleFrames = Mathf.Max(1, sprayFrames.Length - 1);
        int frame = Mathf.Min(visibleFrames - 1, Mathf.FloorToInt(progress * visibleFrames));
        Sprite sprite = sprayFrames[frame];
        if (sprite == null)
        {
            renderer.enabled = false;
            return;
        }

        renderer.sprite = sprite;
        renderer.color = new Color(visualColour.r, visualColour.g, visualColour.b,
            1f - Mathf.InverseLerp(0.55f, 1f, progress));
        renderer.sortingOrder = sortingOrder;
        if (sprayMaterial != null)
            renderer.sharedMaterial = sprayMaterial;
        renderer.transform.position = puff.origin + (Vector3)(puff.direction * puff.range * progress);
        renderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(puff.direction.y, puff.direction.x) * Mathf.Rad2Deg - 90f);
        float width = Mathf.Lerp(Mathf.Max(0.18f, puff.range * 0.06f), puff.endWidth, progress) * visualWidthMultiplier;
        float parentScale = Mathf.Max(0.0001f, Mathf.Abs(visualRoot.lossyScale.x));
        float spriteWidth = Mathf.Max(0.001f, sprite.bounds.size.x);
        renderer.transform.localScale = Vector3.one * (width / (spriteWidth * parentScale));
        renderer.enabled = true;
    }

    private void OnDisable()
    {
        StopSpray();
    }

    private void StopSpray()
    {
        IsSpraying = false;
        hitEnemies.Clear();
        if (puffRenderers != null)
        {
            for (int i = 0; i < puffRenderers.Length; i++)
            {
                if (puffRenderers[i] != null)
                    puffRenderers[i].enabled = false;
                puffs[i].active = false;
            }
        }
        if (visualRoot != null)
            visualRoot.gameObject.SetActive(false);
    }
}
