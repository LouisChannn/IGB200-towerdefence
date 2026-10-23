using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyCombatFeedback : MonoBehaviour
{
    private const float HitFlashDuration = 0.12f;
    private const float BarHeight = 0.12f;
    private static readonly Color Ink = new Color(0.10f, 0.14f, 0.16f);
    private static readonly Color Healthy = new Color(0.28f, 0.86f, 0.53f);
    private static readonly Color Hurt = new Color(1f, 0.73f, 0.25f);
    private static readonly Color Critical = new Color(1f, 0.29f, 0.34f);
    private static readonly Color Paint = new Color(1f, 0.48f, 0.12f);

    private static Texture2D stripTexture;
    private static Texture2D dropTexture;
    private static Sprite stripSprite;
    private static Sprite dropSprite;
    private static Material feedbackMaterial;
    private static int sharedUsers;

    private Health health;
    private SpriteRenderer body;
    private SpriteRenderer hitOverlay;
    private Transform visualRoot;
    private SpriteRenderer barBorder;
    private SpriteRenderer barTrack;
    private SpriteRenderer barTrail;
    private SpriteRenderer barFill;
    private readonly SpriteRenderer[] drops = new SpriteRenderer[3];
    private readonly SpriteRenderer[] dropBorders = new SpriteRenderer[3];
    private float flashRemaining;
    private float animationTime;
    private float trailFraction = 1f;
    private float trailHold;
    private Color flashColour;
    private bool ownsSharedAssets;

    private void Awake()
    {
        health = GetComponent<Health>();
        // Cache the actual body before adding any feedback SpriteRenderers.
        body = GetComponent<SpriteRenderer>();
        if (body == null)
            body = GetComponentInChildren<SpriteRenderer>();
        if (health == null || body == null)
        {
            enabled = false;
            return;
        }

        EnsureSharedAssets();
        sharedUsers++;
        ownsSharedAssets = true;
        GameObject root = new GameObject("Enemy Combat Feedback");
        root.layer = gameObject.layer;
        visualRoot = root.transform;
        visualRoot.SetParent(transform, false);

        barBorder = CreateSprite("Health Bar Border", stripSprite, visualRoot, 8);
        barTrack = CreateSprite("Health Bar Track", stripSprite, visualRoot, 9);
        barTrail = CreateSprite("Health Lost", stripSprite, visualRoot, 10);
        barFill = CreateSprite("Health Remaining", stripSprite, visualRoot, 11);
        barBorder.color = Ink;
        barTrack.color = new Color(0.22f, 0.25f, 0.27f);
        barTrail.color = new Color(1f, 0.93f, 0.64f);

        for (int i = 0; i < drops.Length; i++)
        {
            dropBorders[i] = CreateSprite("DOT Drop Border " + i, dropSprite, visualRoot, 3);
            drops[i] = CreateSprite("DOT Paint Drop " + i, dropSprite, visualRoot, 4);
            dropBorders[i].color = Ink;
            drops[i].color = Paint;
        }
        SetDropsVisible(false);

        hitOverlay = CreateSprite("Hit Flash", body.sprite, body.transform, 1);
        hitOverlay.enabled = false;
        RefreshVisuals(0f);
    }

    public void ShowHit(bool damageOverTime)
    {
        if (health == null || visualRoot == null)
            return;
        if (health.CurrentHitPoints <= 0)
        {
            HideVisuals();
            return;
        }

        flashRemaining = HitFlashDuration;
        flashColour = damageOverTime ? new Color(1f, 0.65f, 0.25f) : Color.white;
        trailHold = 0.18f;
        RefreshVisuals(0f);
    }

    private void LateUpdate()
    {
        RefreshVisuals(Time.deltaTime);
    }

    private void RefreshVisuals(float deltaTime)
    {
        if (health == null || body == null || visualRoot == null)
            return;
        if (health.CurrentHitPoints <= 0 || !body.enabled)
        {
            HideVisuals();
            return;
        }

        visualRoot.gameObject.SetActive(true);
        Bounds bounds = body.bounds;
        visualRoot.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
        visualRoot.rotation = Quaternion.identity;
        // Health bars keep their world size and stay upright for every prefab scale.
        Vector3 parentScale = transform.lossyScale;
        visualRoot.localScale = new Vector3(InverseScale(parentScale.x), InverseScale(parentScale.y), InverseScale(parentScale.z));
        float width = Mathf.Clamp(bounds.size.x * 0.92f, 0.8f, 1.5f);
        float y = bounds.extents.y + 0.19f;
        float fraction = Mathf.Clamp01((float)health.CurrentHitPoints / health.MaxHitPoints);
        trailHold = Mathf.Max(0f, trailHold - deltaTime);
        if (fraction >= trailFraction)
            trailFraction = fraction;
        else if (trailHold <= 0f)
            trailFraction = Mathf.MoveTowards(trailFraction, fraction, deltaTime * 1.8f);

        SetStrip(barBorder, 0f, y, width + 0.065f, BarHeight + 0.065f);
        SetStrip(barTrack, 0f, y, width, BarHeight);
        SetFill(barTrail, y, width, trailFraction);
        SetFill(barFill, y, width, fraction);
        barFill.color = fraction > 0.5f
            ? Color.Lerp(Hurt, Healthy, (fraction - 0.5f) * 2f)
            : Color.Lerp(Critical, Hurt, fraction * 2f);

        animationTime += deltaTime;
        bool dot = health.HasDamageOverTime;
        SetDropsVisible(dot);
        if (dot)
        {
            for (int i = 0; i < drops.Length; i++)
            {
                float phase = Mathf.Repeat(animationTime * 0.9f + i / 3f, 1f);
                float x = (i - 1) * Mathf.Max(0.18f, bounds.extents.x * 0.48f);
                float dropY = -bounds.extents.y * 0.55f + phase * 0.23f;
                float size = 0.15f + Mathf.Sin(phase * Mathf.PI) * 0.035f;
                SetStrip(dropBorders[i], x, dropY, size + 0.045f, size * 1.35f + 0.045f);
                SetStrip(drops[i], x, dropY, size, size * 1.35f);
            }
        }

        flashRemaining = Mathf.Max(0f, flashRemaining - deltaTime);
        hitOverlay.enabled = flashRemaining > 0f;
        if (hitOverlay.enabled)
        {
            hitOverlay.sprite = body.sprite;
            hitOverlay.flipX = body.flipX;
            hitOverlay.flipY = body.flipY;
            hitOverlay.drawMode = body.drawMode;
            hitOverlay.size = body.size;
            hitOverlay.maskInteraction = body.maskInteraction;
            hitOverlay.sortingLayerID = body.sortingLayerID;
            hitOverlay.sortingOrder = body.sortingOrder + 1;
            hitOverlay.color = new Color(flashColour.r, flashColour.g, flashColour.b,
                0.85f * Mathf.Clamp01(flashRemaining / HitFlashDuration) * body.color.a);
        }
    }

    private SpriteRenderer CreateSprite(string name, Sprite sprite, Transform parent, int orderOffset)
    {
        GameObject child = new GameObject(name);
        child.layer = gameObject.layer;
        child.transform.SetParent(parent, false);
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = feedbackMaterial;
        renderer.sortingLayerID = body.sortingLayerID;
        renderer.sortingOrder = body.sortingOrder + orderOffset;
        return renderer;
    }

    private static float InverseScale(float scale)
    {
        return Mathf.Abs(scale) > 0.0001f ? 1f / scale : 1f;
    }

    private static void SetStrip(SpriteRenderer renderer, float x, float y, float width, float height)
    {
        renderer.transform.localPosition = new Vector3(x, y, 0f);
        renderer.transform.localScale = new Vector3(width, height, 1f);
    }

    private static void SetFill(SpriteRenderer renderer, float y, float width, float fraction)
    {
        renderer.enabled = fraction > 0f;
        SetStrip(renderer, -width * (1f - fraction) * 0.5f, y, width * fraction, BarHeight);
    }

    private void SetDropsVisible(bool visible)
    {
        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i] != null) drops[i].enabled = visible;
            if (dropBorders[i] != null) dropBorders[i].enabled = visible;
        }
    }

    private void HideVisuals()
    {
        if (visualRoot != null) visualRoot.gameObject.SetActive(false);
        if (hitOverlay != null) hitOverlay.enabled = false;
        flashRemaining = 0f;
    }

    private void OnDisable()
    {
        HideVisuals();
    }

    private void OnDestroy()
    {
        // The flash may be parented to a nested body rather than the Health object.
        if (hitOverlay != null) Destroy(hitOverlay.gameObject);
        if (visualRoot != null) Destroy(visualRoot.gameObject);
        if (!ownsSharedAssets) return;
        sharedUsers--;
        if (sharedUsers > 0) return;
        Destroy(feedbackMaterial);
        Destroy(stripSprite);
        Destroy(dropSprite);
        Destroy(stripTexture);
        Destroy(dropTexture);
        feedbackMaterial = null;
        stripSprite = null;
        dropSprite = null;
        stripTexture = null;
        dropTexture = null;
    }

    private static void EnsureSharedAssets()
    {
        if (feedbackMaterial != null) return;
        Shader shader = Resources.Load<Shader>("CombatFeedback/EnemyHitFlash");
        if (shader == null)
            throw new System.InvalidOperationException("Enemy combat feedback shader is missing.");
        feedbackMaterial = new Material(shader) { name = "Enemy Feedback (Shared)", hideFlags = HideFlags.HideAndDontSave };
        stripTexture = MakeMask(false);
        dropTexture = MakeMask(true);
        stripSprite = Sprite.Create(stripTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        dropSprite = Sprite.Create(dropTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        stripSprite.hideFlags = dropSprite.hideFlags = HideFlags.HideAndDontSave;
    }

    private static Texture2D MakeMask(bool droplet)
    {
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
        {
            name = droplet ? "DOT Paint Drop Mask" : "Health Strip Mask",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        Color[] pixels = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            float px = (x + 0.5f) / 32f * 2f - 1f;
            float py = (y + 0.5f) / 32f * 2f - 1f;
            float edge;
            if (droplet)
            {
                float radius = py <= -0.1f ? Mathf.Sqrt(Mathf.Max(0f, 0.68f * 0.68f - (py + 0.1f) * (py + 0.1f))) : 0.68f * (0.9f - py);
                edge = Mathf.Min(radius - Mathf.Abs(px), py + 0.78f);
            }
            else
            {
                // Rounded corners retain the hand-painted menu's soft shapes.
                Vector2 corner = new Vector2(Mathf.Max(0f, Mathf.Abs(px) - 0.78f), Mathf.Max(0f, Mathf.Abs(py) - 0.55f));
                edge = 0.21f - corner.magnitude;
            }
            pixels[y * 32 + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(edge * 32f));
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
