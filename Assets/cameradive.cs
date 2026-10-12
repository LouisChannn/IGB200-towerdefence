using System.Collections;
using UnityEngine;

public class cameradive : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform gameplayView;

    [Header("Opening View")]
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0.01f)] private float startOrthographicSize = 14f;

    [Header("Gameplay View")]
    [SerializeField, Min(0.01f)] private float gameplayOrthographicSize = 5f;

    [Header("Dive")]
    [SerializeField, Min(0.01f)] private float diveDuration = 0.9f;
    [SerializeField] private GameObject gameplayRoot;

    private Vector3 gameplayPosition;
    private bool hasStarted;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("Camera Dive requires a Camera reference.", this);
            enabled = false;
            return;
        }

        if (!cam.orthographic)
        {
            Debug.LogError("Camera Dive requires an orthographic camera.", this);
            enabled = false;
            return;
        }

        if (gameplayRoot == null)
        {
            Debug.LogError("Camera Dive requires a Gameplay Root reference.", this);
            enabled = false;
            return;
        }

        if (gameplayRoot == gameObject || transform.IsChildOf(gameplayRoot.transform))
        {
            Debug.LogError("Gameplay Root must not contain the camera running Camera Dive.", this);
            enabled = false;
            return;
        }

        gameplayPosition = gameplayView != null
            ? gameplayView.position
            : transform.position;

        if (gameplayRoot.activeSelf)
        {
            Debug.LogWarning(
                "Gameplay Root should be inactive in the scene so gameplay scripts do not run before the dive.",
                gameplayRoot);
            gameplayRoot.SetActive(false);
        }

        transform.position = startPosition;
        cam.orthographicSize = startOrthographicSize;
    }

    private void Start()
    {
        StartCameraDive();
    }

    public void StartCameraDive()
    {
        if (hasStarted || !enabled)
            return;

        hasStarted = true;
        StartCoroutine(DiveRoutine());
    }

    private IEnumerator DiveRoutine()
    {
        Vector3 initialPosition = transform.position;
        float initialSize = cam.orthographicSize;
        float duration = Mathf.Max(0.01f, diveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(initialPosition, gameplayPosition, easedT);
            cam.orthographicSize = Mathf.Lerp(initialSize, gameplayOrthographicSize, easedT);

            yield return null;
        }

        transform.position = gameplayPosition;
        cam.orthographicSize = gameplayOrthographicSize;
        gameplayRoot.SetActive(true);
    }
}
