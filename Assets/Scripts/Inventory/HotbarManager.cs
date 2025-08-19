using UnityEngine;

// ��������� ������ ��� �������� � �����
[System.Serializable]
public class HotbarItem
{
    public string itemName = "";
    public int amount = 0;
    public Sprite itemIcon;

    // ������������
    public HotbarItem()
    {
        itemName = "";
        amount = 0;
        itemIcon = null;
    }

    public HotbarItem(string name, int amt, Sprite icon = null)
    {
        itemName = name;
        amount = amt;
        itemIcon = icon;
    }

    // ���������, ���� �� ����
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(itemName) || amount <= 0;
    }

    // ������� ����
    public void Clear()
    {
        itemName = "";
        amount = 0;
        itemIcon = null;
    }

    // ���������, ����� �� ���������� � ������ ���������
    public bool CanStackWith(HotbarItem other)
    {
        if (other == null || IsEmpty() || other.IsEmpty())
            return false;

        return itemName == other.itemName;
    }
}

// �������� �������� �������
public class HotbarManager : MonoBehaviour
{
    [Header("��������� �������")]
    public int slotCount = 8;
    public HotbarItem[] slots;

    [Header("����������")]
    public int selectedSlot = 0;
    public float scrollDelay = 0.1f;

    [Header("�������")]
    public System.Action<int> OnSlotChanged;
    public System.Action<HotbarItem, int> OnItemChanged;
    public System.Action OnHotbarUpdated;

    private float lastScrollTime = 0f;

    void Awake()
    {
        InitializeHotbar();
    }

    void Update()
    {
        HandleInput();
    }

