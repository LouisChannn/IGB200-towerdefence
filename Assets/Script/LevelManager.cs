using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    public Transform startPoint;
    public Transform [] path;

    public int paintfuel; //Game Currency
    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        paintfuel = 100; //Starting game currency
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
            //buy item
        }
        else
        {
            Debug.Log("not enough currency");
            return false;
        }
    }

}
