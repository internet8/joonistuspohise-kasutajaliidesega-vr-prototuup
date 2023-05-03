using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    public GameObject rat;
    public int ratCount = 1000;

    void Start()
    {
        for (int i = 0; i < ratCount; i++)
        {
            GameObject ratClone = Instantiate(rat);
            ratClone.transform.position = new Vector3(Random.Range(-23f, 23f), 0.25f, Random.Range(-23f, 23f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
