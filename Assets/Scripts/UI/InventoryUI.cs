using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject slotPrefab; // ������������� ��� ������������ ������� ����
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private int inventorySize = 24;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private List<InventorySlotUI> inventorySlots;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Start()
    {
        // �������������� �������������
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    // ��� �������� �������������
    public void Initialize()
    {
        InitializeInventory();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => ToggleInventory());
        }

        // ���������� ������
        SetInventoryActive(false);
    }

    private void InitializeInventory()
    {
        // ���������, �� ���������������� �� ��� �����
        if (inventorySlots != null && inventorySlots.Count > 0)
            return;

        inventorySlots = new List<InventorySlotUI>();

        // ���� ���� prefab � grid, ������� �����
        if (slotPrefab != null && inventoryGrid != null)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, inventoryGrid);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(i, false);
                    inventorySlots.Add(slotUI);
                }
            }
        }

        // ������������� �� ������� ���������
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateInventorySlot;
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        SetInventoryActive(isOpen);
    }

    private void SetInventoryActive(bool active)
    {
        isOpen = active;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(active);
        }

        // ���������� ��������
        if (active)
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

    private void UpdateInventorySlot(int slotIndex, InventorySlot slot)
    {
        if (inventorySlots != null && slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            inventorySlots[slotIndex].UpdateSlot(slot);
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventorySlot;
        }
    }
}
