using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private GameObject[] slotHighlights;

    private void Start()
    {

        HotbarManager manager = FindObjectOfType<HotbarManager>();
        if (manager != null)
        {
            manager.OnSlotChanged += UpdateActiveSlot;
            UpdateActiveSlot(0);
        }
    }

    private void OnDestroy()
    {
        HotbarManager manager = FindObjectOfType<HotbarManager>();
        if (manager != null)
        {
            manager.OnSlotChanged -= UpdateActiveSlot;
        }
    }

    private void UpdateActiveSlot(int activeSlot)
    {
        for (int i = 0; i < slotHighlights.Length; i++)
        {
            slotHighlights[i].SetActive(i == activeSlot);
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
    }
}
