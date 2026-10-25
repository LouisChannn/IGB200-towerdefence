using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public sealed class VictoryFireworksGraphic : MaskableGraphic
{
    private const float GridSize = 128f;
    private const float FramesPerSecond = 8f;
    private const float LoopSeconds = 2f;
    private const float RocketSeconds = .25f;
    private const float BurstSeconds = .875f;
    private const int Spokes = 12;

    // Coordinates share the supplied artwork's square canvas. Keep the crystals clear.
    private static readonly Vector2[] Centres =
    {
        new Vector2(28f, 99f), new Vector2(102f, 107f), new Vector2(104f, 85f)
    };
    private static readonly float[] Offsets = { 0f, .625f, 1.25f };
    private static readonly Color[] Colours =
    {
        new Color(1f, .83f, .28f), new Color(1f, .48f, .65f), new Color(.40f, .87f, 1f)
    };

    private float elapsed;
    private int displayedFrame;

    protected override void OnEnable()
    {
        raycastTarget = false;
        elapsed = 0f;
        displayedFrame = 0;
        base.OnEnable();
        SetVerticesDirty();
    }

    private void Update()
    {
        // Result screens stop game time; only the decorative fireworks keep moving.
        AdvanceAnimation(Time.unscaledDeltaTime);
    }

    private void AdvanceAnimation(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        elapsed = Mathf.Repeat(elapsed + deltaTime, LoopSeconds);
        int frame = Mathf.FloorToInt(elapsed * FramesPerSecond);
        if (frame == displayedFrame)
            return;

        displayedFrame = frame;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect bounds = GetPixelAdjustedRect();
        float size = Mathf.Min(bounds.width, bounds.height);
        if (size <= 0f)
            return;

        float unit = size / GridSize;
        Vector2 origin = bounds.center - Vector2.one * size * .5f;
        float time = displayedFrame / FramesPerSecond;
        for (int i = 0; i < Centres.Length; i++)
        {
            float age = Mathf.Repeat(time - Offsets[i] + LoopSeconds, LoopSeconds);
            if (age < RocketSeconds)
                DrawRocket(mesh, origin, unit, Centres[i], age / RocketSeconds, Colours[i]);
            else if (age < RocketSeconds + BurstSeconds)
                DrawBurst(mesh, origin, unit, Centres[i], (age - RocketSeconds) / BurstSeconds, Colours[i], i);
        }
    }

    private void DrawRocket(VertexHelper mesh, Vector2 origin, float unit, Vector2 centre, float progress, Color tint)
    {
        Vector2 tip = centre + Vector2.down * Mathf.Lerp(18f, 0f, progress);
        Square(mesh, origin, unit, tip, 1f, Color.Lerp(tint, Color.white, .6f));
        Square(mesh, origin, unit, tip + Vector2.down * 2f, 1f, Fade(tint, .65f));
        Square(mesh, origin, unit, tip + Vector2.down * 4f, 1f, Fade(tint, .3f));
    }

    private void DrawBurst(VertexHelper mesh, Vector2 origin, float unit, Vector2 centre, float progress, Color tint, int burst)
    {
        float radius = Mathf.Lerp(1.5f, 13f, 1f - (1f - progress) * (1f - progress));
        float opacity = progress < .55f ? 1f : Mathf.Clamp01((1f - progress) / .45f);
        float fall = progress * progress * 3f;
        for (int spoke = 0; spoke < Spokes; spoke++)
        {
            float angle = (spoke + burst * .35f) * Mathf.PI * 2f / Spokes;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float length = radius * (spoke % 2 == 0 ? 1f : .82f);
            Vector2 point = centre + direction * length + Vector2.down * fall;
            Color spark = spoke % 3 == 0 ? Color.Lerp(tint, Color.white, .55f) : tint;
            Square(mesh, origin, unit, point, 1f, Fade(spark, opacity));
            if (progress > .15f)
                Square(mesh, origin, unit, point - direction * 2f, 1f, Fade(tint, opacity * .65f));
            if (progress > .35f && (spoke + displayedFrame) % 2 == 0)
                Square(mesh, origin, unit, point - direction * 4f, 1f, Fade(tint, opacity * .3f));
        }

        if (progress < .25f)
            Square(mesh, origin, unit, centre, 2f, Fade(Color.white, opacity));
    }

    private void Square(VertexHelper mesh, Vector2 origin, float unit, Vector2 position, float width, Color tint)
    {
        // Draw flat quads on the artwork's pixel grid, with no enlarged JPEG frames.
        Vector2 min = origin + new Vector2(Mathf.Round(position.x), Mathf.Round(position.y)) * unit;
        Vector2 max = min + Vector2.one * width * unit;
        Color32 vertexColour = tint * color;
        int first = mesh.currentVertCount;
        mesh.AddVert(new Vector3(min.x, min.y, 0f), vertexColour, Vector2.zero);
        mesh.AddVert(new Vector3(min.x, max.y, 0f), vertexColour, Vector2.zero);
        mesh.AddVert(new Vector3(max.x, max.y, 0f), vertexColour, Vector2.zero);
        mesh.AddVert(new Vector3(max.x, min.y, 0f), vertexColour, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first + 2, first + 3, first);
    }

    private static Color Fade(Color tint, float opacity)
    {
        tint.a *= opacity;
        return tint;
    }
}
