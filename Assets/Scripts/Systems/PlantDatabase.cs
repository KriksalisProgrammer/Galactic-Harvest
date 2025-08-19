using UnityEngine;
using System.Collections.Generic;

public class PlantDatabase : MonoBehaviour
{
    public static PlantDatabase Instance;

    public List<PlantData> allPlants = new List<PlantData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public PlantData GetPlantByName(string name)
    {
        return allPlants.Find(p => p.plantName == name);
    }
}
