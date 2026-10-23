using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a circle, sector or ring without a sprite. Angles start at +X and
/// increase counterclockwise; the inner radius is a fraction of the outer one.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public sealed class RadialMenuGraphic : MaskableGraphic
{
    private const int CircleSegments = 64;
    private const float FullCircleTolerance = 0.001f;

    [SerializeField] private float startAngle;
    [SerializeField, Range(0f, 360f)] private float angleDegrees = 360f;
    [SerializeField, Range(0f, 1f)] private float innerRadius;
    [SerializeField, Range(0, 64)] private int levelBandCount;
    [SerializeField] private int filledLevelBands;

    public float StartAngle => startAngle;
    public float AngleDegrees => angleDegrees;
    public float InnerRadius => innerRadius;
    public int LevelBandCount => levelBandCount;
    public int FilledLevelBands => filledLevelBands;

    public void Configure(float startAngle, float angleDegrees, float innerRadius = 0f)
    {
        this.startAngle = Mathf.Repeat(startAngle, 360f);
        this.angleDegrees = Mathf.Clamp(angleDegrees, 0f, 360f);
        this.innerRadius = Mathf.Clamp01(innerRadius);
        SetVerticesDirty();
    }

    /// <summary>
    /// Draws rank bands from the inner edge outwards. Use on a separate,
    /// non-raycast Graphic so a Button can tint its base without tinting progress.
    /// A count of zero preserves the normal solid-circle/sector rendering.
    /// </summary>
    public void ConfigureLevelBands(int count, int filled)
    {
        count = Mathf.Clamp(count, 0, 64);
        filled = Mathf.Clamp(filled, 0, count);
        if (levelBandCount == count && filledLevelBands == filled)
            return;
        levelBandCount = count;
        filledLevelBands = filled;
        SetVerticesDirty();
    }

    public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
    {
        return ContainsScreenPoint(screenPoint, eventCamera) && base.Raycast(screenPoint, eventCamera);
    }

    /// <summary>
    /// Tests only this shape. Parent masks and CanvasGroups are additionally
    /// respected by Raycast. Transform scaling, including a press animation,
    /// is included by converting the screen point into this rect's space.
    /// </summary>
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        Rect rect = rectTransform.rect;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        if (radius <= 0f || angleDegrees <= 0f || innerRadius >= 1f)
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPoint, eventCamera, out Vector2 localPoint))
            return false;

        Vector2 relativePoint = localPoint - rect.center;
        float distanceSquared = relativePoint.sqrMagnitude;
        float holeRadius = radius * innerRadius;
        if (distanceSquared > radius * radius || distanceSquared < holeRadius * holeRadius)
            return false;

        if (angleDegrees >= 360f - FullCircleTolerance || distanceSquared <= Mathf.Epsilon)
            return true;

        float pointAngle = Mathf.Atan2(relativePoint.y, relativePoint.x) * Mathf.Rad2Deg;
        float offset = Mathf.Repeat(pointAngle - startAngle, 360f);
        return offset <= angleDegrees;
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        if (radius <= 0f || angleDegrees <= 0f || innerRadius >= 1f)
            return;

        if (levelBandCount > 0)
        {
            PopulateLevelBands(vertexHelper, rect.center, radius);
            return;
        }

        int segments = Mathf.Max(1, Mathf.CeilToInt(CircleSegments * angleDegrees / 360f));
        Color32 tint = color;
        Vector2 center = rect.center;

        if (innerRadius <= 0f)
        {
            AddVertex(vertexHelper, center, new Vector2(0.5f, 0.5f), tint);
            for (int i = 0; i <= segments; i++)
            {
                Vector2 direction = GetDirection(i, segments);
                AddVertex(vertexHelper, center + direction * radius,
                    Vector2.one * 0.5f + direction * 0.5f, tint);
                if (i > 0)
                    vertexHelper.AddTriangle(0, i + 1, i);
            }
            return;
        }

        for (int i = 0; i <= segments; i++)
        {
            Vector2 direction = GetDirection(i, segments);
            AddVertex(vertexHelper, center + direction * (radius * innerRadius),
                Vector2.one * 0.5f + direction * (innerRadius * 0.5f), tint);
            AddVertex(vertexHelper, center + direction * radius,
                Vector2.one * 0.5f + direction * 0.5f, tint);

            if (i > 0)
            {
                int previous = (i - 1) * 2;
                vertexHelper.AddTriangle(previous, previous + 3, previous + 1);
                vertexHelper.AddTriangle(previous, previous + 2, previous + 3);
            }
        }
    }

    private void PopulateLevelBands(VertexHelper vertexHelper, Vector2 center, float radius)
    {
        float inner = radius * innerRadius;
        float step = (radius - inner) / levelBandCount;
        float halfLine = Mathf.Min(radius * 0.011f, step * 0.3f) * 0.5f;
        Color32 filled = MultiplyTint(new Color(0.12f, 0.9f, 0.32f, 1f));
        // Translucent empty bands let the underlying Button's hover colour show through.
        Color32 empty = MultiplyTint(new Color(0.3f, 0.32f, 0.12f, 0.52f));
        Color32 line = MultiplyTint(Color.black);

        for (int i = 0; i < levelBandCount; i++)
        {
            float bandInner = inner + step * i + halfLine;
            float bandOuter = inner + step * (i + 1) - halfLine;
            AddRingSection(vertexHelper, center, radius, bandInner, bandOuter,
                startAngle, angleDegrees, i < filledLevelBands ? filled : empty);
        }

        // Eleven boundaries create exactly ten concentric sections, not ten extra lines.
        for (int i = 0; i <= levelBandCount; i++)
        {
            float boundary = inner + step * i;
            AddRingSection(vertexHelper, center, radius,
                Mathf.Max(inner, boundary - halfLine), Mathf.Min(radius, boundary + halfLine),
                startAngle, angleDegrees, line);
        }

        if (angleDegrees < 360f - FullCircleTolerance)
        {
            float edgeAngle = Mathf.Min(0.7f, angleDegrees * 0.1f);
            AddRingSection(vertexHelper, center, radius, inner, radius,
                startAngle, edgeAngle, line);
            AddRingSection(vertexHelper, center, radius, inner, radius,
                startAngle + angleDegrees - edgeAngle, edgeAngle, line);
        }
    }

    private Color32 MultiplyTint(Color tint)
    {
        return new Color(tint.r * color.r, tint.g * color.g, tint.b * color.b, tint.a * color.a);
    }

    private static void AddRingSection(VertexHelper vertexHelper, Vector2 center, float radius,
        float inner, float outer, float start, float sweep, Color32 tint)
    {
        if (outer <= inner || sweep <= 0f)
            return;

        int segments = Mathf.Max(1, Mathf.CeilToInt(CircleSegments * sweep / 360f));
        for (int i = 0; i < segments; i++)
        {
            float angleA = (start + sweep * i / segments) * Mathf.Deg2Rad;
            float angleB = (start + sweep * (i + 1) / segments) * Mathf.Deg2Rad;
            Vector2 directionA = new Vector2(Mathf.Cos(angleA), Mathf.Sin(angleA));
            Vector2 directionB = new Vector2(Mathf.Cos(angleB), Mathf.Sin(angleB));
            int first = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, center + directionA * inner,
                Vector2.one * 0.5f + directionA * (inner / radius * 0.5f), tint);
            AddVertex(vertexHelper, center + directionA * outer,
                Vector2.one * 0.5f + directionA * (outer / radius * 0.5f), tint);
            AddVertex(vertexHelper, center + directionB * inner,
                Vector2.one * 0.5f + directionB * (inner / radius * 0.5f), tint);
            AddVertex(vertexHelper, center + directionB * outer,
                Vector2.one * 0.5f + directionB * (outer / radius * 0.5f), tint);
            vertexHelper.AddTriangle(first, first + 3, first + 1);
            vertexHelper.AddTriangle(first, first + 2, first + 3);
        }
    }

    private Vector2 GetDirection(int index, int segments)
    {
        float radians = (startAngle + angleDegrees * index / segments) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Vector2 uv, Color32 tint)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = new Vector3(position.x, position.y, 0f);
        vertex.color = tint;
        vertex.uv0 = new Vector4(uv.x, uv.y, 0f, 0f);
        vertexHelper.AddVert(vertex);
    }
}
