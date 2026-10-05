using System.Globalization;
using TMPro;
using UnityEngine;

public sealed class FloatingDamageNumber : MonoBehaviour
{
    private const float Lifetime = 0.85f;
    private const float RiseDistance = 0.65f;
    private const float FadeStart = 0.35f;

    private TextMeshPro label;
    private Material textMaterial;
    private Camera viewCamera;
    private Vector3 startPosition;
    private float elapsed;

    public static void Show(int damage, Transform enemy)
    {
        if (damage <= 0 || enemy == null)
            return;

        SpriteRenderer enemySprite = enemy.GetComponentInChildren<SpriteRenderer>();
        Vector3 position = enemy.position + Vector3.up * 0.5f;
        if (enemySprite != null)
        {
            Bounds bounds = enemySprite.bounds;
            position = new Vector3(bounds.center.x, bounds.max.y + 0.3f, enemy.position.z);
        }

        // A separate object lets the killing hit finish animating after its enemy is removed.
        GameObject popupObject = new GameObject("Damage Number", typeof(TextMeshPro));
        popupObject.layer = enemy.gameObject.layer;
        popupObject.transform.position = position;
        FloatingDamageNumber popup = popupObject.AddComponent<FloatingDamageNumber>();
        popup.Initialize(damage, enemySprite);
    }

    private void Initialize(int damage, SpriteRenderer enemySprite)
    {
        startPosition = transform.position;
        viewCamera = Camera.main;
        if (viewCamera != null)
            transform.rotation = viewCamera.transform.rotation;

        label = GetComponent<TextMeshPro>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = damage.ToString(CultureInfo.InvariantCulture);
        label.fontSize = 5.5f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.rectTransform.sizeDelta = new Vector2(2f, 1f);
        label.color = new Color(1f, 0.95f, 0.55f, 1f);
        textMaterial = new Material(label.fontSharedMaterial);
        textMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(35, 31, 40, 255));
        textMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
        textMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        label.fontSharedMaterial = textMaterial;
        label.raycastTarget = false;

        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        if (enemySprite != null)
            meshRenderer.sortingLayerID = enemySprite.sortingLayerID;
        meshRenderer.sortingOrder = Mathf.Max(101, enemySprite != null ? enemySprite.sortingOrder + 1 : 101);

        label.ForceMeshUpdate();
    }

    private void LateUpdate()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / Lifetime);
        transform.position = startPosition + Vector3.up * (RiseDistance * progress);
        transform.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, Mathf.Clamp01(progress * 5f));
        if (viewCamera != null)
            transform.rotation = viewCamera.transform.rotation;

        label.alpha = 1f - Mathf.InverseLerp(FadeStart, 1f, progress);
        if (elapsed >= Lifetime)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (textMaterial != null)
            Destroy(textMaterial);
    }
}
