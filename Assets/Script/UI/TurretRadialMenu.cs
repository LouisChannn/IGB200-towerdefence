using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class TurretRadialMenu : MonoBehaviour
{
    private const float Diameter = 640f;
    private const float SectorDiameter = 624f;
    private const float UpgradeInnerRadius = 0.32f;

    private RadialMenuGraphic circle;
    private GameObject actionsPage;
    private GameObject upgradesPage;
    private TMP_Text sellLabel;
    private Button openUpgradesButton;
    private Button sellButton;
    private Button backButton;
    private readonly Button[] upgradeButtons = new Button[3];
    private readonly TMP_Text[] upgradeLabels = new TMP_Text[3];
    private readonly RadialMenuGraphic[] upgradeLevels = new RadialMenuGraphic[3];
    private readonly TurretUpgradeType[] upgradeTypes = new TurretUpgradeType[3];
    private TMP_FontAsset font;
    private Material fontMaterial;
    private AudioSource pressAudio;
    private AudioClip pressSound;
    private bool showingUpgrades;
    private int pageChangedFrame = -1;

    public RectTransform RectTransform => (RectTransform)transform;
    public bool IsShowingUpgrades => showingUpgrades;

    public void Initialize(TMP_Text textTemplate, AudioClip clickSound,
        UnityAction upgrade, UnityAction sell, UnityAction<TurretUpgradeType> buyUpgrade)
    {
        if (textTemplate != null)
        {
            font = textTemplate.font;
            fontMaterial = textTemplate.fontSharedMaterial;
        }

        pressSound = clickSound;
        pressAudio = gameObject.AddComponent<AudioSource>();
        pressAudio.playOnAwake = false;
        pressAudio.spatialBlend = 0f;
        pressAudio.volume = 1f;

        SetRect(RectTransform, Vector2.zero, new Vector2(Diameter, Diameter));
        circle = gameObject.AddComponent<RadialMenuGraphic>();
        circle.Configure(0f, 360f);
        circle.color = new Color(0.9f, 0.84f, 0.68f);

        actionsPage = CreatePage("Actions");
        upgradesPage = CreatePage("Upgrades");
        CreateActionsPage(upgrade, sell);
        CreateUpgradesPage(buyUpgrade);
        actionsPage.SetActive(true);
        upgradesPage.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowActions()
    {
        showingUpgrades = false;
        actionsPage.SetActive(true);
        upgradesPage.SetActive(false);
        pageChangedFrame = Time.frameCount;
        SelectForNewPage(openUpgradesButton);
    }

    public void ShowUpgrades()
    {
        showingUpgrades = true;
        actionsPage.SetActive(false);
        upgradesPage.SetActive(true);
        pageChangedFrame = Time.frameCount;
        SelectForNewPage(FirstAvailableUpgrade());
    }

    public void Refresh(Turret turret, int refund, bool canAffordUpgrade)
    {
        SetText(sellLabel, "Sell\n<size=65%>Refund: " + refund + " CF</size>");

        // The choice page stays inspectable even when the budget or fuel is exhausted.
        openUpgradesButton.interactable = turret != null;
        sellButton.interactable = turret != null;
        upgradeTypes[0] = TurretUpgradeType.Range;
        upgradeTypes[1] = TurretUpgradeType.Damage;
        upgradeTypes[2] = turret != null && turret.UsesDamageOverTime
            ? TurretUpgradeType.DamageOverTime : TurretUpgradeType.FireRate;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            TurretUpgradeType type = upgradeTypes[i];
            bool available = turret != null && turret.IsUpgradeTypeAvailable(type);
            SetText(upgradeLabels[i], UpgradeName(type));
            upgradeLevels[i].ConfigureLevelBands(Turret.MaxUpgradePoints,
                available ? turret.GetUpgradeLevel(type) : 0);
            upgradeButtons[i].interactable = available && !turret.IsMaxLevel() && canAffordUpgrade;
        }

        // Keyboard users can still leave the page after buying their last affordable point.
        if (showingUpgrades && EventSystem.current != null && !IsMouseInteraction())
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (selected == upgradeButtons[i].gameObject && !upgradeButtons[i].interactable)
                {
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                    break;
                }
            }
        }
    }

    public bool ContainsScreenPoint(Vector2 point, Camera eventCamera)
    {
        return circle != null && circle.ContainsScreenPoint(point, eventCamera);
    }

    private void CreateActionsPage(UnityAction upgrade, UnityAction sell)
    {
        openUpgradesButton = CreateButton("Upgrade", actionsPage.transform, Vector2.zero,
            SectorDiameter, 90f, 180f, 0f, () =>
            {
                ShowUpgrades();
                upgrade?.Invoke();
            });
        TMP_Text openLabel = CreateLabel("Label", openUpgradesButton.transform,
            new Vector2(-145f, 0f), new Vector2(250f, 160f), 48f);
        SetText(openLabel, "Upgrade");
        AddFeedback(openUpgradesButton, openLabel);

        sellButton = CreateButton("Sell", actionsPage.transform, Vector2.zero,
            SectorDiameter, -90f, 180f, 0f, sell);
        sellLabel = CreateLabel("Label", sellButton.transform,
            new Vector2(145f, 0f), new Vector2(250f, 160f), 48f);
        AddFeedback(sellButton, sellLabel);
        SetNavigation(openUpgradesButton, null, null, null, sellButton);
        SetNavigation(sellButton, null, null, openUpgradesButton, null);

        var dividerObject = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        dividerObject.layer = gameObject.layer;
        dividerObject.transform.SetParent(actionsPage.transform, false);
        SetRect(dividerObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(4f, 360f));
        Image divider = dividerObject.GetComponent<Image>();
        divider.color = new Color(0.9f, 0.84f, 0.68f, 0.7f);
        divider.raycastTarget = false;
    }

    private void CreateUpgradesPage(UnityAction<TurretUpgradeType> buyUpgrade)
    {
        float[] angles = { 30f, 150f, 270f };
        Vector2[] labelPositions = { new Vector2(0f, 205f), new Vector2(-174f, -110f), new Vector2(174f, -110f) };
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            Button button = CreateButton("Upgrade choice " + i, upgradesPage.transform,
                Vector2.zero, SectorDiameter, angles[i], 120f, UpgradeInnerRadius,
                () => buyUpgrade?.Invoke(upgradeTypes[index]));
            // Progress is a separate mesh: Button tint affects the base, not the band colours.
            var levelsObject = new GameObject("Level bands", typeof(RectTransform), typeof(RadialMenuGraphic));
            levelsObject.layer = gameObject.layer;
            levelsObject.transform.SetParent(button.transform, false);
            SetRect(levelsObject.GetComponent<RectTransform>(), Vector2.zero,
                new Vector2(SectorDiameter, SectorDiameter));
            RadialMenuGraphic levels = levelsObject.GetComponent<RadialMenuGraphic>();
            levels.Configure(angles[i], 120f, UpgradeInnerRadius);
            levels.ConfigureLevelBands(Turret.MaxUpgradePoints, 0);
            levels.color = Color.white;
            levels.raycastTarget = false;
            upgradeLevels[i] = levels;

            Vector2 size = i == 0 ? new Vector2(230f, 90f) : new Vector2(190f, 90f);
            TMP_Text label = CreateLabel("Label", button.transform, labelPositions[i], size, 42f);
            label.fontSizeMin = 40f;
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.16f;
            label.outlineColor = Color.black;
            upgradeButtons[i] = button;
            upgradeLabels[i] = label;
            AddFeedback(button, label);
        }

        // The full inner circle is Back, with no smaller button or inactive annulus.
        float backDiameter = SectorDiameter * UpgradeInnerRadius;
        backButton = CreateButton("Back", upgradesPage.transform, Vector2.zero,
            backDiameter, 0f, 360f, 0f, ShowActions);
        TMP_Text backLabel = CreateLabel("Label", backButton.transform,
            Vector2.zero, new Vector2(160f, 90f), 42f);
        SetText(backLabel, "Back");
        AddFeedback(backButton, backLabel);

        SetNavigation(upgradeButtons[0], null, backButton, upgradeButtons[1], upgradeButtons[2]);
        SetNavigation(upgradeButtons[1], upgradeButtons[0], upgradeButtons[2], null, backButton);
        SetNavigation(upgradeButtons[2], upgradeButtons[0], upgradeButtons[1], backButton, null);
        SetNavigation(backButton, upgradeButtons[0], upgradeButtons[1], upgradeButtons[1], upgradeButtons[2]);
    }

    private static string UpgradeName(TurretUpgradeType type)
    {
        return type == TurretUpgradeType.Range ? "Range" :
            type == TurretUpgradeType.Damage ? "Damage" :
            type == TurretUpgradeType.DamageOverTime ? "DOT" : "Firing rate";
    }

    private Button CreateButton(string title, Transform parent, Vector2 position,
        float diameter, float startAngle, float angleDegrees, float innerRadius, UnityAction action)
    {
        var optionObject = new GameObject(title, typeof(RectTransform), typeof(RadialMenuGraphic), typeof(Button));
        optionObject.layer = gameObject.layer;
        optionObject.transform.SetParent(parent, false);
        SetRect(optionObject.GetComponent<RectTransform>(), position, new Vector2(diameter, diameter));
        RadialMenuGraphic sector = optionObject.GetComponent<RadialMenuGraphic>();
        sector.Configure(startAngle, angleDegrees, innerRadius);
        sector.color = Color.white;

        Button button = optionObject.GetComponent<Button>();
        button.targetGraphic = sector;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colours = button.colors;
        colours.normalColor = new Color(0.16f, 0.22f, 0.19f);
        colours.highlightedColor = new Color(0.22f, 0.7f, 0.53f);
        colours.selectedColor = colours.highlightedColor;
        colours.pressedColor = new Color(0.16f, 0.52f, 0.37f);
        colours.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.75f);
        colours.fadeDuration = 0.12f;
        button.colors = colours;
        button.onClick.AddListener(() =>
        {
            // A page-opening click or submit cannot also buy a newly revealed choice.
            if (Time.frameCount == pageChangedFrame)
                return;
            action?.Invoke();
            if (Input.GetMouseButtonUp(0) && EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == optionObject)
                EventSystem.current.SetSelectedGameObject(null);
        });
        return button;
    }

    private void AddFeedback(Button button, TMP_Text label)
    {
        RandomHoverButton feedback = button.gameObject.AddComponent<RandomHoverButton>();
        feedback.buttonText = label;
        feedback.audioSource = pressAudio;
        feedback.splatSound = pressSound;
    }

    private GameObject CreatePage(string title)
    {
        var page = new GameObject(title, typeof(RectTransform));
        page.layer = gameObject.layer;
        page.transform.SetParent(transform, false);
        SetRect(page.GetComponent<RectTransform>(), Vector2.zero, new Vector2(Diameter, Diameter));
        return page;
    }

    private TMP_Text CreateLabel(string title, Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        var labelObject = new GameObject(title, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.layer = gameObject.layer;
        labelObject.transform.SetParent(parent, false);
        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        SetRect(label.rectTransform, position, size);
        if (font != null)
        {
            label.font = font;
            label.fontSharedMaterial = fontMaterial;
        }
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = fontSize * 0.8f;
        label.fontSizeMax = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private void SelectForNewPage(Button keyboardChoice)
    {
        if (EventSystem.current == null)
            return;
        EventSystem.current.SetSelectedGameObject(null);
        if (!IsMouseInteraction() && keyboardChoice != null && keyboardChoice.interactable)
            EventSystem.current.SetSelectedGameObject(keyboardChoice.gameObject);
    }

    private Button FirstAvailableUpgrade()
    {
        foreach (Button button in upgradeButtons)
        {
            if (button != null && button.interactable)
                return button;
        }
        return backButton;
    }

    private static bool IsMouseInteraction()
    {
        return Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);
    }

    private static void SetNavigation(Button button, Button up, Button down, Button left, Button right)
    {
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up,
            selectOnDown = down,
            selectOnLeft = left,
            selectOnRight = right
        };
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label.text != value)
            label.text = value;
    }

}
