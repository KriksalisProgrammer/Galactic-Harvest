using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Seed", menuName = "Inventory/Plant Seed")]
public class PlantSeed : Item
{
    [Header("Plant Settings")]
    public PlantData plantData;
    public GameObject plantPrefab;
    public GameObject previewPrefab;

    [Header("Planting Rules")]
    public LayerMask validGroundLayers = -1;
    public float minPlantingDistance = 1f;

    public override void Use()
    {
        PlayerPlanting planting = FindObjectOfType<PlayerPlanting>();
        if (planting != null)
        {
            planting.TryPlantSeed(this);
        }
        else
        {
            Debug.LogWarning("PlayerPlanting component not found!");
        }
    }

    public override bool CanUse()
    {
        return plantData != null && plantPrefab != null;
    }

    private void OnValidate()
    {
        itemType = ItemType.Seed;
        isConsumable = true;
        if (string.IsNullOrEmpty(itemName) && plantData != null)
        {
            itemName = plantData.plantName + " Seed";
        }
    }
}
