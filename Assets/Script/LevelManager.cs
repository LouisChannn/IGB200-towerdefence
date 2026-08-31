using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    public Transform startPoint;
    public Transform[] path;

    [Header("Paint Fuel")]
    public int paintfuel = 100;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        paintfuel = 100;
    }

    public void IncreasePaintFuel(int amount)
    {
        paintfuel += amount;
    }

    public bool SpendPaintFuel(int amount)
    {
        if (amount <= paintfuel)
        {
            paintfuel -= amount;
            return true;
        }

        Debug.Log("Not enough Paint Fuel!");
        return false;
    }
}