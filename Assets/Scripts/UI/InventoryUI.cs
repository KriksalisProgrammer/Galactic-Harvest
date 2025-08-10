using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform inventorySlotsParent;
    public GameObject inventorySlotPrefab;
    public Button closeButton;

    [Header("Settings")]
    public int inventorySize = 24;
    public KeyCode toggleKey = KeyCode.Tab;

    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    private bool isInventoryOpen = false;

    public System.Action<bool> OnInventoryToggled;

    private void Start()
    {
        Initialize();

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInventory);
        }

        // Subscribe to inventory changes
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateSlot;
        }

        // Start with inventory closed
        SetInventoryState(false);
    }

    private void Update()
    {
        // Handle Tab key for toggling
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        // Handle Escape key for closing only
        if (Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
        {
            CloseInventory();
        }
    }

    public void Initialize()
    {
        // Check if required references are assigned
        if (inventorySlotsParent == null)
        {
            Debug.LogError("InventoryUI: inventorySlotsParent is not assigned! Please assign it in the inspector.");
            return;
        }

        if (inventorySlotPrefab == null)
        {
            Debug.LogError("InventoryUI: inventorySlotPrefab is not assigned! Please assign it in the inspector.");
            return;
        }

        // Clear existing slots
        foreach (Transform child in inventorySlotsParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        inventorySlots.Clear();

        // Create inventory slots
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventorySlotsParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.OnSlotClicked += OnSlotClicked;
                slotUI.OnItemMoved += OnItemMoved;
                inventorySlots.Add(slotUI);

                // Initialize with empty slot
                InventorySlot emptySlot = new InventorySlot();
                slotUI.SetSlot(emptySlot, i, false);
            }
            else
            {
                Debug.LogError("InventoryUI: InventorySlotUI component not found on inventory slot prefab!");
            }
        }
    }

    public void ToggleInventory()
    {
        SetInventoryState(!isInventoryOpen);
    }

    public void OpenInventory()
    {
        SetInventoryState(true);
    }

    public void CloseInventory()
    {
        SetInventoryState(false);
    }

    private void SetInventoryState(bool open)
    {
        isInventoryOpen = open;
        inventoryPanel.SetActive(open);

        // Don't manage cursor here - let PlayerController handle it
        OnInventoryToggled?.Invoke(open);

        if (open)
        {
            RefreshAllSlots();
        }
    }

    public void UpdateSlot(int index, InventorySlot slot)
    {
        if (index >= 0 && index < inventorySlots.Count)
        {
            inventorySlots[index].SetSlot(slot, index, false);
        }
    }

    private void OnSlotClicked(int slotIndex, bool isHotbarSlot)
    {
        // Handle inventory slot clicks if needed
        Debug.Log($"Inventory slot {slotIndex} clicked");
    }

    private void OnItemMoved(int fromIndex, int toIndex, bool fromHotbar, bool toHotbar)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.MoveItem(fromIndex, toIndex, fromHotbar, toHotbar);
        }
    }

    // Public method to refresh all slots
    public void RefreshAllSlots()
    {
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                InventorySlot slot = InventoryManager.Instance.GetInventorySlot(i);
                if (slot != null)
                {
                    UpdateSlot(i, slot);
                }
            }
        }
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateSlot;
        }
    }
}