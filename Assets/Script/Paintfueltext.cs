using UnityEngine;
using TMPro;

public class ColourFuelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text colourFuelText;

    private void Update()
    {
        if (LevelManager.main != null)
        {
            colourFuelText.text =
                "Colour Fuel: " + LevelManager.main.colourFuel;
        }
    }
}