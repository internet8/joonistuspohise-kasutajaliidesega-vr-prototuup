using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaitSpawner : MonoBehaviour
{
    public GameObject bait;
    public GameObject fire;
    public float baitSpawnHeight = 5;
    public GameObject ratAI;
    HivemindRatController ai;

    private void Start()
    {
        ai = ratAI.GetComponent<HivemindRatController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject clone = Instantiate(bait);
            clone.transform.position = new Vector3(transform.position.x, baitSpawnHeight, transform.position.z);
            ai.AddBait(clone);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ai.enableEvading = !ai.enableEvading;
            ai.enablePlayerSeeking = !ai.enablePlayerSeeking;
        }
        else if (Input.GetMouseButtonDown(2))
        {
            ai.enableBaitSeeking = !ai.enableBaitSeeking;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            fire.SetActive(!fire.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ai.RemoveBaits();
        }
    }
}
