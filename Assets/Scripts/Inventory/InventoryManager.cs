using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 24;
    [SerializeField] private int hotbarSize = 8;

    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private HotbarManager hotbarManager;

    private List<InventorySlot> inventory;
    private List<InventorySlot> hotbar;

    // Events for UI updates
    public System.Action<int, InventorySlot> OnInventoryChanged;
    public System.Action<int, InventorySlot> OnHotbarChanged;
    public System.Action OnInventoryInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Find UI components if not assigned
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();

        if (hotbarManager == null)
            hotbarManager = FindObjectOfType<HotbarManager>();

        // Initialize UI
        if (inventoryUI != null)
            inventoryUI.Initialize();

        // Setup UI event listeners
        SetupUIEventListeners();

        // Load saved inventory
        LoadInventory();

        // Notify that inventory is ready
        OnInventoryInitialized?.Invoke();
    }

    private void SetupUIEventListeners()
    {
        // Connect to hotbar UI
        HotbarUI hotbarUI = FindObjectOfType<HotbarUI>();
        if (hotbarUI != null)
        {
            OnHotbarChanged += hotbarUI.UpdateSlot;
        }

        // Refresh UI with current data
        RefreshAllUI();
    }

    private void InitializeInventory()
    {
        inventory = new List<InventorySlot>();
        hotbar = new List<InventorySlot>();

        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(new InventorySlot());
        }

        for (int i = 0; i < hotbarSize; i++)
        {
            hotbar.Add(new InventorySlot());
        }
    }

    private void RefreshAllUI()
    {
        // Refresh all inventory slots
        for (int i = 0; i < inventory.Count; i++)
        {
            OnInventoryChanged?.Invoke(i, inventory[i]);
        }

        // Refresh all hotbar slots
        for (int i = 0; i < hotbar.Count; i++)
        {
            OnHotbarChanged?.Invoke(i, hotbar[i]);
        }
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int remainingQuantity = quantity;

        // Try to stack with existing items first
        for (int i = 0; i < inventory.Count && remainingQuantity > 0; i++)
        {
            if (inventory[i].CanAddItem(item))
            {
                remainingQuantity = inventory[i].AddItem(item, remainingQuantity);
                OnInventoryChanged?.Invoke(i, inventory[i]);
            }
        }

        // Fill empty slots
        for (int i = 0; i < inventory.Count && remainingQuantity > 0; i++)
        {
            if (inventory[i].IsEmpty())
            {
                remainingQuantity = inventory[i].AddItem(item, remainingQuantity);
                OnInventoryChanged?.Invoke(i, inventory[i]);
            }
        }

        // Try hotbar if inventory is full
        if (remainingQuantity > 0)
        {
            remainingQuantity = TryAddToHotbar(item, remainingQuantity);
        }

        bool success = remainingQuantity < quantity;
        if (success)
        {
            SaveInventory();
            Debug.Log($"Added {quantity - remainingQuantity} {item.itemName} to inventory");
        }

        return remainingQuantity == 0;
    }

    private int TryAddToHotbar(Item item, int quantity)
    {
        int remainingQuantity = quantity;

        // Try to stack with existing items in hotbar
        for (int i = 0; i < hotbar.Count && remainingQuantity > 0; i++)
        {
            if (hotbar[i].CanAddItem(item))
            {
                remainingQuantity = hotbar[i].AddItem(item, remainingQuantity);
                OnHotbarChanged?.Invoke(i, hotbar[i]);
            }
        }

        // Fill empty hotbar slots
        for (int i = 0; i < hotbar.Count && remainingQuantity > 0; i++)
        {
            if (hotbar[i].IsEmpty())
            {
                remainingQuantity = hotbar[i].AddItem(item, remainingQuantity);
                OnHotbarChanged?.Invoke(i, hotbar[i]);
            }
        }

        return remainingQuantity;
    }

    public bool AddItemToHotbar(Item item, int quantity = 1, int preferredSlot = -1)
    {
        if (item == null || quantity <= 0) return false;

        // Try preferred slot first
        if (preferredSlot >= 0 && preferredSlot < hotbar.Count)
        {
            if (hotbar[preferredSlot].CanAddItem(item))
            {
                int remaining = hotbar[preferredSlot].AddItem(item, quantity);
                OnHotbarChanged?.Invoke(preferredSlot, hotbar[preferredSlot]);
                if (remaining == 0)
                {
                    SaveInventory();
                    return true;
                }
                quantity = remaining;
            }
        }

        // Try other slots
        int remainingQuantity = TryAddToHotbar(item, quantity);

        if (remainingQuantity < quantity)
        {
            SaveInventory();
        }

        return remainingQuantity == 0;
    }

    public void UseHotbarItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbar.Count) return;

        InventorySlot slot = hotbar[slotIndex];
        if (slot.IsEmpty()) return;

        Item item = slot.item;
        if (!item.CanUse()) return;

        Debug.Log($"Using {item.itemName}");

        // Handle different item types
        switch (item.itemType)
        {
            case ItemType.Seed:
                UseSeed(item as PlantSeed);
                break;
            case ItemType.Consumable:
                UseConsumable(item);
                break;
            case ItemType.Tool:
                UseTool(item);
                break;
            default:
                item.Use();
                break;
        }

        // Consume item if it's consumable
        if (item.isConsumable)
        {
            slot.RemoveItem(1);
            OnHotbarChanged?.Invoke(slotIndex, slot);
            SaveInventory();
        }
    }

    private void UseSeed(PlantSeed seed)
    {
        if (seed == null) return;

        PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
        if (planting != null)
        {
            planting.TryPlantSeed(seed);
        }
        else
        {
            Debug.LogWarning("PlayerPlanting component not found!");
        }
    }

    private void UseConsumable(Item consumable)
    {
        Debug.Log($"Consuming {consumable.itemName}");
        // Add consumable effects here
    }

    private void UseTool(Item tool)
    {
        Debug.Log($"Using tool {tool.itemName}");
        // Add tool functionality here
    }

    // ИСПРАВЛЕНО: Улучшенная система перемещения предметов
    public void MoveItem(int fromIndex, int toIndex, bool isFromHotbar = false, bool isToHotbar = false)
    {
        List<InventorySlot> fromList = isFromHotbar ? hotbar : inventory;
        List<InventorySlot> toList = isToHotbar ? hotbar : inventory;

        if (fromIndex < 0 || fromIndex >= fromList.Count ||
            toIndex < 0 || toIndex >= toList.Count) return;

        InventorySlot fromSlot = fromList[fromIndex];
        InventorySlot toSlot = toList[toIndex];

        // Если слоты одинаковые, ничего не делаем
        if (fromIndex == toIndex && isFromHotbar == isToHotbar) return;

        // Если в целевом слоте есть предмет того же типа и он может стакаться
        if (!toSlot.IsEmpty() && fromSlot.item == toSlot.item && toSlot.item.isStackable)
        {
            int spaceAvailable = toSlot.item.maxStackSize - toSlot.quantity;
            int amountToMove = Mathf.Min(fromSlot.quantity, spaceAvailable);

            if (amountToMove > 0)
            {
                toSlot.quantity += amountToMove;
                fromSlot.RemoveItem(amountToMove);
            }
        }
        else
        {
            // Полная замена - меняем местами
            Item tempItem = fromSlot.item;
            int tempQuantity = fromSlot.quantity;

            fromSlot.item = toSlot.item;
            fromSlot.quantity = toSlot.quantity;

            toSlot.item = tempItem;
            toSlot.quantity = tempQuantity;
        }

        // Notify UI of changes
        if (isFromHotbar)
            OnHotbarChanged?.Invoke(fromIndex, fromSlot);
        else
            OnInventoryChanged?.Invoke(fromIndex, fromSlot);

        if (isToHotbar)
            OnHotbarChanged?.Invoke(toIndex, toSlot);
        else
            OnInventoryChanged?.Invoke(toIndex, toSlot);

        SaveInventory();
        Debug.Log($"Moved item from slot {fromIndex} to slot {toIndex}");
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int remainingToRemove = quantity;

        // Remove from inventory first
        for (int i = 0; i < inventory.Count && remainingToRemove > 0; i++)
        {
            if (inventory[i].item == item)
            {
                int removeAmount = Mathf.Min(remainingToRemove, inventory[i].quantity);
                inventory[i].RemoveItem(removeAmount);
                remainingToRemove -= removeAmount;
                OnInventoryChanged?.Invoke(i, inventory[i]);
            }
        }

        // Remove from hotbar if needed
        for (int i = 0; i < hotbar.Count && remainingToRemove > 0; i++)
        {
            if (hotbar[i].item == item)
            {
                int removeAmount = Mathf.Min(remainingToRemove, hotbar[i].quantity);
                hotbar[i].RemoveItem(removeAmount);
                remainingToRemove -= removeAmount;
                OnHotbarChanged?.Invoke(i, hotbar[i]);
            }
        }

        bool success = remainingToRemove < quantity;
        if (success)
        {
            SaveInventory();
        }

        return remainingToRemove == 0;
    }

    public int GetItemCount(Item item)
    {
        if (item == null) return 0;

        int count = 0;

        // Count in inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == item)
            {
                count += inventory[i].quantity;
            }
        }

        // Count in hotbar
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i].item == item)
            {
                count += hotbar[i].quantity;
            }
        }

        return count;
    }

    public InventorySlot GetInventorySlot(int index)
    {
        if (index < 0 || index >= inventory.Count) return null;
        return inventory[index];
    }

    public InventorySlot GetHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbar.Count) return null;
        return hotbar[index];
    }

    // ИСПРАВЛЕНО: Добавлен метод для прямой установки слота
    public void SetHotbarSlot(int index, Item item, int quantity)
    {
        if (index < 0 || index >= hotbar.Count) return;

        hotbar[index].item = item;
        hotbar[index].quantity = quantity;
        OnHotbarChanged?.Invoke(index, hotbar[index]);
        SaveInventory();
    }

    public void SetInventorySlot(int index, Item item, int quantity)
    {
        if (index < 0 || index >= inventory.Count) return;

        inventory[index].item = item;
        inventory[index].quantity = quantity;
        OnInventoryChanged?.Invoke(index, inventory[index]);
        SaveInventory();
    }

    private void SaveInventory()
    {
        // Use PlayerPrefs for simple save system
        SaveInventoryToPlayerPrefs();
    }

    private void SaveInventoryToPlayerPrefs()
    {
        // Save inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            InventorySlot slot = inventory[i];
            if (slot.item != null)
            {
                PlayerPrefs.SetString($"Inventory_{i}_Item", slot.item.name);
                PlayerPrefs.SetInt($"Inventory_{i}_Quantity", slot.quantity);
            }
            else
            {
                PlayerPrefs.DeleteKey($"Inventory_{i}_Item");
                PlayerPrefs.DeleteKey($"Inventory_{i}_Quantity");
            }
        }

        // Save hotbar
        for (int i = 0; i < hotbar.Count; i++)
        {
            InventorySlot slot = hotbar[i];
            if (slot.item != null)
            {
                PlayerPrefs.SetString($"Hotbar_{i}_Item", slot.item.name);
                PlayerPrefs.SetInt($"Hotbar_{i}_Quantity", slot.quantity);
            }
            else
            {
                PlayerPrefs.DeleteKey($"Hotbar_{i}_Item");
                PlayerPrefs.DeleteKey($"Hotbar_{i}_Quantity");
            }
        }

        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        LoadInventoryFromPlayerPrefs();
    }

    private void LoadInventoryFromPlayerPrefs()
    {
        // Load inventory
        for (int i = 0; i < inventory.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Inventory_{i}_Item"))
            {
                string itemName = PlayerPrefs.GetString($"Inventory_{i}_Item");
                int quantity = PlayerPrefs.GetInt($"Inventory_{i}_Quantity");

                Item item = LoadItemByName(itemName);
                if (item != null)
                {
                    inventory[i] = new InventorySlot(item, quantity);
                }
            }
        }

        // Load hotbar
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Hotbar_{i}_Item"))
            {
                string itemName = PlayerPrefs.GetString($"Hotbar_{i}_Item");
                int quantity = PlayerPrefs.GetInt($"Hotbar_{i}_Quantity");

                Item item = LoadItemByName(itemName);
                if (item != null)
                {
                    hotbar[i] = new InventorySlot(item, quantity);
                }
            }
        }
    }

    private Item LoadItemByName(string itemName)
    {
        // Try different resource paths
        Item item = Resources.Load<Item>($"ScriptableObjects/{itemName}");
        if (item == null)
        {
            item = Resources.Load<Item>($"ScriptableObjects/Plant/{itemName}");
        }
        if (item == null)
        {
            item = Resources.Load<Item>($"Items/{itemName}");
        }
        if (item == null)
        {
            Debug.LogWarning($"Could not find item: {itemName}");
        }
        return item;
    }

    public void UseActiveHotbarItem()
    {
        HotbarController hotbarController = FindObjectOfType<HotbarController>();
        if (hotbarController != null)
        {
            int activeSlot = hotbarController.GetActiveSlotIndex();
            UseHotbarItem(activeSlot);
        }
        else
        {
            Debug.LogWarning("HotbarController not found! Cannot use active hotbar item.");
        }
    }

    // Auto-save on important events
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveInventory();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveInventory();
    }

    private void OnDestroy()
    {
        SaveInventory();
    }
}