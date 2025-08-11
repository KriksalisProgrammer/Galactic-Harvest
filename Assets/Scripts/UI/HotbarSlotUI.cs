using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Components")]
    public Image itemIcon;
    public Text quantityText;
    public Image backgroundImage;
    public Image selectionBorder;

    [Header("Colors")]
    public Color selectedBorderColor = Color.yellow;
    public Color normalBorderColor = Color.clear;
    public Color dragBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    public Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Drag Settings")]
    public Canvas dragCanvas;
    public GameObject dragPreviewPrefab;

    private InventorySlot currentSlot;
    private int slotIndex;
    private bool isHotbarSlot = true;
    private bool isSelected = false;
    private bool isDragging = false;

    private GameObject dragPreview;
    private CanvasGroup canvasGroup;

    // Events
    public System.Action<int, bool> OnSlotClicked;
    public System.Action<int, int, bool, bool> OnItemMoved;
    public System.Action<int, bool> OnDragStateChanged;

    private void Awake()
    {
        // Проверяем и инициализируем компоненты
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find drag canvas if not assigned
        if (dragCanvas == null)
        {
            dragCanvas = GetComponentInParent<Canvas>();
        }

        // Проверяем все необходимые компоненты
        if (itemIcon == null)
        {
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemIcon == null)
            {
                Debug.LogError("ItemIcon not found in HotbarSlotUI! Please assign it in Inspector.");
            }
        }

        if (quantityText == null)
        {
            quantityText = transform.Find("QuantityText")?.GetComponent<Text>();
            // quantityText необязательный, не показываем ошибку
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        // Initialize selection border if not assigned
        if (selectionBorder == null)
        {
            selectionBorder = transform.Find("SelectionBorder")?.GetComponent<Image>();

            // If still null, create one
            if (selectionBorder == null)
            {
                CreateSelectionBorder();
            }
        }

        // Безопасно устанавливаем начальное состояние
        if (selectionBorder != null)
        {
            SetSelected(false, normalBorderColor);
        }
    }

    private void CreateSelectionBorder()
    {
        GameObject borderObj = new GameObject("SelectionBorder");
        borderObj.transform.SetParent(transform, false);
        borderObj.transform.SetAsFirstSibling();

        RectTransform rectTransform = borderObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        selectionBorder = borderObj.AddComponent<Image>();
        selectionBorder.color = normalBorderColor;
        selectionBorder.raycastTarget = false;
    }

    public void SetSlot(InventorySlot slot, int index, bool hotbar)
    {
        currentSlot = slot;
        slotIndex = index;
        isHotbarSlot = hotbar;

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Проверяем на null перед использованием
        if (itemIcon == null)
        {
            Debug.LogWarning("ItemIcon is null in HotbarSlotUI.UpdateUI()");
            return;
        }

        if (currentSlot == null || currentSlot.IsEmpty())
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;

            if (quantityText != null)
                quantityText.text = "";
        }
        else
        {
            itemIcon.sprite = currentSlot.item.icon;
            itemIcon.color = Color.white;

            if (quantityText != null)
            {
                if (currentSlot.quantity > 1)
                {
                    quantityText.text = currentSlot.quantity.ToString();
                }
                else
                {
                    quantityText.text = "";
                }
            }
        }
    }

    public void SetSelected(bool selected, Color borderColor)
    {
        isSelected = selected;

        if (selectionBorder != null)
        {
            selectionBorder.color = selected ? borderColor : normalBorderColor;
        }
    }

    public void SetDragVisual(Color backgroundColor)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
        {
            OnSlotClicked?.Invoke(slotIndex, isHotbarSlot);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot == null || currentSlot.IsEmpty()) return;

        isDragging = true;

        // Create drag preview
        CreateDragPreview();

        // Make slot semi-transparent
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Change background for better visibility
        SetDragVisual(dragBackgroundColor);

        OnDragStateChanged?.Invoke(slotIndex, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreview != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            dragPreview.transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Destroy drag preview
        if (dragPreview != null)
        {
            Destroy(dragPreview);
            dragPreview = null;
        }

        // Restore normal appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        SetDragVisual(normalBackgroundColor);

        OnDragStateChanged?.Invoke(slotIndex, false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        HotbarSlotUI draggedSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (draggedSlot != null && draggedSlot != this)
        {
            // Move item from dragged slot to this slot
            OnItemMoved?.Invoke(
                draggedSlot.slotIndex,
                slotIndex,
                draggedSlot.isHotbarSlot,
                isHotbarSlot
            );
        }
    }

    private void CreateDragPreview()
    {
        if (dragCanvas == null || currentSlot == null || currentSlot.IsEmpty()) return;

        // Create simple drag preview
        GameObject previewObj = new GameObject("DragPreview");
        previewObj.transform.SetParent(dragCanvas.transform, false);

        Image previewImage = previewObj.AddComponent<Image>();
        previewImage.sprite = currentSlot.item.icon;
        previewImage.raycastTarget = false;

        // Make it slightly smaller
        RectTransform rect = previewObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64, 64);

        // Add quantity text if needed
        if (currentSlot.quantity > 1)
        {
            GameObject textObj = new GameObject("Quantity");
            textObj.transform.SetParent(previewObj.transform, false);

            Text quantityPreview = textObj.AddComponent<Text>();
            quantityPreview.text = currentSlot.quantity.ToString();

            // Пытаемся найти подходящий шрифт
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault();
            }
            quantityPreview.font = font;

            quantityPreview.fontSize = 14;
            quantityPreview.color = Color.white;
            quantityPreview.alignment = TextAnchor.LowerRight;
            quantityPreview.raycastTarget = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        dragPreview = previewObj;
    }

    // Getters
    public InventorySlot GetSlot() => currentSlot;
    public int GetSlotIndex() => slotIndex;
    public bool IsHotbarSlot() => isHotbarSlot;
    public bool IsSelected() => isSelected;
}