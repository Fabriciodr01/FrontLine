using UnityEngine;

public class CinematicCameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float orbitRadius = 35f;
    [SerializeField] private float heightOffset = 20f;

    private float currentAngle = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Enable post-processing for cinematic look
            var urpCamera = mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (urpCamera != null)
            {
                urpCamera.renderPostProcessing = true;
            }
        }
    }

    void Update()
    {
        // Slow orbit around the grid
        currentAngle += Time.deltaTime * rotationSpeed;
        float x = Mathf.Sin(currentAngle) * orbitRadius;
        float z = Mathf.Cos(currentAngle) * orbitRadius;

        transform.position = new Vector3(x, heightOffset, z);
        transform.LookAt(Vector3.zero);
    }
}
