using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RandomHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    public Image buttonImage;
    public TMP_Text buttonText;
    public AudioSource audioSource;
    public AudioClip splatSound;

    public Image colourFill;
    public Color accentColour = new Color(0.2f, 0.7f, 0.55f);
    private bool hovered;
    private bool selected;
    private float fill;
    private Button button;
    private Vector3 restingScale;
    private bool pressed;
    private float submitUntil;

    private void Awake()
    {
        button = GetComponent<Button>();
        restingScale = transform.localScale;
    }

    private void Update()
    {
        fill = Mathf.MoveTowards(fill, hovered || selected ? 1f : 0f, Time.unscaledDeltaTime * 5f);
        if (colourFill != null)
            colourFill.rectTransform.anchorMax = new Vector2(fill, 1f);
        var target = restingScale * (pressed || Time.unscaledTime < submitUntil ? .96f : 1f);
        transform.localScale = Vector3.MoveTowards(transform.localScale, target, Time.unscaledDeltaTime * 2f);
    }

    private void OnDisable()
    {
        hovered = selected = false;
        fill = 0f;
        pressed = false;
        submitUntil = 0f;
        transform.localScale = restingScale;
        if (colourFill != null) colourFill.rectTransform.anchorMax = new Vector2(0f, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnSelect(BaseEventData eventData) { selected = true; }
    public void OnDeselect(BaseEventData eventData) { selected = false; }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !CanPress()) return;
        pressed = true;
        PlayPressSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) pressed = false;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!CanPress()) return;
        submitUntil = Time.unscaledTime + .1f;
        PlayPressSound();
    }

    private bool CanPress() { return button != null && button.IsActive() && button.IsInteractable(); }

    private void PlayPressSound()
    {
        if (splatSound == null) return;
        // A separate voice lets the click finish when Play changes scenes.
        var voice = new GameObject("Menu Button Press", typeof(AudioSource));
        DontDestroyOnLoad(voice);
        var source = voice.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = splatSound;
        source.volume = audioSource != null ? audioSource.volume : .6f;
        source.outputAudioMixerGroup = audioSource != null ? audioSource.outputAudioMixerGroup : null;
        source.Play();
        voice.AddComponent<MenuButtonSoundLifetime>().seconds = splatSound.length + .1f;
    }
}

public class MenuButtonSoundLifetime : MonoBehaviour
{
    public float seconds;
    private System.Collections.IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(gameObject);
    }
}
