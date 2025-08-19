using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "GalacticHarvest/PlantData")]
public class PlantData : ScriptableObject
{
    public string plantName;
    public GameObject plantedPrefab;
    public float growthDuration = 10f;
    public int yieldAmount = 5; 
}