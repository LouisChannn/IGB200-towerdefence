using UnityEngine;
using UnityEngine.UI;

public class MaskToggle : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] private GameObject maskLayer;

    [Header("Plots")]
    [SerializeField] private GameObject[] plots;

    [Header("Reveal Button")]
    [SerializeField] private Button revealButton;

    [Header("Colour Burst")]
    [SerializeField, Min(1)] private int burstDamage = 50;
    [SerializeField] private GameObject[] paintSplatterPrefabs;
    private bool bursting;

    private void Start()
    {
        // Colour Burst starts unavailable until the meter fills.
        if (revealButton != null)
        {
            revealButton.interactable = false;
        }
    }

    private void Update()
    {
        if (LevelManager.main == null)
            return;

        // Keep the ability unavailable while gameplay is paused.
        if (revealButton != null)
        {
            revealButton.interactable =
                !bursting && Time.timeScale > 0f && LevelManager.main.IsColourMeterFull();
        }
    }

    public void ToggleMask()
    {
        if (LevelManager.main == null)
        {
            Debug.LogWarning("LevelManager does not exist!");
            return;
        }

        if (bursting || Time.timeScale <= 0f || !LevelManager.main.TryConsumeFullColourMeter())
            return;

        bursting = true;
        if (revealButton != null) revealButton.interactable = false;
        try
        {
            foreach (Health enemy in FindObjectsByType<Health>())
            {
                if (enemy.isActiveAndEnabled)
                    // Burst kills keep fuel and paint rewards, but cannot recharge themselves.
                    enemy.TakeDamage(burstDamage, paintSplatterPrefabs, false);
            }
        }
        finally
        {
            bursting = false;
        }
    }
}
