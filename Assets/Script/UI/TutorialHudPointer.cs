using UnityEngine;

[DisallowMultipleComponent]
public sealed class TutorialHudPointer : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private RectTransform callout;
    [SerializeField] private Vector2 calloutOffset = new Vector2(0f, -58f);

    public void Configure(RectTransform badge)
    {
        target = badge;
    }

    public void Configure(RectTransform badge, RectTransform calloutBox, Vector2 offset)
    {
        target = badge;
        callout = calloutBox;
        calloutOffset = offset;
    }

    private void LateUpdate()
    {
        if (!isActiveAndEnabled || target == null)
            return;

        if (callout != null)
        {
            Vector3 desiredTop = target.TransformPoint(new Vector3(
                target.rect.center.x + calloutOffset.x,
                target.rect.yMin + calloutOffset.y, 0f));
            Vector3 currentTop = callout.TransformPoint(new Vector3(
                callout.rect.center.x, callout.rect.yMax, 0f));
            callout.position += desiredTop - currentTop;
        }

        // The arrow remains a child of its page, so place it after moving the box.
        transform.position = target.TransformPoint(
            new Vector3(target.rect.center.x, target.rect.yMin - 28f, 0f));
    }
}
