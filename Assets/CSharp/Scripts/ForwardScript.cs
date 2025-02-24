using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardScript : MonoBehaviour
{
    [SerializeField] private Vector3 _forwardVector = new Vector3(0.0f, 0.0f, 1.0f);
    [SerializeField] private Vector3 _upwardVector = new Vector3(0.0f, 1.0f, 0.0f);


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            transform.rotation = Quaternion.LookRotation(_forwardVector, _upwardVector/*��� �̻��� ����Ʈ�� ���ϱ� ���� ����*/);
        }
    }
}
