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


    public bool IsValid()
    {
        return !string.IsNullOrEmpty(itemName) && icon != null;
    }
}

public enum ItemType
{
    Misc,
    Weapon,
    Tool,
    Consumable,
    Material,
    Equipment
}