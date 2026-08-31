using UnityEngine;

public class TestSprayTool : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject paintDotPrefab; // small placeholder sprite, Sorting Order should match your PaintLayer (5)
    [SerializeField] private Transform paintLayerParent; // drag your PaintLayer GameObject here

    [Header("Attributes")]
    [SerializeField] private LayerMask sawLayer; // set this to whatever physics Layer you put the saw's Collider2D on
    [SerializeField] private int sprayCount = 12;
    [SerializeField] private float sprayRadius = 0.75f;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Click to fire a test spray at the mouse position - stand-in for real turret targeting
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 sprayOrigin = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Spray(sprayOrigin);
        }
    }

    private void Spray(Vector2 origin)
    {
        for (int i = 0; i < sprayCount; i++)
        {
            Vector2 point = origin + Random.insideUnitCircle * sprayRadius;

            // Check whether the saw's collider occupies this point
            Collider2D hit = Physics2D.OverlapPoint(point, sawLayer);
            bool blocked = hit != null;

            if (blocked)
            {
                Debug.DrawLine(origin, point, Color.red, 1f); // blocked - visualize in Scene view
                continue;
            }

            Debug.DrawLine(origin, point, Color.green, 1f); // got through - visualize in Scene view
            PlacePaintDot(point);
        }
    }

    private void PlacePaintDot(Vector2 point)
    {
        if (paintDotPrefab == null) return;

        // Stand-in only - once the real paint layer (Texture2D/RenderTexture) exists,
        // this gets replaced by an actual write to that data instead of spawning objects
        Instantiate(paintDotPrefab, point, Quaternion.identity, paintLayerParent);
    }
}