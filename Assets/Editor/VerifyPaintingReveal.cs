#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class VerifyPaintingReveal
{
    private const string Folder = @"C:\Users\omnom\AppData\Local\Temp\codex-painting-reveal-20261025";
    [MenuItem("Tools/Codex/Test Burst Painting Reveal", false, 2120)]
    public static void Test()
    {
        Require(Application.isPlaying && LevelManager.main != null && !LevelManager.main.IsGameOver, "Start a fresh gameplay run.");
        var tutorial = UnityEngine.Object.FindAnyObjectByType<TutorialUI>();
        for (int i = 0; tutorial != null && tutorial.gameObject.activeSelf && i < 20; i++) tutorial.NextPage();
        var reveal = UnityEngine.Object.FindAnyObjectByType<PaintingRevealController>();
        var result = UnityEngine.Object.FindAnyObjectByType<LevelResultUI>();
        var burst = UnityEngine.Object.FindAnyObjectByType<MaskToggle>();
        Require(reveal != null && result != null && burst != null, "Reveal references exist.");
        var saws = (SpriteRenderer[])Get(reveal, "sawArtwork");
        // Sample real game splatters, so capture tests contain paint as well as saw artwork.
        string[] splatters = { "Assets/Art/Prefab/Splatters and sprays/Splatter effect paint brush 1.prefab", "Assets/Art/Prefab/Splatters and sprays/Splatter effect paint brush 2.prefab" };
        for (int i = 0; i < saws.Length; i++)
            UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(splatters[i % 2]), saws[i].bounds.center, Quaternion.identity);
        // Real enemies must leave their burst-created paint in the final capture too.
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefab/Enemies/Enemy.prefab");
        Health[] targets = saws.Select(saw => UnityEngine.Object.Instantiate(enemyPrefab, saw.bounds.center, Quaternion.identity).GetComponent<Health>()).ToArray();
        int paintBeforeBurst = UnityEngine.Object.FindObjectsByType<GameStateTransparency>().Length;
        LevelManager.main.RechargeColourBurst();
        burst.ToggleMask();
        Require(targets.All(target => target.CurrentHitPoints == 0) && UnityEngine.Object.FindObjectsByType<GameStateTransparency>().Length >= paintBeforeBurst + targets.Length, "Real burst kills leave paint before the reveal begins.");
        Require(LevelManager.main.IsGameOver && LevelManager.main.PlayerWon && Time.timeScale == 0f && reveal.IsRevealing, "Burst starts the locked victory reveal.");
        var panel = (GameObject)Get(result, "panel");
        var group = (CanvasGroup)Get(result, "resultGroup");
        var image = (RawImage)Get(result, "paintingImage");
        var mask = (SpriteMask)Get(reveal, "paintMask");
        Require(mask.enabled && mask.gameObject.activeInHierarchy && mask.sprite.name == "sawmask1_0", "Correct mask activated.");
        Require(!panel.activeSelf, "No victory UI during painting reveal.");
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>())
            Require(canvas == result.GetComponent<Canvas>() || !canvas.enabled, "Gameplay UI hidden.");
        foreach (GameStateTransparency paint in UnityEngine.Object.FindObjectsByType<GameStateTransparency>())
        {
            var renderer = paint.GetComponent<SpriteRenderer>();
            Require(!paint.enabled && renderer.color.a == 1f && renderer.maskInteraction == SpriteMaskInteraction.VisibleInsideMask, "Paint stays opaque and clipped inside saws.");
        }
        File.WriteAllText(Path.Combine(Folder, "verification.txt"), "PASS initial: real burst, locked outcome, sawmask1, opaque clipped paint, HUD hidden.\n");
        double started = EditorApplication.timeSinceStartup;
        bool savedPainting = false, sawFade = false;
        EditorApplication.CallbackFunction check = null;
        check = () =>
        {
            try
            {
                if (!Application.isPlaying) { EditorApplication.update -= check; return; }
                double elapsed = EditorApplication.timeSinceStartup - started;
                Require(elapsed < 20, "Reveal finishes within its configured timing.");
                if (!savedPainting && reveal.FinishedPainting != null)
                {
                    Require(!panel.activeSelf, "Painting captured before result UI.");
                    Require(UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsSRGBFormat(reveal.FinishedPainting.graphicsFormat), "Painting keeps display colours when sampled by UI.");
                    File.WriteAllBytes(Path.Combine(Folder, "finished-painting.png"), reveal.FinishedPainting.EncodeToPNG());
                    File.AppendAllText(Path.Combine(Folder, "verification.txt"), "PASS capture: clean painting texture " + reveal.FinishedPainting.width + "x" + reveal.FinishedPainting.height + ".\n");
                    savedPainting = true;
                }
                if (panel.activeSelf && group.alpha > 0f && group.alpha < .99f)
                {
                    Require(!group.interactable, "Buttons wait for fade completion.");
                    sawFade = true;
                }
                if (!reveal.IsRevealing)
                {
                    Require(savedPainting && sawFade && panel.activeSelf && group.alpha == 1f && group.interactable, "Hold and slow fade finish with active controls.");
                    Require(image.texture == reveal.FinishedPainting && image.transform.parent.gameObject.activeSelf, "Finished painting displayed beside animation.");
                    var fireworks = UnityEngine.Object.FindAnyObjectByType<VictoryFireworksGraphic>();
                    Require(fireworks != null && !fireworks.raycastTarget, "Crisp fireworks visible without blocking buttons.");
                    var art = fireworks.transform.parent.GetComponent<RawImage>();
                    Require(art.texture.width == 1280 && art.uvRect == new Rect(0, 0, 1, 1), "Full-resolution original artwork used.");
                    Require(!art.GetComponent<UIAnimatedSpriteStrip>().enabled, "Low-resolution strip is disabled.");
                    File.AppendAllText(Path.Combine(Folder, "verification.txt"), "PASS final: painting hold, smooth fade, interactive result, actual painting shown, 1280px art plus crisp unscaled fireworks.\n");
                    Debug.Log("PAINTING REVEAL VERIFICATION PASS");
                    EditorApplication.update -= check;
                }
            }
            catch (Exception error)
            {
                EditorApplication.update -= check;
                File.AppendAllText(Path.Combine(Folder, "verification.txt"), "FAIL " + error + "\n");
                Debug.LogException(error);
            }
        };
        EditorApplication.update += check;
    }
    private static object Get(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Require(bool value, string text) { if (!value) throw new Exception(text); }
}
#endif