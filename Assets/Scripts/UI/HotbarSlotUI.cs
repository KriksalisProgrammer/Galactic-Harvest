using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image iconImage;
    public Image selectedFrame;
    public Image backgroundImage;

    [Header("Visual Settings")]
    public Color normalBackgroundColor = new Color(1f, 1f, 1f, 0.3f);
    public Color selectedBackgroundColor = new Color(1f, 1f, 0.5f, 0.6f);
    public Color emptyIconColor = new Color(1f, 1f, 1f, 0f);
    public Color normalIconColor = new Color(1f, 1f, 1f, 1f);

    private PlantSeed plantSeed;

    private void Start()
    {
        // �������������� ��������� ���������
        InitializeSlot();
    }

    private void InitializeSlot()
    {
        // ������������� ��������� �������� ��� UI ���������
        if (backgroundImage != null)
        {
            backgroundImage.color = normalBackgroundColor;
        }

        if (selectedFrame != null)
        {
            selectedFrame.enabled = false;
        }

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }
    }

    public void SetItem(PlantSeed seed)
    {
        plantSeed = seed;

        if (iconImage != null)
        {
            if (seed != null && seed.icon != null)
            {
                // ������������� ������ ������
                iconImage.sprite = seed.icon;
                iconImage.enabled = true;
                iconImage.color = normalIconColor;
            }
            else
            {
                // ������� ���� ���� ���� null ��� ��� ������
                ClearSlotVisuals();
            }
        }

        // ��������� ������ ���� � ����������� �� ������� ��������
        UpdateBackgroundVisual();
    }

    public void ClearSlot()
    {
        plantSeed = null;
        ClearSlotVisuals();
        UpdateBackgroundVisual();
    }

    private void ClearSlotVisuals()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.color = emptyIconColor;
        }
    }

    private void UpdateBackgroundVisual()
    {
        if (backgroundImage != null)
        {
            // ������ ��������� ��� ��� ������ ������
            Color targetColor = HasItem() ? normalBackgroundColor :
                               new Color(normalBackgroundColor.r * 0.5f,
                                       normalBackgroundColor.g * 0.5f,
                                       normalBackgroundColor.b * 0.5f,
                                       normalBackgroundColor.a * 0.7f);
            backgroundImage.color = targetColor;
        }
    }

    public bool HasItem()
    {
        return plantSeed != null;
    }

    public PlantSeed GetPlantSeed()
    {
        return plantSeed;
    }

    public void SetSelected(bool isSelected)
    {
        // ��������� ������ ���������
        if (selectedFrame != null)
        {
            selectedFrame.enabled = isSelected;
        }

        // ��������� ������ ����
        if (backgroundImage != null)
        {
            if (isSelected)
            {
                backgroundImage.color = selectedBackgroundColor;
            }
            else
            {
                // ���������� ���������� ���� � ����������� �� ������� ��������
                UpdateBackgroundVisual();
            }
        }

        // �������������� ������ ��� ������ ��� ���������
        if (iconImage != null && HasItem())
        {
            iconImage.color = isSelected ?
                new Color(normalIconColor.r, normalIconColor.g, normalIconColor.b, 1f) :
                normalIconColor;
        }
    }

    // �������������� ������ ��� �������
    public string GetSlotInfo()
    {
        if (HasItem())
        {
            return $"Slot contains: {plantSeed.itemName}";
        }
        return "Empty slot";
    }

    // ����� ��� �������� ������������ ��������� UI
    public bool ValidateUIComponents()
    {
        bool isValid = true;

        if (iconImage == null)
        {
            Debug.LogWarning($"IconImage not assigned on {gameObject.name}");
            isValid = false;
        }

        if (selectedFrame == null)
        {
            Debug.LogWarning($"SelectedFrame not assigned on {gameObject.name}");
            isValid = false;
        }

        if (backgroundImage == null)
        {
            Debug.LogWarning($"BackgroundImage not assigned on {gameObject.name}");
            isValid = false;
        }

        return isValid;
    }

    // ����� ��� �������������� �������� ����� ����������� (�����������)
    public void SetSelectedAnimated(bool isSelected, float animationSpeed = 5f)
    {
        if (selectedFrame != null)
        {
            selectedFrame.enabled = isSelected;
        }

        // ����� �������� �������� ����� �����
        StartCoroutine(AnimateBackgroundColor(isSelected, animationSpeed));
    }

    private System.Collections.IEnumerator AnimateBackgroundColor(bool isSelected, float speed)
    {
        if (backgroundImage == null) yield break;

        Color startColor = backgroundImage.color;
        Color targetColor = isSelected ? selectedBackgroundColor : normalBackgroundColor;

        if (!HasItem() && !isSelected)
        {
            targetColor = new Color(normalBackgroundColor.r * 0.5f,
                                  normalBackgroundColor.g * 0.5f,
                                  normalBackgroundColor.b * 0.5f,
                                  normalBackgroundColor.a * 0.7f);
        }

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * speed;
            backgroundImage.color = Color.Lerp(startColor, targetColor, elapsed);
            yield return null;
        }

        backgroundImage.color = targetColor;
    }
}