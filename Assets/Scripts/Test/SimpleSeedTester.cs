using UnityEngine;

public class SimpleSeedTester : MonoBehaviour
{
    [Header("Assign your PlantSeed ScriptableObjects here")]
    public PlantSeed[] testSeeds;

    [Header("Controls")]
    public KeyCode addSeedsKey = KeyCode.T;

    private void Update()
    {
        if (Input.GetKeyDown(addSeedsKey))
        {
            AddSeedsToInventory();
        }
    }

    private void AddSeedsToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is null!");
            return;
        }

        if (testSeeds == null || testSeeds.Length == 0)
        {
            Debug.LogWarning("No test seeds assigned! Create PlantSeed ScriptableObjects and assign them in the inspector.");
            return;
        }

        foreach (PlantSeed seed in testSeeds)
        {
            if (seed != null)
            {
                bool success = InventoryManager.Instance.AddItem(seed, 3);
                Debug.Log($"Added seed {seed.itemName}: {success}");
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 250));

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        GUILayout.Label("=== PLANTING SYSTEM TEST ===", style);
        GUILayout.Space(5);

        GUILayout.Label($"Press {addSeedsKey} - Add seeds to inventory", style);
        GUILayout.Space(10);

        GUILayout.Label("Steps to test planting:", style);
        GUILayout.Label("1. Press T to add seeds", style);
        GUILayout.Label("2. Press Tab to open inventory", style);
        GUILayout.Label("3. Drag seed to hotbar slot", style);
        GUILayout.Label("4. Press Tab to close inventory", style);
        GUILayout.Label("5. Press 1-8 to select seed", style);
        GUILayout.Label("6. Left click to enter planting mode", style);
        GUILayout.Label("7. Aim at ground, left click to plant", style);
        GUILayout.Label("8. Right click/ESC to cancel", style);

        GUILayout.Space(10);

        if (testSeeds != null && testSeeds.Length > 0)
        {
            GUILayout.Label($"Assigned seeds: {testSeeds.Length}", style);
            foreach (var seed in testSeeds)
            {
                if (seed != null)
                {
                    GUILayout.Label($"• {seed.itemName}", style);
                }
            }
        }
        else
        {
            style.normal.textColor = Color.red;
            GUILayout.Label("⚠️ NO SEEDS ASSIGNED!", style);
            style.normal.textColor = Color.yellow;
            GUILayout.Label("Create PlantSeed ScriptableObjects first!", style);
        }

        GUILayout.EndArea();
    }
}