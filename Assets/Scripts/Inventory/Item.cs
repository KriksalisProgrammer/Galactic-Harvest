using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Properties")]
    public ItemType itemType;
    public bool isStackable = true;
    public int maxStackSize = 64;
    public float weight = 1f;

    [Header("Usage")]
    public bool isConsumable = false;
    public bool isEquippable = false;

    public virtual void Use()
    {
        Debug.Log($"Using item: {itemName}");
    }

    public virtual bool CanUse()
    {
        return true;
    }
}

public enum ItemType
{
    Misc,
    Plant,
    Seed,
    Resource,
    Weapon,
    Tool,
    Consumable,
    Material,
    Equipment
}