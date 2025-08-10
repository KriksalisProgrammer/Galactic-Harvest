using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI interactionText;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask interactableLayers = -1;

    private Dictionary<object, string> currentInteractables = new Dictionary<object, string>();
    private Camera playerCamera;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // ИСПРАВЛЕНО: Не проверяем взаимодействия если в режиме посадки или открыт инвентарь
        PlayerPlanting planting = GetComponent<PlayerPlanting>();
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();

        if ((planting != null && planting.IsInPlantingMode()) ||
            (inventoryUI != null && inventoryUI.IsInventoryOpen()))
        {
            currentInteractables.Clear();
            UpdateInteractionUI();
            return;
        }

        CheckForInteractables();
        HandleInteractionInput();
        UpdateInteractionUI();
    }

    private void CheckForInteractables()
    {
        // Clear previous interactables
        currentInteractables.Clear();

        // Raycast from camera to check for interactables
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Check for different types of interactables
            CheckItemPickup(hit.collider);
            CheckPlantedPlant(hit.collider);
        }
    }

    private void CheckItemPickup(Collider collider)
    {
        ItemPickup pickup = collider.GetComponent<ItemPickup>();
        if (pickup != null && pickup.item != null)
        {
            SetInteractable(pickup, $"Pick up {pickup.item.itemName} ({pickup.quantity})");
        }
    }

    private void CheckPlantedPlant(Collider collider)
    {
        PlantedPlant plant = collider.GetComponent<PlantedPlant>();
        if (plant != null)
        {
            if (plant.CanHarvest())
            {
                SetInteractable(plant, $"Harvest {plant.plantData.plantName}");
            }
            else
            {
                // ИСПРАВЛЕНО: Показываем информацию о росте
                float progress = plant.GetGrowthProgress();
                string stageName = plant.GetCurrentStageName();
                SetInteractable(plant, $"{plant.plantData.plantName} - {stageName} ({progress * 100:F0}%)");
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractables.Count > 0)
        {
            InteractWithClosest();
        }
    }

    private void InteractWithClosest()
    {
        foreach (var interactable in currentInteractables.Keys)
        {
            if (interactable is ItemPickup pickup)
            {
                pickup.TryPickup(gameObject);
                break;
            }
            else if (interactable is PlantedPlant plant)
            {
                if (plant.CanHarvest())
                {
                    plant.Harvest();
                }
                else
                {
                    // ИСПРАВЛЕНО: Показываем информацию о росте при клике
                    Debug.Log($"{plant.plantData.plantName} - {plant.GetCurrentStageName()} - {plant.GetGrowthProgress() * 100:F1}% grown");
                }
                break;
            }
            else if (interactable is MonoBehaviour mb && mb != null)
            {
                // Try to call Interact method if it exists
                var interactMethod = mb.GetType().GetMethod("Interact");
                if (interactMethod != null)
                {
                    interactMethod.Invoke(mb, new object[] { gameObject });
                    break;
                }
            }
        }
    }

    private void UpdateInteractionUI()
    {
        if (interactionPrompt == null) return;

        bool hasInteractables = currentInteractables.Count > 0;
        interactionPrompt.SetActive(hasInteractables);

        if (hasInteractables && interactionText != null)
        {
            string prompt = "";
            foreach (var interaction in currentInteractables.Values)
            {
                prompt = interaction; // Just show the first one for now
                break;
            }

            // ИСПРАВЛЕНО: Разный текст для разных типов взаимодействий
            foreach (var interactable in currentInteractables.Keys)
            {
                if (interactable is PlantedPlant plant)
                {
                    if (plant.CanHarvest())
                    {
                        interactionText.text = $"Press {interactionKey} to {prompt}";
                    }
                    else
                    {
                        interactionText.text = prompt; // Только информация, без кнопки
                    }
                }
                else
                {
                    interactionText.text = $"Press {interactionKey} to {prompt}";
                }
                break;
            }
        }
    }

    public void SetInteractable(object interactable, string interactionPromptText)
    {
        if (!currentInteractables.ContainsKey(interactable))
        {
            currentInteractables[interactable] = interactionPromptText;
        }
    }

    public void ClearInteractable(object interactable)
    {
        if (currentInteractables.ContainsKey(interactable))
        {
            currentInteractables.Remove(interactable);
        }
    }

    public void ClearAllInteractables()
    {
        currentInteractables.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Gizmos.DrawRay(rayOrigin, rayDirection * interactionRange);
        }
    }
}