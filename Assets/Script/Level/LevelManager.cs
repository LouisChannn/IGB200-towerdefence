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

    [Header("Colour Fuel")]
    [SerializeField] private int startingColourFuel = 100;

    public int colourFuel { get; private set; }

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
        if (damage <= 0)
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
        Debug.Log("PLAYER DEFEATED!");

        // Stop the game
        Time.timeScale = 0f;

        // You can add a Game Over UI here later
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
        if (amount <= colourFuel)
        {
            colourFuel -= amount;
            return true;
        }

        Debug.Log("Not enough Colour Fuel!");
        return false;
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

    // =========================
    // LEVEL COMPLETION
    // =========================

    public void FinishLevel()
    {
        Debug.Log("LEVEL COMPLETE! PLAYER WINS!");

        // Stop the entire game
        Time.timeScale = 0f;
    }
}
