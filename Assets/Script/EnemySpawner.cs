using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
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
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;


    private void Awake()
    {
       onEnemyDestroyed.AddListener(EnemyDestroyed);
        
    }
private void Start()
    {
        // Start the first wave when the game starts
        StartWave();
    }

    private void Update()
    {
        if (!isSpawning) return; // If not currently spawning, exit the update loop

        // If there are no enemies left to spawn and no enemies alive, start a new wave
        timeSinceLastspawn += Time.deltaTime;
        // Check if it's time to spawn a new enemy and if there are enemies left to spawn
        if (timeSinceLastspawn > (1f / enemiesPerSecond)&& enemiesLeftToSpawn > 0)
        {
            // Spawn a new enemy and update the counters
            SpawnEnemy();
            enemiesLeftToSpawn--; // Decrement the number of enemies left to spawn
            enemiesAlive++; // Increment the number of enemies alive
            timeSinceLastspawn = 0f; // Reset the timer for the next spawn
        }
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--; // Decrement the number of enemies alive
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void StartWave()
    {
        // Increment the wave counter and reset the enemy counters
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave(); // Calculate the number of enemies to spawn for the current wave
        
    }

    public void SpawnEnemy()
    {
        // Instantiate the enemy prefab at the spawner's position and rotation
        GameObject prefabToSpawn = enemyPrefab; 
        // Use the start point's position and a default rotation for the spawned enemy:
        Instantiate(prefabToSpawn, LevelManager.main.startPoint.position,Quaternion.identity); 
        
    }

    private int EnemiesPerWave()
    {
        // Calculate the number of enemies based on the current wave and difficulty scaling factor
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficualtyScalitingFactor));
    }

    // Update is called once per frame
    
}
