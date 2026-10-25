#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ConfigurePaintingReveal
{
    private const string Gameplay = "Assets/Scenes/Hue Harmony （actual map).unity";
    private const string MaskPath = "Assets/Art/Prefab/sawmask1.png";
    private const string ArtworkPath = "Assets/Art/Sprite/UI/VictoryArtwork.jpg";
    private const string MaterialPath = "Assets/Resources/PaintingOutline.mat";

    [MenuItem("Tools/Codex/Configure Painting Reveal", false, 2113)]
    public static void Apply()
    {
        Require(!Application.isPlaying && !SceneManager.GetActiveScene().isDirty, "Use Edit mode with the current scene saved.");
        Scene scene = SceneManager.GetActiveScene().path == Gameplay ? SceneManager.GetActiveScene() : EditorSceneManager.OpenScene(Gameplay);
        Require(!Application.isPlaying && scene.path == Gameplay && !scene.isDirty, "Use the saved gameplay scene in Edit mode.");
        GameObject[] roots = scene.GetRootGameObjects();
        LevelResultUI result = roots.SelectMany(root => root.GetComponentsInChildren<LevelResultUI>(true)).Single();
        var resultData = new SerializedObject(result);
        Require(new[] { "panel", "victoryBackdrop", "paintingReveal", "paintingImage", "resultGroup" }.All(name => resultData.FindProperty(name) != null), "The updated result fields exist.");
        GameObject panel = resultData.FindProperty("panel").objectReferenceValue as GameObject;
        GameObject backdrop = resultData.FindProperty("victoryBackdrop").objectReferenceValue as GameObject;
        Camera camera = roots.SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Single(item => item.CompareTag("MainCamera"));
        GameObject sawRoot = roots.Single(root => root.name == "SawPath");
        SpriteRenderer[] artwork = sawRoot.GetComponentsInChildren<SpriteRenderer>(true).Where(item => item.sprite != null).ToArray();
        Require(panel != null && backdrop != null && camera != null && artwork.Length == 5, "The result panel, victory backdrop, camera and five saw sprites exist.");
        RawImage hero = backdrop.GetComponentsInChildren<RawImage>(true).Single(item => item.name == "Victory Fireworks Animation");
        Require(hero.GetComponent<AspectRatioFitter>() != null && hero.GetComponent<UIAnimatedSpriteStrip>() != null, "The existing artwork animation and fitter are present.");
        Require(!roots.Any(root => root.name == "Finished Painting Mask" || root.name == "Finished Painting Outlines") && panel.transform.Find("Finished Painting Frame") == null, "Painting reveal has not already been configured.");
        Shader shader = Shader.Find("HueHarmony/PaintingOutline");
        Require(shader != null, "The painting-outline shader is imported.");
        bool panelActive = panel.activeSelf, backdropActive = backdrop.activeSelf;
        ImportTexture(ArtworkPath, false); ImportTexture(MaskPath, true);
        Texture2D victoryTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtworkPath);
        Sprite maskSprite = AssetDatabase.LoadAllAssetsAtPath(MaskPath).OfType<Sprite>().Single(item => item.name == "sawmask1_0");
        Require(victoryTexture != null && maskSprite != null, "The supplied victory artwork and saw-mask sprite are imported.");
        Material outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (outlineMaterial == null)
        {
            outlineMaterial = new Material(shader) { name = "PaintingOutline" };
            AssetDatabase.CreateAsset(outlineMaterial, MaterialPath);
        }
        Require(outlineMaterial.shader == shader, "The outline material uses the painting-outline shader.");
        Bounds bounds = TightWorldBounds(artwork);
        Require(bounds.size.x > 0f && bounds.size.y > 0f && maskSprite.bounds.size.x > 0f && maskSprite.bounds.size.y > 0f, "The artwork and mask have nonempty bounds.");
        var maskRoot = new GameObject("Finished Painting Mask");
        Undo.RegisterCreatedObjectUndo(maskRoot, "Configure Painting Reveal");
        maskRoot.SetActive(false);
        SpriteMask mask = maskRoot.AddComponent<SpriteMask>(); mask.sprite = maskSprite;
        Vector3 scale = new Vector3(bounds.size.x / maskSprite.bounds.size.x, bounds.size.y / maskSprite.bounds.size.y, 1f);
        maskRoot.transform.localScale = scale;
        maskRoot.transform.position = bounds.center - Vector3.Scale(maskSprite.bounds.center, scale);
        mask.isCustomRangeActive = true;
        mask.frontSortingLayerID = mask.backSortingLayerID = 0;
        mask.frontSortingOrder = 20; mask.backSortingOrder = 6;
        MaskToggle burst = roots.SelectMany(root => root.GetComponentsInChildren<MaskToggle>(true)).Single();
        var burstData = new SerializedObject(burst);
        Set(burstData, "maskLayer", maskRoot); burstData.ApplyModifiedPropertiesWithoutUndo();
        var outlines = new GameObject("Finished Painting Outlines");
        Undo.RegisterCreatedObjectUndo(outlines, "Configure Painting Reveal");
        outlines.SetActive(false);
        foreach (SpriteRenderer source in artwork)
        {
            var child = new GameObject(source.name + " Outline", typeof(SpriteRenderer));
            child.layer = source.gameObject.layer; child.transform.SetParent(outlines.transform, false);
            child.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            child.transform.localScale = source.transform.lossyScale;
            SpriteRenderer copy = child.GetComponent<SpriteRenderer>(); EditorUtility.CopySerialized(source, copy);
            copy.sharedMaterial = outlineMaterial; copy.sortingLayerID = source.sortingLayerID; copy.sortingOrder = 21;
            copy.maskInteraction = SpriteMaskInteraction.None;
        }
        PaintingRevealController controller = result.GetComponent<PaintingRevealController>();
        if (controller == null) controller = Undo.AddComponent<PaintingRevealController>(result.gameObject);
        var revealData = new SerializedObject(controller);
        Set(revealData, "paintingCamera", camera); Set(revealData, "paintMask", mask); Set(revealData, "outlineLayer", outlines);
        SerializedProperty saws = revealData.FindProperty("sawArtwork"); Require(saws != null && saws.isArray, "The reveal controller exposes sawArtwork.");
        saws.arraySize = artwork.Length;
        for (int i = 0; i < artwork.Length; i++) saws.GetArrayElementAtIndex(i).objectReferenceValue = artwork[i];
        revealData.ApplyModifiedPropertiesWithoutUndo();
        RectTransform frame = RectChild(panel.transform, "Finished Painting Frame", new Vector2(.04f, .24f), new Vector2(.55f, .69f));
        Image white = frame.gameObject.AddComponent<Image>(); white.color = Color.white; white.raycastTarget = false;
        RectTransform imageRect = RectChild(frame, "Finished Painting", Vector2.zero, Vector2.one);
        RawImage painting = imageRect.gameObject.AddComponent<RawImage>(); painting.color = Color.white; painting.raycastTarget = false;
        AspectRatioFitter paintingFit = imageRect.gameObject.AddComponent<AspectRatioFitter>();
        paintingFit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; paintingFit.aspectRatio = 16f / 9f;
        frame.gameObject.SetActive(false);
        RectTransform heroRegion = RectChild(backdrop.transform, "Victory Artwork Region", new Vector2(.60f, .22f), new Vector2(.96f, .70f));
        hero.transform.SetParent(heroRegion, false); Stretch((RectTransform)hero.transform, Vector2.zero, Vector2.one);
        hero.GetComponent<UIAnimatedSpriteStrip>().enabled = false;
        hero.texture = victoryTexture; hero.uvRect = new Rect(0f, 0f, 1f, 1f); hero.raycastTarget = false;
        AspectRatioFitter heroFit = hero.GetComponent<AspectRatioFitter>(); heroFit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; heroFit.aspectRatio = 1f;
        RectTransform fireworks = RectChild(hero.transform, "Victory Fireworks", Vector2.zero, Vector2.one);
        fireworks.gameObject.AddComponent<VictoryFireworksGraphic>().raycastTarget = false;
        CanvasGroup group = panel.GetComponent<CanvasGroup>(); if (group == null) group = Undo.AddComponent<CanvasGroup>(panel);
        group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true;
        Set(resultData, "paintingReveal", controller); Set(resultData, "paintingImage", painting); Set(resultData, "resultGroup", group);
        resultData.ApplyModifiedPropertiesWithoutUndo();
        Require(panel.activeSelf == panelActive && backdrop.activeSelf == backdropActive, "Original result-panel visibility is preserved.");
        Require(mask != null && outlines != null && painting != null && group != null && controller != null, "All painting-reveal references are assigned.");
        EditorUtility.SetDirty(outlineMaterial); AssetDatabase.SaveAssetIfDirty(outlineMaterial);
        EditorSceneManager.MarkSceneDirty(scene); Require(EditorSceneManager.SaveScene(scene), "The gameplay scene was saved.");
        Debug.Log("[Painting Reveal] Saved the fitted saw mask, outline-only copies, finished-painting frame and victory artwork/fireworks. Loss artwork and existing buttons remain configured.");
    }

    private static Bounds TightWorldBounds(SpriteRenderer[] sources)
    {
        Bounds bounds = default; bool first = true;
        foreach (SpriteRenderer source in sources)
            foreach (Vector2 vertex in source.sprite.vertices)
            {
                Vector3 local = new Vector3(vertex.x * (source.flipX ? -1f : 1f), vertex.y * (source.flipY ? -1f : 1f), 0f);
                Vector3 point = source.transform.TransformPoint(local);
                if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
            }
        return bounds;
    }

    private static void ImportTexture(string path, bool preserveSprites)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        string[] before = preserveSprites ? SpriteIdentity(path) : Array.Empty<string>();
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Require(importer != null, "Texture importer exists: " + path);
        importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
        foreach (string target in new[] { "DefaultTexturePlatform", "Standalone" })
        {
            TextureImporterPlatformSettings platform = target == "DefaultTexturePlatform" ? importer.GetDefaultPlatformTextureSettings() : importer.GetPlatformTextureSettings(target);
            platform.maxTextureSize = 2048; platform.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(platform);
        }
        importer.SaveAndReimport();
        Require(!preserveSprites || before.SequenceEqual(SpriteIdentity(path)), "Mask sprite IDs and rectangles are unchanged.");
    }
    private static string[] SpriteIdentity(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
        .Select(sprite => GlobalObjectId.GetGlobalObjectIdSlow(sprite) + "|" + sprite.name + "|" + sprite.rect).OrderBy(value => value).ToArray();
    private static RectTransform RectChild(Transform parent, string name, Vector2 min, Vector2 max)
    {
        Require(parent.Find(name) == null, "No existing child is overwritten: " + name);
        var child = new GameObject(name, typeof(RectTransform)); child.layer = parent.gameObject.layer;
        Undo.RegisterCreatedObjectUndo(child, "Configure Painting Reveal"); child.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)child.transform; Stretch(rect, min, max); return rect;
    }
    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(.5f, .5f);
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity; rect.anchoredPosition3D = Vector3.zero; rect.sizeDelta = Vector2.zero;
    }
    private static void Set(SerializedObject data, string name, UnityEngine.Object value)
    {
        SerializedProperty field = data.FindProperty(name); Require(field != null, "Serialized field exists: " + name); field.objectReferenceValue = value;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif