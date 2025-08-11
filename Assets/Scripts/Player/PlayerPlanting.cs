using UnityEngine;

public class PlayerPlanting : MonoBehaviour
{
    [Header("Planting Settings")]
    public LayerMask groundLayer = 1;
    public float plantingRange = 5f;
    public float plantingCooldown = 0.5f;

    [Header("Preview")]
    public GameObject currentPreview;
    public Material previewValidMaterial;
    public Material previewInvalidMaterial;

    private Camera playerCamera;
    private PlantSeed currentSeed;
    private bool isPlantingMode = false;
    private float lastPlantTime = 0f;
    private HotbarUI hotbarUI;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        hotbarUI = FindObjectOfType<HotbarUI>();

        if (hotbarUI != null)
        {
            hotbarUI.OnSlotSelected += OnHotbarSlotSelected;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdatePreview();
    }

    private void HandleInput()
    {
        // Right click to plant
        if (Input.GetMouseButtonDown(1) && isPlantingMode && currentSeed != null)
        {
            TryPlantSeed(currentSeed);
        }

        // Escape to cancel planting mode
        if (Input.GetKeyDown(KeyCode.Escape) && isPlantingMode)
        {
            ExitPlantingMode();
        }
    }

    private void OnHotbarSlotSelected(int slotIndex)
    {
        if (InventoryManager.Instance == null) return;

        InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(slotIndex);

        if (slot != null && !slot.IsEmpty() && slot.item is PlantSeed seed)
        {
            EnterPlantingMode(seed);
        }
        else
        {
            ExitPlantingMode();
        }
    }

    private void EnterPlantingMode(PlantSeed seed)
    {
        currentSeed = seed;
        isPlantingMode = true;

        // Create preview object
        if (seed.previewPrefab != null)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }

            currentPreview = Instantiate(seed.previewPrefab);

            // Make preview semi-transparent
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 0.6f;
                        material.color = color;
                    }
                }
            }
        }

        Debug.Log($"Entered planting mode with {seed.itemName}");
    }

    public void ForceExitPlantingMode()
    {
        ExitPlantingMode();
    }

    private void ExitPlantingMode()
    {
        isPlantingMode = false;
        currentSeed = null;

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        Debug.Log("Exited planting mode");
    }

    private void UpdatePreview()
    {
        if (!isPlantingMode || currentPreview == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, plantingRange, groundLayer))
        {
            Vector3 plantPosition = hit.point;
            currentPreview.transform.position = plantPosition;
            currentPreview.SetActive(true);

            // Check if position is valid for planting
            bool canPlant = CanPlantAt(plantPosition);
            SetPreviewMaterial(canPlant);
        }
        else
        {
            currentPreview.SetActive(false);
        }
    }

    private bool CanPlantAt(Vector3 position)
    {
        if (currentSeed == null) return false;

        // Check for overlapping plants
        Collider[] overlapping = Physics.OverlapSphere(position, currentSeed.minPlantingDistance);
        foreach (var collider in overlapping)
        {
            if (collider.GetComponent<PlantedPlant>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private void SetPreviewMaterial(bool canPlant)
    {
        if (currentPreview == null) return;

        Material materialToUse = canPlant ? previewValidMaterial : previewInvalidMaterial;
        if (materialToUse == null) return;

        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialToUse;
            }
            renderer.materials = materials;
        }
    }

    public bool TryPlantSeed(PlantSeed seed)
    {
        if (Time.time - lastPlantTime < plantingCooldown) return false;
        if (seed == null || seed.plantData == null || seed.plantPrefab == null) return false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, plantingRange, seed.validGroundLayers))
        {
            Vector3 plantPosition = hit.point;

            if (CanPlantAt(plantPosition))
            {
                // Create the planted plant
                GameObject plantObj = Instantiate(seed.plantPrefab, plantPosition, Quaternion.identity);
                PlantedPlant plantedPlant = plantObj.GetComponent<PlantedPlant>();

                if (plantedPlant == null)
                {
                    plantedPlant = plantObj.AddComponent<PlantedPlant>();
                }

                // Initialize the plant with data
                plantedPlant.InitializePlant(seed.plantData);

                // Subscribe to plant events if needed
                plantedPlant.OnPlantFullyGrown += OnPlantFullyGrown;
                plantedPlant.OnPlantHarvested += OnPlantHarvested;

                // Remove seed from inventory
                if (hotbarUI != null)
                {
                    int selectedSlot = hotbarUI.GetSelectedSlotIndex();
                    InventoryManager.Instance?.RemoveFromHotbar(selectedSlot, 1);
                }

                lastPlantTime = Time.time;

                Debug.Log($"Planted {seed.itemName} at {plantPosition}");
                return true;
            }
            else
            {
                Debug.Log("Cannot plant here - too close to another plant or invalid location");
            }
        }
        else
        {
            Debug.Log("Cannot plant - no valid ground found");
        }

        return false;
    }

    public bool IsInPlantingMode()
    {
        return isPlantingMode;
    }

    public PlantSeed GetCurrentSeed()
    {
        return currentSeed;
    }

    private void OnPlantFullyGrown(PlantedPlant plant)
    {
        Debug.Log($"Plant {plant.plantData.plantName} is fully grown!");
        // «десь можно добавить дополнительную логику
    }

    private void OnPlantHarvested(PlantedPlant plant)
    {
        Debug.Log($"Plant {plant.plantData.plantName} was harvested!");
        // «десь можно добавить дополнительную логику
    }

    private void OnDestroy()
    {
        if (hotbarUI != null)
        {
            hotbarUI.OnSlotSelected -= OnHotbarSlotSelected;
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }

    // For debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, plantingRange);
    }
}