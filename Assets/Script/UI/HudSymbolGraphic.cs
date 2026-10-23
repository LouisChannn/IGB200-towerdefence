using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public sealed class HudSymbolGraphic : MaskableGraphic
{
    public enum Symbol { Heart, PaintDrop }

    [SerializeField] private Symbol symbol;
    private const int SegmentsPerCurve = 8;

    private static readonly Vector2[] HeartOutline =
    {
        new Vector2(0f, .2f),
        new Vector2(.18f, .62f), new Vector2(.66f, .4f), new Vector2(.45f, .04f),
        new Vector2(.3f, -.15f), new Vector2(.12f, -.38f), new Vector2(0f, -.5f),
        new Vector2(-.12f, -.38f), new Vector2(-.3f, -.15f), new Vector2(-.45f, .04f),
        new Vector2(-.66f, .4f), new Vector2(-.18f, .62f), new Vector2(0f, .2f)
    };

    private static readonly Vector2[] DropOutline =
    {
        new Vector2(0f, .5f),
        new Vector2(.15f, .25f), new Vector2(.48f, -.08f), new Vector2(.4f, -.27f),
        new Vector2(.32f, -.55f), new Vector2(-.32f, -.55f), new Vector2(-.4f, -.27f),
        new Vector2(-.48f, -.08f), new Vector2(-.15f, .25f), new Vector2(0f, .5f)
    };

    public void Configure(Symbol shape)
    {
        symbol = shape;
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnEnable()
    {
        raycastTarget = false;
        base.OnEnable();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        float size = Mathf.Min(rect.width, rect.height);
        if (size <= 0f)
            return;

        Vector2[] outline = symbol == Symbol.Heart ? HeartOutline : DropOutline;
        int segments = (outline.Length - 1) / 3 * SegmentsPerCurve;
        Color32 tint = color;
        mesh.AddVert(rect.center + new Vector2(0f, -.1f * size), tint, Vector2.zero);
        for (int i = 0; i <= segments; i++)
        {
            Vector2 point = Evaluate(outline, (float)i / segments);
            mesh.AddVert(rect.center + point * size, tint, Vector2.zero);
            if (i > 0)
                mesh.AddTriangle(0, i, i + 1);
        }
    }

    private static Vector2 Evaluate(Vector2[] points, float progress)
    {
        int curveCount = (points.Length - 1) / 3;
        float scaled = progress * curveCount;
        int curve = Mathf.Min(Mathf.FloorToInt(scaled), curveCount - 1);
        float t = scaled - curve;
        float s = 1f - t;
        int start = curve * 3;
        return s * s * s * points[start] + 3f * s * s * t * points[start + 1] +
            3f * s * t * t * points[start + 2] + t * t * t * points[start + 3];
    }
}
