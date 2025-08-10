using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanting : MonoBehaviour
{
    [Header("Planting Settings")]
    [SerializeField] private float plantingRange = 5f;
    [SerializeField] private LayerMask groundLayer = 1;

    [Header("Preview")]
    [SerializeField] private GameObject currentPreview;

    private Camera playerCamera;
    private PlantSeed currentSeed;

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
        HandlePlantingPreview();
    }

    private void HandlePlantingPreview()
    {
        if (InventoryManager.Instance != null)
        {
            HotbarManager hotbarManager = FindObjectOfType<HotbarManager>();
            if (hotbarManager != null)
            {
                int activeSlot = hotbarManager.GetActiveSlotIndex();
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(activeSlot);

                if (slot != null && !slot.IsEmpty() && slot.item is PlantSeed)
                {
                    PlantSeed seed = slot.item as PlantSeed;
                    ShowPlantingPreview(seed);
                }
                else
                {
                    HidePlantingPreview();
                }
            }
        }
    }

    private void ShowPlantingPreview(PlantSeed seed)
    {
        if (currentSeed != seed)
        {
            HidePlantingPreview();
            currentSeed = seed;

            if (seed.previewPrefab != null)
            {
                Vector3 plantPosition = GetPlantingPosition();
                if (plantPosition != Vector3.zero)
                {
                    currentPreview = Instantiate(seed.previewPrefab, plantPosition, Quaternion.identity);
                }
            }
        }
        else if (currentPreview != null)
        {
            Vector3 plantPosition = GetPlantingPosition();
            if (plantPosition != Vector3.zero)
            {
                currentPreview.transform.position = plantPosition;
            }
        }
    }

    private void HidePlantingPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        currentSeed = null;
    }

    public void TryPlantSeed(PlantSeed seed)
    {
        Vector3 plantPosition = GetPlantingPosition();
        if (plantPosition == Vector3.zero)
        {
            Debug.Log("Невозможно посадить здесь");
            return;
        }

        if (!CanPlantHere(plantPosition, seed))
        {
            Debug.Log("Слишком близко к другим растениям");
            return;
        }

        PlantSeedAt(seed, plantPosition);
    }

    private Vector3 GetPlantingPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, plantingRange, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private bool CanPlantHere(Vector3 position, PlantSeed seed)
    {
        Collider[] nearbyPlants = Physics.OverlapSphere(position, seed.minPlantingDistance);

        foreach (Collider col in nearbyPlants)
        {
            if (col.GetComponent<PlantedPlant>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private void PlantSeedAt(PlantSeed seed, Vector3 position)
    {
        if (seed.plantPrefab != null && seed.plantData != null)
        {
            GameObject plantObject = Instantiate(seed.plantPrefab, position, Quaternion.identity);

            PlantedPlant plantedPlant = plantObject.GetComponent<PlantedPlant>();
            if (plantedPlant == null)
            {
                plantedPlant = plantObject.AddComponent<PlantedPlant>();
            }
            plantedPlant.Initialize(seed.plantData);

            Debug.Log($"Посажено: {seed.plantData.plantName}");
        }
        else
        {
            Debug.LogError("Не настроены prefab или данные растения для семян: " + seed.itemName);
        }
    }

    private void OnDestroy()
    {
        HidePlantingPreview();
    }
}