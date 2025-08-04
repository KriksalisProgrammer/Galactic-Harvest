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
        public GameObject modelPrefab;     // Префаб для визуализации этой стадии
        public float timeToNextStage;      // Время до следующей стадии
    }
    
    public List<GrowthStage> growthStages;
    public int fruitAmount;               // Количество плодов при финальной стадии
    public GameObject fruitPrefab;        // Что появляется при сборе
    
    // ДОБАВЛЯЕМ ТОЛЬКО ЭТИ МЕТОДЫ для совместимости:
    
    /// <summary>
    /// Проверить, корректны ли данные растения
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        errorMessage = "";
        
        // Проверяем название
        if (string.IsNullOrEmpty(plantName))
        {
            errorMessage = "Не указано название растения";
            return false;
        }
        
        // Проверяем наличие стадий роста
        if (growthStages == null || growthStages.Count == 0)
        {
            errorMessage = "Не указаны стадии роста";
            return false;
        }
        
        // Проверяем каждую стадию
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
