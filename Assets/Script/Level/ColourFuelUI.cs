using UnityEngine;
using TMPro;

public class ColourFuelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text colourFuelText;

    private void Start()
    {
        if (colourFuelText == null) return;
        colourFuelText.fontSizeMax = colourFuelText.fontSize;
        colourFuelText.fontSizeMin = 18;
        colourFuelText.enableAutoSizing = true;
    }

    private void Update()
    {
        if (LevelManager.main != null && colourFuelText != null)
        {
            colourFuelText.text =
                LevelManager.main.UnlimitedColourFuel
                    ? "Fuel: Unlimited"
                    : "Colour Fuel: " + LevelManager.main.colourFuel;
        }
    }
}
