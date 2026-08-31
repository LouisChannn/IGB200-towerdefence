using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColourMeterUI : MonoBehaviour
{
    [Header("Colour Meter Slider")]
    [SerializeField] private Slider colourMeterSlider;

    [Header("Optional Text")]
    [SerializeField] private TMP_Text meterText;

    private void Start()
    {
        if (LevelManager.main == null)
            return;

        // Set slider range
        colourMeterSlider.minValue = 0;
        colourMeterSlider.maxValue =
            LevelManager.main.GetMaxColourMeter();

        // Start at current meter value
        colourMeterSlider.value =
            LevelManager.main.GetColourMeter();
    }

    private void Update()
    {
        if (LevelManager.main == null)
            return;

        // Get current Colour Meter
        int current =
            LevelManager.main.GetColourMeter();

        // Update slider
        colourMeterSlider.value = current;

        // Optional text
        if (meterText != null)
        {
            int max =
                LevelManager.main.GetMaxColourMeter();

            meterText.text = current + " / " + max;
        }
    }
}