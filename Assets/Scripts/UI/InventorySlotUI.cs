using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IDropHandler
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

    public System.Action<int, bool> OnSlotClicked;
    public System.Action<int, int, bool, bool> OnItemMoved; // from, to, fromHotbar, toHotbar

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
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
        slotBackground.color = selected ? selectedColor : normalColor;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (slotBackground.color != selectedColor)
        {
            slotBackground.color = highlighted ? highlightColor : normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotIndex, isHotbarSlot);

        // Visual feedback for click
        if (slotBackground != null)
        {
            SetHighlighted(true);
            StartCoroutine(ResetHighlightAfterDelay(0.1f));
        }
    }

    private System.Collections.IEnumerator ResetHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetHighlighted(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty()) return;

        if (draggedItem == null)
        {
            CreateDraggedItem();
        }

        draggedItem.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Destroy(draggedItem);
            draggedItem = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot != null && draggedSlot != this)
        {
            OnItemMoved?.Invoke(draggedSlot.slotIndex, slotIndex, draggedSlot.isHotbarSlot, isHotbarSlot);
        }
    }

    private void CreateDraggedItem()
    {
        if (slot == null || slot.IsEmpty()) return;

        draggedItem = new GameObject("DraggedItem");
        draggedItem.transform.SetParent(canvas.transform, false);
        draggedItem.transform.SetAsLastSibling();

        Image dragImage = draggedItem.AddComponent<Image>();
        dragImage.sprite = slot.item.icon;
        dragImage.color = new Color(1, 1, 1, 0.8f);
        dragImage.raycastTarget = false;

        RectTransform rectTransform = draggedItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
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
}