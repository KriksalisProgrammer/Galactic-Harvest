using System.Collections;
using UnityEngine;

public class PlantedPlant : MonoBehaviour
{
    [Header("Plant Data")]
    public PlantData plantData;

    [Header("Current State")]
    public int currentStage = 0;
    public float stageTimer = 0f;
    public bool isFullyGrown = false;

    [Header("Visual")]
    public GameObject currentStageObject;

    [Header("Events")]
    public System.Action<PlantedPlant> OnPlantFullyGrown;
    public System.Action<PlantedPlant> OnPlantHarvested;
    public System.Action<PlantedPlant, int> OnStageChanged;

    public void InitializePlant(PlantData data)
    {
        plantData = data;
        currentStage = 0;
        stageTimer = 0f;
        isFullyGrown = false;

        if (plantData != null && plantData.growthStages.Count > 0)
        {
            ShowCurrentStage();
        }
        else
        {
            Debug.LogError("PlantData is null or has no growth stages!");
        }
    }

    private void Start()
    {
        if (plantData != null && plantData.growthStages.Count > 0)
        {
            ShowCurrentStage();
        }
        else
        {
            Debug.LogError("PlantData is null or has no growth stages!");
        }
    }

    private void Update()
    {
        if (!isFullyGrown && plantData != null && currentStage < plantData.growthStages.Count)
        {
            stageTimer += Time.deltaTime;

            PlantGrowthStage stage = plantData.GetStage(currentStage);
            if (stage != null && stageTimer >= stage.timeToNextStage)
            {
                GrowToNextStage();
            }
        }
    }

    private void GrowToNextStage()
    {
        currentStage++;
        stageTimer = 0f;

        // Проверяем, достигли ли мы последней стадии
        if (currentStage >= plantData.growthStages.Count)
        {
            currentStage = plantData.growthStages.Count - 1;
            isFullyGrown = true;
            OnPlantFullyGrown?.Invoke(this);
        }

        ShowCurrentStage();
        OnStageChanged?.Invoke(this, currentStage);
    }

    private void ShowCurrentStage()
    {
        // Удаляем предыдущую модель
        if (currentStageObject != null)
        {
            DestroyImmediate(currentStageObject);
        }

        // Создаем новую модель для текущей стадии
        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage != null && stage.modelPrefab != null)
        {
            currentStageObject = Instantiate(stage.modelPrefab, transform.position, transform.rotation, transform);
        }
    }

    public bool CanHarvest()
    {
        return isFullyGrown;
    }

    public void Harvest()
    {
        if (!CanHarvest()) return;

        // Вызываем событие перед сбором урожая
        OnPlantHarvested?.Invoke(this);

        // Создаем предметы для сбора урожая
        if (plantData.fruitPrefab != null)
        {
            for (int i = 0; i < plantData.fruitAmount; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
                spawnPos.y = transform.position.y + 0.5f;
                Instantiate(plantData.fruitPrefab, spawnPos, Quaternion.identity);
            }
        }

        // Уничтожаем растение после сбора урожая
        Destroy(gameObject);
    }

    public float GetGrowthProgress()
    {
        if (isFullyGrown) return 1f;

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return 0f;

        return stageTimer / stage.timeToNextStage;
    }

    public string GetCurrentStageName()
    {
        if (plantData != null && currentStage >= 0 && currentStage < plantData.growthStages.Count)
        {
            return $"Stage {currentStage + 1}";
        }
        return "Unknown";
    }

    public int GetCurrentStageIndex()
    {
        return currentStage;
    }

    public int GetTotalStages()
    {
        return plantData != null ? plantData.growthStages.Count : 0;
    }

    // Для отладки
    private void OnValidate()
    {
        if (Application.isPlaying && plantData != null)
        {
            ShowCurrentStage();
        }
    }
}