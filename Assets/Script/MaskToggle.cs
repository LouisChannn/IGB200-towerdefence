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

    private bool levelFinished = false;

    private void Start()
    {
        // Reveal button starts disabled
        if (revealButton != null)
        {
            revealButton.interactable = false;
        }
    }

    private void Update()
    {
        if (LevelManager.main == null || levelFinished)
            return;

        // Enable Reveal button when Colour Meter is full
        if (revealButton != null)
        {
            revealButton.interactable =
                LevelManager.main.IsColourMeterFull();
        }
    }

    public void ToggleMask()
    {
        if (LevelManager.main == null)
        {
            Debug.LogWarning("LevelManager does not exist!");
            return;
        }

        // Don't allow reveal until meter is full
        if (!LevelManager.main.IsColourMeterFull())
        {
            Debug.Log("Colour Meter is not full!");
            return;
        }

        // Prevent the level from finishing twice
        if (levelFinished)
            return;

        levelFinished = true;

        // Turn mask ON
        if (maskLayer != null)
        {
            maskLayer.SetActive(true);
        }

        // Turn all plots OFF
        foreach (GameObject plot in plots)
        {
            if (plot != null)
            {
                plot.SetActive(false);
            }
        }

        // Disable reveal button
        if (revealButton != null)
        {
            revealButton.interactable = false;
        }

        // Finish the level
        LevelManager.main.FinishLevel();

        Debug.Log("PLAYER WINS!");
    }
}