using UnityEngine;

// ����������: ��������� HotbarManager � HotbarController � ���� �����
public class HotbarManager : MonoBehaviour
{
    [Header("Settings")]
    public int hotbarSize = 8;
    public KeyCode[] hotbarKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8
    };

    private int activeSlotIndex = 0;
    private HotbarUI hotbarUI;
    private InventoryManager inventoryManager;

    public System.Action<int> OnSlotChanged;

    private void Start()
    {
        hotbarUI = FindObjectOfType<HotbarUI>();
        inventoryManager = InventoryManager.Instance;

        // ��������� ������ ���� ��� �������� ��� ������
        SetActiveSlot(0);
    }

    private void Update()
    {
        HandleHotbarInput();
    }

    private void HandleHotbarInput()
    {
        // ��������� �� ������ �� ���������
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            return; // �� ������������ ������ ���� ��������� ������
        }

        // ��������� �� � ������ ������� �� ��
        PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
        bool inPlantingMode = planting != null && planting.IsInPlantingMode();

        // Check number key presses
        for (int i = 0; i < Mathf.Min(hotbarSize, hotbarKeys.Length); i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i]))
            {
                SetActiveSlot(i);
                break;
            }
        }

        // Handle mouse wheel scrolling (������ ���� �� � ������ �������)
        if (!inPlantingMode)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.1f)
            {
                if (scroll > 0f)
                {
                    SetActiveSlot((activeSlotIndex + 1) % hotbarSize);
                }
                else if (scroll < 0f)
                {
                    SetActiveSlot((activeSlotIndex - 1 + hotbarSize) % hotbarSize);
                }
            }
        }

        // Use active item on left click (if not in planting mode)
        if (Input.GetMouseButtonDown(0) && !inPlantingMode)
        {
            UseActiveItem();
        }
    }

    public void SetActiveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return;

        // ���� ���� ��� �������, ������ �� ������
        if (activeSlotIndex == slotIndex) return;

        activeSlotIndex = slotIndex;
        OnSlotChanged?.Invoke(activeSlotIndex);

        Debug.Log($"Active hotbar slot changed to: {activeSlotIndex}");
    }

    public void UseActiveItem()
    {
        if (inventoryManager != null)
        {
            inventoryManager.UseHotbarItem(activeSlotIndex);
        }
    }

    public int GetActiveSlotIndex()
    {
        return activeSlotIndex;
    }

    public InventorySlot GetActiveSlot()
    {
        if (inventoryManager != null)
        {
            return inventoryManager.GetHotbarSlot(activeSlotIndex);
        }
        return null;
    }

    public Item GetActiveItem()
    {
        InventorySlot activeSlot = GetActiveSlot();
        return activeSlot?.item;
    }

    // ����� ��� ��������������� ���������� UI
    public void RefreshHotbarUI()
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            InventorySlot slot = inventoryManager?.GetHotbarSlot(i);
            if (slot != null && hotbarUI != null)
            {
                hotbarUI.UpdateSlot(i, slot);
            }
        }
    }
}