using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTestScript_2 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider_2");
    }
}
