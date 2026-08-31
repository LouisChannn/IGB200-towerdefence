using UnityEngine;

public class MaskToggle : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] private GameObject maskLayer;

    [Header("Plots")]
    [SerializeField] private GameObject[] plots;

    public void ToggleMask()
    {
        if (maskLayer == null)
        {
            Debug.LogWarning("Mask Layer has not been assigned!");
            return;
        }

        // Turn mask ON
        maskLayer.SetActive(true);

        // Turn ALL plots OFF
        foreach (GameObject plot in plots)
        {
            if (plot != null)
            {
                plot.SetActive(false);
            }
        }
    }
}