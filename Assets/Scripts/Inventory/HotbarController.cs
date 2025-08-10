using UnityEngine;

public class HotbarController : MonoBehaviour
{
    [Header("Settings")]
    public int hotbarSize = 8;
    public KeyCode[] hotbarKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8
    };

    private int activeSlotIndex = 0;
    private HotbarUI hotbarUI;
    private InventoryManager inventoryManager;

    public System.Action<int> OnActiveSlotChanged;

    private void Start()
    {
        hotbarUI = FindObjectOfType<HotbarUI>();
        inventoryManager = InventoryManager.Instance;

        // Subscribe to hotbar manager events
        HotbarManager hotbarManager = FindObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged += SetActiveSlot;
        }
    }

    private void Update()
    {
        HandleHotbarInput();
    }

    private void HandleHotbarInput()
    {
        // Check number key presses
        for (int i = 0; i < Mathf.Min(hotbarSize, hotbarKeys.Length); i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i]))
            {
                SetActiveSlot(i);
                break;
            }
        }

        // Handle mouse wheel scrolling
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SetActiveSlot((activeSlotIndex + 1) % hotbarSize);
        }
        else if (scroll < 0f)
        {
            SetActiveSlot((activeSlotIndex - 1 + hotbarSize) % hotbarSize);
        }

        // Use active item on left click (if not in planting mode)
        if (Input.GetMouseButtonDown(0))
        {
            PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
            if (planting == null || !planting.IsInPlantingMode())
            {
                UseActiveItem();
            }
        }
    }

    public void SetActiveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return;

        activeSlotIndex = slotIndex;
        OnActiveSlotChanged?.Invoke(activeSlotIndex);

        // Update UI
        if (hotbarUI != null)
        {
            // The HotbarUI will handle visual updates through the HotbarManager
        }
    }

    public void UseActiveItem()
    {
        if (inventoryManager != null)
        {
            inventoryManager.UseHotbarItem(activeSlotIndex);
        }
    }

    public int GetActiveSlotIndex()
    {
        return activeSlotIndex;
    }

    public InventorySlot GetActiveSlot()
    {
        if (inventoryManager != null)
        {
            return inventoryManager.GetHotbarSlot(activeSlotIndex);
        }
        return null;
    }

    public Item GetActiveItem()
    {
        InventorySlot activeSlot = GetActiveSlot();
        return activeSlot?.item;
    }
}