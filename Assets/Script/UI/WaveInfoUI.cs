using System;
using System.Text;
using TMPro;
using UnityEngine;

public sealed class WaveInfoUI : MonoBehaviour
{
    [Serializable]
    public struct EnemyLabel
    {
        public GameObject prefab;
        public string label;
    }

    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private EnemyLabel[] enemyLabels;

    private int previewWave = -1;
    private string composition = "";
    private readonly StringBuilder builder = new StringBuilder(128);

    public void Configure(EnemySpawner source, TMP_Text title, TMP_Text preview, EnemyLabel[] labels)
    {
        spawner = source;
        waveText = title;
        previewText = preview;
        enemyLabels = labels;
        previewWave = -1;
    }

    private void LateUpdate()
    {
        if (spawner == null || waveText == null || previewText == null)
            return;

        string title = "WAVE " + spawner.CurrentWave;
        if (spawner.IsWaitingForWave)
            title += " IN " + Mathf.CeilToInt(spawner.SecondsUntilWave) + "s";
        else if (spawner.LevelStarted)
            title += "  |  " + spawner.EnemiesRemaining + " LEFT";
        SetText(waveText, title);

        if (previewWave != spawner.UpcomingWaveNumber)
        {
            composition = BuildComposition();
            previewWave = spawner.UpcomingWaveNumber;
        }
        bool incoming = !spawner.LevelStarted || spawner.IsWaitingForWave;
        SetText(previewText, (incoming ? "Incoming: " : "Next: ") + composition);
    }

    private string BuildComposition()
    {
        builder.Clear();
        var preview = spawner.UpcomingWavePreview;
        if (preview == null || preview.Count == 0)
            return "None";

        // Keep familiar types in a stable order, independent of their randomized spawn order.
        if (enemyLabels != null)
        {
            foreach (EnemyLabel entry in enemyLabels)
            {
                for (int i = 0; i < preview.Count; i++)
                    if (preview[i].Prefab == entry.prefab)
                        Append(preview[i].Count, entry.label);
            }
        }
        for (int i = 0; i < preview.Count; i++)
        {
            bool known = false;
            if (enemyLabels != null)
                foreach (EnemyLabel entry in enemyLabels)
                    if (entry.prefab == preview[i].Prefab) { known = true; break; }
            if (!known)
                Append(preview[i].Count, preview[i].Prefab != null ? preview[i].Prefab.name : "Enemy");
        }
        return builder.ToString();
    }

    private void Append(int count, string label)
    {
        if (builder.Length > 0) builder.Append(", ");
        builder.Append(count).Append(' ').Append(label);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text.text != value) text.text = value;
    }
}
