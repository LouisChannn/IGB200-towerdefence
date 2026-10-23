using UnityEngine;

[DisallowMultipleComponent]
public sealed class TurretUpgradeVisuals : MonoBehaviour
{
    private static readonly Color RangeColour = new Color(0.25f, 0.82f, 1f);
    private static readonly Color DamageColour = new Color(1f, 0.38f, 0.35f);
    private static readonly Color RateColour = new Color(0.29f, 0.9f, 0.57f);
    private static readonly Color DotColour = new Color(1f, 0.6f, 0.17f);
    private static readonly Color Ink = new Color(0.09f, 0.14f, 0.16f);
    private static Texture2D dotTexture;
    private static Texture2D ringTexture;
    private static Sprite dotSprite;
    private static Sprite ringSprite;
    private static Material sharedMaterial;
    private static int sharedUsers;

    private Turret turret;
    private Transform bodyArt;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 muzzlePivot;
    private Transform visualRoot;
    private Transform markers;
    private SpriteRenderer rangeRing;
    private SpriteRenderer upgradePulse;
    private readonly SpriteRenderer[] levelMarkers = new SpriteRenderer[Turret.MaxUpgradePoints];
    private float markerRadius;
    private float pulseRemaining;
    private bool selected;
    private bool initialized;

    public void Initialize(Turret owner, Transform muzzle)
    {
        if (initialized || owner == null)
            return;
        SpriteRenderer body = null;
        foreach (SpriteRenderer renderer in owner.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer.sprite != null)
            {
                body = renderer;
                break;
            }
        }
        if (body == null)
            return;

        turret = owner;
        bodyArt = body.transform;
        originalScale = bodyArt.localScale;
        originalPosition = bodyArt.localPosition;
        muzzlePivot = muzzle != null && bodyArt.parent != null
            ? bodyArt.parent.InverseTransformPoint(muzzle.position) : originalPosition;
        markerRadius = Mathf.Clamp(body.sprite.bounds.size.x * Mathf.Abs(bodyArt.lossyScale.x) * 0.5f + 0.1f, 0.38f, 0.6f);
        EnsureSharedAssets();
        sharedUsers++;
        initialized = true;

