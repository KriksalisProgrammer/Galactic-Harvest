using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Seed", menuName = "Inventory/Plant Seed")]
public class PlantSeed : Item
{
    [Header("Plant Settings")]
    public PlantData plantData;        // Данные растения
    public GameObject plantPrefab;     // Префаб для посадки в мир
    public GameObject previewPrefab;   // Префаб для предпросмотра

    [Header("Planting Requirements")]
    public LayerMask validGroundLayers = -1;  // На каких слоях можно сажать
    public float minPlantingDistance = 1f;    // Минимальное расстояние между растениями

    private void OnValidate()
    {
        // Автоматически устанавливаем правильные настройки для семян
        itemType = ItemType.Plant;
        isConsumable = true;  // Семена расходуются при использовании
        isEquippable = false;

        // Если не указано имя предмета, берем из plantData
        if (string.IsNullOrEmpty(itemName) && plantData != null)
        {
            itemName = $"Семена {plantData.plantName}";
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

        // Проверяем наличие компонента для посадки
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting == null)
        {
            Debug.LogWarning("Нет компонента PlayerPlanting для посадки семян!");
            return false;
        }

        // Проверяем корректность данных растения
        if (plantData == null)
        {
            Debug.LogError("У семени нет данных растения!");
            return false;
        }

        string errorMessage;
        if (!plantData.IsValid(out errorMessage))
        {
            Debug.LogError($"Некорректные данные растения: {errorMessage}");
            return false;
        }

        return true;
    }

    public override void Use(GameObject user)
    {
        if (!CanUse(user)) return;

        // Активируем режим посадки
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting != null)
        {
            planting.SetSelectedSeed(this);
            Debug.Log($"Выбраны семена: {GetDisplayName()}");
        }
    }

    public override void OnEquip(GameObject user)
    {
        // Семена не экипируются, но можно добавить логику выбора
        Debug.Log($"Готовы к посадке: {GetDisplayName()}");
    }

    public override void OnUnequip(GameObject user)
    {
        // Отменяем режим посадки при смене предмета
        PlayerPlanting planting = user.GetComponent<PlayerPlanting>();
        if (planting != null)
        {
            planting.ClearSelectedSeed();
        }
    }

    // Специальные методы для семян
    public bool CanPlantAt(Vector3 position, LayerMask groundMask)
    {
        // Проверяем, подходит ли позиция для посадки
        return (validGroundLayers & (1 << LayerMask.NameToLayer("Ground"))) != 0;
    }

    public PlantedPlant PlantAt(Vector3 position, Quaternion rotation)
    {
        if (plantPrefab == null || plantData == null) return null;

        // Создаем растение
        GameObject plantObj = Instantiate(plantPrefab, position, rotation);

        // Настраиваем компонент растения
        PlantedPlant plantComponent = plantObj.GetComponent<PlantedPlant>();
        if (plantComponent != null)
        {
            plantComponent.plantData = plantData;
        }
        else
        {
            Debug.LogError($"У префаба растения {plantPrefab.name} нет компонента PlantedPlant!");
        }

        // Устанавливаем тег
        if (!plantObj.CompareTag("Plant"))
        {
            plantObj.tag = "Plant";
        }

        return plantComponent;
    }
}