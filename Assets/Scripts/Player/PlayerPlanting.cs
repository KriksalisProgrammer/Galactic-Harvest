using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanting : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float plantingRadius = 1f;
    [SerializeField] private float maxRaycastDistance = 100f;
    [SerializeField] private Vector2 plantingAreaSize = new Vector2(1f, 1f);

    [Header("Визуализация зон")]
    [SerializeField] private bool showPlantingZones = true;
    [SerializeField] private bool showOtherPlantsZones = true;

    [Header("References")]
    [SerializeField] private HotbarController hotbarController;

    // Текущие настройки посадки (берутся из выбранного семени)
    private PlantSeed currentSeed;
    private GameObject currentPreviewPrefab;
    private GameObject currentPlantPrefab;
    private PlantData currentPlantData;

    private bool isPreviewActive = false;
    private GameObject currentPreview;
    private GameObject currentZoneIndicator;
    private List<GameObject> otherPlantsZones = new List<GameObject>();
    private Camera playerCamera;
    private Material[] originalMaterials;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("PlayerPlanting: Не найдена камера!");
        }

        // Находим HotbarController если не назначен
        if (hotbarController == null)
        {
            hotbarController = FindObjectOfType<HotbarController>();
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Проверяем выбранный предмет в хотбаре
        UpdateSelectedSeed();

        if (Input.GetKeyDown(KeyCode.E) && currentSeed != null)
        {
            if (!isPreviewActive)
            {
                Vector3 startPos = GetTargetPlantingPosition();
                if (startPos != Vector3.zero)
                {
                    ActivatePreview(startPos);
                }
                else
                {
                    Debug.Log("Не удалось найти позицию для посадки!");
                }
            }
            else
            {
                Vector3 targetPos = GetTargetPlantingPosition();

                if (targetPos != Vector3.zero && CanPlantHere(targetPos))
                {
                    PlantReal(targetPos);
                    DeactivatePreview();

                    // Удаляем семечко из инвентаря
                    ConsumeSeed();
                }
                else
                {
                    Debug.Log("Не можем посадить здесь!");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isPreviewActive)
        {
            DeactivatePreview();
        }

        if (isPreviewActive && currentPreview != null)
        {
            UpdatePreview();
        }
    }

    private void UpdateSelectedSeed()
    {
        if (hotbarController == null) return;

        Item selectedItem = hotbarController.GetSelectedItem();
        PlantSeed selectedSeed = selectedItem as PlantSeed;

        // Если выбрали другое семечко или убрали семечко
        if (selectedSeed != currentSeed)
        {
            // Если был активен предпросмотр - отключаем его
            if (isPreviewActive)
            {
                DeactivatePreview();
            }

            currentSeed = selectedSeed;

            if (currentSeed != null)
            {
                // Обновляем текущие настройки из выбранного семени
                currentPreviewPrefab = currentSeed.previewPrefab;
                currentPlantPrefab = currentSeed.plantPrefab;
                currentPlantData = currentSeed.plantData;

                Debug.Log($"Выбрано семечко: {currentSeed.itemName}");
            }
            else
            {
                // Очищаем настройки
                currentPreviewPrefab = null;
                currentPlantPrefab = null;
                currentPlantData = null;
            }
        }
    }

    public void SetSelectedSeed(PlantSeed seed)
    {
        currentSeed = seed;
        if (seed != null)
        {
            currentPreviewPrefab = seed.previewPrefab;
            currentPlantPrefab = seed.plantPrefab;
            currentPlantData = seed.plantData;

            Debug.Log($"Активирован режим посадки: {seed.GetDisplayName()}");
        }
    }

    public void ClearSelectedSeed()
    {
        if (isPreviewActive)
        {
            DeactivatePreview();
        }

        currentSeed = null;
        currentPreviewPrefab = null;
        currentPlantPrefab = null;
        currentPlantData = null;

        Debug.Log("Режим посадки отключен");
    }

    private void ConsumeSeed()
    {
        if (hotbarController == null || currentSeed == null) return;

        // Находим активный слот и удаляем предмет
        int activeSlot = hotbarController.GetComponent<HotbarManager>().GetActiveSlotIndex();
        hotbarController.RemoveItem(activeSlot);

        Debug.Log($"Использовано семечко: {currentSeed.itemName}");
    }

    private Material CreateZoneMaterial(bool canPlant)
    {
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;

        if (canPlant)
        {
            material.color = new Color(0, 1, 0, 0.3f);
        }
        else
        {
            material.color = new Color(1, 0, 0, 0.3f);
        }
        return material;
    }

    private GameObject CreateSquareZoneIndicator(Vector3 position, Vector2 size, Material material)
    {
        GameObject zoneIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zoneIndicator.name = "SquareZoneIndicator";
        zoneIndicator.transform.position = position + Vector3.up * 0.005f;
        zoneIndicator.transform.localScale = new Vector3(size.x, 0.01f, size.y);

        Collider collider = zoneIndicator.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = zoneIndicator.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }

        return zoneIndicator;
    }

    private void ShowOtherPlantsZones()
    {
        if (!showOtherPlantsZones) return;

        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");

        foreach (GameObject plant in plants)
        {
            if (plant == currentPreview) continue;

            Vector3 plantPosition = plant.transform.position;
            Material redMaterial = CreateZoneMaterial(false);
            GameObject zoneIndicator = CreateSquareZoneIndicator(plantPosition, plantingAreaSize, redMaterial);
            otherPlantsZones.Add(zoneIndicator);
        }
    }

    private void HideOtherPlantsZones()
    {
        foreach (GameObject zone in otherPlantsZones)
        {
            if (zone != null)
            {
                Destroy(zone);
            }
        }
        otherPlantsZones.Clear();
    }

    private void ActivatePreview(Vector3 startPos)
    {
        if (currentPreviewPrefab == null)
        {
            Debug.LogError("PlayerPlanting: Нет префаба предпросмотра для выбранного семени!");
            return;
        }

        isPreviewActive = true;
        currentPreview = Instantiate(currentPreviewPrefab, startPos, Quaternion.identity);
        currentPreview.transform.localScale = Vector3.one;

        if (showPlantingZones)
        {
            bool canPlant = CanPlantHere(startPos);
            Material zoneMaterial = CreateZoneMaterial(canPlant);
            currentZoneIndicator = CreateSquareZoneIndicator(startPos, plantingAreaSize, zoneMaterial);
        }

        ShowOtherPlantsZones();
        MakePreviewVisible();

        Collider[] previewColliders = currentPreview.GetComponentsInChildren<Collider>();
        foreach (Collider col in previewColliders)
        {
            col.enabled = false;
        }

        Debug.Log($"Активирован предпросмотр посадки: {currentSeed.itemName}");
    }

    private void DeactivatePreview()
    {
        isPreviewActive = false;

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        if (currentZoneIndicator != null)
        {
            Destroy(currentZoneIndicator);
            currentZoneIndicator = null;
        }

        HideOtherPlantsZones();
        originalMaterials = null;
    }

    private void MakePreviewVisible()
    {
        if (currentPreview == null) return;

        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material newMat = new Material(materials[i]);

                if (newMat.HasProperty("_Color"))
                {
                    Color color = newMat.color;
                    color.a = 0.6f;
                    newMat.color = color;
                }

                if (newMat.color.a < 0.1f)
                {
                    newMat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                    Color color = newMat.color;
                    color.a = 0.6f;
                    newMat.color = color;
                }

                materials[i] = newMat;
            }
            rend.materials = materials;
        }
    }

    private Vector3 GetTargetPlantingPosition()
    {
        if (playerCamera == null) return Vector3.zero;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayerMask))
        {
            return hit.point;
        }

        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private void UpdatePreview()
    {
        Vector3 position = GetTargetPlantingPosition();

        if (position == Vector3.zero) return;

        currentPreview.transform.position = position;

        if (currentZoneIndicator != null && showPlantingZones)
        {
            currentZoneIndicator.transform.position = position + Vector3.up * 0.005f;

            bool canPlant = CanPlantHere(position);

            Material newMaterial = CreateZoneMaterial(canPlant);
            Renderer renderer = currentZoneIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = newMaterial;
            }
        }

        bool canPlantHere = CanPlantHere(position);
        SetPreviewColor(canPlantHere ? Color.green : Color.red);
    }

    private void PlantReal(Vector3 position)
    {
        if (currentPlantPrefab == null || currentPlantData == null)
        {
            Debug.LogError("PlayerPlanting: Не назначены префаб растения или данные растения!");
            return;
        }

        string errorMessage;
        if (!currentPlantData.IsValid(out errorMessage))
        {
            Debug.LogError($"PlayerPlanting: Данные растения некорректны: {errorMessage}");
            return;
        }

        GameObject planted = Instantiate(currentPlantPrefab, position, Quaternion.identity);
        planted.transform.localScale = Vector3.one;

        if (!planted.CompareTag("Plant"))
        {
            planted.tag = "Plant";
        }

        PlantedPlant plantScript = planted.GetComponent<PlantedPlant>();
        if (plantScript != null)
        {
            plantScript.plantData = currentPlantData;
        }
        else
        {
            Debug.LogError("Planted prefab missing PlantedPlant component.");
        }

        Debug.Log($"Посажено растение: {currentPlantData.plantName}");
    }

    private void SetPreviewColor(Color color)
    {
        if (currentPreview == null) return;

        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].HasProperty("_Color"))
                {
                    color.a = 0.6f;
                    materials[i].color = color;
                }
            }

            rend.materials = materials;
        }
    }

    private bool CanPlantHere(Vector3 position)
    {
        Vector3 halfExtents = new Vector3(plantingAreaSize.x * 0.5f, 0.5f, plantingAreaSize.y * 0.5f);
        Collider[] colliders = Physics.OverlapBox(position, halfExtents, Quaternion.identity);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Plant") && col.gameObject != currentPreview && !col.name.Contains("ZoneIndicator"))
            {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (isPreviewActive && currentPreview != null)
        {
            Vector3 position = GetTargetPlantingPosition();
            if (position != Vector3.zero)
            {
                bool canPlant = CanPlantHere(position);
                Gizmos.color = canPlant ? Color.green : Color.red;
                Vector3 size = new Vector3(plantingAreaSize.x, 0.1f, plantingAreaSize.y);
                Gizmos.DrawWireCube(position, size);
                Gizmos.DrawCube(position, size * 0.1f);
            }
        }
    }
}