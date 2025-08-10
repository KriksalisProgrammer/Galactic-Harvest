using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject slotPrefab;

    private List<InventorySlotUI> inventorySlots;
    private bool isInventoryOpen = false;

    public void Initialize()
    {
        inventorySlots = new List<InventorySlotUI>();

        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < 24; i++) 
            {
                GameObject slotObj = Instantiate(slotPrefab, inventoryGrid);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Initialize(i, false);
                    inventorySlots.Add(slotUI);
                }
            }

            InventoryManager.Instance.OnInventoryChanged += UpdateSlot;
        }

        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void UpdateSlot(int slotIndex, InventorySlot slot)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            inventorySlots[slotIndex].UpdateSlot(slot);
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateSlot;
        }
    }
}
