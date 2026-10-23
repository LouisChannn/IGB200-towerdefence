using UnityEngine;
using TMPro;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private void Update()
    {
        if (LevelManager.main == null || hpText == null)
            return;

        hpText.text = LevelManager.main.GetPlayerHP().ToString();
    }
}

