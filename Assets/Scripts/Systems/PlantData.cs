using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Plants/Plant Data")]
public class PlantData : ScriptableObject
{
    [Header("Plant Info")]
    public string plantName = "Plant";

    [Header("Growth Stages")]
    public List<PlantGrowthStage> growthStages = new List<PlantGrowthStage>();

    [Header("Harvest")]
    public int fruitAmount = 1;
    public GameObject fruitPrefab;

    public float GetTotalGrowthTime()
    {
        float total = 0f;
        foreach (var stage in growthStages)
        {
            total += stage.timeToNextStage;
        }
        return total;
    }

    public int GetStageCount()
    {
        return growthStages.Count;
    }

    public PlantGrowthStage GetStage(int index)
    {
        if (index >= 0 && index < growthStages.Count)
        {
            return growthStages[index];
        }
        return null;
    }
}