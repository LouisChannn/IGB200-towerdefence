using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject buildMenu;

    [Header("Turret Costs - Colour Fuel")]
    [SerializeField] private int[] towerCosts;

    [Header("Colour Meter")]
    [SerializeField] private int turretMeterReward = 10;

    [Header("Attributes")]
    [SerializeField] private float menuYOffset = 0.75f;

    [SerializeField, Range(0f, 1f)]
    private float buildMenuTimeScale = 0.2f;

    private Plot selectedPlot;
    private RectTransform buildMenuRect;

    private void Awake()
    {
        main = this;

        buildMenuRect = buildMenu.GetComponent<RectTransform>();

        buildMenu.SetActive(false);
    }

    private void Update()
    {
        if (selectedPlot == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        // Don't close the menu when clicking its buttons
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check if the player clicked the selected plot
        Vector2 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null && hit.gameObject == selectedPlot.gameObject)
            return;

        // Close menu if player clicked somewhere else
        CloseBuildMenu();
    }

    public void OpenBuildMenu(Plot plot)
    {
        selectedPlot = plot;

        Vector3 desiredPos =
            plot.transform.position +
            new Vector3(0f, menuYOffset, 0f);

        buildMenu.transform.position =
            ClampToScreen(desiredPos);

        buildMenu.SetActive(true);

        Time.timeScale = buildMenuTimeScale;
    }

    // Keeps the popup fully within the camera's view
    private Vector3 ClampToScreen(Vector3 desiredPos)
    {
        Camera cam = Camera.main;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;

        float menuHalfWidth =
            (buildMenuRect.rect.width *
            buildMenu.transform.localScale.x) / 2f;

        float menuHalfHeight =
            (buildMenuRect.rect.height *
            buildMenu.transform.localScale.y) / 2f;

        float minX =
            camPos.x - camHalfWidth + menuHalfWidth;

        float maxX =
            camPos.x + camHalfWidth - menuHalfWidth;

        float minY =
            camPos.y - camHalfHeight + menuHalfHeight;

        float maxY =
            camPos.y + camHalfHeight - menuHalfHeight;

        float clampedX =
            Mathf.Clamp(desiredPos.x, minX, maxX);

        float clampedY =
            Mathf.Clamp(desiredPos.y, minY, maxY);

        return new Vector3(
            clampedX,
            clampedY,
            desiredPos.z
        );
    }

    public void CloseBuildMenu()
    {
        selectedPlot = null;

        buildMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    // Called by each turret button
    public void SelectTurretToBuild(int towerIndex)
    {
        if (selectedPlot == null)
            return;

        // Check turret index
        if (towerIndex < 0 ||
            towerIndex >= towerPrefabs.Length)
        {
            Debug.LogError("Invalid turret index!");
            return;
        }

        // Check that a cost exists
        if (towerIndex >= towerCosts.Length)
        {
            Debug.LogError(
                "No cost assigned for turret " + towerIndex
            );

            return;
        }

        int cost = towerCosts[towerIndex];

        // Spend Colour Fuel
        if (!LevelManager.main.SpendColourFuel(cost))
        {
            Debug.Log(
                "Not enough Colour Fuel to build this turret!"
            );

            return;
        }

        // Build the turret
        selectedPlot.BuildTower(
            towerPrefabs[towerIndex]
        );

        // Increase Colour Meter
        LevelManager.main.IncreaseColourMeter(
            turretMeterReward
        );

        Debug.Log(
            "Turret built! Cost: " +
            cost +
            " Colour Fuel | +" +
            turretMeterReward +
            " Colour Meter"
        );

        CloseBuildMenu();
    }
}