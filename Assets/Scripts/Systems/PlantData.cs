using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlant", menuName = "Farming/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;
    
    [System.Serializable]
    public class GrowthStage
    {
        public GameObject modelPrefab;     
        public float timeToNextStage;     
    }
    
    public List<GrowthStage> growthStages;
    public int fruitAmount;               
    public GameObject fruitPrefab;        
    

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
}
