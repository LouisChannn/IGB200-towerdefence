using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject buildMenu;

    [Header("Turret Costs")]
    [SerializeField] private int[] towerCosts;

    [Header("Attributes")]
    [SerializeField] private float menuYOffset = 0.75f;

    [SerializeField, Range(0f, 1f)]
    private float buildMenuTimeScale = 0.2f;

    private Plot selectedPlot;

    private void Awake()
    {
        main = this;
        buildMenu.SetActive(false);
    }

    private void Update()
    {
        if (selectedPlot == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        // Don't close the menu when clicking its buttons
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check if the player clicked the selected plot
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null && hit.gameObject == selectedPlot.gameObject)
            return;

        // Close menu if player clicked somewhere else
        CloseBuildMenu();
    }

    public void OpenBuildMenu(Plot plot)
    {
        selectedPlot = plot;

        buildMenu.transform.position =
            plot.transform.position + new Vector3(0f, menuYOffset, 0f);

        buildMenu.SetActive(true);

        Time.timeScale = buildMenuTimeScale;
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
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            Debug.LogError("Invalid turret index!");
            return;
        }

        // Check that a cost exists
        if (towerIndex >= towerCosts.Length)
        {
            Debug.LogError("No cost assigned for turret " + towerIndex);
            return;
        }

        int cost = towerCosts[towerIndex];

        // Try to spend Paint Fuel
        if (!LevelManager.main.SpendPaintFuel(cost))
        {
            Debug.Log("Not enough Paint Fuel to build this turret!");
            return;
        }

        // Build the turret
        selectedPlot.BuildTower(towerPrefabs[towerIndex]);

        Debug.Log("Turret built! Cost: " + cost + " Paint Fuel");

        CloseBuildMenu();
    }
}