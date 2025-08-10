using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlant", menuName = "Farming/Plant Data")]
public class PlantData : ScriptableObject
{
    [Header("Basic Info")]
    public string plantName;

    [System.Serializable]
    public class GrowthStage
    {
        public GameObject modelPrefab;
        public float timeToNextStage;
    }

    [Header("Growth Stages")]
    public List<GrowthStage> growthStages = new List<GrowthStage>();

    [Header("Harvest")]
    public int fruitAmount = 1;
    public GameObject fruitPrefab;

    [Header("Requirements")]
    public float minTemperature = 0f;
    public float maxTemperature = 40f;
    public float waterNeed = 1f; 

    public bool IsValid(out string errorMessage)
    {
        errorMessage = "";

        if (string.IsNullOrEmpty(plantName))
        {
            errorMessage = "Не указано название растения";
            return false;
        }
        if (growthStages == null || growthStages.Count == 0)
        {
            errorMessage = "Не указаны стадии роста";
            return false;
        }
        for (int i = 0; i < growthStages.Count; i++)
        {
            var stage = growthStages[i];

            if (stage.modelPrefab == null)
            {
                errorMessage = $"У стадии {i + 1} отсутствует префаб модели";
                return false;
            }
        }

        return true;
    }

    public float GetTotalGrowthTime()
    {
        float total = 0f;
        foreach (var stage in growthStages)
        {
            total += stage.timeToNextStage;
        }
        return total;
    }

    public int GetTotalStages()
    {
        return growthStages.Count;
    }
}