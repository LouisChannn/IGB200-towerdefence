using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsPanel;

    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    private bool isPaused = false;
    private bool fallbackSettingsOpen = false;

    private void Start()
    {
        // Make sure game starts normally
        Time.timeScale = 1f;

        SetActiveIfAssigned(pauseMenu, false);
        SetActiveIfAssigned(settingsPanel, false);

        RefreshSettingsControls();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings are currently open,
            // Esc returns to the pause menu
            if (IsSettingsOpen())
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        SetActiveIfAssigned(pauseMenu, true);
        SetActiveIfAssigned(settingsPanel, false);
        fallbackSettingsOpen = false;

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        GameSettings.Save();
        SetActiveIfAssigned(pauseMenu, false);
        SetActiveIfAssigned(settingsPanel, false);
        fallbackSettingsOpen = false;

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        RefreshSettingsControls();
        SetActiveIfAssigned(pauseMenu, false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            fallbackSettingsOpen = true;
        }
    }

    public void CloseSettings()
    {
        GameSettings.Save();
        SetActiveIfAssigned(settingsPanel, false);
        SetActiveIfAssigned(pauseMenu, true);
        fallbackSettingsOpen = false;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        GameSettings.SetFullscreen(isFullscreen);
        RefreshSettingsControls();
    }

    public void SetVolume(float volume)
    {
        GameSettings.SetVolume(volume);
        RefreshSettingsControls();
    }

    private void RefreshSettingsControls()
    {
        GameSettings.Initialize();
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;
            volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        }
    }

    public void GoToMainMenu()
    {
        GameSettings.Save();
        Time.timeScale = 1f;

        SceneManager.LoadScene("Main Menu");
    }

    private bool IsSettingsOpen()
    {
        if (settingsPanel != null)
        {
            return settingsPanel.activeSelf;
        }

        return fallbackSettingsOpen;
    }

    private void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private bool UsesFallbackPanels()
    {
        return pauseMenu == null || settingsPanel == null;
    }

    private void OnGUI()
    {
        // Fallback keeps play mode usable if the scene has not been wired in the Inspector yet.
        if (!isPaused || !UsesFallbackPanels())
        {
            return;
        }

        Matrix4x4 originalMatrix = GUI.matrix;
        float uiScale = Mathf.Clamp(Mathf.Min(Screen.width / 800f, Screen.height / 600f), 0.5f, 2f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        float screenWidth = Screen.width / uiScale;
        float screenHeight = Screen.height / uiScale;
        float width = Mathf.Min(280f, screenWidth - 24f);
        float height = IsSettingsOpen() ? 200f : 180f;
        Rect area = new Rect(
            (screenWidth - width) * 0.5f,
            (screenHeight - height) * 0.5f,
            width,
            height
        );

        Color originalColor = GUI.color;
        GUI.color = new Color(0.14f, 0.14f, 0.16f, 1f);
        GUI.DrawTexture(area, Texture2D.whiteTexture);
        GUI.color = originalColor;
        GUILayout.BeginArea(area, GUI.skin.window);

        if (IsSettingsOpen())
        {
            DrawFallbackSettings();
        }
        else
        {
            DrawFallbackPauseMenu();
        }

        GUILayout.EndArea();
        GUI.matrix = originalMatrix;
    }

    private void DrawFallbackPauseMenu()
    {
        GUILayout.Label("Paused");

        if (GUILayout.Button("Resume"))
        {
            ResumeGame();
        }

        if (GUILayout.Button("Settings"))
        {
            OpenSettings();
        }

        if (GUILayout.Button("Main Menu"))
        {
            GoToMainMenu();
        }
    }

    private void DrawFallbackSettings()
    {
        GUILayout.Label("Settings");

        bool fullscreen = GUILayout.Toggle(GameSettings.Fullscreen, "Fullscreen");

        if (fullscreen != GameSettings.Fullscreen)
        {
            SetFullscreen(fullscreen);
        }

        GUILayout.Space(8f);
        GUILayout.Label("Volume " + Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%");
        float volume = GUILayout.HorizontalSlider(GameSettings.MasterVolume, 0f, 1f);
        if (!Mathf.Approximately(volume, GameSettings.MasterVolume))
            SetVolume(volume);

        if (GUILayout.Button("Back"))
        {
            CloseSettings();
        }
    }
}
