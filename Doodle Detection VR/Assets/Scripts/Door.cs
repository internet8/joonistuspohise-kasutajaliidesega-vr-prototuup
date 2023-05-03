using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Material dissolveMat;
    public float spawnTime = 3;
    public GameObject tasksObj;
    public AudioClip door;
    float timeSpawningStarted = 0;
    Animation openAnimation;
    BoxCollider bc;
    bool isOpened, spawning;

    void Start()
    {
        openAnimation = GetComponent<Animation>();
        bc = GetComponent<BoxCollider>();
        dissolveMat.SetFloat("_CutoffHeight", -1.1f);
    }

    public void SpawnDoor()
    {
        if (!isOpened)
        {
            spawning = true;
            bc.enabled = true;
            timeSpawningStarted = Time.time;
        }
    }

    void Update()
    {
        if (spawning)
        {
            float progress = Time.time - timeSpawningStarted;
            if (progress < spawnTime)
            {
                dissolveMat.SetFloat("_CutoffHeight", Mathf.Lerp(-1.1f, 3f, progress / spawnTime));
            } else if (!isOpened)
            {
                openAnimation.Play();
                GetComponent<AudioSource>().PlayOneShot(door, 2f);
                isOpened = true;
                spawning = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            tasksObj.GetComponent<Tasks>().task5Comp = true;
        }
    }
}
