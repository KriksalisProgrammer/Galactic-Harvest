using UnityEngine;

// ���� ����� ������� ��������� ������������� � ����� ������������ �����
// �������� ��� � ������ HotbarManager ��� HotbarController

public class HotbarCompatibilityHelper : MonoBehaviour
{
    [Header("References")]
    public HotbarUI hotbarUI;

    private void Start()
    {
        if (hotbarUI == null)
            hotbarUI = FindObjectOfType<HotbarUI>();
    }

    // ������ ��� ������������� �� ������ �����
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

// �������������, �� ������ �������� ��� ������ ����� � ���� ������������ ������:
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