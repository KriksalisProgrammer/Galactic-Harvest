using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public int slotCount = 5;
    public HotbarSlot[] slots;

    void Awake()
    {
        slots = new HotbarSlot[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = new HotbarSlot();
        }
    }

    public void AddItem(Item item, int amount)
    {
        // Проверка: есть ли уже этот предмет
        for (int i = 0; i < slotCount; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemData == item)
            {
                slots[i].amount += amount;
                Debug.Log($"Добавлено {amount} x {item.itemName}. Всего: {slots[i].amount}");
                return;
            }
        }

        // Ищем пустой слот
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].itemData = item;
                slots[i].amount = amount;
                Debug.Log($"Новый предмет: {amount} x {item.itemName}");
                return;
            }
        }

        Debug.Log("Хотбар заполнен!");
    }

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return false;

        HotbarSlot slot = slots[slotIndex];
        if (slot.IsEmpty) return false;

        if (slot.itemData.CanUse())
        {
            slot.itemData.Use();
            if (slot.itemData.isConsumable)
            {
                slot.amount--;
                if (slot.amount <= 0)
                    slot.Clear();
            }
            return true;
        }

        Debug.Log($"Нельзя использовать {slot.itemData.itemName}");
        return false;
    }
}
