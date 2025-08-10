using System.Collections;
using UnityEngine;

public class PlantedPlant : MonoBehaviour
{
    [Header("Plant Settings")]
    public PlantData plantData;

    [Header("Current State")]
    public int currentStage = 0;
    public float currentGrowthTime = 0f;

    [Header("Debug Info")]
    public bool debugMode = false;

    private GameObject currentModel;
    private bool isFullyGrown = false;
    private bool canHarvest = false;

    public System.Action<PlantedPlant> OnPlantFullyGrown;
    public System.Action<PlantedPlant> OnPlantHarvested;

    private void Start()
    {
        if (plantData != null && plantData.GetStageCount() > 0)
        {
            SpawnCurrentStageModel();
            if (debugMode)
            {
                Debug.Log($"PlantedPlant: Started growing {plantData.plantName} with {plantData.GetStageCount()} stages");
            }
        }
        else
        {
            Debug.LogError("PlantedPlant: No plant data or growth stages assigned!");
        }
    }

    private void Update()
    {
        if (!isFullyGrown && plantData != null)
        {
            GrowPlant();
        }
    }

    private void GrowPlant()
    {
        if (currentStage >= plantData.GetStageCount())
        {
            if (!isFullyGrown)
            {
                isFullyGrown = true;
                canHarvest = true;
                OnPlantFullyGrown?.Invoke(this);
                Debug.Log($"{plantData.plantName} is fully grown and ready for harvest!");
            }
            return;
        }

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return;

        currentGrowthTime += Time.deltaTime;

        if (debugMode && Time.frameCount % 60 == 0) // Debug каждую секунду
        {
            float progress = currentGrowthTime / stage.timeToNextStage;
            Debug.Log($"{plantData.plantName} Stage {currentStage + 1}: {progress * 100:F1}% complete");
        }

        if (currentGrowthTime >= stage.timeToNextStage)
        {
            AdvanceToNextStage();
        }
    }

    private void AdvanceToNextStage()
    {
        currentStage++;
        currentGrowthTime = 0f;

        if (debugMode)
        {
            Debug.Log($"{plantData.plantName} advanced to stage {currentStage + 1}");
        }

        if (currentStage >= plantData.GetStageCount())
        {
            // Plant is fully grown
            isFullyGrown = true;
            canHarvest = true;
            OnPlantFullyGrown?.Invoke(this);
            Debug.Log($"{plantData.plantName} is fully grown and ready for harvest!");
        }
        else
        {
            // Move to next stage
            SpawnCurrentStageModel();
        }
    }

