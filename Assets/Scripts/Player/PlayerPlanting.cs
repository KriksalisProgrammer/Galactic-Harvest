using UnityEngine;

public class PlayerPlanting : MonoBehaviour
{
    [Header("Planting Settings")]
    public float plantingRange = 5f;
    public LayerMask groundLayerMask = 1;
    public LayerMask obstacleLayerMask = -1;

    [Header("Preview Settings")]
    public Material previewMaterialGreen;
    public Material previewMaterialRed;

    private Camera playerCamera;
    private GameObject currentPreview;
    private PlantSeed currentSeed;
    private bool isPlantingMode = false;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isPlantingMode && currentSeed != null)
        {
            UpdatePlantingPreview();
            HandlePlantingInput();
        }
    }

    public bool TryPlantSeed(PlantSeed seed)
    {
        if (seed == null || seed.plantData == null)
        {
            Debug.LogWarning("PlayerPlanting: Invalid seed data!");
            return false;
        }

        currentSeed = seed;
        EnterPlantingMode();
        return true;
    }

    private void EnterPlantingMode()
    {
        isPlantingMode = true;
        CreatePlantingPreview();
    }

    private void ExitPlantingMode()
    {
        isPlantingMode = false;
        currentSeed = null;
        DestroyPreview();
    }

    private void CreatePlantingPreview()
    {
        if (currentSeed?.previewPrefab != null)
        {
            currentPreview = Instantiate(currentSeed.previewPrefab);
        }
        else
        {
            // Create basic preview if no preview prefab is set
            currentPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentPreview.transform.localScale = new Vector3(1f, 0.1f, 1f);

            // Remove collider from preview
            Collider previewCollider = currentPreview.GetComponent<Collider>();
            if (previewCollider != null)
            {
                Destroy(previewCollider);
            }
        }

        if (currentPreview != null)
        {
            // Make preview semi-transparent and non-interactive
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = previewMaterialGreen;
                }
                renderer.materials = materials;
            }
        }
    }

    private void UpdatePlantingPreview()
    {
        if (currentPreview == null) return;

        Vector3 targetPosition;
        bool canPlace = GetPlantingPosition(out targetPosition);

        currentPreview.transform.position = targetPosition;

        // Update preview material based on whether we can place here
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        Material materialToUse = canPlace ? previewMaterialGreen : previewMaterialRed;

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialToUse;
            }
            renderer.materials = materials;
        }
    }

    private void HandlePlantingInput()
    {
        // Plant with left click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 plantPosition;
            if (GetPlantingPosition(out plantPosition) && CanPlantAt(plantPosition))
            {
                PlantSeed(plantPosition);
            }
        }

        // Cancel planting with right click or ESC
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPlantingMode();
        }
    }

    private bool GetPlantingPosition(out Vector3 position)
    {
        position = Vector3.zero;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, plantingRange, groundLayerMask))
        {
            position = hit.point;
            return true;
        }

        return false;
    }

    private bool CanPlantAt(Vector3 position)
    {
        if (currentSeed == null) return false;

        // Check if position is on valid ground
        if (!IsValidGround(position)) return false;

        // Check minimum planting distance
        if (!CheckPlantingDistance(position)) return false;

        // Check for obstacles
        if (HasObstacles(position)) return false;

        return true;
    }

    private bool IsValidGround(Vector3 position)
    {
        if (currentSeed == null) return false;

        // Cast ray downward to check ground type
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, out hit, 1f, currentSeed.validGroundLayers))
        {
            return true;
        }

        return false;
    }

    private bool CheckPlantingDistance(Vector3 position)
    {
        if (currentSeed == null) return true;

        // Find all planted plants in the area
        PlantedPlant[] nearbyPlants = FindObjectsOfType<PlantedPlant>();

        foreach (PlantedPlant plant in nearbyPlants)
        {
            float distance = Vector3.Distance(position, plant.transform.position);
            if (distance < currentSeed.minPlantingDistance)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasObstacles(Vector3 position)
    {
        // Check for obstacles in a small radius around the planting position
        Collider[] obstacles = Physics.OverlapSphere(position, 0.5f, obstacleLayerMask);
        return obstacles.Length > 0;
    }

    private void PlantSeed(Vector3 position)
    {
        if (currentSeed == null || currentSeed.plantPrefab == null) return;

        // Instantiate the plant
        GameObject plantObject = Instantiate(currentSeed.plantPrefab, position, Quaternion.identity);

        // Set up the planted plant component
        PlantedPlant plantedPlant = plantObject.GetComponent<PlantedPlant>();
        if (plantedPlant == null)
        {
            plantedPlant = plantObject.AddComponent<PlantedPlant>();
        }

        // Initialize the plant with data from the seed
        plantedPlant.InitializePlant(currentSeed.plantData);

        // Subscribe to plant events
        plantedPlant.OnPlantFullyGrown += OnPlantFullyGrown;
        plantedPlant.OnPlantHarvested += OnPlantHarvested;

        Debug.Log($"Planted {currentSeed.plantData.plantName} at {position}");

        // Remove seed from inventory (this will be handled by InventoryManager)
        // The InventoryManager should handle consuming the seed when this method is called

        ExitPlantingMode();
    }

    private void DestroyPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void OnPlantFullyGrown(PlantedPlant plant)
    {
        Debug.Log($"{plant.plantData.plantName} is ready to harvest!");
        // You can add visual/audio feedback here
    }

    private void OnPlantHarvested(PlantedPlant plant)
    {
        Debug.Log($"Harvested {plant.plantData.plantName}");
        // You can add harvesting rewards to inventory here
    }

    public bool IsInPlantingMode()
    {
        return isPlantingMode;
    }

    public PlantSeed GetCurrentSeed()
    {
        return currentSeed;
    }

    private void OnDisable()
    {
        ExitPlantingMode();
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, plantingRange);
    }
}