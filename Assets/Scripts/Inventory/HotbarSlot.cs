[System.Serializable]
public class HotbarSlot
{
    public Item itemData;
    public int amount;

    public bool IsEmpty => itemData == null;

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }
}
