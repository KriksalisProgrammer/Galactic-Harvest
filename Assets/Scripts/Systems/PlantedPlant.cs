using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantedPlant : MonoBehaviour
{
    [Header("Plant Data")]
    [SerializeField] private PlantData plantData;
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float currentGrowthTime = 0f;
    [SerializeField] private bool isFullyGrown = false;
    [SerializeField] private bool hasBeenHarvested = false;

    [Header("Visual")]
    [SerializeField] private GameObject currentStageModel;

    private void Update()
    {
        if (!isFullyGrown && plantData != null)
        {
            GrowPlant();
        }
    }

    public void Initialize(PlantData data)
    {
        plantData = data;
        currentStage = 0;
        currentGrowthTime = 0f;
        isFullyGrown = false;
        hasBeenHarvested = false;

        UpdatePlantModel();
    }

    private void GrowPlant()
    {
        if (currentStage >= plantData.growthStages.Count) return;

        currentGrowthTime += Time.deltaTime;

        // ���������, ���� �� ���������� � ��������� ������
        if (currentGrowthTime >= plantData.growthStages[currentStage].timeToNextStage)
        {
            currentStage++;
            currentGrowthTime = 0f;

            if (currentStage >= plantData.growthStages.Count)
            {
                isFullyGrown = true;
                Debug.Log($"{plantData.plantName} ��������� �������!");
            }

            UpdatePlantModel();
        }
    }

    private void UpdatePlantModel()
    {
        // ������� ���������� ������
        if (currentStageModel != null)
        {
            DestroyImmediate(currentStageModel);
        }

        // ������� ����� ������ ���� ���� ������
        if (plantData != null && currentStage < plantData.growthStages.Count)
        {
            GameObject stagePrefab = plantData.growthStages[currentStage].modelPrefab;
            if (stagePrefab != null)
            {
                currentStageModel = Instantiate(stagePrefab, transform.position, transform.rotation, transform);
            }
        }
    }

    public bool CanHarvest()
    {
        return isFullyGrown && !hasBeenHarvested;
    }

    public void Harvest()
    {
        if (!CanHarvest()) return;

        hasBeenHarvested = true;

        // ��������� ����� ����� � ���������
        if (InventoryManager.Instance != null)
        {
            Item fruitItem = GetFruitItem();
            if (fruitItem != null)
            {
                // ������� �������� � ������
                if (!InventoryManager.Instance.AddItemToHotbar(fruitItem, plantData.fruitAmount))
                {
                    // ���� �� ����������, ��������� � ���������
                    if (InventoryManager.Instance.AddItem(fruitItem, plantData.fruitAmount))
                    {
                        Debug.Log($"�������: {fruitItem.itemName} x{plantData.fruitAmount}");
                    }
                    else
                    {
                        Debug.Log("��������� �����! ������ �������.");
                    }
                }
                else
                {
                    Debug.Log($"�������: {fruitItem.itemName} x{plantData.fruitAmount}");
                }
            }
        }

        // ���������� �������� ����� ����� ������
        Destroy(gameObject);
    }

    private Item GetFruitItem()
    {
        // �������� ����� ��������������� Item ��� �����
        // ��� ����� ��������� � ����������� �� ����� ��������� ScriptableObjects
        string fruitName = plantData.plantName + "GameObject";
        Item fruitItem = Resources.Load<Item>($"ScriptableObjects/Plant/{fruitName}SO");

        if (fruitItem == null)
        {
            // ������� ��������� Item ���� �� ������
            fruitItem = ScriptableObject.CreateInstance<Item>();
            fruitItem.itemName = plantData.plantName;
            fruitItem.itemType = ItemType.Resource;
        }

        return fruitItem;
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // ��� �������������� � ��������� (���� �����)
    private void OnMouseDown()
    {
        if (CanHarvest())
        {
            // ��������� ���������� �� ������
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= 5f) // ��������� ��������������
                {
                    Harvest();
                }
                else
                {
                    Debug.Log("������� ������ �� ��������");
                }
            }
        }
    }

    // ���������� ��� �������
    private void OnDrawGizmos()
    {
        if (isFullyGrown && !hasBeenHarvested)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else if (isFullyGrown && hasBeenHarvested)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }

    // ������� ��� UI � ������ ������
    public PlantData GetPlantData() => plantData;
    public int GetCurrentStage() => currentStage;
    public float GetGrowthProgress()
    {
        if (plantData == null || currentStage >= plantData.growthStages.Count) return 1f;
        return currentGrowthTime / plantData.growthStages[currentStage].timeToNextStage;
    }
    public bool IsFullyGrown() => isFullyGrown;
    public bool HasBeenHarvested() => hasBeenHarvested;
}