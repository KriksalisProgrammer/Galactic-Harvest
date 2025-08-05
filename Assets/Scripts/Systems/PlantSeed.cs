using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Seed", menuName = "Inventory/Plant Seed")]
public class PlantSeed : Item
{
    [Header("Plant Settings")]
    public PlantData plantData;        // ������ ��������
    public GameObject plantPrefab;     // ������ ��� ������� � ���
    public GameObject previewPrefab;   // ������ ��� �������������

    [Header("Planting Requirements")]
    public LayerMask validGroundLayers = -1;  // �� ����� ����� ����� ������
    public float minPlantingDistance = 1f;    // ����������� ���������� ����� ����������

    private void OnValidate()
    {
        // ������������� ������������� ���������� ��������� ��� �����
        itemType = ItemType.Plant;
        isConsumable = true;  // ������ ����������� ��� �������������
        isEquippable = false;

        // ���� �� ������� ��� ��������, ����� �� plantData
        if (string.IsNullOrEmpty(itemName) && plantData != null)
        {
            itemName = $"������ {plantData.plantName}";
        }
    }

    public override bool IsValid()
    {
        bool baseValid = base.IsValid();
        return baseValid && plantData != null && plantPrefab != null;
    }

    public override bool CanUse(GameObject user)
    {
        if (!base.CanUse(user)) return false;

        // ��������� ������� ���������� ��� �������
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting == null)
        {
            Debug.LogWarning("��� ���������� PlayerPlanting ��� ������� �����!");
            return false;
        }

        // ��������� ������������ ������ ��������
        if (plantData == null)
        {
            Debug.LogError("� ������ ��� ������ ��������!");
            return false;
        }

        string errorMessage;
        if (!plantData.IsValid(out errorMessage))
        {
            Debug.LogError($"������������ ������ ��������: {errorMessage}");
            return false;
        }

        return true;
    }

    public override void Use(GameObject user)
    {
        if (!CanUse(user)) return;

        // ���������� ����� �������
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting != null)
        {
            planting.SetSelectedSeed(this);
            Debug.Log($"������� ������: {GetDisplayName()}");
        }
    }

    public override void OnEquip(GameObject user)
    {
        // ������ �� �����������, �� ����� �������� ������ ������
        Debug.Log($"������ � �������: {GetDisplayName()}");
    }

    public override void OnUnequip(GameObject user)
    {
        // �������� ����� ������� ��� ����� ��������
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting != null)
        {
            planting.ClearSelectedSeed();
        }
    }

    // ����������� ������ ��� �����
    public bool CanPlantAt(Vector3 position, LayerMask groundMask)
    {
        // ���������, �������� �� ������� ��� �������
        return (validGroundLayers & (1 << LayerMask.NameToLayer("Ground"))) != 0;
    }

    public PlantedPlant PlantAt(Vector3 position, Quaternion rotation)
    {
        if (plantPrefab == null || plantData == null) return null;

        // ������� ��������
        GameObject plantObj = Instantiate(plantPrefab, position, rotation);

        // ����������� ��������� ��������
        PlantedPlant plantComponent = plantObj.GetComponent<PlantedPlant>();
        if (plantComponent != null)
        {
            plantComponent.plantData = plantData;
        }
        else
        {
            Debug.LogError($"� ������� �������� {plantPrefab.name} ��� ���������� PlantedPlant!");
        }

        // ������������� ���
        if (!plantObj.CompareTag("Plant"))
        {
            plantObj.tag = "Plant";
        }

        return plantComponent;
    }
}