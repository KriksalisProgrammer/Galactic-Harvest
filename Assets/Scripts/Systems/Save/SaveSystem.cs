using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    [Header("Save Settings")]
    public string saveFileName = "GameSave.json";
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 60f; // seconds

    private string savePath;
    private float autoSaveTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (autoSaveEnabled)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }

    public void SaveGame()
    {
        try
        {
            SaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, jsonData);

            Debug.Log($"Game saved successfully to: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("Save file not found. Starting new game.");
                return false;
            }

            string jsonData = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

            ApplySaveData(saveData);

            Debug.Log("Game loaded successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }

    private void AutoSave()
    {
        SaveGame();
        Debug.Log("Auto-save completed");
    }

    private SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        // Save inventory data
        if (InventoryManager.Instance != null)
        {
            // Save main inventory
            for (int i = 0; i < 24; i++) // Assuming 24 inventory slots
            {
                InventorySlot slot = InventoryManager.Instance.GetInventorySlot(i);
                if (slot != null && !slot.IsEmpty())
                {
                    saveData.inventorySlots.Add(new SlotSaveData(slot.item.name, slot.quantity));
                }
                else
                {
                    saveData.inventorySlots.Add(new SlotSaveData());
                }
            }

            // Save hotbar
            for (int i = 0; i < 8; i++) // Assuming 8 hotbar slots
            {
                InventorySlot slot = InventoryManager.Instance.GetHotbarSlot(i);
                if (slot != null && !slot.IsEmpty())
                {
                    saveData.hotbarSlots.Add(new SlotSaveData(slot.item.name, slot.quantity));
                }
                else
                {
                    saveData.hotbarSlots.Add(new SlotSaveData());
                }
            }
        }

        // Save player position and rotation
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            saveData.playerPosition = playerController.GetPlayerPosition();
            saveData.playerRotation = playerController.GetPlayerRotation();
        }

        return saveData;
    }

    private void ApplySaveData(SaveData saveData)
    {
        // Load inventory data
        if (InventoryManager.Instance != null && saveData.inventorySlots != null)
        {
            // Load main inventory
            for (int i = 0; i < saveData.inventorySlots.Count && i < 24; i++)
            {
                SlotSaveData slotData = saveData.inventorySlots[i];
                if (!string.IsNullOrEmpty(slotData.itemName))
                {
                    Item item = LoadItemByName(slotData.itemName);
                    if (item != null)
                    {
                        InventoryManager.Instance.AddItem(item, slotData.quantity);
                    }
                }
            }

            // Load hotbar - need to implement direct slot setting
            // This would require additional methods in InventoryManager
        }

        // Load player position
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.transform.position = saveData.playerPosition;
            playerController.transform.eulerAngles = saveData.playerRotation;
        }
    }

    private Item LoadItemByName(string itemName)
    {
        // Try to load from Resources folder
        Item item = Resources.Load<Item>($"ScriptableObjects/{itemName}");
        if (item == null)
        {
            // Try alternative paths
            item = Resources.Load<Item>($"ScriptableObjects/Plant/{itemName}");
        }

        if (item == null)
        {
            Debug.LogWarning($"Could not find item: {itemName}");
        }

        return item;
    }

    // Called when application is paused (mobile) or loses focus
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveEnabled)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveEnabled)
        {
            SaveGame();
        }
    }

    // Called when application quits
    private void OnApplicationQuit()
    {
        if (autoSaveEnabled)
        {
            SaveGame();
        }
    }

    // Public methods for manual save/load
    public void QuickSave()
    {
        SaveGame();
    }

    public void QuickLoad()
    {
        LoadGame();
    }
}