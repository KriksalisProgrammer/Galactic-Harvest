using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    [SerializeField] private int activeSlotIndex = 0;
    [SerializeField] private int maxSlots = 8;

    public System.Action<int> OnSlotChanged;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetActiveSlot(i);
                break;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SetActiveSlot((activeSlotIndex + 1) % maxSlots);
        }
        else if (scroll < 0f)
        {
            SetActiveSlot((activeSlotIndex - 1 + maxSlots) % maxSlots);
        }
    }

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= maxSlots) return;

        activeSlotIndex = index;
        OnSlotChanged?.Invoke(activeSlotIndex);
    }

    public int GetActiveSlotIndex()
    {
        return activeSlotIndex;
    }
}