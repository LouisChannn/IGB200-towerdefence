using UnityEngine;

public class HealthBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private string spriteNames = "paint can";
    //public int health;
    private Sprite[] sprites;
    public SpriteRenderer HealthCan;
    void Start()
    {
        HealthCan = gameObject.GetComponent<SpriteRenderer>();
        sprites = Resources.LoadAll<Sprite>(spriteNames);

    }
    // Update is called once per frame
    void Update()
    {
        if (LevelManager.main.GetPlayerHP() < 10)
        {
            if (LevelManager.main.GetPlayerHP() == 8)
            {
                HealthCan.sprite = sprites[3];
            }
        }
    }
}