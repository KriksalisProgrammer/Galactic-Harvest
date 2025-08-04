using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantedPlant : MonoBehaviour
{
    [Header("Plant Settings")]
    public PlantData plantData;

    [Header("Debug Info")]
    [SerializeField] private int currentStage = 0;
    [SerializeField] private bool isGrowing = false;
    [SerializeField] private bool isReadyForHarvest = false;

    private GameObject currentModel;
    private Coroutine growthCoroutine;

    void Start()
    {
        if (!ValidatePlantData())
            return;

        StartGrowth();
    }

    private bool ValidatePlantData()
    {
        if (plantData == null)
        {
            Debug.LogError($"PlantData �� �������� ��� {gameObject.name}!");
            return false;
        }

        if (plantData.growthStages == null || plantData.growthStages.Count == 0)
        {
            Debug.LogError($"� PlantData {plantData.name} ��� ������ �����!");
            return false;
        }

        // ���������, ��� ��� ������ ����� �������
        for (int i = 0; i < plantData.growthStages.Count; i++)
        {
            if (plantData.growthStages[i].modelPrefab == null)
            {
                Debug.LogError($"� ������ {i} � {plantData.name} ����������� modelPrefab!");
                return false;
            }
        }

        return true;
    }

    public void StartGrowth()
    {
        if (isGrowing)
        {
            Debug.LogWarning($"�������� {gameObject.name} ��� ������!");
            return;
        }

        growthCoroutine = StartCoroutine(GrowthProcess());
    }

    private IEnumerator GrowthProcess()
    {
        isGrowing = true;
        currentStage = 0;

        for (int stageIndex = 0; stageIndex < plantData.growthStages.Count; stageIndex++)
        {
            var stage = plantData.growthStages[stageIndex];

            // ���������� ���������� ������
            if (currentModel != null)
            {
                Destroy(currentModel);
                currentModel = null;
            }

            // ������� ����� ������ ��� ������� ������
            if (stage.modelPrefab != null)
            {
                currentModel = Instantiate(stage.modelPrefab, transform.position, transform.rotation, transform);
                // ���������� ��������� �������, ����� ������ ���� ����� � ������
                currentModel.transform.localPosition = Vector3.zero;
            }

            currentStage = stageIndex;

            // ���� ��� �� ��������� ������, ���� ����� �� ���������
            if (stageIndex < plantData.growthStages.Count - 1)
            {
                yield return new WaitForSeconds(stage.timeToNextStage);
            }
        }

        // ���� ��������
        isGrowing = false;
        isReadyForHarvest = true;
        currentStage = plantData.growthStages.Count;

        Debug.Log($"�������� {plantData.plantName} ������� � ������ � �����!");
    }
    public bool IsReadyForHarvest()
    {
        return isReadyForHarvest && !isGrowing;
    }

    public bool IsGrowing()
    {
        return isGrowing;
    }

    public float GetGrowthProgress()
    {
        if (plantData == null || plantData.growthStages.Count == 0)
            return 0f;

        return (float)currentStage / plantData.growthStages.Count;
    }

    public string GetCurrentStageName()
    {
        if (isReadyForHarvest)
            return "������ � �����";

        if (plantData == null || currentStage >= plantData.growthStages.Count)
            return "����������";

        return $"������ {currentStage + 1}/{plantData.growthStages.Count}";
    }

    public void Harvest()
    {
        if (!IsReadyForHarvest())
        {
            Debug.LogWarning($"�������� {plantData?.plantName ?? "Unknown"} ��� �� ������ � �����!");
            return;
        }

        if (plantData == null)
        {
            Debug.LogError("���������� ������� ������: plantData == null");
            return;
        }

        // ������� �����
        if (plantData.fruitPrefab != null && plantData.fruitAmount > 0)
        {
            for (int i = 0; i < plantData.fruitAmount; i++)
            {
                // ������������ ����� � ��������� �������
                Vector3 spawnPosition = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 0.3f),
                    Random.Range(-0.5f, 0.5f)
                );

                GameObject fruit = Instantiate(plantData.fruitPrefab, spawnPosition, Quaternion.identity);

                // ��������� ��������� ������� ������, ���� � ��� ���� Rigidbody
                Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();
                if (fruitRb != null)
                {
                    Vector3 randomForce = new Vector3(
                        Random.Range(-2f, 2f),
                        Random.Range(1f, 3f),
                        Random.Range(-2f, 2f)
                    );
                    fruitRb.AddForce(randomForce, ForceMode.Impulse);
                }
            }

            Debug.Log($"������� {plantData.fruitAmount} {plantData.plantName}!");
        }

        // ���������� ��������
        Destroy(gameObject);
    }

    // �������������� ��������� ����� (���� �����)
    public void StopGrowth()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }
        isGrowing = false;
    }

    private void OnDestroy()
    {
        StopGrowth();
    }

    // ��� ������� � ����������
    private void OnValidate()
    {
        if (Application.isPlaying && plantData != null)
        {
            gameObject.name = $"Plant_{plantData.plantName}_{GetInstanceID()}";
        }
    }
}