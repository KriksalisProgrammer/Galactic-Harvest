using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    [Header("Save Settings")]
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 60f; // секунд

    private string saveFilePath;
    private Coroutine autoSaveCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

            if (autoSave)
            {
                autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        // Сохраняем инвентарь
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < 24; i++) // Размер инвентаря
            {
                InventorySlot slot = InventoryManager.Instance.GetInventorySlot(i);
                if (slot != null && !slot.IsEmpty())
                {
                    saveData.inventorySlots.Add(new SlotSaveData(slot.item.name, slot.quantity));
                }
                else
                {
                    saveData.inventorySlots.Add(new SlotSaveData("", 0));
                }
            }

            // Сохраняем хотбар
            for (int i = 0; i < 8; i++) // Размер хотбара
            {
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(i);
                if (slot != null && !slot.IsEmpty())
                {
                    saveData.hotbarSlots.Add(new SlotSaveData(slot.item.name, slot.quantity));
                }
                else
                {
                    saveData.hotbarSlots.Add(new SlotSaveData("", 0));
                }
            }
        }

        // Сохраняем позицию игрока
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            saveData.playerPosition = player.transform.position;
            saveData.playerRotation = player.transform.eulerAngles;
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Игра сохранена: " + saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка сохранения: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("Файл сохранения не найден");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData != null)
            {
                LoadInventoryData(saveData);
                LoadPlayerData(saveData);
                Debug.Log("Игра загружена");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка загрузки: " + e.Message);
        }
    }

    private void LoadInventoryData(SaveData saveData)
    {
        if (InventoryManager.Instance == null) return;

        // Очищаем текущий инвентарь
        for (int i = 0; i < saveData.inventorySlots.Count && i < 24; i++)
        {
            SlotSaveData slotData = saveData.inventorySlots[i];
            if (!string.IsNullOrEmpty(slotData.itemName) && slotData.quantity > 0)
            {
                Item item = LoadItemByName(slotData.itemName);
                if (item != null)
                {
                    InventorySlot slot = InventoryManager.Instance.GetInventorySlot(i);
                    if (slot != null)
                    {
                        slot.item = item;
                        slot.quantity = slotData.quantity;
                        InventoryManager.Instance.OnInventoryChanged?.Invoke(i, slot);
                    }
                }
            }
        }

        // Загружаем хотбар
        for (int i = 0; i < saveData.hotbarSlots.Count && i < 8; i++)
        {
            SlotSaveData slotData = saveData.hotbarSlots[i];
            if (!string.IsNullOrEmpty(slotData.itemName) && slotData.quantity > 0)
            {
                Item item = LoadItemByName(slotData.itemName);
                if (item != null)
                {
                    InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(i);
                    if (slot != null)
                    {
                        slot.item = item;
                        slot.quantity = slotData.quantity;
                        InventoryManager.Instance.OnHotbarChanged?.Invoke(i, slot);
                    }
                }
            }
        }
    }

    private void LoadPlayerData(SaveData saveData)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // Отключаем CharacterController для телепортации
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = saveData.playerPosition;
                player.transform.eulerAngles = saveData.playerRotation;
                controller.enabled = true;
            }
        }
    }

    private Item LoadItemByName(string itemName)
    {
        // Пробуем найти в разных папках Resources
        Item item = Resources.Load<Item>($"ScriptableObjects/Plant/{itemName}");
        if (item == null)
        {
            item = Resources.Load<Item>($"ScriptableObjects/{itemName}");
        }
        if (item == null)
        {
            item = Resources.Load<Item>(itemName);
        }

        return item;
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveGame();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Сохранение удалено");
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }

    private void OnDestroy()
    {
        SaveGame();
    }
}