using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform hotbarSlotsParent;
    public GameObject hotbarSlotPrefab;

    [Header("Selection Settings")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;
    public float selectionBorderWidth = 3f;

    [Header("Drag Visual Settings")]
    public Color dragColor = new Color(1f, 1f, 1f, 0.5f);
    public Color normalDragColor = Color.white;

    private List<HotbarSlotUI> hotbarSlots = new List<HotbarSlotUI>();
    private int selectedSlotIndex = 0;
    private const int HOTBAR_SIZE = 8;

    public System.Action<int> OnSlotSelected;

    private void Start()
    {
        Initialize();

        // Subscribe to inventory changes  
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged += UpdateHotbarSlot;
        }

        // Set initial selection
        SelectSlot(0);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Handle number keys 1-8
        for (int i = 0; i < HOTBAR_SIZE; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                break;
            }
        }

        // Handle mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            int newIndex = (selectedSlotIndex + direction + HOTBAR_SIZE) % HOTBAR_SIZE;
            SelectSlot(newIndex);
        }
    }

    private void Initialize()
    {
        if (hotbarSlotsParent == null || hotbarSlotPrefab == null)
        {
            Debug.LogError("HotbarUI: Required references not assigned!");
            return;
        }

        // Clear existing slots
        foreach (Transform child in hotbarSlotsParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        hotbarSlots.Clear();

        // Create hotbar slots
        for (int i = 0; i < HOTBAR_SIZE; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarSlotsParent);
            HotbarSlotUI slotUI = slotObj.GetComponent<HotbarSlotUI>();

            if (slotUI != null)
            {
                slotUI.OnSlotClicked += OnSlotClicked;
                slotUI.OnItemMoved += OnItemMoved;
                slotUI.OnDragStateChanged += OnDragStateChanged;
                hotbarSlots.Add(slotUI);

                // Initialize with empty slot
                InventorySlot emptySlot = new InventorySlot();
                slotUI.SetSlot(emptySlot, i, true);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SIZE) return;

        // Remove selection from previous slot
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetSelected(false, normalColor);
        }

        selectedSlotIndex = index;

        // Set selection on new slot
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetSelected(true, selectedColor);
        }

        OnSlotSelected?.Invoke(selectedSlotIndex);
    }

    public void UpdateHotbarSlot(int index, InventorySlot slot)
    {
        if (index >= 0 && index < hotbarSlots.Count)
        {
            hotbarSlots[index].SetSlot(slot, index, true);

            // Maintain selection state
            if (index == selectedSlotIndex)
            {
                hotbarSlots[index].SetSelected(true, selectedColor);
            }
        }
    }

    // Метод для совместимости со старым кодом
    public void UpdateSlot(int index, InventorySlot slot)
    {
        UpdateHotbarSlot(index, slot);
    }

    private void OnSlotClicked(int slotIndex, bool isHotbarSlot)
    {
        if (isHotbarSlot)
        {
            SelectSlot(slotIndex);

            // Try to use item if it's usable
            if (InventoryManager.Instance != null)
            {
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(slotIndex);
                if (slot != null && !slot.IsEmpty() && slot.item.CanUse())
                {
                    slot.item.Use();
                    // Remove one item if it's consumable
                    if (slot.item.isConsumable)
                    {
                        InventoryManager.Instance.RemoveFromHotbar(slotIndex, 1);
                    }
                }
            }
        }
    }

    private void OnItemMoved(int fromIndex, int toIndex, bool fromHotbar, bool toHotbar)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.MoveItem(fromIndex, toIndex, fromHotbar, toHotbar);
        }
    }

    private void OnDragStateChanged(int slotIndex, bool isDragging)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            // Change visual during drag to make item more visible
            Color dragVisualColor = isDragging ? dragColor : normalDragColor;
            hotbarSlots[slotIndex].SetDragVisual(dragVisualColor);
        }
    }

    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public Item GetSelectedItem()
    {
        if (InventoryManager.Instance != null)
        {
            InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(selectedSlotIndex);
            return slot?.item;
        }
        return null;
    }

    public void RefreshAllSlots()
    {
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < HOTBAR_SIZE; i++)
            {
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(i);
                if (slot != null)
                {
                    UpdateHotbarSlot(i, slot);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged -= UpdateHotbarSlot;
        }
    }
}