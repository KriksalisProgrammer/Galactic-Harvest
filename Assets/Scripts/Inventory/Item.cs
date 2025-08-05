using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("World Object")]
    public GameObject prefab;

    [Header("Properties")]
    public ItemType itemType = ItemType.Misc;
    public bool isStackable = true;
    [Min(1)]
    public int maxStackSize = 64;
    public float weight = 1f;

    [Header("Usage")]
    public bool isConsumable = false;
    public bool isEquippable = false;

    public string GetDisplayName()
    {
        return !string.IsNullOrEmpty(itemName) ? itemName : name;
    }

    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(itemName) && icon != null;
    }

    // Виртуальные методы для переопределения в наследниках
    public virtual void Use(GameObject user)
    {
        Debug.Log($"Использован предмет: {GetDisplayName()}");

        // Базовая логика по типам
        switch (itemType)
        {
            case ItemType.Consumable:
                Debug.Log($"Потреблен: {GetDisplayName()}");
                break;
            case ItemType.Tool:
            case ItemType.Weapon:
            case ItemType.Equipment:
                Debug.Log($"Экипирован: {GetDisplayName()}");
                break;
            default:
                Debug.Log($"Предмет {GetDisplayName()} не имеет специального использования");
                break;
        }
    }

    public virtual void OnEquip(GameObject user)
    {
        if (isEquippable)
        {
            Debug.Log($"Экипирован: {GetDisplayName()}");
        }
    }

    public virtual void OnUnequip(GameObject user)
    {
        if (isEquippable)
        {
            Debug.Log($"Снят: {GetDisplayName()}");
        }
    }

    public virtual bool CanUse(GameObject user)
    {
        return true; // Базово все предметы можно использовать
    }
}

public enum ItemType
{
    Misc,
    Plant,      // Для семян растений
    Weapon,
    Tool,
    Consumable,
    Material,
    Equipment
}