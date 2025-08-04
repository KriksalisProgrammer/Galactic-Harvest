using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons;      // Иконки предметов в слотах
    [SerializeField] private GameObject[] slotHighlights; // Подсветка слотов (рамки)

    private int activeSlot = 0;

    void Start()
    {
        UpdateActiveSlot();
    }

    void Update()
    {
        // Смена слота клавишами 1-8
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                activeSlot = i;
                UpdateActiveSlot();
            }
        }
    }

    // Обновление подсветки слотов
    private void UpdateActiveSlot()
    {
        for (int i = 0; i < slotHighlights.Length; i++)
        {
            slotHighlights[i].SetActive(i == activeSlot);
        }
    }

    // Метод для установки иконки в слот (например, из инвентаря)
    public void SetSlotIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= slotIcons.Length) return;

        slotIcons[slotIndex].sprite = icon;
        slotIcons[slotIndex].enabled = icon != null;
    }
}
