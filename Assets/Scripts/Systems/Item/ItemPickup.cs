using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private Item item;
    [SerializeField] private int quantity = 1;

    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask playerLayer = 1;

    private Vector3 startPosition;
    private bool canPickup = true;
    private PlayerController player;

    private void Start()
    {
        startPosition = transform.position;
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (!canPickup) return;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= pickupRange)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryPickup();
                }
            }
        }
    }

    private void TryPickup()
    {
        if (item == null || InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.AddItemToHotbar(item, quantity))
        {
            PickupItem();
            return;
        }

        if (InventoryManager.Instance.AddItem(item, quantity))
        {
            PickupItem();
        }
        else
        {
            Debug.Log("Инвентарь полон!");
        }
    }

    private void PickupItem()
    {
        canPickup = false;
        Debug.Log($"Подобран предмет: {item.itemName} x{quantity}");

        StartCoroutine(PickupAnimation());
    }

    private IEnumerator PickupAnimation()
    {
        float duration = 0.3f;
        Vector3 targetPos = player.transform.position + Vector3.up;
        Vector3 startPos = transform.position;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, progress);
            yield return null;
        }

        Destroy(gameObject);
    }

    public static GameObject CreateItemPickup(Item item, Vector3 position, int quantity = 1)
    {
        if (item.prefab == null) return null;

        GameObject pickup = Instantiate(item.prefab, position, Quaternion.identity);
        ItemPickup pickupComponent = pickup.GetComponent<ItemPickup>();

        if (pickupComponent == null)
        {
            pickupComponent = pickup.AddComponent<ItemPickup>();
        }

        pickupComponent.item = item;
        pickupComponent.quantity = quantity;

        return pickup;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}