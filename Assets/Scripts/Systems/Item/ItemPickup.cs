using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public Item item;
    public int quantity = 1;

    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public bool autoPickup = true;
    public float autoPickupDelay = 1f;

    [Header("Visual Effects")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    private float spawnTime;
    private bool canBePickedUp = false;
    private AudioSource audioSource;

    private void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Add some random rotation to make it look more natural
        if (GetComponent<Rigidbody>() != null)
        {
            Vector3 randomTorque = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f)
            );
            GetComponent<Rigidbody>().AddTorque(randomTorque);
        }
    }

    private void Update()
    {
        // Enable pickup after delay
        if (!canBePickedUp && Time.time - spawnTime >= autoPickupDelay)
        {
            canBePickedUp = true;
        }

        if (canBePickedUp && autoPickup)
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
        if (!canBePickedUp || item == null) return false;

        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null) return false;

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

        // Destroy the pickup object
        Destroy(gameObject, audioSource != null && pickupSound != null ? pickupSound.length : 0f);
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
                    Destroy(child.gameObject);
                }
            }

            // Create new visual
            GameObject visual = Instantiate(item.prefab, transform);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

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
                if (!(script is Renderer) && !(script is Transform))
                {
                    script.enabled = false;
                }
            }
        }
    }

    // Manual pickup trigger (for interaction system)
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