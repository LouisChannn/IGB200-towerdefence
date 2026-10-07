using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSliderReadout : MonoBehaviour
{
    public TMP_Text valueLabel;
    private Slider slider;

    private void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(Refresh);
        Refresh(slider.value);
    }

    private void OnDisable()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(Refresh);
    }

    private void LateUpdate()
    {
        // Settings refreshes use SetValueWithoutNotify when reopening the panel.
        Refresh(slider.value);
    }

    private void Refresh(float value)
    {
        if (valueLabel != null) valueLabel.SetText("{0}%", Mathf.RoundToInt(value * 100f));
    }
}
