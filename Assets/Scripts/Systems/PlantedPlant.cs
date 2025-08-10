using System.Collections;
using UnityEngine;

public class PlantedPlant : MonoBehaviour
{
    [Header("Plant Settings")]
    public PlantData plantData;

    [Header("Current State")]
    public int currentStage = 0;
    public float currentGrowthTime = 0f;

    private GameObject currentModel;
    private bool isFullyGrown = false;
    private bool canHarvest = false;

    public System.Action<PlantedPlant> OnPlantFullyGrown;
    public System.Action<PlantedPlant> OnPlantHarvested;

    private void Start()
    {
        if (plantData != null && plantData.GetStageCount() > 0)
        {
            SpawnCurrentStageModel();
        }
        else
        {
            Debug.LogError("PlantedPlant: No plant data or growth stages assigned!");
        }
    }

    private void Update()
    {
        if (!isFullyGrown && plantData != null)
        {
            GrowPlant();
        }
    }

    private void GrowPlant()
    {
        if (currentStage >= plantData.GetStageCount()) return;

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return;

        currentGrowthTime += Time.deltaTime;

        if (currentGrowthTime >= stage.timeToNextStage)
        {
            AdvanceToNextStage();
        }
    }

    private void AdvanceToNextStage()
    {
        currentStage++;
        currentGrowthTime = 0f;

        if (currentStage >= plantData.GetStageCount())
        {
            // Plant is fully grown
            isFullyGrown = true;
            canHarvest = true;
            OnPlantFullyGrown?.Invoke(this);
            Debug.Log($"{plantData.plantName} is fully grown and ready for harvest!");
        }
        else
        {
            // Move to next stage
            SpawnCurrentStageModel();
        }
    }

    private void SpawnCurrentStageModel()
    {
        // Destroy current model
        if (currentModel != null)
        {
            DestroyImmediate(currentModel);
        }

        // Spawn new model for current stage
        if (currentStage < plantData.GetStageCount())
        {
            PlantGrowthStage stage = plantData.GetStage(currentStage);
            if (stage != null && stage.modelPrefab != null)
            {
                currentModel = Instantiate(stage.modelPrefab, transform);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;
            }
        }
    }

    public bool CanHarvest()
    {
        return canHarvest && isFullyGrown;
    }

    public void Harvest()
    {
        if (!CanHarvest()) return;

        // Spawn fruits/harvest items
        if (plantData.fruitPrefab != null && plantData.fruitAmount > 0)
        {
            for (int i = 0; i < plantData.fruitAmount; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 1f;
                spawnPos.y = transform.position.y + 0.5f;

                GameObject fruit = Instantiate(plantData.fruitPrefab, spawnPos, Quaternion.identity);

                // Add some random force to make fruits scatter
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
        }

        OnPlantHarvested?.Invoke(this);

        // Destroy the plant after harvest
        StartCoroutine(DestroyAfterDelay(0.1f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public float GetGrowthProgress()
    {
        if (isFullyGrown) return 1f;

        if (plantData == null || currentStage >= plantData.GetStageCount()) return 0f;

        PlantGrowthStage stage = plantData.GetStage(currentStage);
        if (stage == null) return 0f;

        return currentGrowthTime / stage.timeToNextStage;
    }

    public string GetCurrentStageName()
    {
        if (isFullyGrown) return "Ready to Harvest";
        return $"Stage {currentStage + 1}/{plantData.GetStageCount()}";
    }

    // Method to set plant data (useful for planting system)
    public void InitializePlant(PlantData data)
    {
        plantData = data;
        currentStage = 0;
        currentGrowthTime = 0f;
        isFullyGrown = false;
        canHarvest = false;

        if (plantData != null && plantData.GetStageCount() > 0)
        {
            SpawnCurrentStageModel();
        }
    }

    private void OnMouseDown()
    {
        if (CanHarvest())
        {
            Harvest();
        }
    }
}