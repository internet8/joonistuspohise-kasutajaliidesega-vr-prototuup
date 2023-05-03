using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tasks : MonoBehaviour
{
    public AudioClip introStart, introInstructions, task1Desc, task1DescLong, task1ShortDesc, task2Desc, task2ShortDesc, task3Desc, task3ShortDesc, task4Desc, task4ShortDesc, task5Desc, task5ShortDesc, completed, taskCompleted, grab;
    public GameObject ratSpawner, light, canvas;
    AudioSource audioSource;
    Inputs inputs;
    int textIndex = 0;
    public bool task1Comp, task2Comp, task3Comp, task4Comp, task5Comp, started, finished;
    // aniamtion
    bool lightAnimation;
    float lightAnimationLen = 1;
    float lightAnimationStart = 0;
    void Start()
    {
        inputs = GetComponent<Inputs>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (lightAnimation)
        {
            float progress = Time.time - (lightAnimationStart + lightAnimationLen);
            if (progress < lightAnimationLen)
            {
                light.GetComponent<Light>().intensity = Mathf.Lerp(150, 0, progress / lightAnimationLen);
                ratSpawner.GetComponent<HivemindRatController>().ratSizeFactor = Mathf.Lerp(0, 0.09f, progress / lightAnimationLen);
            } else
            {
                lightAnimation = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && !started)
        {
            audioSource.PlayOneShot(introStart, 1f);
            Time.timeScale = 1;
            started = true;
            textIndex++;
        }
        if (audioSource.isPlaying)
        {
            return;
        }
        if (textIndex == 1 && inputs.repeating)
        {
            textIndex++;
            audioSource.PlayOneShot(task1DescLong, 1f);
        }
        else if (textIndex == 2 && task1Comp)
        {
            textIndex++;
            audioSource.PlayOneShot(taskCompleted, 0.5f);
            audioSource.PlayOneShot(task2Desc, 1f);
        }
        else if (textIndex == 3 && task2Comp)
        {
            textIndex++;
            audioSource.PlayOneShot(taskCompleted, 0.5f);
            audioSource.PlayOneShot(task3Desc, 1f);
        }
        else if (textIndex == 4 && task3Comp)
        {
            textIndex++;
            audioSource.PlayOneShot(taskCompleted, 0.5f);
            audioSource.PlayOneShot(task4Desc, 1f);
            lightAnimation = true;
            lightAnimationStart = Time.time;
            ratSpawner.SetActive(true);
        }
        else if (textIndex == 5 && task4Comp)
        {
            textIndex++;
            canvas.GetComponent<Drawing>().doorDisabled = false;
            audioSource.PlayOneShot(taskCompleted, 0.5f);
            audioSource.PlayOneShot(task5Desc, 1f);
        }
        else if (textIndex == 6 && task5Comp)
        {
            textIndex++;
            audioSource.PlayOneShot(taskCompleted, 0.5f);
            finished = true;
            audioSource.PlayOneShot(completed, 1f);
        }
        else if (finished)
        {
            Application.Quit();
            Debug.Log("Game exit triggered.");
        }
        else if (inputs.repeating)
        {
            switch (textIndex)
            {
                case 2:
                    audioSource.PlayOneShot(task1ShortDesc, 1f);
                    break;
                case 3:
                    audioSource.PlayOneShot(task2ShortDesc, 1f);
                    break;
                case 4:
                    audioSource.PlayOneShot(task3ShortDesc, 1f);
                    break;
                case 5:
                    audioSource.PlayOneShot(task4ShortDesc, 1f);
                    break;
                case 6:
                    audioSource.PlayOneShot(task5ShortDesc, 1f);
                    break;
            }
        }
    }

    public void PlayGrab()
    {
        audioSource.PlayOneShot(grab, 2f);
    }
}
