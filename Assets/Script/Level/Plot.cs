using UnityEngine;
using UnityEngine.EventSystems;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    private GameObject tower;
    private int towerBuildCost;
    private Color startColor;

    public bool HasTower => tower != null;
    public int TowerBuildCost => HasTower ? towerBuildCost : 0;
    public Turret TowerTurret => HasTower ? tower.GetComponent<Turret>() : null;

    private void Start()
    {
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        sr.color = startColor;
    }

    private void OnMouseDown()
    {
        if (Time.timeScale <= 0f || BuildManager.main == null)
            return;

        // UI clicks must not also select a plot underneath the menu.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        BuildManager.main.SelectPlot(this);
    }

    public bool BuildTower(GameObject towerPrefab, int buildCost)
    {
        if (HasTower || towerPrefab == null || buildCost < 0)
            return false;

        tower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
        towerBuildCost = buildCost;
        sr.color = startColor;
        return true;
    }

    public bool TrySellTower(out int originalBuildCost)
    {
        originalBuildCost = 0;
        if (!HasTower)
            return false;

        GameObject soldTower = tower;
        originalBuildCost = towerBuildCost;

        // Clear ownership before paying the refund so a second click cannot sell twice.
        tower = null;
        towerBuildCost = 0;
        soldTower.SetActive(false);
        Destroy(soldTower);
        sr.color = startColor;
        return true;
    }
}
