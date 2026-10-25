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

    [Header("Victory Artwork")]
    [SerializeField] private GameObject victoryBackdrop;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private PaintingRevealController paintingReveal;
    [SerializeField] private RawImage paintingImage;
    [SerializeField] private CanvasGroup resultGroup;

    private Color normalTitleColour;
    private Color normalMessageColour;
    private Vector2 normalTitlePosition;
    private Vector2 normalMessagePosition;
    private Vector2 normalReplayPosition;
    private Vector2 normalMenuPosition;

    private void Awake()
    {
        normalTitleColour = title.color;
        normalMessageColour = message.color;
        normalTitlePosition = title.rectTransform.anchoredPosition;
        normalMessagePosition = message.rectTransform.anchoredPosition;
        normalReplayPosition = ((RectTransform)replayButton.transform).anchoredPosition;
        if (mainMenuButton != null)
            normalMenuPosition = ((RectTransform)mainMenuButton.transform).anchoredPosition;
        if (victoryBackdrop != null)
            victoryBackdrop.SetActive(false);
        if (paintingImage != null)
            paintingImage.transform.parent.gameObject.SetActive(false);
        panel.SetActive(false);
    }

    public void Show(bool victory)
    {
        if (pauseMenu != null)
            pauseMenu.CloseForLevelEnd();
        if (victory && paintingReveal != null)
        {
            paintingReveal.BeginReveal(this);
            return;
        }
        ShowResult(victory);
    }

    public void SetFinishedPainting(Texture texture)
    {
        if (paintingImage == null) return;
        paintingImage.texture = texture;
        var fitter = paintingImage.GetComponent<AspectRatioFitter>();
        if (fitter != null && texture != null)
            fitter.aspectRatio = (float)texture.width / texture.height;
    }

    public void PresentVictory()
    {
        ShowResult(true);
    }

    public void SetResultFade(float alpha, bool interactive)
    {
        if (resultGroup == null) return;
        resultGroup.alpha = alpha;
        resultGroup.interactable = interactive;
        resultGroup.blocksRaycasts = true;
        if (interactive && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(replayButton.gameObject);
    }

    private void ShowResult(bool victory)
    {
        title.text = victory ? "Victory!" : "You Lost";
        message.text = victory
            ? "Canvas fully painted on Wave " + spawner.CurrentWave + "!"
            : "You survived until Wave " + spawner.CurrentWave + "!";
        replayLabel.text = victory ? "Play Again" : "Retry";
        ApplyVictoryArtwork(victory);
        panel.SetActive(true);
        SetResultFade(1f, true);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(replayButton.gameObject);
        }
    }

    private void ApplyVictoryArtwork(bool victory)
    {
        if (victoryBackdrop == null) return;
        victoryBackdrop.SetActive(victory);
        if (paintingImage != null)
            paintingImage.transform.parent.gameObject.SetActive(victory && paintingImage.texture != null);
        title.color = victory ? new Color(1f, .89f, .55f) : normalTitleColour;
        message.color = victory ? new Color(1f, .98f, .9f) : normalMessageColour;
        title.rectTransform.anchoredPosition = victory ? new Vector2(0f, 210f) : normalTitlePosition;
        message.rectTransform.anchoredPosition = victory ? new Vector2(0f, 155f) : normalMessagePosition;
        ((RectTransform)replayButton.transform).anchoredPosition = victory ? new Vector2(-150f, -205f) : normalReplayPosition;
        if (mainMenuButton != null)
            ((RectTransform)mainMenuButton.transform).anchoredPosition = victory ? new Vector2(150f, -205f) : normalMenuPosition;
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
