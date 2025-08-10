using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform hotbarGrid;
    [SerializeField] private GameObject hotbarSlotPrefab;
    [SerializeField] private Image[] slotIcons; // ��� �������� �������������
    [SerializeField] private GameObject[] slotHighlights; // ��� �������� �������������
    [SerializeField] private TextMeshProUGUI[] quantityTexts; // ��� �������� �������������

    [Header("Visual Settings")]
    [SerializeField] private Color activeSlotColor = Color.yellow;
    [SerializeField] private Color inactiveSlotColor = Color.white;

    private InventorySlotUI[] hotbarSlots;
    private Image[] slotBackgrounds;
    private int currentActiveSlot = 0;

    private void Start()
    {
        // ���� ���� ������� ����� � ����������, ���������� ��
        if (slotIcons != null && slotIcons.Length > 0)
        {
            InitializeExistingHotbar();
        }
        else
        {
            // ����� ������� �����
            InitializeNewHotbar();
        }

        // ������������� �� �������
        HotbarManager hotbarManager = FindObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged += UpdateActiveSlot;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged += UpdateSlot; // ��� �������� �������������
        }
    }

    // ��� ������ � ������������� ������� �� ����������
    private void InitializeExistingHotbar()
    {
        slotBackgrounds = new Image[slotIcons.Length];

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
            {
                // �������� ��� �����
                slotBackgrounds[i] = slotIcons[i].transform.parent?.GetComponent<Image>();
            }
        }

        UpdateActiveSlot(0);
    }

    // ��� �������� ����� ������ ����������
    private void InitializeNewHotbar()
    {
        if (hotbarGrid == null || hotbarSlotPrefab == null) return;

        int hotbarSize = 8;
        hotbarSlots = new InventorySlotUI[hotbarSize];
        slotBackgrounds = new Image[hotbarSize];

        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarGrid);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(i, true);
                hotbarSlots[i] = slotUI;
                slotBackgrounds[i] = slotObj.GetComponent<Image>();
            }

            // ��������� ����� �������
            CreateSlotNumber(slotObj, i);
        }

        UpdateActiveSlot(0);
    }

    private void CreateSlotNumber(GameObject slotObj, int index)
    {
        GameObject numberObj = new GameObject("SlotNumber");
        numberObj.transform.SetParent(slotObj.transform);

        TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
        numberText.text = (index + 1).ToString();
        numberText.fontSize = 12;
        numberText.color = Color.white;
        numberText.alignment = TextAlignmentOptions.Center; // ����������!

        RectTransform numberRect = numberObj.GetComponent<RectTransform>();
        numberRect.anchorMin = new Vector2(0, 1);
        numberRect.anchorMax = new Vector2(1, 1);
        numberRect.offsetMin = new Vector2(0, -15);
        numberRect.offsetMax = new Vector2(0, 0);
    }

    // ��� �������� ������������� � ������������ �����
    public void UpdateSlot(int slotIndex, InventorySlot slot)
    {
        if (slotIndex < 0 || slotIndex >= 8) return;

        // ���� ���������� ����� ������� ������
        if (hotbarSlots != null && slotIndex < hotbarSlots.Length && hotbarSlots[slotIndex] != null)
        {
            hotbarSlots[slotIndex].UpdateSlot(slot);
        }
        // ���� ���������� ������ ������� � ��������� �� ����������
        else if (slotIcons != null && slotIndex < slotIcons.Length)
        {
            UpdateSlotOldStyle(slotIndex, slot);
        }
    }

    // ��� �������� �������������
    public void SetSlotIcon(int slotIndex, Sprite icon)
    {
        if (slotIcons != null && slotIndex >= 0 && slotIndex < slotIcons.Length && slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].sprite = icon;
            slotIcons[slotIndex].enabled = icon != null;
        }
    }

    // ��� �������� �������������
    public void ClearSlot(int slotIndex)
    {
        SetSlotIcon(slotIndex, null);
        if (quantityTexts != null && slotIndex < quantityTexts.Length && quantityTexts[slotIndex] != null)
        {
            quantityTexts[slotIndex].text = "";
        }
    }

    private void UpdateSlotOldStyle(int slotIndex, InventorySlot slot)
    {
        if (slot != null && !slot.IsEmpty())
        {
            slotIcons[slotIndex].sprite = slot.item.icon;
            slotIcons[slotIndex].enabled = true;

            if (quantityTexts != null && slotIndex < quantityTexts.Length && quantityTexts[slotIndex] != null)
            {
                quantityTexts[slotIndex].text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
        }
        else
        {
            slotIcons[slotIndex].sprite = null;
            slotIcons[slotIndex].enabled = false;

            if (quantityTexts != null && slotIndex < quantityTexts.Length && quantityTexts[slotIndex] != null)
            {
                quantityTexts[slotIndex].text = "";
            }
        }
    }

    private void UpdateActiveSlot(int activeSlotIndex)
    {
        currentActiveSlot = activeSlotIndex;

        // ��������� ��������� ��� ����� �������
        if (slotBackgrounds != null)
        {
            for (int i = 0; i < slotBackgrounds.Length; i++)
            {
                if (slotBackgrounds[i] != null)
                {
                    slotBackgrounds[i].color = (i == activeSlotIndex) ? activeSlotColor : inactiveSlotColor;
                }
            }
        }

        // ��������� ��������� ��� ������ �������
        if (slotHighlights != null)
        {
            for (int i = 0; i < slotHighlights.Length; i++)
            {
                if (slotHighlights[i] != null)
                {
                    slotHighlights[i].SetActive(i == activeSlotIndex);
                }
            }
        }
    }

    private void OnDestroy()
    {
        HotbarManager hotbarManager = FindObjectOfType<HotbarManager>();
        if (hotbarManager != null)
        {
            hotbarManager.OnSlotChanged -= UpdateActiveSlot;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged -= UpdateSlot;
        }
    }
}