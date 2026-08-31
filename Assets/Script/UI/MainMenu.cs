using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuButtons;
    public GameObject settingsPanel;

    public Toggle fullscreenToggle;

    private void Start()
    {
        // Make the toggle match the current fullscreen state
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    // PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene("Hue Harmony （actual map)");
    }

    // OPEN SETTINGS
    public void OpenSettings()
    {
        mainMenuButtons.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // CLOSE SETTINGS
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    // FULLSCREEN
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // QUIT
    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Game Quit");
    }
}