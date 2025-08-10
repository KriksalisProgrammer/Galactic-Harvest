using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private int slotIndex;
    private bool isHotbarSlot;
    private InventorySlot currentSlot;

    private GameObject dragObject;
    private Canvas canvas;
    private Transform originalParent;

    public void Initialize(int index, bool isHotbar)
    {
        slotIndex = index;
        isHotbarSlot = isHotbar;
        canvas = GetComponentInParent<Canvas>();

        UpdateSlot(null);
    }

    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;

        if (slot != null && !slot.IsEmpty())
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = true;
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            quantityText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentSlot != null && !currentSlot.IsEmpty())
            {
                if (isHotbarSlot)
                {
                    InventoryManager.Instance?.UseHotbarItem(slotIndex);
                }
                else
                {
                    InventoryManager.Instance?.AddItemToHotbar(currentSlot.item, 1);
                    currentSlot.RemoveItem(1);
                    InventoryManager.Instance?.OnInventoryChanged?.Invoke(slotIndex, currentSlot);
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot == null || currentSlot.IsEmpty()) return;

        dragObject = new GameObject("DragItem");
        dragObject.transform.SetParent(canvas.transform);

        Image dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = currentSlot.item.icon;
        dragImage.raycastTarget = false;

        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = iconImage.rectTransform.sizeDelta;

        originalParent = transform;

        SetSlotAlpha(0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            SetSlotAlpha(1f);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (sourceSlot != null && sourceSlot != this)
        {
            bool sourceIsHotbar = sourceSlot.isHotbarSlot;
            bool targetIsHotbar = this.isHotbarSlot;

            InventoryManager.Instance?.MoveItem(
                sourceSlot.slotIndex,
                this.slotIndex,
                sourceIsHotbar,
                targetIsHotbar
            );
        }
    }

    private void SetSlotAlpha(float alpha)
    {
        Color color = iconImage.color;
        color.a = alpha;
        iconImage.color = color;
    }

    private void OnMouseEnter()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
    }

    private void OnMouseExit()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}