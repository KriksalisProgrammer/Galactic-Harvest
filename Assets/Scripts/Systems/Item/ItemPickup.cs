using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public Item item;
    public int quantity = 1;

    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public bool autoPickup = true;
    public float autoPickupDelay = 0.5f;

    [Header("Visual Effects")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    private float spawnTime;
    private bool canBePickedUp = false;
    private AudioSource audioSource;
    private bool isBeingPickedUp = false;

    private void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ИСПРАВЛЕНО: Убираем лишнее вращение - предметы должны лежать спокойно
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.drag = 3f;
            rb.angularDrag = 5f;
        }

        // ИСПРАВЛЕНО: Добавляем коллайдер если его нет
        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = true;
        }

        // ИСПРАВЛЕНО: Убираем визуальное представление предмета если не задано
        UpdateVisualRepresentation();
    }

    private void Update()
    {
        // Enable pickup after delay
        if (!canBePickedUp && Time.time - spawnTime >= autoPickupDelay)
        {
            canBePickedUp = true;
        }

        if (canBePickedUp && autoPickup && !isBeingPickedUp)
        {
            CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= pickupRange)
            {
                TryPickup(player);
            }
        }
    }

    public bool TryPickup(GameObject player)
    {
        if (!canBePickedUp || item == null || isBeingPickedUp) return false;

        isBeingPickedUp = true;

        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            isBeingPickedUp = false;
            return false;
        }

        // Try to add item to inventory
        bool success = inventoryManager.AddItem(item, quantity);

        if (success)
        {
            OnPickedUp();
            return true;
        }
        else
        {
            Debug.Log("Inventory is full!");
            isBeingPickedUp = false;
            return false;
        }
    }

    private void OnPickedUp()
    {
        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Spawn pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }

        Debug.Log($"Picked up {quantity} {item.itemName}");

        // Destroy the pickup object
        float destroyDelay = audioSource != null && pickupSound != null ? pickupSound.length : 0f;
        Destroy(gameObject, destroyDelay);
    }

    public void SetItem(Item newItem, int newQuantity = 1)
    {
        item = newItem;
        quantity = newQuantity;

        // Update visual representation if needed
        UpdateVisualRepresentation();
    }

    private void UpdateVisualRepresentation()
    {
        if (item != null && item.prefab != null)
        {
            // Clear existing visual
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Visual"))
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }

            // Create new visual
            GameObject visual = Instantiate(item.prefab, transform);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * 0.8f; // Немного меньше оригинала

            // Remove any colliders from the visual (we want to use the pickup's collider)
            Collider[] colliders = visual.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Disable any scripts that shouldn't run on pickup items
            MonoBehaviour[] scripts = visual.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (!(script is Renderer) && !(script is Transform) && !(script is MeshFilter))
                {
                    script.enabled = false;
                }
            }
        }
        else if (item != null)
        {
            // ИСПРАВЛЕНО: Создаем простой куб если нет префаба
            GameObject simpleCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            simpleCube.transform.SetParent(transform);
            simpleCube.transform.localPosition = Vector3.zero;
            simpleCube.transform.localRotation = Quaternion.identity;
            simpleCube.transform.localScale = Vector3.one * 0.5f;
            simpleCube.name = "SimpleVisual";

            // Убираем коллайдер от куба
            Collider cubeCol = simpleCube.GetComponent<Collider>();
            if (cubeCol != null) Destroy(cubeCol);

            // Меняем цвет материала
            Renderer cubeRenderer = simpleCube.GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material = new Material(Shader.Find("Standard"));
                cubeRenderer.material.color = new Color(Random.value, Random.value, Random.value, 1f);
            }
        }
    }

    // Manual pickup trigger (for interaction system) - НЕ ИСПОЛЬЗУЕТСЯ если autoPickup = true
    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickup && other.CompareTag("Player"))
        {
            InteractionSystem interaction = other.GetComponent<InteractionSystem>();
            if (interaction != null)
            {
                interaction.SetInteractable(this, $"Pick up {item?.itemName ?? "Item"}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!autoPickup && other.CompareTag("Player"))
        {
            InteractionSystem interaction = other.GetComponent<InteractionSystem>();
            if (interaction != null)
            {
                interaction.ClearInteractable(this);
            }
        }
    }

    public void Interact(GameObject player)
    {
        TryPickup(player);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}