using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("Tutorial Pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Level")]
    [SerializeField] private EnemySpawner enemySpawner;

    private int currentPage = 0;

    private void Start()
    {
        // Stop the game while tutorial is open
        Time.timeScale = 0f;

        ShowPage();
    }

    public void NextPage()
    {
        currentPage++;

        if (currentPage >= pages.Length)
        {
            FinishTutorial();
            return;
        }

        ShowPage();
    }

    private void ShowPage()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        pages[currentPage].SetActive(true);
    }

    private void FinishTutorial()
    {
        // Hide tutorial
        gameObject.SetActive(false);

        // Allow the game to run
        Time.timeScale = 1f;

        // Start the enemy spawning
        if (enemySpawner != null)
        {
            enemySpawner.StartLevel();
        }

        Debug.Log("Tutorial finished! Level started!");
    }
}
