using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public List<SlotSaveData> inventorySlots = new List<SlotSaveData>();
    public List<SlotSaveData> hotbarSlots = new List<SlotSaveData>();
    public Vector3 playerPosition;
    public Vector3 playerRotation;
}
