using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyType[] enemyTypes;
    [SerializeField] private GameObject wave1Enemy;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 8;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficualtyScalitingFactor = 0.75f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroyed = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastspawn;
    private int enemiesAlive;
    private bool isSpawning = false;
    private bool levelStarted = false;

    private List<GameObject> waveQueue = new List<GameObject>();
    private int spawnIndex;

    private void Awake()
    {
        onEnemyDestroyed.AddListener(EnemyDestroyed);
    }

    private void Start()
    {
        // DO NOT start the level here.
        // The TutorialUI will call StartLevel()
        // when the tutorial is finished.
    }

    private void Update()
    {
        // Don't do anything until the tutorial is finished
        if (!levelStarted)
            return;

        if (!isSpawning)
            return;

        timeSinceLastspawn += Time.deltaTime;

        if (timeSinceLastspawn > (1f / enemiesPerSecond) &&
            spawnIndex < waveQueue.Count)
        {
            SpawnEnemy(waveQueue[spawnIndex]);

            spawnIndex++;
            enemiesAlive++;
            timeSinceLastspawn = 0f;
        }

        if (enemiesAlive == 0 &&
            spawnIndex >= waveQueue.Count)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    // Called by TutorialUI when the player finishes the tutorial
    public void StartLevel()
    {
        if (levelStarted)
            return;

        levelStarted = true;

        Debug.Log("LEVEL STARTED!");

        StartCoroutine(StartWave());
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        BuildWaveQueue();

        spawnIndex = 0;
        isSpawning = true;
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastspawn = 0f;
        currentWave++;

        StartCoroutine(StartWave());
    }

    private void BuildWaveQueue()
    {
        waveQueue.Clear();

        if (currentWave == 1)
        {
            GameObject waveOneEnemy =
                wave1Enemy != null
                    ? wave1Enemy
                    : enemyTypes[0].prefab;

            for (int i = 0; i < baseEnemies; i++)
            {
                waveQueue.Add(waveOneEnemy);
            }

            return;
        }

        int weightBudget = WaveWeightBudget();

        List<EnemyType> affordable =
            new List<EnemyType>();

        while (weightBudget > 0)
        {
            affordable.Clear();

            foreach (EnemyType type in enemyTypes)
            {
                if (type.weight <= weightBudget)
                {
                    affordable.Add(type);
                }
            }

            if (affordable.Count == 0)
                break;

            EnemyType pick =
                affordable[Random.Range(0, affordable.Count)];

            waveQueue.Add(pick.prefab);

            weightBudget -= pick.weight;
        }
    }

    private int WaveWeightBudget()
    {
        return Mathf.RoundToInt(
            baseEnemies *
            Mathf.Pow(
                currentWave,
                difficualtyScalitingFactor
            )
        );
    }

    private void SpawnEnemy(GameObject prefabToSpawn)
    {
        Instantiate(
            prefabToSpawn,
            LevelManager.main.startPoint.position,
            Quaternion.identity
        );
    }
}
