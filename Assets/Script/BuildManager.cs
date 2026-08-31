using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject buildMenu; // small World Space Canvas popup, starts inactive in the scene

    [Header("Attributes")]
    [SerializeField] private float menuYOffset = 0.75f; // nudges the menu above the plot so it doesn't sit on top of it
    [SerializeField, Range(0f, 1f)] private float buildMenuTimeScale = 0.2f; // how slow time runs while the menu is open (0 = full pause)

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

        // If the click landed on the menu's own buttons, let their OnClick handle it - do nothing here
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // If the click landed on the currently selected plot, OnMouseDown already re-opens the menu there - do nothing
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
        if (hit != null && hit.gameObject == selectedPlot.gameObject) return;

        // Otherwise, the player clicked away from both the plot and the menu - close it
        CloseBuildMenu();
    }

    public void OpenBuildMenu(Plot plot)
    {
        selectedPlot = plot;
        buildMenu.transform.position = plot.transform.position + new Vector3(0f, menuYOffset, 0f);
        buildMenu.SetActive(true);
        Time.timeScale = buildMenuTimeScale;
    }

    public void CloseBuildMenu()
    {
        selectedPlot = null;
        buildMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    // Hook this to each turret button's OnClick() in the Inspector, one per tower prefab index
    public void SelectTurretToBuild(int towerIndex)
    {
        if (selectedPlot == null) return;

        selectedPlot.BuildTower(towerPrefabs[towerIndex]);
        CloseBuildMenu();
    }
}