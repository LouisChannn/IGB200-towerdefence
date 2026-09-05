
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColourMeterUI : MonoBehaviour
{
    [Header("Colour Meter")]
    [SerializeField] private Slider colourMeterSlider;

    [Header("Optional Text")]
    [SerializeField] private TMP_Text meterText;

    [Header("Particle Effect")]
    [SerializeField] private ParticleSystem meterParticles;

    [Header("Particle Position")]
    [Tooltip("Extra Y movement in Unity units.")]
    [SerializeField] private float particleYOffset = 0f;

    [Tooltip("Multiplier for how far the particle travels vertically.")]
    [SerializeField] private float particleYMultiplier = 1f;

    private int previousMeterValue;
    private int previousPercentage = -1;

    private void Start()
    {
        if (LevelManager.main == null)
        {
            Debug.LogError("LevelManager not found!");
            return;
        }

        if (colourMeterSlider == null)
        {
            Debug.LogError("Colour Meter Slider is not assigned!");
            return;
        }

        int current = LevelManager.main.GetColourMeter();
        int max = LevelManager.main.GetMaxColourMeter();

        // Set up slider
        colourMeterSlider.minValue = 0;
        colourMeterSlider.maxValue = max;
        colourMeterSlider.value = current;

        previousMeterValue = current;

        if (meterParticles != null)
        {
            meterParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        // Set initial particle position
        UpdateParticlePosition();

        previousPercentage = Mathf.FloorToInt(
            colourMeterSlider.normalizedValue * 100f
        );
    }

    private void Update()
    {
        if (LevelManager.main == null)
            return;

        int current = LevelManager.main.GetColourMeter();
        int max = LevelManager.main.GetMaxColourMeter();

        if (max <= 0)
            return;

        // Update slider
        colourMeterSlider.value = current;

        // Update text
        if (meterText != null)
        {
            meterText.text = current + " / " + max;
        }

        // Calculate exact percentage
        float percentage = colourMeterSlider.normalizedValue;

        int currentPercentage = Mathf.FloorToInt(
            percentage * 100f
        );

        // Update particle position every 1%
        if (currentPercentage != previousPercentage)
        {
            UpdateParticlePosition();

            previousPercentage = currentPercentage;

            Debug.Log(
                "Particle position updated at " +
                currentPercentage +
                "%"
            );
        }

        // Play particles when meter increases
        if (current > previousMeterValue)
        {
            PlayMeterParticles();
        }

        previousMeterValue = current;
    }

    private void UpdateParticlePosition()
    {
        if (colourMeterSlider == null || meterParticles == null)
            return;

        RectTransform fillRect = colourMeterSlider.fillRect;

        if (fillRect == null)
        {
            Debug.LogWarning("Slider Fill Rect is not assigned!");
            return;
        }

        // Get meter percentage from 0 to 1
        float percentage = colourMeterSlider.normalizedValue;

        // Get the actual rectangle of the fill area
        Rect rect = fillRect.rect;

        // Keep particle centred horizontally
        float x = rect.center.x;

        // Calculate vertical position based on the fill percentage
        float y = Mathf.Lerp(
            rect.yMin,
            rect.yMax,
            percentage
        );

        // Apply Y multiplier
        float centreY = rect.yMin;

        y = centreY +
            ((y - centreY) * particleYMultiplier);

        // Apply additional offset
        y += particleYOffset;

        // Convert UI local position to world position
        Vector3 uiWorldPosition = fillRect.TransformPoint(
            new Vector3(x, y, 0f)
        );

        // Convert UI world position to screen position
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                uiWorldPosition
            );

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found!");
            return;
        }

        // Keep the particle at its current Z distance
        float distanceFromCamera =
            Mathf.Abs(
                meterParticles.transform.position.z -
                cam.transform.position.z
            );

        // Convert screen position back into world position
        Vector3 worldPosition =
            cam.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    distanceFromCamera
                )
            );

        // Keep particle Z unchanged
        worldPosition.z =
            meterParticles.transform.position.z;

        // Move particle
        meterParticles.transform.position =
            worldPosition;
    }

    private void PlayMeterParticles()
    {
        if (meterParticles == null)
            return;

        meterParticles.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        meterParticles.Play();
    }
}
