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
    private Vector3 lastValidPlantPosition;
    private bool lastPositionValid = false;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // ИСПРАВЛЕНО: Создаем материалы по умолчанию если не назначены
        if (previewMaterialGreen == null)
        {
            previewMaterialGreen = CreateDefaultMaterial(Color.green);
        }
        if (previewMaterialRed == null)
        {
            previewMaterialRed = CreateDefaultMaterial(Color.red);
        }
    }

    private Material CreateDefaultMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(color.r, color.g, color.b, 0.5f);
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
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

        // ИСПРАВЛЕНО: Проверяем что игрок не в UI режиме
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            Debug.Log("Cannot enter planting mode while inventory is open");
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
        Debug.Log($"Entered planting mode with {currentSeed.itemName}");
    }

    private void ExitPlantingMode()
    {
        isPlantingMode = false;
        currentSeed = null;
        DestroyPreview();
        Debug.Log("Exited planting mode");
    }

    private void CreatePlantingPreview()
    {
        if (currentSeed?.previewPrefab != null)
        {
            currentPreview = Instantiate(currentSeed.previewPrefab);
        }
        else
        {
            // ИСПРАВЛЕНО: Создаем более заметный preview
            currentPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentPreview.name = "PlantPreview";
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
            // ИСПРАВЛЕНО: Правильно настраиваем материалы preview
            SetupPreviewMaterials();
        }
    }

    private void SetupPreviewMaterials()
    {
        if (currentPreview == null) return;

        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMaterialGreen;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private void UpdatePlantingPreview()
    {
        if (currentPreview == null) return;

        Vector3 targetPosition;
        bool canPlace = GetPlantingPosition(out targetPosition);

        if (canPlace)
        {
            currentPreview.transform.position = targetPosition;
            lastValidPlantPosition = targetPosition;
            lastPositionValid = true;
        }
        else if (lastPositionValid)
        {
            // Показываем preview на последней валидной позиции
            currentPreview.transform.position = lastValidPlantPosition;
            canPlace = CanPlantAt(lastValidPlantPosition);
        }

        // ИСПРАВЛЕНО: Обновляем материал в зависимости от возможности размещения
        UpdatePreviewMaterial(canPlace);
    }

    private void UpdatePreviewMaterial(bool canPlace)
    {
        if (currentPreview == null) return;

        Material materialToUse = canPlace ? previewMaterialGreen : previewMaterialRed;
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = materialToUse;
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
            else
            {
                Debug.Log("Cannot plant here!");
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

        // ИСПРАВЛЕНО: Используем центр экрана для рейкаста
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * plantingRange, Color.red, 0.1f);

        if (Physics.Raycast(ray, out hit, plantingRange))
        {
            // ИСПРАВЛЕНО: Проверяем что это земля и корректируем позицию
            if ((groundLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                // Размещаем немного выше поверхности
                position = hit.point + Vector3.up * 0.01f;
                return true;
            }
        }

        return false;
    }

    private bool CanPlantAt(Vector3 position)
    {
        if (currentSeed == null) return false;

        // Check if position is on valid ground
        if (!IsValidGround(position))
        {
            Debug.Log("Invalid ground for planting");
            return false;
        }

        // Check minimum planting distance
        if (!CheckPlantingDistance(position))
        {
            Debug.Log("Too close to other plants");
            return false;
        }

        // Check for obstacles
        if (HasObstacles(position))
        {
            Debug.Log("Obstacles in the way");
            return false;
        }

        return true;
    }

    private bool IsValidGround(Vector3 position)
    {
        if (currentSeed == null) return false;

        // ИСПРАВЛЕНО: Проверяем землю с небольшим отступом
        Vector3 checkPosition = position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(checkPosition, Vector3.down, out hit, 1f, currentSeed.validGroundLayers))
        {
            return true;
        }

        // Если validGroundLayers не установлен, используем groundLayerMask
        if (Physics.Raycast(checkPosition, Vector3.down, out hit, 1f, groundLayerMask))
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
        // ИСПРАВЛЕНО: Проверяем препятствия в небольшом радиусе над землей
        Vector3 checkPosition = position + Vector3.up * 0.5f;
        Collider[] obstacles = Physics.OverlapSphere(checkPosition, 0.5f, obstacleLayerMask);

        // Исключаем землю из препятствий
        foreach (Collider obstacle in obstacles)
        {
            // Проверяем что это не земля
            if ((groundLayerMask.value & (1 << obstacle.gameObject.layer)) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private void PlantSeed(Vector3 position)
    {
        if (currentSeed == null || currentSeed.plantPrefab == null) return;

        // ИСПРАВЛЕНО: Правильно размещаем растение на поверхности с правильным поворотом
        Vector3 plantPosition = position;
        Quaternion plantRotation = Quaternion.identity;

        // Дополнительная проверка поверхности
        RaycastHit groundHit;
        if (Physics.Raycast(position + Vector3.up * 1f, Vector3.down, out groundHit, 2f, groundLayerMask))
        {
            plantPosition = groundHit.point;
            // Поворачиваем растение в соответствии с нормалью поверхности
            plantRotation = Quaternion.FromToRotation(Vector3.up, groundHit.normal);
        }

        // Instantiate the plant
        GameObject plantObject = Instantiate(currentSeed.plantPrefab, plantPosition, plantRotation);

        // ИСПРАВЛЕНО: Добавляем Collider если его нет
        if (plantObject.GetComponent<Collider>() == null)
        {
            BoxCollider plantCollider = plantObject.AddComponent<BoxCollider>();
            plantCollider.size = new Vector3(1f, 2f, 1f);
            plantCollider.center = Vector3.up;
            plantCollider.isTrigger = true;
        }

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

        Debug.Log($"Planted {currentSeed.plantData.plantName} at {plantPosition}");

        // ИСПРАВЛЕНО: Уведомляем InventoryManager об использовании семени
        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null)
        {
            HotbarManager hotbarManager = FindObjectOfType<HotbarManager>();
            if (hotbarManager != null)
            {
                int activeSlot = hotbarManager.GetActiveSlotIndex();
                InventorySlot slot = inventoryManager.GetHotbarSlot(activeSlot);
                if (slot != null && slot.item == currentSeed)
                {
                    slot.RemoveItem(1);
                    inventoryManager.OnHotbarChanged?.Invoke(activeSlot, slot);
                }
            }
        }

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

    // ИСПРАВЛЕНО: Принудительный выход из режима посадки
    public void ForceExitPlantingMode()
    {
        ExitPlantingMode();
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, plantingRange);

        if (isPlantingMode && lastPositionValid)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastValidPlantPosition, 0.5f);
        }
    }
}