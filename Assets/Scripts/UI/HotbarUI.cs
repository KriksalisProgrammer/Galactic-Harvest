using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private GameObject[] slotHighlights;
    [SerializeField] private TextMeshProUGUI[] quantityTexts;

    private void Start()
    {
        HotbarManager manager = FindObjectOfType<HotbarManager>();
        if (manager != null)
        {
            manager.OnSlotChanged += UpdateActiveSlot;
            UpdateActiveSlot(0);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged += UpdateSlot;

            for (int i = 0; i < slotIcons.Length; i++)
            {
                UpdateSlot(i, InventoryManager.Instance.GetHotbarSlot(i));
            }
        }
    }

    private void OnDestroy()
    {
        HotbarManager manager = FindObjectOfType<HotbarManager>();
        if (manager != null)
        {
            manager.OnSlotChanged -= UpdateActiveSlot;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHotbarChanged -= UpdateSlot;
        }
    }

    private void UpdateActiveSlot(int activeSlot)
    {
        for (int i = 0; i < slotHighlights.Length; i++)
        {
            slotHighlights[i].SetActive(i == activeSlot);
        }
    }

    public void UpdateSlot(int slotIndex, InventorySlot slot)
    {
        if (slotIndex < 0 || slotIndex >= slotIcons.Length) return;

        if (slot != null && !slot.IsEmpty())
        {
            slotIcons[slotIndex].sprite = slot.item.icon;
            slotIcons[slotIndex].enabled = true;

            if (quantityTexts != null && slotIndex < quantityTexts.Length)
            {
                quantityTexts[slotIndex].text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
        }
        else
        {
            slotIcons[slotIndex].sprite = null;
            slotIcons[slotIndex].enabled = false;

            if (quantityTexts != null && slotIndex < quantityTexts.Length)
            {
                quantityTexts[slotIndex].text = "";
            }
        }
    }

    public void SetSlotIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= slotIcons.Length) return;

        slotIcons[slotIndex].sprite = icon;
        slotIcons[slotIndex].enabled = icon != null;
    }

    public void ClearSlot(int slotIndex)
    {
        SetSlotIcon(slotIndex, null);
        if (quantityTexts != null && slotIndex < quantityTexts.Length)
        {
            quantityTexts[slotIndex].text = "";
        }
    }
}