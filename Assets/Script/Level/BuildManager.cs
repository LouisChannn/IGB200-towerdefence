using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject buildMenu;

    [Header("HUD Clearance")]
    [SerializeField] private RectTransform topHudArea;
    [SerializeField] private RectTransform colourBurstArea;

    [Header("Turret Costs - Colour Fuel")]
    [SerializeField] private int[] towerCosts;

    private const float SellRefundFraction = 0.4f;

    [Header("Turret Menu Audio")]
    [SerializeField] private AudioClip turretMenuClickSound;

    [Header("Placement Audio")]
    [SerializeField] private AudioClip placementSound;
    [SerializeField, Range(0f, 1f)] private float placementVolume = 0.65f;
    private AudioSource placementAudio;

    [Header("Colour Meter")]
    [SerializeField] private int turretMeterReward = 10;

    [Header("Attributes")]
    [SerializeField] private float menuYOffset = 0.75f;
    [SerializeField, Range(0f, 1f)] private float buildMenuTimeScale = 0.2f;

    private Plot selectedPlot;
    private RectTransform buildMenuRect;
    private GameObject[] buildMenuContents;
    private bool[] buildMenuContentStates;
    private TurretRadialMenu turretMenu;
    private bool showingTurretMenu;
    private float previousTimeScale = 1f;
    private int menuOpenedFrame = -1;
    private int dismissedMenuFrame = -1;
    private Vector3 buildMenuBaseScale;
    private readonly Vector3[] screenBoundsCorners = new Vector3[4];
    private int placementScreenWidth = -1;
    private int placementScreenHeight = -1;
    private Rect placementCameraRect;
    private Rect placementSafeArea;
    private Camera placementCamera;

    private void Awake()
    {
        main = this;
        buildMenuRect = buildMenu.GetComponent<RectTransform>();
        buildMenuBaseScale = buildMenu.transform.localScale;
        Canvas menuCanvas = buildMenu.GetComponent<Canvas>();
        if (menuCanvas != null)
        {
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 150;
        }

        // Keep the existing build choices intact while showing turret actions.
        buildMenuContents = new GameObject[buildMenu.transform.childCount];
        buildMenuContentStates = new bool[buildMenuContents.Length];
        for (int i = 0; i < buildMenuContents.Length; i++)
        {
            buildMenuContents[i] = buildMenu.transform.GetChild(i).gameObject;
            buildMenuContentStates[i] = buildMenuContents[i].activeSelf;
        }

        CreateTurretMenu();
        buildMenu.SetActive(false);

        if (placementSound != null)
        {
            var audioObject = new GameObject("Turret Placement Audio", typeof(AudioSource));
            audioObject.transform.SetParent(transform, false);
            placementAudio = audioObject.GetComponent<AudioSource>();
            placementAudio.playOnAwake = false;
            placementAudio.spatialBlend = 0f;
        }
    }

    private void Update()
    {
        if (selectedPlot == null)
            return;

        // A pause, defeat or win takes priority over the slowed menu state.
        if (Time.timeScale <= 0f)
        {
            CloseBuildMenu();
            return;
        }

        if (PlacementViewportChanged())
            RepositionMenu();

        if (Time.frameCount == menuOpenedFrame || !Input.GetMouseButtonDown(0))
        {
            if (showingTurretMenu)
                RefreshTurretMenu();
            return;
        }

        // Test the actual circle before generic UI hits, including transparent corners.
        if (showingTurretMenu)
        {
            Canvas canvas = buildMenu.GetComponent<Canvas>();
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (!turretMenu.ContainsScreenPoint(Input.mousePosition, eventCamera))
                DismissTurretMenu();
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D plotCollider = selectedPlot.GetComponent<Collider2D>();
        if (plotCollider != null && plotCollider.OverlapPoint(mouseWorld))
            return;

        CloseBuildMenu();
    }

    public void OpenBuildMenu(Plot plot)
    {
        OpenMenu(plot, false);
    }

    public void SelectPlot(Plot plot)
    {
        if (Time.frameCount == dismissedMenuFrame)
            return;

        // An outside plot click closes the current circle without clicking through it.
        if (selectedPlot != null && showingTurretMenu)
        {
            DismissTurretMenu();
            return;
        }

        OpenMenu(plot, plot != null && plot.HasTower);
    }

    private void OpenMenu(Plot plot, bool managingTurret)
    {
        if (plot == null || plot.HasTower != managingTurret || Time.timeScale <= 0f)
            return;

        if (selectedPlot == null)
            previousTimeScale = Time.timeScale;

        if (selectedPlot != null && selectedPlot.TowerTurret != null)
            selectedPlot.TowerTurret.SetUpgradePreviewVisible(false);
        selectedPlot = plot;
        showingTurretMenu = managingTurret;
        menuOpenedFrame = Time.frameCount;

        for (int i = 0; i < buildMenuContents.Length; i++)
            buildMenuContents[i].SetActive(!managingTurret && buildMenuContentStates[i]);
        turretMenu.gameObject.SetActive(managingTurret);

        if (managingTurret)
        {
            if (plot.TowerTurret != null)
                plot.TowerTurret.SetUpgradePreviewVisible(true);
            turretMenu.ShowActions();
            RefreshTurretMenu();
        }

        RepositionMenu();
        buildMenu.SetActive(true);
        Time.timeScale = Mathf.Min(previousTimeScale, Mathf.Max(0.01f, buildMenuTimeScale));
    }

    private Vector3 ClampToScreen(Vector3 desiredPos, RectTransform menuRect)
    {
        Camera cam = Camera.main;
        // Every fit starts from the original size, so reopening or enlarging the window cannot accumulate shrinkage.
        buildMenu.transform.localScale = buildMenuBaseScale;
        buildMenu.transform.position = desiredPos;
        if (cam == null || menuRect == null)
            return desiredPos;

        Canvas.ForceUpdateCanvases();
        Rect safeRect = GetMenuSafeRect(cam);
        Rect menuBounds = GetScreenBounds(menuRect, cam);
        float fit = Mathf.Min(1f, Mathf.Min(
            safeRect.width / Mathf.Max(0.001f, menuBounds.width),
            safeRect.height / Mathf.Max(0.001f, menuBounds.height)));
        if (fit < 1f)
        {
            buildMenu.transform.localScale = buildMenuBaseScale * fit;
            menuBounds = GetScreenBounds(menuRect, cam);
        }

        Vector2 offset = Vector2.zero;
        if (menuBounds.xMin < safeRect.xMin)
            offset.x = safeRect.xMin - menuBounds.xMin;
        else if (menuBounds.xMax > safeRect.xMax)
            offset.x = safeRect.xMax - menuBounds.xMax;
        if (menuBounds.yMin < safeRect.yMin)
            offset.y = safeRect.yMin - menuBounds.yMin;
        else if (menuBounds.yMax > safeRect.yMax)
            offset.y = safeRect.yMax - menuBounds.yMax;

        // Keep the menu on its current camera-depth plane while applying the screen-space correction.
        Vector3 screenPosition = cam.WorldToScreenPoint(desiredPos);
        screenPosition.x += offset.x;
        screenPosition.y += offset.y;
        return cam.ScreenToWorldPoint(screenPosition);
    }

    private Rect GetMenuSafeRect(Camera cam)
    {
        Rect viewport = cam.pixelRect;
        Rect deviceSafeArea = Screen.safeArea;
        float left = Mathf.Max(viewport.xMin, deviceSafeArea.xMin);
        float bottom = Mathf.Max(viewport.yMin, deviceSafeArea.yMin);
        float right = Mathf.Min(viewport.xMax, deviceSafeArea.xMax);
        float top = Mathf.Min(viewport.yMax, deviceSafeArea.yMax);
        if (right <= left || top <= bottom)
        {
            left = viewport.xMin;
            bottom = viewport.yMin;
            right = viewport.xMax;
            top = viewport.yMax;
        }

        float padding = Mathf.Min(Screen.height * 0.015f,
            Mathf.Min((right - left) * 0.25f, (top - bottom) * 0.25f));
        left += padding;
        bottom += padding;
        right -= padding;
        top -= padding;

        // Both the build choices and turret circle use the same protected area.
        if (topHudArea != null)
            top = Mathf.Min(top, GetHudScreenBounds(topHudArea, cam).yMin - padding);
        else
            top = Mathf.Min(top, viewport.yMax - viewport.height * 0.25f);
        if (colourBurstArea != null)
            right = Mathf.Min(right, GetHudScreenBounds(colourBurstArea, cam).xMin - padding);
        else
            right = Mathf.Min(right, viewport.xMax - viewport.width * 0.18f);

        // An invalid or oversized HUD rectangle must not invert the clamp interval.
        right = Mathf.Max(left + 1f, right);
        top = Mathf.Max(bottom + 1f, top);
        return Rect.MinMaxRect(left, bottom, right, top);
    }

    private Rect GetHudScreenBounds(RectTransform rect, Camera fallbackCamera)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
        Camera eventCamera = rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas != null && rootCanvas.worldCamera != null ? rootCanvas.worldCamera : fallbackCamera;
        return GetScreenBounds(rect, eventCamera);
    }

    private Rect GetScreenBounds(RectTransform rect, Camera camera)
    {
        rect.GetWorldCorners(screenBoundsCorners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int i = 0; i < screenBoundsCorners.Length; i++)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, screenBoundsCorners[i]);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void RepositionMenu()
    {
        if (selectedPlot == null)
            return;
        Vector3 desiredPos = selectedPlot.transform.position + new Vector3(0f, menuYOffset, 0f);
        RectTransform menuRect = showingTurretMenu ? turretMenu.RectTransform : buildMenuRect;
        buildMenu.transform.position = ClampToScreen(desiredPos, menuRect);
        placementScreenWidth = Screen.width;
        placementScreenHeight = Screen.height;
        placementCamera = Camera.main;
        placementCameraRect = placementCamera != null ? placementCamera.pixelRect : default;
        placementSafeArea = Screen.safeArea;
    }

    private bool PlacementViewportChanged()
    {
        Camera cam = Camera.main;
        return Screen.width != placementScreenWidth || Screen.height != placementScreenHeight ||
            cam != placementCamera || (cam != null && cam.pixelRect != placementCameraRect) ||
            Screen.safeArea != placementSafeArea;
    }

    public void CloseBuildMenu()
    {
        bool wasOpen = selectedPlot != null;
        if (selectedPlot != null && selectedPlot.TowerTurret != null)
            selectedPlot.TowerTurret.SetUpgradePreviewVisible(false);
        selectedPlot = null;
        showingTurretMenu = false;
        buildMenu.SetActive(false);
        turretMenu.gameObject.SetActive(false);

        // Closing a menu must not undo a pause or the end of the level.
        if (wasOpen && Time.timeScale > 0f)
            Time.timeScale = previousTimeScale;
    }

    public void SelectTurretToBuild(int towerIndex)
    {
        if (selectedPlot == null || selectedPlot.HasTower || showingTurretMenu || Time.timeScale <= 0f)
            return;

        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length || towerPrefabs[towerIndex] == null)
        {
            Debug.LogError("Invalid turret index or prefab!");
            return;
        }

        if (towerIndex >= towerCosts.Length)
        {
            Debug.LogError("No cost assigned for turret " + towerIndex);
            return;
        }

        int cost = towerCosts[towerIndex];
        bool chargedFuel = !LevelManager.main.UnlimitedColourFuel;
        if (!LevelManager.main.SpendColourFuel(cost))
        {
            Debug.Log("Not enough Colour Fuel to build this turret!");
            return;
        }

        if (!selectedPlot.BuildTower(towerPrefabs[towerIndex], cost))
        {
            if (chargedFuel)
                LevelManager.main.IncreaseColourFuel(cost);
            return;
        }

        if (placementAudio != null && placementSound != null)
            placementAudio.PlayOneShot(placementSound, placementVolume);

        LevelManager.main.IncreaseColourMeter(turretMeterReward);
        Debug.Log("Turret built! Cost: " + cost + " Colour Fuel | +" + turretMeterReward + " Colour Meter");
        CloseBuildMenu();
    }

    public void SellSelectedTurret()
    {
        if (selectedPlot == null || !showingTurretMenu || Time.timeScale <= 0f || LevelManager.main == null)
            return;

        Turret turret = selectedPlot.TowerTurret;
        int upgradeSpend = turret != null ? turret.TotalUpgradeSpend : 0;
        if (selectedPlot.TrySellTower(out int originalBuildCost))
        {
            int refund = CalculateSellRefund(originalBuildCost, upgradeSpend);
            LevelManager.main.IncreaseColourFuel(refund);
            Debug.Log("Turret sold! Refund: " + refund + " Colour Fuel");
        }

        CloseBuildMenu();
    }

    private int CalculateSellRefund(int buildCost, int upgradeSpend)
    {
        return Mathf.FloorToInt((Mathf.Max(0, buildCost) + Mathf.Max(0, upgradeSpend)) * SellRefundFraction);
    }

    public void UpgradeSelectedTurret()
    {
        if (selectedPlot == null || !showingTurretMenu || Time.timeScale <= 0f || LevelManager.main == null)
            return;

        turretMenu.ShowUpgrades();
        RefreshTurretMenu();
    }

    public void BuySelectedUpgrade(TurretUpgradeType upgradeType)
    {
        if (selectedPlot == null || !showingTurretMenu || Time.timeScale <= 0f || LevelManager.main == null)
            return;

        Turret turret = selectedPlot.TowerTurret;
        if (turret != null && turret.TryUpgrade(upgradeType))
            RefreshTurretMenu();
    }

    private void RefreshTurretMenu()
    {
        if (selectedPlot == null || !selectedPlot.HasTower)
        {
            CloseBuildMenu();
            return;
        }

        Turret turret = selectedPlot.TowerTurret;
        bool canAfford = LevelManager.main != null && turret != null &&
            (LevelManager.main.UnlimitedColourFuel || LevelManager.main.colourFuel >= turret.GetUpgradeCost());
        int upgradeSpend = turret != null ? turret.TotalUpgradeSpend : 0;
        turretMenu.Refresh(turret, CalculateSellRefund(selectedPlot.TowerBuildCost, upgradeSpend), canAfford);
    }

    private void DismissTurretMenu()
    {
        dismissedMenuFrame = Time.frameCount;
        CloseBuildMenu();
    }

    private void CreateTurretMenu()
    {
        TMP_Text textTemplate = buildMenu.GetComponentInChildren<TMP_Text>(true);
        var menuObject = new GameObject("Turret Actions", typeof(RectTransform), typeof(TurretRadialMenu));
        menuObject.layer = buildMenu.layer;
        menuObject.transform.SetParent(buildMenu.transform, false);
        turretMenu = menuObject.GetComponent<TurretRadialMenu>();
        turretMenu.Initialize(textTemplate, turretMenuClickSound, UpgradeSelectedTurret, SellSelectedTurret, BuySelectedUpgrade);
    }
}
