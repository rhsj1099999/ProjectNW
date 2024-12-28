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
            Debug.Assert(false, "�浹ü�� �ݵ�� �ʿ��մϴ�");
            Debug.Break();
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("������ �� �ִ� ��ü�� ���� �����ߴ�");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("������ �� �ִ� ��ü�� ���� �����");
    }
}
