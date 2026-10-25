using UnityEngine;
using UnityEngine.EventSystems;
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
    private float resumeTimeScale = 1f;

    private void Start()
    {
        // Tutorial and level state own the initial game clock.
        SetActiveIfAssigned(pauseMenu, false);
        SetActiveIfAssigned(settingsPanel, false);

        RefreshSettingsControls();
    }

    private void Update()
    {
        if (LevelManager.main != null && LevelManager.main.IsGameOver)
            return;

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
        if (LevelManager.main != null && LevelManager.main.IsGameOver)
            return;

        if (isPaused)
            return;

        // Closing a slowed turret menu restores its normal running speed first.
        if (BuildManager.main != null)
            BuildManager.main.CloseBuildMenu();
        resumeTimeScale = Time.timeScale;

        SetActiveIfAssigned(pauseMenu, true);
        SetActiveIfAssigned(settingsPanel, false);
        fallbackSettingsOpen = false;

        Time.timeScale = 0f;
        isPaused = true;
        FocusFirstButton(pauseMenu);
    }

    public void ResumeGame()
    {
        if (LevelManager.main != null && LevelManager.main.IsGameOver)
            return;

        if (!isPaused)
            return;

        GameSettings.Save();
        SetActiveIfAssigned(pauseMenu, false);
        SetActiveIfAssigned(settingsPanel, false);
        fallbackSettingsOpen = false;

        Time.timeScale = resumeTimeScale;
        isPaused = false;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        if (LevelManager.main != null && LevelManager.main.IsGameOver)
            return;

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
        FocusFirstButton(settingsPanel);
    }

    public void CloseSettings()
    {
        if (LevelManager.main != null && LevelManager.main.IsGameOver)
            return;

        GameSettings.Save();
        SetActiveIfAssigned(settingsPanel, false);
        SetActiveIfAssigned(pauseMenu, true);
        fallbackSettingsOpen = false;
        FocusFirstButton(pauseMenu);
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

    public void CloseForLevelEnd()
    {
        SetActiveIfAssigned(pauseMenu, false);
        SetActiveIfAssigned(settingsPanel, false);
        fallbackSettingsOpen = false;
        isPaused = false;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void GoToMainMenu()
    {
        GameSettings.Save();
        isPaused = false;
        fallbackSettingsOpen = false;
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

    private void FocusFirstButton(GameObject panel)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        eventSystem.SetSelectedGameObject(null);
        if (panel == null)
            return;

        foreach (Button button in panel.GetComponentsInChildren<Button>())
        {
            if (button.IsActive() && button.IsInteractable())
            {
                eventSystem.SetSelectedGameObject(button.gameObject);
                return;
            }
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
