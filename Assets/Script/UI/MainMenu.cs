using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuButtons;
    public GameObject settingsPanel;
    public AudioSource menuMusic;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    private void Start()
    {
        RefreshSettingsControls();
    }

    // PLAY
    public void PlayGame()
    {
        GameSettings.Save();
        SceneManager.LoadScene("Hue Harmony （actual map)");
    }

    // OPEN SETTINGS
    public void OpenSettings()
    {
        RefreshSettingsControls();
        mainMenuButtons.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // CLOSE SETTINGS
    public void CloseSettings()
    {
        GameSettings.Save();
        settingsPanel.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    // FULLSCREEN
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

    // QUIT
    public void QuitGame()
    {
        GameSettings.Save();
        Application.Quit();

        Debug.Log("Game Quit");
    }
}
