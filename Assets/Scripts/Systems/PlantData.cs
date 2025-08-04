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
        public GameObject modelPrefab;     // ������ ��� ������������ ���� ������
        public float timeToNextStage;      // ����� �� ��������� ������
    }
    
    public List<GrowthStage> growthStages;
    public int fruitAmount;               // ���������� ������ ��� ��������� ������
    public GameObject fruitPrefab;        // ��� ���������� ��� �����
    
    // ��������� ������ ��� ������ ��� �������������:
    
    /// <summary>
    /// ���������, ��������� �� ������ ��������
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        errorMessage = "";
        
        // ��������� ��������
        if (string.IsNullOrEmpty(plantName))
        {
            errorMessage = "�� ������� �������� ��������";
            return false;
        }
        
        // ��������� ������� ������ �����
        if (growthStages == null || growthStages.Count == 0)
        {
            errorMessage = "�� ������� ������ �����";
            return false;
        }
        
        // ��������� ������ ������
        for (int i = 0; i < growthStages.Count; i++)
        {
            var stage = growthStages[i];
            
            if (stage.modelPrefab == null)
            {
                errorMessage = $"� ������ {i + 1} ����������� ������ ������";
                return false;
            }
        }
        
        return true;
    }
}