        visualRoot = new GameObject("Turret Upgrade Visuals").transform;
        visualRoot.gameObject.layer = gameObject.layer;
        visualRoot.SetParent(transform, false);
        markers = new GameObject("Upgrade Level Markers").transform;
        markers.SetParent(visualRoot, false);
        for (int i = 0; i < levelMarkers.Length; i++)
        {
            float angle = (90f - i * 360f / Turret.MaxUpgradePoints) * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * markerRadius;
            SpriteRenderer border = MakeSprite("Level Socket " + (i + 1), dotSprite, markers, body, 2);
            border.color = Ink;
            border.transform.localPosition = position;
            border.transform.localScale = Vector3.one * 0.145f;
            SpriteRenderer marker = MakeSprite("Upgrade Level " + (i + 1), dotSprite, markers, body, 3);
            marker.transform.localPosition = position;
            marker.transform.localScale = Vector3.one * 0.105f;
            levelMarkers[i] = marker;
        }
        rangeRing = MakeSprite("Current Turret Range", ringSprite, visualRoot, body, -2);
        rangeRing.color = new Color(RangeColour.r, RangeColour.g, RangeColour.b, 0.3f);
        upgradePulse = MakeSprite("Upgrade Purchased Pulse", ringSprite, visualRoot, body, 1);
        upgradePulse.enabled = false;
        Refresh(false);
        PositionVisuals();
    }

    public void SetSelected(bool value)
    {
        selected = value;
        if (rangeRing != null)
            rangeRing.enabled = value && isActiveAndEnabled;
    }

    public void Refresh(bool celebrate)
    {
        if (!initialized || turret == null)
            return;
        int damageRank = turret.GetUpgradeLevel(TurretUpgradeType.Damage);
        float growth = 1f + damageRank * 0.012f;
        // Grow only the leaf artwork around its muzzle; targeting and the fire point stay fixed.
        if (bodyArt != transform)
        {
            bodyArt.localScale = originalScale * growth;
            bodyArt.localPosition = muzzlePivot + (originalPosition - muzzlePivot) * growth;
        }
        int rangeRanks = turret.GetUpgradeLevel(TurretUpgradeType.Range);
        int damageRanks = turret.GetUpgradeLevel(TurretUpgradeType.Damage);
        int specialRanks = turret.UsesDamageOverTime
            ? turret.GetUpgradeLevel(TurretUpgradeType.DamageOverTime)
            : turret.GetUpgradeLevel(TurretUpgradeType.FireRate);
        markers.gameObject.SetActive(turret.UpgradePointsUsed > 0);
        for (int i = 0; i < levelMarkers.Length; i++)
        {
            levelMarkers[i].color = i < rangeRanks ? RangeColour :
                i < rangeRanks + damageRanks ? DamageColour :
                i < rangeRanks + damageRanks + specialRanks ? (turret.UsesDamageOverTime ? DotColour : RateColour) :
                new Color(0.24f, 0.29f, 0.3f);
        }
        rangeRing.transform.localScale = Vector3.one * (turret.CurrentRange * 2f);
        rangeRing.enabled = selected && isActiveAndEnabled;
        if (celebrate)
            pulseRemaining = 0.5f;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;
        PositionVisuals();
        float delta = Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f;
        pulseRemaining = Mathf.Max(0f, pulseRemaining - delta);
        upgradePulse.enabled = pulseRemaining > 0f;
        if (upgradePulse.enabled)
        {
            float progress = 1f - pulseRemaining / 0.5f;
            upgradePulse.transform.localScale = Vector3.one * markerRadius * 2f * Mathf.Lerp(1f, 1.7f, progress);
            upgradePulse.color = new Color(1f, 0.86f, 0.4f, (1f - progress) * 0.8f);
        }
    }

    private void PositionVisuals()
    {
        visualRoot.position = transform.position;
        visualRoot.rotation = Quaternion.identity;
        Vector3 scale = transform.lossyScale;
        visualRoot.localScale = new Vector3(Inverse(scale.x), Inverse(scale.y), Inverse(scale.z));
    }

    private static float Inverse(float value)
    {
        return Mathf.Abs(value) > 0.0001f ? 1f / value : 1f;
    }

    private SpriteRenderer MakeSprite(string name, Sprite sprite, Transform parent, SpriteRenderer body, int orderOffset)
    {
        var child = new GameObject(name);
        child.layer = gameObject.layer;
        child.transform.SetParent(parent, false);
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = sharedMaterial;
        renderer.sortingLayerID = body.sortingLayerID;
        renderer.sortingOrder = body.sortingOrder + orderOffset;
        return renderer;
    }

    private void OnDisable()
    {
        if (rangeRing != null) rangeRing.enabled = false;
        if (upgradePulse != null) upgradePulse.enabled = false;
        pulseRemaining = 0f;
        selected = false;
    }

    private void OnDestroy()
    {
        if (!initialized)
            return;
        if (bodyArt != null && bodyArt != transform)
        {
            bodyArt.localScale = originalScale;
            bodyArt.localPosition = originalPosition;
        }
        if (visualRoot != null) Destroy(visualRoot.gameObject);
        sharedUsers--;
        if (sharedUsers > 0) return;
        Destroy(sharedMaterial);
        Destroy(dotSprite);
        Destroy(ringSprite);
        Destroy(dotTexture);
        Destroy(ringTexture);
        sharedMaterial = null;
        dotSprite = null;
        ringSprite = null;
        dotTexture = null;
        ringTexture = null;
    }

    private static void EnsureSharedAssets()
    {
        if (sharedMaterial != null)
            return;
        Shader shader = Resources.Load<Shader>("CombatFeedback/EnemyHitFlash");
        if (shader == null)
            throw new System.InvalidOperationException("Turret upgrade visual shader is missing.");
        sharedMaterial = new Material(shader) { name = "Turret Upgrade Markers (Shared)", hideFlags = HideFlags.HideAndDontSave };
        dotTexture = MakeMask(32, false);
        ringTexture = MakeMask(256, true);
        dotSprite = Sprite.Create(dotTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        ringSprite = Sprite.Create(ringTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 256f);
        dotSprite.hideFlags = ringSprite.hideFlags = HideFlags.HideAndDontSave;
    }

    private static Texture2D MakeMask(int size, bool ring)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = ring ? "Turret Range Ring" : "Upgrade Marker",
            filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float radius = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f).magnitude;
            float alpha = ring ? Mathf.Clamp01((0.99f - radius) * size) * Mathf.Clamp01((radius - 0.965f) * size) :
                Mathf.Clamp01((0.92f - radius) * size);
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
