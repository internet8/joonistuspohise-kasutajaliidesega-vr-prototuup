using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatBuilding : MonoBehaviour
{
    public GameObject treeObj, planksObj, logsObj, boatObj, tasksObj;
    public ParticleSystem particleEffect;
    public AudioClip axe, saw, hammer;
    AudioSource audioSource;
    int progressIndex = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "saw" && progressIndex == 0)
        {
            progressIndex++;
            treeObj.SetActive(false);
            logsObj.SetActive(true);
            particleEffect.Play();
            audioSource.PlayOneShot(saw, 1f);
        }
        else if (other.tag == "axe" && progressIndex == 1)
        {
            progressIndex++;
            logsObj.SetActive(false);
            planksObj.SetActive(true);
            particleEffect.Play();
            audioSource.PlayOneShot(axe, 1f);
        }
        else if (other.tag == "hammer" && progressIndex == 2)
        {
            progressIndex++;
            planksObj.SetActive(false);
            boatObj.SetActive(true);
            particleEffect.Play();
            tasksObj.GetComponent<Tasks>().task3Comp = true;
            audioSource.PlayOneShot(hammer, 1f);
        }
    }
}
