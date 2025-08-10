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
        CheckForInteractables();
        HandleInteractionInput();
        UpdateInteractionUI();
    }

    private void CheckForInteractables()
    {
        // Raycast from camera to check for interactables
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayers))
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
        if (plant != null && plant.CanHarvest())
        {
            SetInteractable(plant, $"Harvest {plant.plantData.plantName}");
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
                plant.Harvest();
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
            interactionText.text = $"Press {interactionKey} to {prompt}";
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