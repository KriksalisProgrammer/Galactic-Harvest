using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IDropHandler, IBeginDragHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Image slotBackground;

    [Header("Settings")]
    public Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color highlightColor = new Color(1f, 1f, 0.8f, 1f);
    public Color selectedColor = new Color(0.8f, 1f, 0.8f, 1f);
    public Color canDropColor = new Color(0.8f, 0.8f, 1f, 1f);

    private InventorySlot slot;
    private int slotIndex;
    private bool isHotbarSlot;
    private Canvas canvas;
    private GameObject draggedItem;
    private bool isDragging = false;
    private Color originalColor;

    public System.Action<int, bool> OnSlotClicked;
    public System.Action<int, int, bool, bool> OnItemMoved; // from, to, fromHotbar, toHotbar

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        originalColor = normalColor;
    }

    public void SetSlot(InventorySlot newSlot, int index, bool isHotbar = false)
    {
        slot = newSlot;
        slotIndex = index;
        isHotbarSlot = isHotbar;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // Check if UI components are assigned
        if (itemIcon == null)
        {
            Debug.LogError("InventorySlotUI: itemIcon is not assigned! Please assign it in the inspector.");
            return;
        }

        if (quantityText == null)
        {
            Debug.LogError("InventorySlotUI: quantityText is not assigned! Please assign it in the inspector.");
            return;
        }

        if (slot == null || slot.IsEmpty())
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            quantityText.text = "";
        }
        else
        {
            itemIcon.sprite = slot.item.icon;
            itemIcon.color = Color.white;

            if (slot.quantity > 1)
            {
                quantityText.text = slot.quantity.ToString();
            }
            else
            {
                quantityText.text = "";
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (slotBackground != null)
        {
            slotBackground.color = selected ? selectedColor : normalColor;
            originalColor = slotBackground.color;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (slotBackground != null && slotBackground.color != selectedColor)
        {
            slotBackground.color = highlighted ? highlightColor : originalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotIndex, isHotbarSlot);

        // Visual feedback for click
        if (slotBackground != null)
        {
            SetHighlighted(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty()) return;

        isDragging = true;
        CreateDraggedItem();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || draggedItem == null) return;

        // ИСПРАВЛЕНО: Правильное позиционирование перетаскиваемого предмета
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint);
        draggedItem.transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (draggedItem != null)
        {
            Destroy(draggedItem);
            draggedItem = null;
        }

        // Сброс подсветки
        SetHighlighted(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot != null && draggedSlot != this)
        {
            Debug.Log($"Moving item from slot {draggedSlot.slotIndex} to slot {slotIndex}");
            OnItemMoved?.Invoke(draggedSlot.slotIndex, slotIndex, draggedSlot.isHotbarSlot, isHotbarSlot);
        }

        // Сброс подсветки
        SetHighlighted(false);
    }

    private void CreateDraggedItem()
    {
        if (slot == null || slot.IsEmpty() || canvas == null) return;

        draggedItem = new GameObject("DraggedItem");
        draggedItem.transform.SetParent(canvas.transform, false);
        draggedItem.transform.SetAsLastSibling();

        Image dragImage = draggedItem.AddComponent<Image>();
        dragImage.sprite = slot.item.icon;
        dragImage.color = new Color(1, 1, 1, 0.8f);
        dragImage.raycastTarget = false;

        RectTransform rectTransform = draggedItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);

        // ИСПРАВЛЕНО: Добавляем компонент для игнорирования GraphicRaycaster
        CanvasGroup canvasGroup = draggedItem.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
    }

    // ИСПРАВЛЕНО: Добавляем методы для подсветки при hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && slotBackground.color != selectedColor)
        {
            SetHighlighted(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            SetHighlighted(false);
        }
    }

    private System.Collections.IEnumerator ResetHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isDragging)
        {
            SetHighlighted(false);
        }
    }

    public InventorySlot GetSlot()
    {
        return slot;
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }

    public bool IsHotbarSlot()
    {
        return isHotbarSlot;
    }

    public bool CanDropItem(InventorySlotUI draggedSlot)
    {
        if (draggedSlot == null || draggedSlot == this) return false;

        if (draggedSlot.slot.IsEmpty()) return false;

        if (slot.IsEmpty()) return true;
        if (slot.item == draggedSlot.slot.item && slot.item.isStackable)
        {
            return slot.quantity < slot.item.maxStackSize;
        }

        return true;
    }
}