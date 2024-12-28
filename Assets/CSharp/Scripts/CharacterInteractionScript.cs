using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractionScript : MonoBehaviour
{
    [SerializeField] private Collider _collider = null;

    private void Awake()
    {
        if (_collider == null)
        {
            Debug.Assert(false, "충돌체가 반드시 필요합니다");
            Debug.Break();
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("감지할 수 있는 객체가 나를 감지했다");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("감지할 수 있는 객체가 나를 벗어났다");
    }
}
