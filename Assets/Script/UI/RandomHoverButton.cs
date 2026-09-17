using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RandomHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonImage;
    public TMP_Text buttonText;
    public AudioSource audioSource;
    public AudioClip splatSound;

    private Color[] colours =
    {
        Color.black,
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        new Color(1f, 0.5f, 0f), // Orange
        new Color(0.5f, 0f, 1f)  // Purple
    };

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.PlayOneShot(splatSound);

        int buttonColourIndex = Random.Range(0, colours.Length);
        int textColourIndex = Random.Range(0, colours.Length);

        while (textColourIndex == buttonColourIndex)
        {
            textColourIndex = Random.Range(0, colours.Length);
        }

        buttonImage.color = colours[buttonColourIndex];
        buttonText.color = colours[textColourIndex];
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Return to original appearance
        buttonImage.color = Color.white;
        buttonText.color = Color.black;
    }
}