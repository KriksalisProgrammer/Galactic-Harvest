using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact();
    bool CanInteract();
}

public class InteractionSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayers = -1;

    [Header("UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;

    private Camera playerCamera;
    private IInteractable currentInteractable;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    private void CheckForInteractable()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        IInteractable newInteractable = null;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayers))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        if (newInteractable != currentInteractable)
        {
            currentInteractable = newInteractable;
            UpdateInteractionUI();
        }
    }

    private void UpdateInteractionUI()
    {
        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
                if (interactionText != null)
                {
                    interactionText.text = currentInteractable.GetInteractionPrompt();
                }
            }
        }
        else
        {
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, forward * interactionRange);
        }
    }
}


public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private int quantity = 1;

    public void SetItem(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public string GetInteractionPrompt()
    {
        return $"Нажмите E чтобы подобрать {item.itemName} x{quantity}";
    }

    public void Interact()
    {
        if (InventoryManager.Instance != null)
        {

            if (InventoryManager.Instance.AddItemToHotbar(item, quantity))
            {
                Destroy(gameObject);
                return;
            }

            if (InventoryManager.Instance.AddItem(item, quantity))
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Инвентарь полон!");
            }
        }
    }

    public bool CanInteract()
    {
        return item != null && quantity > 0;
    }
}

public class HarvestableePlant : MonoBehaviour, IInteractable
{
    private PlantedPlant plantedPlant;

    private void Start()
    {
        plantedPlant = GetComponent<PlantedPlant>();
    }

    public string GetInteractionPrompt()
    {
        if (plantedPlant != null && plantedPlant.CanHarvest())
        {
            return $"Нажмите E чтобы собрать {plantedPlant.GetPlantData().plantName}";
        }
        return "";
    }

    public void Interact()
    {
        if (plantedPlant != null && plantedPlant.CanHarvest())
        {
            plantedPlant.Harvest();
        }
    }

    public bool CanInteract()
    {
        return plantedPlant != null && plantedPlant.CanHarvest();
    }
}