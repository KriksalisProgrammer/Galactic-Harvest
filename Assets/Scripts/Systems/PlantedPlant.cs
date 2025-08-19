using UnityEngine;
using System.Collections;

public class PlantedPlant : MonoBehaviour
{
    private PlantData plantData;
    private float growthTimer = 0f;
    private bool isMature = false;

    public void Initialize(PlantData data)
    {
        plantData = data;
        growthTimer = 0f;
        isMature = false;
    }

    void Update()
    {
        if (plantData == null || isMature) return;

        growthTimer += Time.deltaTime;
        if (growthTimer >= plantData.growthDuration)
        {
            isMature = true;
            Debug.Log($"{plantData.plantName} выросло!");
        }
    }

    public bool IsMature()
    {
        return isMature;
    }

    public PlantData GetPlantData()
    {
        return plantData;
    }
}
