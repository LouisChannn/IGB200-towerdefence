using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    [Header("Path")]
    public Transform startPoint;
    public Transform[] path;

    [Header("Player HP")]
    [SerializeField] private int startingHP = 10;

    public int playerHP { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool PlayerWon { get; private set; }

    [Header("Level Result")]
    [SerializeField] private LevelResultUI resultUI;

    [Header("Colour Fuel")]
    [SerializeField] private int startingColourFuel = 100;

    public int colourFuel { get; private set; }
    public bool UnlimitedColourFuel { get; private set; }
    public float EnemySpeedMultiplier { get; private set; } = 1f;

    [Header("Colour Meter")]
    [SerializeField] private int maxColourMeter = 100;

    public int colourMeter { get; private set; }

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        playerHP = startingHP;
        colourFuel = startingColourFuel;
        colourMeter = 0;
    }

    // =========================
    // PLAYER HP
    // =========================

    public void TakePlayerDamage(int damage)
    {
        if (IsGameOver || damage <= 0)
            return;

        playerHP -= damage;

        // Prevent HP from going below 0
        playerHP = Mathf.Max(playerHP, 0);

        Debug.Log(
            "Player took " +
            damage +
            " damage! HP remaining: " +
            playerHP
        );

        if (playerHP <= 0)
        {
            PlayerDefeated();
        }
    }

    public int GetPlayerHP()
    {
        return playerHP;
    }

    public int GetMaxPlayerHP()
    {
        return startingHP;
    }

    private void PlayerDefeated()
    {
        EndLevel(false);
    }

    private void EndLevel(bool victory)
    {
        if (IsGameOver)
            return;

        // Resolve the outcome once before closing menus or stopping the clock.
        IsGameOver = true;
        PlayerWon = victory;
        Time.timeScale = 0f;
        if (BuildManager.main != null)
            BuildManager.main.CloseBuildMenu();
        if (resultUI != null)
            resultUI.Show(victory);

        Debug.Log(victory ? "LEVEL COMPLETE! PLAYER WINS!" : "PLAYER DEFEATED!");
    }

    // =========================
    // COLOUR FUEL
    // =========================

    public void IncreaseColourFuel(int amount)
    {
        colourFuel += amount;
    }

    public bool SpendColourFuel(int amount)
    {
        if (amount < 0)
            return false;
        if (UnlimitedColourFuel)
            return true;

        if (amount <= colourFuel)
        {
            colourFuel -= amount;
            return true;
        }

        Debug.Log("Not enough Colour Fuel!");
        return false;
    }

    public void SetUnlimitedColourFuel(bool enabled)
    {
        UnlimitedColourFuel = enabled;
    }

    public void SetSlowEnemies(bool enabled)
    {
        EnemySpeedMultiplier = enabled ? 0.25f : 1f;
    }

    public void RechargeColourBurst()
    {
        colourMeter = maxColourMeter;
    }

    // =========================
    // COLOUR METER
    // =========================

    public void IncreaseColourMeter(int amount)
    {
        colourMeter += amount;

        colourMeter = Mathf.Clamp(
            colourMeter,
            0,
            maxColourMeter
        );
    }

    public int GetColourMeter()
    {
        return colourMeter;
    }

    public int GetMaxColourMeter()
    {
        return maxColourMeter;
    }

    public bool IsColourMeterFull()
    {
        return colourMeter >= maxColourMeter;
    }

    public bool TryConsumeFullColourMeter()
    {
        if (maxColourMeter <= 0 || !IsColourMeterFull())
            return false;

        colourMeter = 0;
        return true;
    }

    // =========================
    // LEVEL COMPLETION
    // =========================

    public void FinishLevel()
    {
        EndLevel(playerHP > 0);
    }
}
