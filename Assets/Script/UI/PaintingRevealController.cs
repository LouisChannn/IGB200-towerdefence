using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public sealed class PaintingRevealController : MonoBehaviour
{
    [SerializeField] private Camera paintingCamera;
    [SerializeField] private SpriteRenderer[] sawArtwork;
    [SerializeField] private SpriteMask paintMask;
    [SerializeField] private GameObject outlineLayer;
    [SerializeField, Min(0f)] private float cameraMoveDuration = 1.25f;
    [SerializeField, Min(0f)] private float paintingHoldDuration = 2.5f;
    [SerializeField, Min(0.1f)] private float victoryFadeDuration = 1.8f;

    public bool IsRevealing { get; private set; }
    public Texture2D FinishedPainting { get; private set; }

    public void BeginReveal(LevelResultUI result)
    {
        if (IsRevealing || FinishedPainting != null)
            return;
        IsRevealing = true;
        StartCoroutine(Reveal(result));
    }

    private IEnumerator Reveal(LevelResultUI result)
    {
        Bounds bounds = PreparePainting(result);
        Vector3 startPosition = paintingCamera.transform.position;
        Vector3 endPosition = new Vector3(bounds.center.x, bounds.center.y, startPosition.z);
        float startSize = paintingCamera.orthographicSize;
        float endSize = Mathf.Max(bounds.extents.y, bounds.extents.x / paintingCamera.aspect) * 1.08f;
        for (float elapsed = 0f; elapsed < cameraMoveDuration; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(.001f, cameraMoveDuration));
            paintingCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            paintingCamera.orthographicSize = Mathf.Lerp(startSize, endSize, t);
            yield return null;
        }
        paintingCamera.transform.position = endPosition;
        paintingCamera.orthographicSize = endSize;

        // Render the final mask, opacity and deferred enemy removals before taking the painting.
        yield return new WaitForEndOfFrame();
        Texture2D capture = ScreenCapture.CaptureScreenshotAsTexture();
        FinishedPainting = capture;
        if (!GraphicsFormatUtility.IsSRGBFormat(capture.graphicsFormat))
        {
            // Screen pixels already contain display colours; tag them as sRGB for UI sampling.
            FinishedPainting = new Texture2D(capture.width, capture.height, capture.format, false, false);
            FinishedPainting.LoadRawTextureData(capture.GetRawTextureData<byte>());
            FinishedPainting.Apply(false, false);
            Destroy(capture);
        }
        FinishedPainting.name = "Your Finished Painting";
        FinishedPainting.filterMode = FilterMode.Bilinear;
        FinishedPainting.wrapMode = TextureWrapMode.Clamp;
        result.SetFinishedPainting(FinishedPainting);
        yield return new WaitForSecondsRealtime(paintingHoldDuration);

        result.PresentVictory();
        result.SetResultFade(0f, false);
        for (float elapsed = 0f; elapsed < victoryFadeDuration; elapsed += Time.unscaledDeltaTime)
        {
            result.SetResultFade(Mathf.SmoothStep(0f, 1f, elapsed / victoryFadeDuration), false);
            yield return null;
        }
        result.SetResultFade(1f, true);
        IsRevealing = false;
    }

    private Bounds PreparePainting(LevelResultUI result)
    {
        var visible = new HashSet<Renderer>();
        Bounds localMask = paintMask.sprite.bounds;
        Bounds bounds = new Bounds(paintMask.transform.TransformPoint(localMask.center),
            Vector3.Scale(localMask.size, paintMask.transform.lossyScale));
        foreach (SpriteRenderer saw in sawArtwork)
        {
            visible.Add(saw);

        }
        foreach (GameStateTransparency paint in FindObjectsByType<GameStateTransparency>())
        {
            SpriteRenderer renderer = paint.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;
            paint.enabled = false;
            Color colour = renderer.color;
            colour.a = 1f;
            renderer.color = colour;
            renderer.sortingLayerID = sawArtwork[0].sortingLayerID;
            renderer.sortingOrder = 11;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            visible.Add(renderer);
        }
        outlineLayer.SetActive(true);
        foreach (SpriteRenderer outline in outlineLayer.GetComponentsInChildren<SpriteRenderer>())
            visible.Add(outline);
        paintMask.gameObject.SetActive(true);
        paintMask.enabled = true;
        foreach (Renderer renderer in FindObjectsByType<Renderer>())
            renderer.enabled = visible.Contains(renderer) || renderer.gameObject == paintMask.gameObject;

        Canvas resultCanvas = result.GetComponent<Canvas>();
        foreach (Canvas canvas in FindObjectsByType<Canvas>())
        {
            if (canvas == resultCanvas) continue;
            canvas.enabled = false;
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = false;
        }
        paintingCamera.clearFlags = CameraClearFlags.SolidColor;
        paintingCamera.backgroundColor = Color.white;
        return bounds;
    }

    private void OnDestroy()
    {
        if (FinishedPainting != null)
            Destroy(FinishedPainting);
    }
}
