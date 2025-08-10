using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform hotbarSlotsParent;
    public GameObject hotbarSlotPrefab;

    [Header("Settings")]
    public int hotbarSize = 8;

    private List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    private int selectedSlotIndex = 0;
    private HotbarManager hotbarManager;

    private void Start()
    {
        hotbarManager = FindObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged += OnActiveSlotChanged;
        }

        InitializeHotbar();

        // Subscribe to inventory changes
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged += UpdateSlot;
        }
    }

    private void InitializeHotbar()
    {
        // Check if required references are assigned
        if (hotbarSlotsParent == null)
        {
            Debug.LogError("HotbarUI: hotbarSlotsParent is not assigned! Please assign it in the inspector.");
            return;
        }

        if (hotbarSlotPrefab == null)
        {
            Debug.LogError("HotbarUI: hotbarSlotPrefab is not assigned! Please assign it in the inspector.");
            return;
        }

        // Clear existing slots
        foreach (Transform child in hotbarSlotsParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        hotbarSlots.Clear();

        // Create hotbar slots
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarSlotsParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.OnSlotClicked += OnSlotClicked;
                slotUI.OnItemMoved += OnItemMoved;
                hotbarSlots.Add(slotUI);

                // Initialize with empty slot
                InventorySlot emptySlot = new InventorySlot();
                slotUI.SetSlot(emptySlot, i, true);
            }
            else
            {
                Debug.LogError("HotbarUI: InventorySlotUI component not found on hotbar slot prefab!");
            }
        }

        // Set first slot as selected
        if (hotbarSlots.Count > 0)
        {
            hotbarSlots[0].SetSelected(true);
        }
    }

    public void UpdateSlot(int index, InventorySlot slot)
    {
        if (index >= 0 && index < hotbarSlots.Count)
        {
            hotbarSlots[index].SetSlot(slot, index, true);
        }
    }

    private void OnActiveSlotChanged(int newIndex)
    {
        // Deselect old slot
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetSelected(false);
        }

        // Select new slot
        selectedSlotIndex = newIndex;
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetSelected(true);
        }
    }

    private void OnSlotClicked(int slotIndex, bool isHotbarSlot)
    {
        if (isHotbarSlot && hotbarManager != null)
        {
            hotbarManager.SetActiveSlot(slotIndex);
        }
    }

    private void OnItemMoved(int fromIndex, int toIndex, bool fromHotbar, bool toHotbar)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.MoveItem(fromIndex, toIndex, fromHotbar, toHotbar);
        }
    }

    private void OnDestroy()
    {
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged -= OnActiveSlotChanged;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged -= UpdateSlot;
        }
    }

    // Public method to refresh all slots (useful for initialization)
    public void RefreshAllSlots()
    {
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < hotbarSize; i++)
            {
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(i);
                if (slot != null)
                {
                    UpdateSlot(i, slot);
                }
            }
        }
    }
}