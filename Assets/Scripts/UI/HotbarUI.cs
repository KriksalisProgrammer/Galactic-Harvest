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
    private HotbarController hotbarController;

    private void Start()
    {
        hotbarController = FindObjectOfType<HotbarController>();

        InitializeHotbar();

        // ИСПРАВЛЕНО: Подписываемся на события HotbarController
        if (hotbarController != null)
        {
            hotbarController.OnActiveSlotChanged += OnActiveSlotChanged;
        }

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
        SetSelectedSlot(0);
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
        SetSelectedSlot(newIndex);
    }

    // ИСПРАВЛЕНО: Отдельный метод для установки выбранного слота
    private void SetSelectedSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= hotbarSlots.Count) return;

        // Deselect old slot
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetSelected(false);
        }

        // Select new slot
        selectedSlotIndex = newIndex;
        hotbarSlots[selectedSlotIndex].SetSelected(true);

        Debug.Log($"HotbarUI: Active slot changed to: {newIndex}");
    }

    private void OnSlotClicked(int slotIndex, bool isHotbarSlot)
    {
        if (isHotbarSlot && hotbarController != null)
        {
            hotbarController.SetActiveSlot(slotIndex);
        }
    }

    private void OnItemMoved(int fromIndex, int toIndex, bool fromHotbar, bool toHotbar)
    {
        if (InventoryManager.Instance != null)
        {
            Debug.Log($"HotbarUI: Moving item from {fromIndex} ({(fromHotbar ? "hotbar" : "inventory")}) to {toIndex} ({(toHotbar ? "hotbar" : "inventory")})");
            InventoryManager.Instance.MoveItem(fromIndex, toIndex, fromHotbar, toHotbar);
        }
    }

    private void OnDestroy()
    {
        if (hotbarController != null)
        {
            hotbarController.OnActiveSlotChanged -= OnActiveSlotChanged;
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

    // ИСПРАВЛЕНО: Метод для получения текущего выбранного слота
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }
}