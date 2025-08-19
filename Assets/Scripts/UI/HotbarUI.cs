using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform hotbarSlotsParent;
    public GameObject hotbarSlotPrefab;
    public int hotbarSize = 8;

    [Header("Input Settings")]
    public KeyCode[] slotKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
                                  KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8 };
    public float scrollDelay = 0.1f;

    private List<HotbarSlotUI> hotbarSlots = new List<HotbarSlotUI>();
    private int selectedIndex = 0;
    private float lastScrollTime = 0f;

    // События для уведомления других систем
    public System.Action<int> OnSlotSelected;
    public System.Action<PlantSeed, int> OnItemChanged;

    private void Start()
    {
        InitializeHotbar();
        UpdateSelectionVisual();
    }

    private void Update()
    {
        HandleMouseScroll();
        HandleKeyboardInput();
    }

    public void InitializeHotbar()
    {
        // Очищаем существующие слоты
        foreach (var slot in hotbarSlots)
        {
            if (slot != null && slot.gameObject != null)
                DestroyImmediate(slot.gameObject);
        }

        hotbarSlots.Clear();

        // Создаем новые слоты
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarSlotsParent);
            slotObj.name = $"HotbarSlot_{i}";

            HotbarSlotUI slotUI = slotObj.GetComponent<HotbarSlotUI>();
            if (slotUI == null)
            {
                Debug.LogError($"HotbarSlotUI component not found on prefab at slot {i}");
                continue;
            }

            hotbarSlots.Add(slotUI);
        }

        selectedIndex = 0;
        UpdateSelectionVisual();
    }

    private void HandleMouseScroll()
    {
        if (Time.time - lastScrollTime < scrollDelay) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0.1f)
        {
            SelectNext();
            lastScrollTime = Time.time;
        }
        else if (scroll < -0.1f)
        {
            SelectPrevious();
            lastScrollTime = Time.time;
        }
    }

    private void HandleKeyboardInput()
    {
        for (int i = 0; i < slotKeys.Length && i < hotbarSlots.Count; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                SelectSlot(i);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index >= 0 && index < hotbarSlots.Count)
        {
            selectedIndex = index;
            UpdateSelectionVisual();
            OnSlotSelected?.Invoke(selectedIndex);
        }
    }

    public void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % hotbarSlots.Count;
        UpdateSelectionVisual();
        OnSlotSelected?.Invoke(selectedIndex);
    }

    public void SelectPrevious()
    {
        selectedIndex = (selectedIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count;
        UpdateSelectionVisual();
        OnSlotSelected?.Invoke(selectedIndex);
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] != null)
                hotbarSlots[i].SetSelected(i == selectedIndex);
        }
    }

    public void SetItemToSlot(int slotIndex, PlantSeed seed)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSlots.Count)
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return;
        }

        if (hotbarSlots[slotIndex] != null)
        {
            hotbarSlots[slotIndex].SetItem(seed);
            OnItemChanged?.Invoke(seed, slotIndex);
        }
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count && hotbarSlots[slotIndex] != null)
        {
            hotbarSlots[slotIndex].ClearSlot();
            OnItemChanged?.Invoke(null, slotIndex);
        }
    }

    public HotbarSlotUI GetSelectedSlot()
    {
        if (selectedIndex >= 0 && selectedIndex < hotbarSlots.Count)
            return hotbarSlots[selectedIndex];
        return null;
    }

    public HotbarSlotUI GetSlot(int index)
    {
        if (index >= 0 && index < hotbarSlots.Count)
            return hotbarSlots[index];
        return null;
    }

    public PlantSeed GetSelectedPlantSeed()
    {
        var selectedSlot = GetSelectedSlot();
        return selectedSlot?.GetPlantSeed();
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public bool HasItemInSelectedSlot()
    {
        var selectedSlot = GetSelectedSlot();
        return selectedSlot != null && selectedSlot.HasItem();
    }
}