using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private HotbarUI hotbarUI;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private HotbarSlot[] slots = new HotbarSlot[8];
    [SerializeField] private Item[] testItems;
    void Start()
    {
        InitializeSlots();
        UpdateUI();


        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged += OnSlotChanged;
        }
        for (int i = 0; i < testItems.Length; i++)
        {
            if (testItems[i] != null)
            {
                TryAddItem(testItems[i]);
            }
        }
    }

    void Update()
    {
        HandleUseInput();


        Item currentItem = GetSelectedItem();
        if (currentItem != null && Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Выбран: " + currentItem.itemName);
        }
    }

    private void OnDestroy()
    {
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged -= OnSlotChanged;
        }
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new HotbarSlot();
        }
    }

    private void OnSlotChanged(int newSlotIndex)
    {

        Item selectedItem = GetSelectedItem();
        if (selectedItem != null)
        {
            Debug.Log($"Переключен на слот {newSlotIndex}: {selectedItem.itemName}");
        }
    }

    public void SetItem(int index, Item item)
    {
        if (index < 0 || index >= slots.Length) return;

        slots[index].item = item;
        UpdateUI();
    }

    public Item GetSelectedItem()
    {
        if (hotbarManager == null) return null;

        int index = hotbarManager.GetActiveSlotIndex();
        if (index < 0 || index >= slots.Length) return null;

        return slots[index].item;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        slots[index].item = null;
        UpdateUI();
    }

    public bool HasItem(int index)
    {
        if (index < 0 || index >= slots.Length) return false;
        return slots[index].item != null;
    }

    private void UpdateUI()
    {
        if (hotbarUI == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            Sprite icon = slots[i].item != null ? slots[i].item.icon : null;
            hotbarUI.SetSlotIcon(i, icon);
        }
    }

    void HandleUseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseSelectedItem();
        }

        if (Input.GetMouseButtonDown(1)) 
        {
            UseSelectedItemAlt();
        }
    }

    private void UseSelectedItem()
    {
        Item selectedItem = GetSelectedItem();
        if (selectedItem == null) return;

        Debug.Log($"Используем предмет: {selectedItem.itemName}");

        switch (selectedItem.itemType)
        {
            case ItemType.Consumable:
                ConsumeItem(selectedItem);
                break;
            case ItemType.Tool:
            case ItemType.Weapon:
                EquipItem(selectedItem);
                break;
            case ItemType.Material:
            case ItemType.Misc:
                PlaceItemInWorld(selectedItem);
                break;
        }
    }

    private void UseSelectedItemAlt()
    {
        Item selectedItem = GetSelectedItem();
        if (selectedItem == null) return;


        Debug.Log($"Альтернативное использование: {selectedItem.itemName}");
        DropItem(selectedItem);
    }

    private void ConsumeItem(Item item)
    {
        Debug.Log($"Потребляем: {item.itemName}");


        int currentSlot = hotbarManager.GetActiveSlotIndex();
        RemoveItem(currentSlot);
    }

    private void EquipItem(Item item)
    {
        Debug.Log($"Экипируем: {item.itemName}");

    }

    private void PlaceItemInWorld(Item item)
    {
        Debug.Log($"Размещаем в мире: {item.itemName}");

        int currentSlot = hotbarManager.GetActiveSlotIndex();
        RemoveItem(currentSlot);
    }

    private void DropItem(Item item)
    {
        Debug.Log($"Выбрасываем: {item.itemName}");


        int currentSlot = hotbarManager.GetActiveSlotIndex();
        RemoveItem(currentSlot);
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
                return i;
        }
        return -1;
    }

    public bool TryAddItem(Item item)
    {
        int emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            SetItem(emptySlot, item);
            return true;
        }
        return false;
    }
}