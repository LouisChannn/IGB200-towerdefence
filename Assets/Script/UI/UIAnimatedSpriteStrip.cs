using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public sealed class UIAnimatedSpriteStrip : MonoBehaviour
{
    [SerializeField] private RawImage targetImage;
    [SerializeField, Min(1)] private int frameCount = 16;
    [SerializeField, Min(0.1f)] private float framesPerSecond = 8f;
    private float elapsed;

    private void OnEnable()
    {
        if (targetImage == null)
            targetImage = GetComponent<RawImage>();
        elapsed = 0f;
        ApplyFrame(0);
    }

    private void Update()
    {
        // End screens stop gameplay, so their animation uses the real-time clock.
        AdvanceAnimation(Time.unscaledDeltaTime);
    }

    private void AdvanceAnimation(float deltaTime)
    {
        if (targetImage == null || targetImage.texture == null)
            return;

        int count = Mathf.Max(1, frameCount);
        float rate = Mathf.Max(0.1f, framesPerSecond);
        elapsed = Mathf.Repeat(elapsed + Mathf.Max(0f, deltaTime), count / rate);
        ApplyFrame(Mathf.Min(count - 1, Mathf.FloorToInt(elapsed * rate)));
    }

    private void ApplyFrame(int frame)
    {
        if (targetImage != null)
        {
            float width = 1f / Mathf.Max(1, frameCount);
            targetImage.uvRect = new Rect(frame * width, 0f, width, 1f);
        }
    }
}
