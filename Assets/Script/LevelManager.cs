using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    [Header("Path")]
    public Transform startPoint;
    public Transform[] path;

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
}