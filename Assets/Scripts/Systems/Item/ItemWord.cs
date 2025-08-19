using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    public Item itemData;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HotbarController hotbar = other.GetComponent<HotbarController>();
            if (hotbar != null)
            {
                hotbar.AddItem(itemData, amount);
                Destroy(gameObject); 
            }
        }
    }
}
