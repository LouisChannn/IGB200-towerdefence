using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LevelResultUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text replayLabel;
    [SerializeField] private Button replayButton;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private EnemySpawner spawner;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Show(bool victory)
    {
        if (pauseMenu != null)
            pauseMenu.CloseForLevelEnd();


        title.text = victory ? "Victory!" : "You Lost";

        message.text = victory
            ? "Canvas fully painted on Wave " + spawner.CurrentWave + "!"
            : "You survived until Wave " + spawner.CurrentWave + "!";

        replayLabel.text = victory ? "Play Again" : "Retry";
        panel.SetActive(true);


        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(replayButton.gameObject);
        }
    }

    public void ReplayLevel()
    {
        GameSettings.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        GameSettings.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}
