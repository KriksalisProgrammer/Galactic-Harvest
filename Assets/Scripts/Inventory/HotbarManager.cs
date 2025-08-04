using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public GameObject[] slots;
    private int activeSlotIndex = 0;

    void Start()
    {
        UpdateSlotHighlight();
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleScrollInput();
    }

    void HandleKeyboardInput()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                activeSlotIndex = i;
                UpdateSlotHighlight();
                break;
            }
        }
    }

    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            activeSlotIndex = (activeSlotIndex + 1) % slots.Length;
            UpdateSlotHighlight();
        }
        else if (scroll < 0f)
        {

            activeSlotIndex = (activeSlotIndex - 1 + slots.Length) % slots.Length;
            UpdateSlotHighlight();
        }
    }

    void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Transform highlight = slots[i].transform.Find("Highlight");
            if (highlight != null)
                highlight.gameObject.SetActive(i == activeSlotIndex);
        }
    }

    public int GetActiveSlotIndex()
    {
        return activeSlotIndex;
    }

    public GameObject GetActiveSlot()
    {
        if (activeSlotIndex >= 0 && activeSlotIndex < slots.Length)
            return slots[activeSlotIndex];
        return null;
    }
}