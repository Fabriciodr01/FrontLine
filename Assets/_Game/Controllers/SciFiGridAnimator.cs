using UnityEngine;
using System.Collections.Generic;

public class SciFiGridAnimator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private float cubeScale = 0.8f;

    [Header("Animation Settings")]
    [SerializeField] private float globalSpeed = 1.5f;
    [SerializeField] private float waveAmplitude = 1.2f;
    [SerializeField] private float rotationAmount = 45f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float colorIntensity = 1.5f;

    [Header("Visual Effects")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private Material cubeMaterial;
    [SerializeField] private bool addGlowEffect = true;
    [SerializeField] private float glowIntensity = 0.8f;

    [Header("Wave Patterns")]
    [SerializeField] private AnimationCurve waveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float waveComplexity = 2.5f;

    private List<GridCube> cubes = new List<GridCube>();
    private float globalTime;
    private MaterialPropertyBlock propertyBlock;

    [System.Serializable]
    public class GridCube
    {
        public Transform transform;
        public Vector3 originalPosition;
        public Vector2 gridPosition;
        public Renderer renderer;
        public float phaseOffset;
    }

    void Start()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab not assigned!");
            return;
        }

        propertyBlock = new MaterialPropertyBlock();
        CreateGrid();
        SetupCameraForCinematic();
    }

    void CreateGrid()
    {
        Vector3 startPos = new Vector3(-(gridSize.x * spacing) / 2, 0, -(gridSize.y * spacing) / 2);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 position = startPos + new Vector3(x * spacing, 0, z * spacing);
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                cube.transform.localScale = Vector3.one * cubeScale;

                // Apply material
                if (cubeMaterial != null)
                {
                    Renderer rend = cube.GetComponent<Renderer>();
                    rend.material = cubeMaterial;

                    // Add emission for URP glow
                    if (addGlowEffect && rend.material.HasProperty("_EmissionColor"))
                    {
                        rend.material.EnableKeyword("_EMISSION");
                        rend.material.SetColor("_EmissionColor", Color.cyan * glowIntensity);
                    }
                }

                GridCube gridCube = new GridCube
                {
                    transform = cube.transform,
                    originalPosition = position,
                    gridPosition = new Vector2(x, z),
                    renderer = cube.GetComponent<Renderer>(),
                    phaseOffset = Random.Range(0f, Mathf.PI * 2)
                };

                cubes.Add(gridCube);
            }
        }
    }

    void SetupCameraForCinematic()
    {
        // Try to find main camera and add slight rotation if not set
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Position camera for dramatic angle if not manually positioned
            if (mainCam.transform.position == new Vector3(0, 0, -10))
            {
                mainCam.transform.position = new Vector3(15, 25, -25);
                mainCam.transform.rotation = Quaternion.Euler(35, 45, 0);
            }
        }
    }

    void Update()
    {
        globalTime += Time.deltaTime * globalSpeed;
        AnimateGrid();
        UpdateColors();
    }

    void AnimateGrid()
    {
        foreach (GridCube cube in cubes)
        {
            // Multiple wave layers for complex motion
            float xNorm = cube.gridPosition.x / gridSize.x;
            float zNorm = cube.gridPosition.y / gridSize.y;

            // Primary sine wave - radial from center
            float centerX = (cube.gridPosition.x - gridSize.x / 2) / gridSize.x;
            float centerZ = (cube.gridPosition.y - gridSize.y / 2) / gridSize.y;
            float distanceFromCenter = Mathf.Sqrt(centerX * centerX + centerZ * centerZ);

            float wave1 = Mathf.Sin(globalTime * 1.5f + distanceFromCenter * waveComplexity * 8f) * 0.8f;

            // Secondary cosine wave - traveling wave
            float wave2 = Mathf.Cos(globalTime * 1.2f + xNorm * waveComplexity * 6f) * 0.6f;
            float wave3 = Mathf.Sin(globalTime * 0.8f + zNorm * waveComplexity * 6f) * 0.6f;

            // Tertiary wave - intricate pattern
            float wave4 = Mathf.Sin(globalTime * 2f + (cube.gridPosition.x * 0.5f + cube.gridPosition.y * 0.3f) * waveComplexity) * 0.4f;
            float wave5 = Mathf.Cos(globalTime * 1.8f + (cube.gridPosition.x * 0.3f - cube.gridPosition.y * 0.5f) * waveComplexity) * 0.4f;

            // Combine waves
            float combinedWave = (wave1 + wave2 + wave3 + wave4 + wave5) * waveAmplitude;
            float height = waveCurve.Evaluate((combinedWave + 1) / 2) * waveAmplitude;

            // Apply vertical movement
            Vector3 newPos = cube.originalPosition;
            newPos.y = height;
            cube.transform.position = newPos;

            // Rotation animation
            float rotX = Mathf.Sin(globalTime * 1.5f + cube.gridPosition.x * 0.3f) * rotationAmount;
            float rotZ = Mathf.Cos(globalTime * 1.3f + cube.gridPosition.y * 0.3f) * rotationAmount;
            cube.transform.localRotation = Quaternion.Euler(rotX, globalTime * 50f, rotZ);

            // Scale pulsing
            float scalePulse = 0.7f + Mathf.Sin(globalTime * pulseSpeed * 2f + distanceFromCenter * 10f) * 0.3f;
            float finalScale = cubeScale * scalePulse;
            cube.transform.localScale = Vector3.one * finalScale;
        }
    }

    void UpdateColors()
    {
        if (colorGradient == null || cubes.Count == 0) return;

        foreach (GridCube cube in cubes)
        {
            // Dynamic color based on position, height, and time
            float heightNorm = Mathf.Clamp01((cube.transform.position.y + waveAmplitude) / (waveAmplitude * 2));
            float xNorm = cube.gridPosition.x / gridSize.x;
            float zNorm = cube.gridPosition.y / gridSize.y;

            // Color oscillates with time and position
            float colorPhase = globalTime * 0.8f + xNorm * 3f + zNorm * 3f + heightNorm * 5f;
            float colorValue = (Mathf.Sin(colorPhase) + 1) / 2;

            Color cubeColor = colorGradient.Evaluate(colorValue);

            // Enhance colors for cinematic look
            cubeColor = new Color(
                cubeColor.r * (0.8f + heightNorm * 0.5f),
                cubeColor.g * (0.8f + heightNorm * 0.5f),
                cubeColor.b * (1f + heightNorm * 0.8f),
                1f
            );

            if (cube.renderer != null)
            {
                propertyBlock.SetColor("_BaseColor", cubeColor);
                propertyBlock.SetColor("_Color", cubeColor);

                if (addGlowEffect)
                {
                    Color emissionColor = cubeColor * (colorIntensity + heightNorm);
                    propertyBlock.SetColor("_EmissionColor", emissionColor);
                }

                cube.renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize grid in editor
        Gizmos.color = Color.cyan;
        Vector3 startPos = new Vector3(-(gridSize.x * spacing) / 2, 0, -(gridSize.y * spacing) / 2);

        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = startPos + new Vector3(x * spacing, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSize.y * spacing);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= gridSize.y; z++)
        {
            Vector3 start = startPos + new Vector3(0, 0, z * spacing);
            Vector3 end = start + new Vector3(gridSize.x * spacing, 0, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}