    private void InitializeHotbar()
    {
        // ������� ������ ������ ���� �� �� ������
        if (slots == null || slots.Length != slotCount)
        {
            slots = new HotbarItem[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                slots[i] = new HotbarItem();
            }
        }

        // ���������, ��� ��� ����� ����������������
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new HotbarItem();
            }
        }

        // ����������, ��� ��������� ���� � �������� �������
        selectedSlot = Mathf.Clamp(selectedSlot, 0, slotCount - 1);
    }

    private void HandleInput()
    {
        HandleScrollInput();
        HandleKeyboardInput();
    }

    private void HandleScrollInput()
    {
        // ��������� �������� ��� ���������
        if (Time.time - lastScrollTime < scrollDelay)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0.1f)
        {
            SelectNextSlot();
            lastScrollTime = Time.time;
        }
        else if (scroll < -0.1f)
        {
            SelectPreviousSlot();
            lastScrollTime = Time.time;
        }
    }

    private void HandleKeyboardInput()
    {
        // ��������� ������� �������� ������
        for (int i = 0; i < Mathf.Min(slotCount, 9); i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                SelectSlot(i);
                break;
            }
        }

        // �������������� �������� ��� ������� 0 (10-� ����)
        if (Input.GetKeyDown(KeyCode.Alpha0) && slotCount > 9)
        {
            SelectSlot(9);
        }
    }

    public void SelectSlot(int index)
    {
        if (index >= 0 && index < slotCount)
        {
            int previousSlot = selectedSlot;
            selectedSlot = index;

            if (previousSlot != selectedSlot)
            {
                OnSlotChanged?.Invoke(selectedSlot);
                Debug.Log($"������ ���� {selectedSlot + 1}");
            }
        }
    }

    public void SelectNextSlot()
    {
        SelectSlot((selectedSlot + 1) % slotCount);
    }

    public void SelectPreviousSlot()
    {
        SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
    }

    public bool AddItem(string itemName, int amount, Sprite icon = null)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0)
        {
            Debug.LogWarning("������� �������� ������������ �������");
            return false;
        }

        // ������� ���� ������������ ������� ��� ��������
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].itemName == itemName)
            {
                slots[i].amount += amount;
                OnItemChanged?.Invoke(slots[i], i);
                OnHotbarUpdated?.Invoke();
                Debug.Log($"��������� {amount} x {itemName}. ����� � ����� {i + 1}: {slots[i].amount}");
                return true;
            }
        }

        // ���� �� ����� ������������, ���� ������ ����
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i] = new HotbarItem(itemName, amount, icon);
                OnItemChanged?.Invoke(slots[i], i);
                OnHotbarUpdated?.Invoke();
                Debug.Log($"����� ������� � ����� {i + 1}: {amount} x {itemName}");
                return true;
            }
        }

        Debug.Log("������ ��������! �� ������� �������� �������.");
        return false;
    }

    public bool UseItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slotCount)
        {
            Debug.LogWarning($"������������ ������ �����: {slotIndex}");
            return false;
        }

        var slot = slots[slotIndex];

        if (slot.IsEmpty() || slot.amount < amount)
        {
            Debug.Log($"������������ ��������� � ����� {slotIndex + 1}");
            return false;
        }

        string itemName = slot.itemName;
        slot.amount -= amount;

        if (slot.amount <= 0)
        {
            slot.Clear();
        }

        OnItemChanged?.Invoke(slot, slotIndex);
        OnHotbarUpdated?.Invoke();
        Debug.Log($"������������ {amount} x {itemName} �� ����� {slotIndex + 1}. ��������: {slot.amount}");
        return true;
    }

    public bool UseSelectedItem(int amount = 1)
    {
        return UseItem(selectedSlot, amount);
    }

    public bool RemoveItem(string itemName, int amount)
    {
        int totalRemoved = 0;

        // �������� �� ���� ������ � ������� ��������
        for (int i = 0; i < slotCount && totalRemoved < amount; i++)
        {
            if (slots[i].itemName == itemName)
            {
                int toRemove = Mathf.Min(slots[i].amount, amount - totalRemoved);
                slots[i].amount -= toRemove;
                totalRemoved += toRemove;

                if (slots[i].amount <= 0)
                {
                    slots[i].Clear();
                }

                OnItemChanged?.Invoke(slots[i], i);
            }
        }

        if (totalRemoved > 0)
        {
            OnHotbarUpdated?.Invoke();
            Debug.Log($"������� {totalRemoved} x {itemName}");
            return totalRemoved == amount;
        }

        Debug.Log($"������� {itemName} �� ������ � �������");
        return false;
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            slots[slotIndex].Clear();
            OnItemChanged?.Invoke(slots[slotIndex], slotIndex);
            OnHotbarUpdated?.Invoke();
        }
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            slots[i].Clear();
        }
        OnHotbarUpdated?.Invoke();
        Debug.Log("������ ������");
    }

    // �������
    public HotbarItem GetSelectedItem()
    {
        return slots[selectedSlot];
    }

    public HotbarItem GetItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
            return slots[slotIndex];
        return null;
    }

    public string GetSelectedItemName()
    {
        var item = GetSelectedItem();
        return item?.itemName ?? "";
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }

    public bool HasItem(string itemName, int minAmount = 1)
    {
        int totalAmount = 0;
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].itemName == itemName)
            {
                totalAmount += slots[i].amount;
                if (totalAmount >= minAmount)
                    return true;
            }
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int totalAmount = 0;
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].itemName == itemName)
            {
                totalAmount += slots[i].amount;
            }
        }
        return totalAmount;
    }

    // ������ ��� �������
    [ContextMenu("Print Hotbar Contents")]
    public void PrintHotbarContents()
    {
        Debug.Log("=== HOTBAR CONTENTS ===");
        for (int i = 0; i < slotCount; i++)
        {
            var slot = slots[i];
            if (slot.IsEmpty())
            {
                Debug.Log($"Slot {i + 1}: Empty");
            }
            else
            {
                string selected = (i == selectedSlot) ? " [SELECTED]" : "";
                Debug.Log($"Slot {i + 1}: {slot.amount} x {slot.itemName}{selected}");
            }
        }
    }
}