#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public static class LevelResultPreview
{
    [MenuItem("Tools/Codex/Preview Loss", false, 2150)]
    public static void Loss() { Prepare(); LevelManager.main.TakePlayerDamage(LevelManager.main.playerHP); }
    [MenuItem("Tools/Codex/Preview Victory", false, 2151)]
    public static void Victory() { Prepare(); LevelManager.main.FinishLevel(); }
    private static void Prepare()
    {
        if (!Application.isPlaying || LevelManager.main == null || LevelManager.main.IsGameOver)
            throw new System.InvalidOperationException("Preview requires a fresh Play mode run.");
        var tutorial = Object.FindAnyObjectByType<TutorialUI>();
        for (int i = 0; tutorial != null && tutorial.gameObject.activeSelf && i < 20; i++) tutorial.NextPage();
    }
}
#endif