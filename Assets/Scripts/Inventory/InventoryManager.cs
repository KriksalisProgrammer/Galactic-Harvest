// Добавьте эти методы и события к вашему InventoryManager

using System.Collections.Generic;
using UnityEngine;

public partial class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int inventorySize = 24;
    public int hotbarSize = 8;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private List<InventorySlot> hotbarSlots = new List<InventorySlot>();

    // Events
    public System.Action<int, InventorySlot> OnInventoryChanged;
    public System.Action<int, InventorySlot> OnHotbarChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        // Initialize inventory slots
        inventorySlots.Clear();
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }

        // Initialize hotbar slots
        hotbarSlots.Clear();
        for (int i = 0; i < hotbarSize; i++)
        {
            hotbarSlots.Add(new InventorySlot());
        }
    }

    // Hotbar methods
    public InventorySlot GetHotbarSlot(int index)
    {
        if (index >= 0 && index < hotbarSlots.Count)
        {
            return hotbarSlots[index];
        }
        return null;
    }

    public InventorySlot GetInventorySlot(int index)
    {
        if (index >= 0 && index < inventorySlots.Count)
        {
            return inventorySlots[index];
        }
        return null;
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        int remaining = quantity;

        // Try to add to existing stacks first
        for (int i = 0; i < inventorySlots.Count && remaining > 0; i++)
        {
            if (inventorySlots[i].CanAddItem(item))
            {
                remaining = inventorySlots[i].AddItem(item, remaining);
                OnInventoryChanged?.Invoke(i, inventorySlots[i]);
            }
        }

        // If couldn't fit all items
        return remaining == 0;
    }

    public bool AddToHotbar(Item item, int quantity = 1, int slotIndex = -1)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            // Add to specific slot
            if (hotbarSlots[slotIndex].CanAddItem(item))
            {
                int remaining = hotbarSlots[slotIndex].AddItem(item, quantity);
                OnHotbarChanged?.Invoke(slotIndex, hotbarSlots[slotIndex]);
                return remaining == 0;
            }
            return false;
        }
        else
        {
            // Find first available slot
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (hotbarSlots[i].CanAddItem(item))
                {
                    int remaining = hotbarSlots[i].AddItem(item, quantity);
                    OnHotbarChanged?.Invoke(i, hotbarSlots[i]);
                    return remaining == 0;
                }
            }
            return false;
        }
    }

    public void RemoveFromHotbar(int slotIndex, int quantity = 1)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            hotbarSlots[slotIndex].RemoveItem(quantity);
            OnHotbarChanged?.Invoke(slotIndex, hotbarSlots[slotIndex]);
        }
    }

    public bool UseHotbarItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSlots.Count)
            return false;

        InventorySlot slot = hotbarSlots[slotIndex];
        if (slot == null || slot.IsEmpty() || slot.item == null)
            return false;

        // Check if item can be used
        if (!slot.item.CanUse())
            return false;

        // Use the item
        slot.item.Use();

        // Remove one item if it's consumable
        if (slot.item.isConsumable)
        {
            RemoveFromHotbar(slotIndex, 1);
        }

        return true;
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        int remaining = quantity;

        // Try to remove from hotbar first
        for (int i = 0; i < hotbarSlots.Count && remaining > 0; i++)
        {
            if (hotbarSlots[i].item == item && hotbarSlots[i].quantity > 0)
            {
                int removeAmount = Mathf.Min(remaining, hotbarSlots[i].quantity);
                hotbarSlots[i].RemoveItem(removeAmount);
                remaining -= removeAmount;
                OnHotbarChanged?.Invoke(i, hotbarSlots[i]);
            }
        }

        // Then try to remove from main inventory
        for (int i = 0; i < inventorySlots.Count && remaining > 0; i++)
        {
            if (inventorySlots[i].item == item && inventorySlots[i].quantity > 0)
            {
                int removeAmount = Mathf.Min(remaining, inventorySlots[i].quantity);
                inventorySlots[i].RemoveItem(removeAmount);
                remaining -= removeAmount;
                OnInventoryChanged?.Invoke(i, inventorySlots[i]);
            }
        }

        return remaining == 0;
    }

    public void RemoveFromInventory(int slotIndex, int quantity = 1)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            inventorySlots[slotIndex].RemoveItem(quantity);
            OnInventoryChanged?.Invoke(slotIndex, inventorySlots[slotIndex]);
        }
    }

    public void MoveItem(int fromIndex, int toIndex, bool fromHotbar, bool toHotbar)
    {
        InventorySlot fromSlot = fromHotbar ? GetHotbarSlot(fromIndex) : GetInventorySlot(fromIndex);
        InventorySlot toSlot = toHotbar ? GetHotbarSlot(toIndex) : GetInventorySlot(toIndex);

        if (fromSlot == null || toSlot == null) return;

        // Swap items
        Item tempItem = toSlot.item;
        int tempQuantity = toSlot.quantity;

        toSlot.item = fromSlot.item;
        toSlot.quantity = fromSlot.quantity;

        fromSlot.item = tempItem;
        fromSlot.quantity = tempQuantity;

        // Trigger events
        if (fromHotbar)
            OnHotbarChanged?.Invoke(fromIndex, fromSlot);
        else
            OnInventoryChanged?.Invoke(fromIndex, fromSlot);

        if (toHotbar)
            OnHotbarChanged?.Invoke(toIndex, toSlot);
        else
            OnInventoryChanged?.Invoke(toIndex, toSlot);
    }

    public bool HasItem(Item item, int quantity = 1)
    {
        int totalCount = 0;

        // Check inventory
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item)
            {
                totalCount += slot.quantity;
            }
        }

        // Check hotbar
        foreach (var slot in hotbarSlots)
        {
            if (slot.item == item)
            {
                totalCount += slot.quantity;
            }
        }

        return totalCount >= quantity;
    }

    public void ClearSlot(int slotIndex, bool isHotbar)
    {
        if (isHotbar && slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            hotbarSlots[slotIndex].Clear();
            OnHotbarChanged?.Invoke(slotIndex, hotbarSlots[slotIndex]);
        }
        else if (!isHotbar && slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            inventorySlots[slotIndex].Clear();
            OnInventoryChanged?.Invoke(slotIndex, inventorySlots[slotIndex]);
        }
    }
}