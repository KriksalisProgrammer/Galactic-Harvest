using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject highlightFrame;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
    [SerializeField] private Color dragColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private int slotIndex;
    private bool isHotbarSlot;
    private InventorySlot currentSlot;
    private bool isDragging = false;

    // Drag & Drop
    private GameObject dragPreview;
    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        // Автоматически находим компоненты если не назначены
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (quantityText == null)
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (highlightFrame != null)
            highlightFrame.SetActive(false);
    }

    public void Initialize(int index, bool isHotbar)
    {
        slotIndex = index;
        isHotbarSlot = isHotbar;
        UpdateSlotVisuals();
    }

    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        UpdateSlotVisuals();
    }

    private void UpdateSlotVisuals()
    {
        if (currentSlot != null && !currentSlot.IsEmpty())
        {
            if (iconImage != null)
            {
                iconImage.sprite = currentSlot.item.icon;
                iconImage.color = Color.white;
                iconImage.gameObject.SetActive(true);
            }

            if (quantityText != null)
            {
                if (currentSlot.quantity > 1)
                {
                    quantityText.text = currentSlot.quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
            if (quantityText != null)
                quantityText.gameObject.SetActive(false);
        }
    }

    #region Mouse Events

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            SetBackgroundColor(hoverColor);
            if (highlightFrame != null)
                highlightFrame.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            SetBackgroundColor(normalColor);
            if (highlightFrame != null)
                highlightFrame.SetActive(false);
        }
    }

    #endregion

    #region Drag & Drop

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot == null || currentSlot.IsEmpty()) return;

        isDragging = true;
        CreateDragPreview();
        SetBackgroundColor(dragColor);

        if (highlightFrame != null)
            highlightFrame.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreview != null && parentCanvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                parentCanvas.worldCamera,
                out Vector2 localPoint
            );

            dragPreview.transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        DestroyDragPreview();
        SetBackgroundColor(normalColor);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (sourceSlot != null && sourceSlot != this)
        {
            SwapSlots(sourceSlot);
        }
    }

    #endregion

    #region Helper Methods

    private void HandleLeftClick()
    {
        if (currentSlot != null && !currentSlot.IsEmpty())
        {
            Debug.Log($"Левый клик по {currentSlot.item.itemName}");
        }
    }

    private void HandleRightClick()
    {
        if (currentSlot == null || currentSlot.IsEmpty()) return;

        if (isHotbarSlot)
        {
            InventoryManager.Instance?.UseHotbarItem(slotIndex);
        }
        else
        {
            if (currentSlot.item.itemType == ItemType.Seed)
            {
                InventoryManager.Instance?.AddItemToHotbar(currentSlot.item, 1);
                currentSlot.RemoveItem(1);
                InventoryManager.Instance?.OnInventoryChanged?.Invoke(slotIndex, currentSlot);
            }
        }
    }

    private void SwapSlots(InventorySlotUI sourceSlot)
    {
        InventoryManager.Instance?.MoveItem(
            sourceSlot.slotIndex,
            this.slotIndex,
            sourceSlot.isHotbarSlot,
            this.isHotbarSlot
        );
    }

    private void CreateDragPreview()
    {
        if (currentSlot == null || currentSlot.IsEmpty() || parentCanvas == null) return;

        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(parentCanvas.transform);
        dragPreview.transform.SetAsLastSibling();

        Image dragImage = dragPreview.AddComponent<Image>();
        dragImage.sprite = currentSlot.item.icon;
        dragImage.raycastTarget = false;
        dragImage.color = new Color(1, 1, 1, 0.8f);

        RectTransform dragRect = dragPreview.GetComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(60, 60);
    }

    private void DestroyDragPreview()
    {
        if (dragPreview != null)
        {
            Destroy(dragPreview);
            dragPreview = null;
        }
    }

    private void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
            backgroundImage.color = color;
    }

    #endregion
}