using System.Collections;
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

    public System.Action<int, InventorySlot> OnInventoryChanged;
    public System.Action<int, InventorySlot> OnHotbarChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (inventoryUI != null)
            inventoryUI.Initialize();

        OnHotbarChanged += (index, slot) => FindObjectOfType<HotbarUI>()?.UpdateSlot(index, slot);
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

    public bool AddItem(Item item, int quantity = 1)
    {
        int remainingQuantity = quantity;

        for (int i = 0; i < inventory.Count && remainingQuantity > 0; i++)
        {
            if (inventory[i].CanAddItem(item))
            {
                remainingQuantity = inventory[i].AddItem(item, remainingQuantity);
                OnInventoryChanged?.Invoke(i, inventory[i]);
            }
        }

        for (int i = 0; i < inventory.Count && remainingQuantity > 0; i++)
        {
            if (inventory[i].IsEmpty())
            {
                remainingQuantity = inventory[i].AddItem(item, remainingQuantity);
                OnInventoryChanged?.Invoke(i, inventory[i]);
            }
        }

        SaveInventory();
        return remainingQuantity == 0;
    }

    public bool AddItemToHotbar(Item item, int quantity = 1, int preferredSlot = -1)
    {
        if (preferredSlot >= 0 && preferredSlot < hotbar.Count)
        {
            if (hotbar[preferredSlot].CanAddItem(item))
            {
                int remaining = hotbar[preferredSlot].AddItem(item, quantity);
                OnHotbarChanged?.Invoke(preferredSlot, hotbar[preferredSlot]);
                SaveInventory();
                return remaining == 0;
            }
        }

        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i].CanAddItem(item))
            {
                int remaining = hotbar[i].AddItem(item, quantity);
                OnHotbarChanged?.Invoke(i, hotbar[i]);
                SaveInventory();
                return remaining == 0;
            }
        }

        return false;
    }

    public void UseActiveHotbarItem()
    {
        int activeSlot = hotbarManager.GetActiveSlotIndex();
        UseHotbarItem(activeSlot);
    }

    public void UseHotbarItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbar.Count) return;

        InventorySlot slot = hotbar[slotIndex];
        if (slot.IsEmpty()) return;

        Item item = slot.item;
        if (item.CanUse())
        {
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

            if (item.isConsumable)
            {
                slot.RemoveItem(1);
                OnHotbarChanged?.Invoke(slotIndex, slot);
                SaveInventory();
            }
        }
    }

    private void UseSeed(PlantSeed seed)
    {
        if (seed != null)
        {
            PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
            if (planting != null)
            {
                planting.TryPlantSeed(seed);
            }
        }
    }

    private void UseConsumable(Item consumable)
    {
        Debug.Log($"Consuming {consumable.itemName}");

    }

    private void UseTool(Item tool)
    {
        Debug.Log($"Using tool {tool.itemName}");
    }

    public void MoveItem(int fromIndex, int toIndex, bool isFromHotbar = false, bool isToHotbar = false)
    {
        List<InventorySlot> fromList = isFromHotbar ? hotbar : inventory;
        List<InventorySlot> toList = isToHotbar ? hotbar : inventory;

        if (fromIndex < 0 || fromIndex >= fromList.Count ||
            toIndex < 0 || toIndex >= toList.Count) return;

        InventorySlot fromSlot = fromList[fromIndex];
        InventorySlot toSlot = toList[toIndex];

        Item tempItem = fromSlot.item;
        int tempQuantity = fromSlot.quantity;

        fromSlot.item = toSlot.item;
        fromSlot.quantity = toSlot.quantity;

        toSlot.item = tempItem;
        toSlot.quantity = tempQuantity;

        if (isFromHotbar)
            OnHotbarChanged?.Invoke(fromIndex, fromSlot);
        else
            OnInventoryChanged?.Invoke(fromIndex, fromSlot);

        if (isToHotbar)
            OnHotbarChanged?.Invoke(toIndex, toSlot);
        else
            OnInventoryChanged?.Invoke(toIndex, toSlot);

        SaveInventory();
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

    private void SaveInventory()
    {
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
        for (int i = 0; i < inventory.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Inventory_{i}_Item"))
            {
                string itemName = PlayerPrefs.GetString($"Inventory_{i}_Item");
                int quantity = PlayerPrefs.GetInt($"Inventory_{i}_Quantity");

                Item item = Resources.Load<Item>($"ScriptableObjects/{itemName}");
                if (item != null)
                {
                    inventory[i] = new InventorySlot(item, quantity);
                }
            }
        }

        for (int i = 0; i < hotbar.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Hotbar_{i}_Item"))
            {
                string itemName = PlayerPrefs.GetString($"Hotbar_{i}_Item");
                int quantity = PlayerPrefs.GetInt($"Hotbar_{i}_Quantity");

                Item item = Resources.Load<Item>($"ScriptableObjects/{itemName}");
                if (item != null)
                {
                    hotbar[i] = new InventorySlot(item, quantity);
                }
            }
        }
    }

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