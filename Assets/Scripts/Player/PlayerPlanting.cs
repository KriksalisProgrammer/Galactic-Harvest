using UnityEngine;

public class PlayerPlanting : MonoBehaviour
{
    [Header("Planting Settings")]
    public float maxPlantDistance = 5f;
    public LayerMask groundLayer = 1;
    public LayerMask obstacleLayer = 0; // слой для проверки препятствий

    [Header("References")]
    public HotbarUI hotbarUI;
    public Transform previewParent;

    [Header("Visual Settings")]
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private GameObject currentPreview;
    private bool canPlantAtCurrentPosition = false;
    private Camera playerCamera;

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
    }

    private void Update()
    {
        HandlePlantPreview();
        HandlePlanting();
    }

    private void HandlePlantPreview()
    {
        PlantSeed plantSeed = hotbarUI.GetSelectedPlantSeed();

        if (plantSeed != null && plantSeed.previewPrefab != null)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxPlantDistance, groundLayer))
            {
                Vector3 plantPosition = hit.point;
                canPlantAtCurrentPosition = CanPlantAtPosition(plantPosition);

                // Создаем или обновляем превью
                if (currentPreview == null)
                {
                    currentPreview = Instantiate(plantSeed.previewPrefab, plantPosition, Quaternion.identity, previewParent);
                    SetupPreviewVisuals(currentPreview);
                }
                else
                {
                    currentPreview.transform.position = plantPosition;
                }

                // Обновляем материал превью в зависимости от возможности посадки
                UpdatePreviewMaterial(currentPreview, canPlantAtCurrentPosition);
            }
            else
            {
                DestroyPreview();
                canPlantAtCurrentPosition = false;
            }
        }
        else
        {
            DestroyPreview();
            canPlantAtCurrentPosition = false;
        }
    }

    private bool CanPlantAtPosition(Vector3 position)
    {
        // Проверяем, нет ли препятствий в радиусе посадки
        float checkRadius = 0.5f;
        Collider[] obstacles = Physics.OverlapSphere(position, checkRadius, obstacleLayer);

        return obstacles.Length == 0;
    }

    private void SetupPreviewVisuals(GameObject preview)
    {
        // Отключаем коллайдеры у превью
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;

        // Делаем превью полупрозрачным
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = 0.7f;
                    material.color = color;
                }
            }
        }
    }

    private void UpdatePreviewMaterial(GameObject preview, bool canPlant)
    {
        if (validPreviewMaterial == null || invalidPreviewMaterial == null)
            return;

        Material targetMaterial = canPlant ? validPreviewMaterial : invalidPreviewMaterial;

        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = targetMaterial;
        }
    }

    private void DestroyPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void HandlePlanting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlantSeed plantSeed = hotbarUI.GetSelectedPlantSeed();

            if (plantSeed != null && canPlantAtCurrentPosition)
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, maxPlantDistance, groundLayer))
                {
                    if (TryPlantSeed(plantSeed, hit.point))
                    {
                        // Убираем семя из хотбара после успешной посадки
                        // (опционально, зависит от игровой логики)
                        // hotbarUI.ClearSlot(hotbarUI.GetSelectedIndex());
                    }
                }
            }
            else if (plantSeed == null)
            {
                Debug.Log("Нет семян в выбранном слоте");
            }
            else if (!canPlantAtCurrentPosition)
            {
                Debug.Log("Невозможно посадить в этом месте");
            }
        }
    }

    public bool TryPlantSeed(PlantSeed seed, Vector3 position)
    {
        if (seed == null || seed.plantPrefab == null)
        {
            Debug.LogWarning("Семя или префаб растения отсутствует");
            return false;
        }

        if (!CanPlantAtPosition(position))
        {
            Debug.Log("В этом месте нельзя посадить растение");
            return false;
        }

        // Создаем растение
        GameObject planted = Instantiate(seed.plantPrefab, position, Quaternion.identity);
        planted.name = $"Plant_{seed.plantData.plantName}_{System.DateTime.Now.Ticks}";

        // Инициализируем компонент растения
        PlantedPlant plantedPlant = planted.GetComponent<PlantedPlant>();
        if (plantedPlant != null)
        {
            plantedPlant.Initialize(seed.plantData);
        }
        else
        {
            Debug.LogWarning($"PlantedPlant component not found on {planted.name}");
        }

        Debug.Log($"Посажено растение: {seed.plantData.plantName} в позиции {position}");
        return true;
    }

    private void OnDisable()
    {
        DestroyPreview();
    }
}