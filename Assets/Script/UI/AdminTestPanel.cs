using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminTestPanel : MonoBehaviour
{
    [SerializeField] private string activationCode = "HUE2026";
    [SerializeField] private TMP_FontAsset uiFont;

    public bool IsUnlocked { get; private set; }
    public TMP_InputField CodeInput { get; private set; }
    public GameObject ControlsPanel { get; private set; }
    public Toggle UnlimitedToggle { get; private set; }
    public Toggle SlowToggle { get; private set; }

    private GameObject entry;
    private GameObject launcher;
    private GameObject activationStatus;
    private TMP_Text entryFeedback;
    private TMP_Text actionFeedback;
    private readonly Color ink = new Color(.12f,.17f,.18f);
    private readonly Color green = new Color(.14f,.56f,.40f);

    private void Awake()
    {
        BuildUI();
    }

    public bool TryActivate(string code)
    {
        if (IsUnlocked) return true;
        if (!string.Equals((code ?? "").Trim(),activationCode,StringComparison.OrdinalIgnoreCase))
        {
            entryFeedback.text = "Code not recognised";
            return false;
        }

        IsUnlocked = true;
        entry.SetActive(false);
        launcher.SetActive(true);
        activationStatus.SetActive(true);
        ControlsPanel.SetActive(true);
        return true;
    }

    public void ToggleControls()
    {
        if (IsUnlocked) ControlsPanel.SetActive(!ControlsPanel.activeSelf);
    }

    public void RechargeBurst()
    {
        if (!IsUnlocked || LevelManager.main == null) return;
        LevelManager.main.RechargeColourBurst();
        actionFeedback.text = "Colour Burst ready";
    }

    public void SetUnlimitedFuel(bool enabled)
    {
        if (!IsUnlocked || LevelManager.main == null) return;
        LevelManager.main.SetUnlimitedColourFuel(enabled);
    }

    public void SetSlowEnemies(bool enabled)
    {
        if (!IsUnlocked || LevelManager.main == null) return;
        LevelManager.main.SetSlowEnemies(enabled);
    }

    private void BuildUI()
    {
        var canvasObject = new GameObject("Admin Test Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform,false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.matchWidthOrHeight = .5f;
        var root = (RectTransform)canvasObject.transform;

        entry = Box(root,"Cheat Code Entry",new Vector2(0,0),new Vector2(16,16),new Vector2(330,76)).gameObject;
        Text(entry.transform,"Code Label","Cheat Code",new Vector2(10,-8),new Vector2(95,30),15);
        var inputBox = Box((RectTransform)entry.transform,"Code Input",new Vector2(0,1),new Vector2(106,-8),new Vector2(122,30));
        inputBox.GetComponent<Image>().color = Color.white;
        CodeInput = inputBox.gameObject.AddComponent<TMP_InputField>();
        var viewport = new GameObject("Text Area",typeof(RectTransform),typeof(RectMask2D));
        viewport.transform.SetParent(inputBox,false);
        var viewportRect = (RectTransform)viewport.transform;
        viewportRect.anchorMin = Vector2.zero; viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(6,2); viewportRect.offsetMax = new Vector2(-6,-2);
        var inputText = Text(viewport.transform,"Input Text","",Vector2.zero,new Vector2(110,26),15);
        inputText.rectTransform.anchorMin = Vector2.zero; inputText.rectTransform.anchorMax = Vector2.one;
        inputText.rectTransform.offsetMin = inputText.rectTransform.offsetMax = Vector2.zero;
        CodeInput.textViewport = viewportRect;
        CodeInput.textComponent = inputText;
        CodeInput.contentType = TMP_InputField.ContentType.Alphanumeric;
        CodeInput.characterLimit = 24;
        CodeInput.lineType = TMP_InputField.LineType.SingleLine;
        CodeInput.onSubmit.AddListener(code => TryActivate(code));
        Command(entry.transform,"Activate",new Vector2(238,-8),new Vector2(82,30),() => TryActivate(CodeInput.text));
        entryFeedback = Text(entry.transform,"Code Feedback","",new Vector2(10,-42),new Vector2(310,24),14);

        launcher = Command(root,"Admin",new Vector2(16,-16),new Vector2(110,36),ToggleControls).gameObject;
        var launcherRect = (RectTransform)launcher.transform;
        launcherRect.anchorMin = launcherRect.anchorMax = Vector2.zero;
        launcherRect.pivot = Vector2.zero; launcherRect.anchoredPosition = new Vector2(16,16);
        launcher.SetActive(false);

        ControlsPanel = Box(root,"Admin Controls",Vector2.zero,new Vector2(16,104),new Vector2(330,224)).gameObject;
        Text(ControlsPanel.transform,"Heading","Admin Controls",new Vector2(14,-8),new Vector2(250,30),18);
        Command(ControlsPanel.transform,"X",new Vector2(288,-8),new Vector2(28,28),ToggleControls);
        Command(ControlsPanel.transform,"Recharge Colour Burst",new Vector2(14,-48),new Vector2(302,36),RechargeBurst);
        UnlimitedToggle = Check(ControlsPanel.transform,"Unlimited Colour Fuel",new Vector2(14,-96),SetUnlimitedFuel);
        SlowToggle = Check(ControlsPanel.transform,"Slow Enemies",new Vector2(14,-138),SetSlowEnemies);
        actionFeedback = Text(ControlsPanel.transform,"Action Feedback","",new Vector2(14,-182),new Vector2(302,26),14);
        ControlsPanel.SetActive(false);

        activationStatus = Box(root,"Cheat Activation Status",new Vector2(1,0),new Vector2(-16,16),new Vector2(250,36)).gameObject;
        Text(activationStatus.transform,"Status Text","Cheat code activated",new Vector2(10,-3),new Vector2(230,30),16);
        activationStatus.SetActive(false);
    }

    private RectTransform Box(RectTransform parent,string name,Vector2 anchor,Vector2 position,Vector2 size)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = new Color(.93f,.96f,.95f);
        return rect;
    }

    private TMP_Text Text(Transform parent,string name,string content,Vector2 position,Vector2 size,int fontSize)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0,1);
        rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        var text = go.GetComponent<TMP_Text>();
        if (uiFont != null) text.font = uiFont;
        text.text = content; text.fontSize = fontSize; text.color = ink;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        return text;
    }

    private Button Command(Transform parent,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var rect = Box((RectTransform)parent,label,new Vector2(0,1),position,size);
        rect.GetComponent<Image>().color = new Color(.78f,.9f,.84f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);
        var text = Text(rect,label+" Text",label,new Vector2(6,0),size-new Vector2(12,0),15);
        text.alignment = TextAlignmentOptions.Midline;
        return button;
    }

    private Toggle Check(Transform parent,string label,Vector2 position,UnityEngine.Events.UnityAction<bool> action)
    {
        var row = Box((RectTransform)parent,label,new Vector2(0,1),position,new Vector2(302,30));
        row.GetComponent<Image>().color = Color.clear;
        var toggle = row.gameObject.AddComponent<Toggle>();
        var box = Box(row,"Checkbox",new Vector2(0,1),new Vector2(0,-5),new Vector2(20,20));
        box.GetComponent<Image>().color = new Color(.7f,.77f,.74f);
        var mark = Box(box,"Check",new Vector2(.5f,.5f),Vector2.zero,new Vector2(12,12));
        mark.GetComponent<Image>().color = green;
        mark.GetComponent<Image>().raycastTarget = false;
        toggle.targetGraphic = box.GetComponent<Image>();
        toggle.graphic = mark.GetComponent<Image>();
        toggle.toggleTransition = Toggle.ToggleTransition.None;
        toggle.isOn = false;
        Text(row,label+" Text",label,new Vector2(30,0),new Vector2(270,30),16);
        toggle.onValueChanged.AddListener(action);
        return toggle;
    }
}
