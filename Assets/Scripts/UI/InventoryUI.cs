using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject slotPrefab; // Переименовано для соответствия старому коду
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private int inventorySize = 24;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private List<InventorySlotUI> inventorySlots;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Start()
    {
        // Автоматическая инициализация
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    // Для обратной совместимости
    public void Initialize()
    {
        InitializeInventory();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => ToggleInventory());
        }

        // Изначально закрыт
        SetInventoryActive(false);
    }

    private void InitializeInventory()
    {
        // Проверяем, не инициализированы ли уже слоты
        if (inventorySlots != null && inventorySlots.Count > 0)
            return;

        inventorySlots = new List<InventorySlotUI>();

        // Если есть prefab и grid, создаем слоты
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

        // Подписываемся на события инвентаря
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

        // Управление курсором
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
