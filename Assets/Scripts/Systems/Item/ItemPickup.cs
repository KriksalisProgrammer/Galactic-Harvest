using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemData;
    public int amount = 1;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            HotbarController hotbar = other.GetComponent<HotbarController>();
            if (hotbar != null && itemData != null)
            {
                hotbar.AddItem(itemData, amount);
                Debug.Log($"Подобрано: {amount} x {itemData.itemName}");
                Destroy(gameObject);
            }
        }
    }
}
