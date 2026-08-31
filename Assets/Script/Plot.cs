using UnityEngine;
using UnityEngine.EventSystems;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;
    private GameObject tower;
    private Color startColor;

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
        // OnMouseDown fires from a physics raycast, independent of the UI -
        // without this check, clicking a menu button that overlaps another
        // plot underneath it would also trigger that plot's OnMouseDown
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (tower != null) return; // already has a turret, ignore clicks

        // Ask the BuildManager to pop the build menu open on this plot,
        // instead of building the pre-selected tower immediately
        BuildManager.main.OpenBuildMenu(this);
    }

    // Called by BuildManager once the player picks a turret from the popup menu
    public void BuildTower(GameObject towerPrefab)
    {
        tower = Instantiate(towerPrefab, transform.position, Quaternion.identity);
    }
}