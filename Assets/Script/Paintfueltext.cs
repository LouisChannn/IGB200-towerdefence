using UnityEngine;
using TMPro;

public class PaintFuelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text paintFuelText;

    private void Update()
    {
        if (LevelManager.main != null)
        {
            paintFuelText.text = "Paint Fuel: " + LevelManager.main.paintfuel;
        }
    }
}