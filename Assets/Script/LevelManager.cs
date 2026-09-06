using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    [Header("events")]
    public static UnityEvent onEnemyPassed = new UnityEvent();

    [Header("Path")]
    public Transform startPoint;
    public Transform[] path;

    [Header("Colour Fuel")]
    [SerializeField] private int startingColourFuel = 100;

    public int colourFuel { get; private set; }

    [Header("Colour Meter")]
    [SerializeField] private int maxColourMeter = 100;

    public int colourMeter { get; private set; }

    //change the name or smth? idk i couldn't think of anything better to call it
    [Header("difficulty")]
    //anyhow, a large value seems good if we want to change damage values later.
    //allows greater range of damage and stuff and ig that just feels better than all enemie having the same set damage
    [SerializeField] public int totalHealth=1000;

    private void Awake()
    {
        main = this;
        onEnemyPassed.AddListener(Health);
    }

    private void Start()
    {
        colourFuel = startingColourFuel;
        colourMeter = 0;
    }

    // =========================
    // COLOUR FUEL (turret currency)
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
    // COLOUR METER (ability to win the level)
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
    /*=============
     * health (or lack thereof TwT)
     *=============
     */
    public void Health()
    {
        totalHealth--;
        if (totalHealth <= 0)
        {
            Debug.Log("you are dead, not big suprise");
            //rework this gameover bit. atm just pauses everything, but probably should add a proper gameover screen :3
            Time.timeScale = 0.0f;
        }
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
    public void FinishLevel()
    {
        Debug.Log("LEVEL COMPLETE! PLAYER WINS!");

        // Stop the entire game
        Time.timeScale = 0f;
    }
}