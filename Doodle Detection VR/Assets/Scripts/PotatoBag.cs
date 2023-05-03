using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotatoBag : MonoBehaviour
{
    public GameObject tasksObj;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "lantern")
        {
            tasksObj.GetComponent<Tasks>().task4Comp = true;
        }
    }
}
