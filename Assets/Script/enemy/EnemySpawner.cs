using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public readonly struct WaveEnemyCount
{
    public GameObject Prefab { get; }
    public int Count { get; }

    public WaveEnemyCount(GameObject prefab, int count)
    {
        Prefab = prefab;
        Count = count;
    }
}

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
    private List<GameObject> preparedWaveQueue = new List<GameObject>();
    private readonly List<WaveEnemyCount> upcomingPreview = new List<WaveEnemyCount>();
    private IReadOnlyList<WaveEnemyCount> readOnlyUpcomingPreview;
    private System.Random waveRandom;
    private int preparedWaveNumber;
    private bool isWaitingForWave;
    private float secondsUntilWave;
    private int spawnIndex;
    private bool campaignComplete;

    public int CurrentWave => currentWave;
    public bool LevelStarted => levelStarted;
    public bool IsWaitingForWave => levelStarted && isWaitingForWave;
    public float SecondsUntilWave => IsWaitingForWave ? Mathf.Max(0f, secondsUntilWave) : 0f;
    public int CurrentWaveTotal { get; private set; }
    public int UpcomingWaveNumber => preparedWaveNumber;
    public IReadOnlyList<WaveEnemyCount> UpcomingWavePreview => readOnlyUpcomingPreview;
    public int EnemiesRemaining => enemiesAlive +
        (!levelStarted || isWaitingForWave ? preparedWaveQueue.Count : Mathf.Max(0, waveQueue.Count - spawnIndex));

    private void Awake()
    {
        onEnemyDestroyed.AddListener(EnemyDestroyed);
        // One seed per level keeps queue planning independent of combat's shared random stream.
        waveRandom = new System.Random(Random.Range(0, int.MaxValue));
        readOnlyUpcomingPreview = upcomingPreview.AsReadOnly();
        PrepareUpcomingWave(currentWave);
        CurrentWaveTotal = preparedWaveQueue.Count;
    }

    private void OnDestroy()
    {
        onEnemyDestroyed.RemoveListener(EnemyDestroyed);
    }

    private void Update()
    {
        AdvanceWave(Time.deltaTime);
    }

    private void AdvanceWave(float deltaTime)
    {
        // Don't do anything until the tutorial is finished
        if (!levelStarted || campaignComplete || deltaTime <= 0f || (LevelManager.main != null && LevelManager.main.IsGameOver))
            return;

        if (isWaitingForWave)
        {
            secondsUntilWave = Mathf.Max(0f, secondsUntilWave - deltaTime);
            if (secondsUntilWave <= 0f)
                StartPreparedWave();
            return;
        }

        if (!isSpawning)
            return;

        timeSinceLastspawn += deltaTime;

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
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    // Called by TutorialUI when the player finishes the tutorial
    public void StartLevel()
    {
        if (levelStarted || campaignComplete || (LevelManager.main != null && LevelManager.main.IsGameOver))
            return;

        levelStarted = true;

        Debug.Log("LEVEL STARTED!");

        BeginWaveCountdown();
    }

    private void BeginWaveCountdown()
    {
        if (preparedWaveNumber != currentWave)
            PrepareUpcomingWave(currentWave);

        isSpawning = false;
        isWaitingForWave = true;
        secondsUntilWave = Mathf.Max(0f, timeBetweenWaves);
        CurrentWaveTotal = preparedWaveQueue.Count;
    }

    private void StartPreparedWave()
    {
        // Promote the exact queue that was previewed; reuse the completed queue as preparation storage.
        List<GameObject> completedQueue = waveQueue;
        waveQueue = preparedWaveQueue;
        preparedWaveQueue = completedQueue;
        spawnIndex = 0;
        timeSinceLastspawn = 0f;
        isWaitingForWave = false;
        secondsUntilWave = 0f;
        isSpawning = true;
        CurrentWaveTotal = waveQueue.Count;
        PrepareUpcomingWave(currentWave + 1);
    }


    private void EndWave()
    {
        if (campaignComplete)
            return;

        isSpawning = false;
        timeSinceLastspawn = 0f;

        // Advance to the next wave indefinitely
        currentWave++;

        BeginWaveCountdown();
    }


    private void PrepareUpcomingWave(int waveNumber)
    {
        BuildWaveQueue(waveNumber, preparedWaveQueue);
        preparedWaveNumber = waveNumber;
        foreach (GameObject prefab in preparedWaveQueue)
        {
            int index = -1;
            for (int i = 0; i < upcomingPreview.Count; i++)
            {
                if (upcomingPreview[i].Prefab == prefab)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                upcomingPreview.Add(new WaveEnemyCount(prefab, 1));
            else
                upcomingPreview[index] = new WaveEnemyCount(prefab, upcomingPreview[index].Count + 1);
        }
    }

    private void BuildWaveQueue(int waveNumber, List<GameObject> queue)
    {
        queue.Clear();

        if (waveNumber == 1)
        {
            GameObject waveOneEnemy =
                wave1Enemy != null
                    ? wave1Enemy
                    : enemyTypes != null && enemyTypes.Length > 0 ? enemyTypes[0].prefab : null;

            for (int i = 0; i < baseEnemies && waveOneEnemy != null; i++)
            {
                queue.Add(waveOneEnemy);
            }

            return;
        }

        int weightBudget = WaveWeightBudget(waveNumber);
        if (enemyTypes == null)
            return;

        List<EnemyType> affordable =
            new List<EnemyType>();

        while (weightBudget > 0)
        {
            affordable.Clear();

            foreach (EnemyType type in enemyTypes)
            {
                if (type != null && type.prefab != null && type.weight > 0 && type.weight <= weightBudget)
                {
                    affordable.Add(type);
                }
            }

            if (affordable.Count == 0)
                break;

            EnemyType pick =
                affordable[waveRandom.Next(affordable.Count)];

            queue.Add(pick.prefab);

            weightBudget -= pick.weight;
        }
    }

    private int WaveWeightBudget(int waveNumber)
    {
        return Mathf.RoundToInt(
            baseEnemies *
            Mathf.Pow(
                waveNumber,
                difficualtyScalitingFactor
            )
        );
    }

    private void SpawnEnemy(GameObject prefabToSpawn)
    {
        GameObject enemy = Instantiate(
            prefabToSpawn,
            LevelManager.main.startPoint.position,
            Quaternion.identity
        );

        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            float rewardMultiplier = 1f / Mathf.Pow(
                currentWave,
                difficualtyScalitingFactor
            );
            health.SetColourFuelRewardMultiplier(rewardMultiplier);

            int bonusHealth = Mathf.RoundToInt(
                Mathf.Pow(currentWave, difficualtyScalitingFactor)
            ) - 1;
            // Enemy health and turret damage use ten times the original units.
            health.AddBonusHealth(bonusHealth * 10);
        }
    }
}
