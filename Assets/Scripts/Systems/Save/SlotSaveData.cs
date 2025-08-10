using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SlotSaveData
{
    public string itemName;
    public int quantity;

    public SlotSaveData() { }

    public SlotSaveData(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}
