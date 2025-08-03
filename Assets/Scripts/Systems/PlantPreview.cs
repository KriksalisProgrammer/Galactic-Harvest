using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantPreview : MonoBehaviour
{
    [SerializeField] private Renderer areaRenderer;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material redMat;

    public bool CanPlace { get; private set; }

    void Update()
    {
        CheckCollision();
    }

    void CheckCollision()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, Vector3.one * 0.5f);
        CanPlace = colliders.Length == 0;
        areaRenderer.material = CanPlace ? greenMat : redMat;
    }
}