    private void SpawnCurrentStageModel()
    {
        // ИСПРАВЛЕНО: Сохраняем позицию и поворот при смене модели
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // Destroy current model
        if (currentModel != null)
        {
            if (Application.isPlaying)
                Destroy(currentModel);
            else
                DestroyImmediate(currentModel);
        }

        // Spawn new model for current stage
        if (currentStage < plantData.GetStageCount())
        {
            PlantGrowthStage stage = plantData.GetStage(currentStage);
            if (stage != null && stage.modelPrefab != null)
            {
                currentModel = Instantiate(stage.modelPrefab, transform);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;

                if (debugMode)
                {
                    Debug.Log($"Spawned model for stage {currentStage + 1}: {stage.modelPrefab.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No model prefab for stage {currentStage + 1}");
            }
        }
    }

    public bool CanHarvest()
    {
        return canHarvest && isFullyGrown;
    }

    public void Harvest()
    {
        if (!CanHarvest())
        {
            Debug.Log($"Cannot harvest {plantData.plantName} - not ready yet");
            return;
        }

        Debug.Log($"Harvesting {plantData.plantName}");

        // ИСПРАВЛЕНО: Добавляем предметы прямо в инвентарь вместо создания ItemPickup
        InventoryManager inventoryManager = InventoryManager.Instance;

        if (plantData.fruitPrefab != null && plantData.fruitAmount > 0 && inventoryManager != null)
        {
            // Получаем Item из префаба фрукта
            ItemPickup fruitPickup = plantData.fruitPrefab.GetComponent<ItemPickup>();
            if (fruitPickup != null && fruitPickup.item != null)
            {
                // Добавляем прямо в инвентарь
                bool success = inventoryManager.AddItem(fruitPickup.item, plantData.fruitAmount);

                if (success)
                {
                    Debug.Log($"Added {plantData.fruitAmount} {fruitPickup.item.itemName} to inventory");
                }
                else
                {
                    Debug.Log("Inventory full! Creating pickup items on ground");
                    // Если инвентарь полон, создаем предметы на земле
                    CreateGroundPickups();
                }
            }
            else
            {
                // Fallback - создаем предметы на земле если нет ItemPickup компонента
                CreateGroundPickups();
            }
        }

        OnPlantHarvested?.Invoke(this);

        // Destroy the plant after harvest
        StartCoroutine(DestroyAfterDelay(0.1f));
    }

    private void CreateGroundPickups()
    {
        for (int i = 0; i < plantData.fruitAmount; i++)
        {
            // Создаем позицию для дропа вокруг растения (ближе к игроку)
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0.2f,
                Random.Range(-0.5f, 0.5f)
            );
            Vector3 spawnPos = transform.position + randomOffset;

            GameObject fruit = Instantiate(plantData.fruitPrefab, spawnPos, Quaternion.identity);

            // Настраиваем ItemPickup компонент
            ItemPickup pickup = fruit.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = fruit.AddComponent<ItemPickup>();
                pickup.quantity = 1;
                pickup.pickupRange = 2f;
                pickup.autoPickup = true;
                pickup.autoPickupDelay = 0.5f; // Быстрый подбор
            }

            // ИСПРАВЛЕНО: Слабая физика чтобы предметы не разлетались далеко
            Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();
            if (fruitRb != null)
            {
                // Очень слабые силы
                Vector3 randomForce = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-0.5f, 0.5f)
                );
                fruitRb.AddForce(randomForce, ForceMode.Impulse);
                fruitRb.drag = 5f; // Высокое сопротивление
            }
            else
            {
                // Если нет Rigidbody, не добавляем его - пусть просто лежит
                fruit.transform.position = spawnPos;
            }
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public float GetGrowthProgress()
    {
        if (isFullyGrown) return 1f;

        if (plantData == null || currentStage >= plantData.GetStageCount()) return 0f;

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return 0f;

        return currentGrowthTime / stage.timeToNextStage;
    }

    public string GetCurrentStageName()
    {
        if (isFullyGrown) return "Ready to Harvest";
        return $"Stage {currentStage + 1}/{plantData.GetStageCount()}";
    }

    // Method to set plant data (useful for planting system)
    public void InitializePlant(PlantData data)
    {
        plantData = data;
        currentStage = 0;
        currentGrowthTime = 0f;
        isFullyGrown = false;
        canHarvest = false;

        if (debugMode)
        {
            Debug.Log($"Initialized plant: {plantData.plantName}");
        }

        if (plantData != null && plantData.GetStageCount() > 0)
        {
            SpawnCurrentStageModel();
        }
    }

    // ИСПРАВЛЕНО: Улучшенное взаимодействие с мышью
    private void OnMouseDown()
    {
        // Проверяем что не в режиме посадки и инвентарь закрыт
        PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();

        if (planting != null && planting.IsInPlantingMode()) return;
        if (inventoryUI != null && inventoryUI.IsInventoryOpen()) return;

        if (CanHarvest())
        {
            Harvest();
        }
        else
        {
            Debug.Log($"{plantData.plantName} - {GetCurrentStageName()} - Progress: {GetGrowthProgress() * 100:F1}%");
        }
    }

    // ИСПРАВЛЕНО: Добавляем методы для принудительного роста (для тестирования)
    public void ForceGrowToNextStage()
    {
        if (!isFullyGrown)
        {
            AdvanceToNextStage();
        }
    }

    public void ForceFullGrowth()
    {
        while (!isFullyGrown && currentStage < plantData.GetStageCount())
        {
            AdvanceToNextStage();
        }
    }

    // ИСПРАВЛЕНО: Метод для получения времени до следующей стадии
    public float GetTimeToNextStage()
    {
        if (isFullyGrown) return 0f;

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return 0f;

        return stage.timeToNextStage - currentGrowthTime;
    }
}