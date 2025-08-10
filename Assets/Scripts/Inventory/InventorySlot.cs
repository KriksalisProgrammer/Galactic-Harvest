using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    public InventorySlot(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }

    public bool CanAddItem(Item newItem)
    {
        if (IsEmpty()) return true;
        if (item == newItem && item.isStackable && quantity < item.maxStackSize)
            return true;
        return false;
    }

    public int AddItem(Item newItem, int amount)
    {
        if (IsEmpty())
        {
            item = newItem;
            quantity = Mathf.Min(amount, item.maxStackSize);
            return amount - quantity;
        }
        else if (item == newItem && item.isStackable)
        {
            int canAdd = item.maxStackSize - quantity;
            int adding = Mathf.Min(amount, canAdd);
            quantity += adding;
            return amount - adding;
        }
        return amount;
    }

    public void RemoveItem(int amount)
    {
        quantity -= amount;
        if (quantity <= 0)
        {
            item = null;
            quantity = 0;
        }
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}