using UnityEngine;

// Этот класс поможет сохранить совместимость с вашим существующим кодом
// Добавьте его к вашему HotbarManager или HotbarController

public class HotbarCompatibilityHelper : MonoBehaviour
{
    [Header("References")]
    public HotbarUI hotbarUI;

    private void Start()
    {
        if (hotbarUI == null)
            hotbarUI = FindObjectOfType<HotbarUI>();
    }

    // Методы для совместимости со старым кодом
    public void UpdateSlot(int index, InventorySlot slot)
    {
        if (hotbarUI != null)
        {
            hotbarUI.UpdateSlot(index, slot);
        }
    }

    public void UpdateHotbarSlot(int index, InventorySlot slot)
    {
        if (hotbarUI != null)
        {
            hotbarUI.UpdateHotbarSlot(index, slot);
        }
    }

    public void SelectSlot(int index)
    {
        if (hotbarUI != null)
        {
            hotbarUI.SelectSlot(index);
        }
    }

    public int GetSelectedSlotIndex()
    {
        if (hotbarUI != null)
        {
            return hotbarUI.GetSelectedSlotIndex();
        }
        return 0;
    }

    public Item GetSelectedItem()
    {
        if (hotbarUI != null)
        {
            return hotbarUI.GetSelectedItem();
        }
        return null;
    }

    public void RefreshAllSlots()
    {
        if (hotbarUI != null)
        {
            hotbarUI.RefreshAllSlots();
        }
    }
}

// Альтернативно, вы можете добавить эти методы прямо в ваши существующие классы:
public static class HotbarExtensions
{
    public static void UpdateSlotSafe(this HotbarUI hotbar, int index, InventorySlot slot)
    {
        if (hotbar != null)
        {
            hotbar.UpdateSlot(index, slot);
        }
    }
}