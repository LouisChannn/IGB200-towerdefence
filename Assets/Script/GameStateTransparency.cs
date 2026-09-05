using UnityEngine;

public class GameStateTransparency : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool levelFinished = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Drop alpha to 105 when game starts
        SetSpriteAlpha(105f / 255f);
    }

    void Update()
    {
        // Continuously check if LevelManager exists and if the level has finished
        if (!levelFinished && LevelManager.main != null)
        {
            if (LevelManager.main.IsColourMeterFull())
            {
                // Bring alpha back to 255 when the game ends
                SetSpriteAlpha(1.0f);
                levelFinished = true; // Prevent running this multiple times
            }
        }
    }

    private void SetSpriteAlpha(float alphaValue)
    {
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = alphaValue;
            spriteRenderer.color = currentColor;
        }
    }
}
