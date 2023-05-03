using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapesPuzzle : MonoBehaviour
{
    public GameObject tasksObj;
    bool circle, square, triangle, completed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "circle")
        {
            circle = true;
        } else if (other.tag == "triangle")
        {
            triangle = true;
        } else if (other.tag == "square")
        {
            square = true;
        }
    }

    void Update()
    {
        if (!completed)
        {
            if (triangle && square && circle)
            {
                completed = true;
                tasksObj.GetComponent<Tasks>().task2Comp = true;
            }
        }
    }
}
