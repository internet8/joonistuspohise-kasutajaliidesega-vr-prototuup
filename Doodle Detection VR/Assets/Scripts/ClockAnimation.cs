using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockAnimation : MonoBehaviour
{
    public float animationLength = 0.5f;
    public float stillLength = 2f;
    public float size = 2;
    Vector3 sizeVector;
    bool growing = false;
    bool shrinking = false;
    float animationStart = -100;
    public GameObject tasksObject;
    Tasks tasks;

    void Start()
    {
        tasks = tasksObject.GetComponent<Tasks>();
        sizeVector = new Vector3(size, size, size);
    }

    void Update()
    {
        float progress = Time.time - animationStart;
        if (progress < animationLength && growing)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, sizeVector, progress / animationLength);
        } else if (progress < animationLength + stillLength)
        {
            growing = false;
            shrinking = true;
        } else if (progress < stillLength + 2 * animationLength && shrinking)
        {
            transform.localScale = Vector3.Lerp(sizeVector, Vector3.zero,(animationLength - ((stillLength + 2 * animationLength) - progress)) / animationLength);
        } else if (shrinking)
        {
            transform.localScale = Vector3.zero;
            shrinking = false;
        }
    }

    public void Animate()
    {
        growing = true;
        animationStart = Time.time;
        tasks.task1Comp = true;
    }
}
