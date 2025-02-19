using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTestScript_1 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter_1");
    }
}
