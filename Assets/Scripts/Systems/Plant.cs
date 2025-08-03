using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float growTime = 30f; 
    private float timer = 0f;
    private bool isGrown = false;

    public GameObject seedlingModel;
    public GameObject grownModel;

    void Start()
    {
        seedlingModel.SetActive(true);
        grownModel.SetActive(false);
    }

    void Update()
    {
        if (!isGrown)
        {
            timer += Time.deltaTime;
            if (timer >= growTime)
            {
                Grow();
            }
        }
    }

    void Grow()
    {
        isGrown = true;
        seedlingModel.SetActive(false);
        grownModel.SetActive(true);
    }

    public bool IsGrown()
    {
        return isGrown;
    }
}

