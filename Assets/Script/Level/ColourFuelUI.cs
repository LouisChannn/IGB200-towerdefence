using UnityEngine;
using TMPro;

public class ColourFuelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text colourFuelText;

    private void Start()
    {
        if (colourFuelText == null) return;
        colourFuelText.fontSizeMax = colourFuelText.fontSize;
        colourFuelText.fontSizeMin = Mathf.Min(colourFuelText.fontSizeMin, colourFuelText.fontSizeMax);
        colourFuelText.enableAutoSizing = true;
    }

    private void Update()
    {
        if (LevelManager.main != null && colourFuelText != null)
        {
            colourFuelText.text =
                LevelManager.main.UnlimitedColourFuel
                    ? "Unlimited"
                    : LevelManager.main.colourFuel.ToString();
        }
    }
}
