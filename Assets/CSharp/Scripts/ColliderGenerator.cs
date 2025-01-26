using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class CapsuleColliderDesc
{
    public string _colliderName = "";
    public GameObject _capsulePrefab = null;
    public Transform _startTransform = null;
    public Transform _endTransform = null;
    public bool _transfomrBetweenIsHemi = false;
    public float _radius = 1.0f;
    public float _heightRatio = 1.0f;
}



public class ColliderGenerator : MonoBehaviour
{
    //������ ���۵Ǹ� Humanoid �� ���� ������Ʈ�� �������ִ� ��ü
    //�޽����ٰ� Ray Casting�� �ϱ⿡�� �ﰢ���� �ʹ� ������ ���꿡 ���ϰ� �ִ�.
    //[SerializeField] private List<CapsuleColliderDesc> _colliderDesc = new List<CapsuleColliderDesc>();
    //private List<GameObject> _createdColliders = new List<GameObject>();

    [SerializeField] private CapsuleColliderDesc _desc = new CapsuleColliderDesc();

    public void GenerateCapsulecollider()
    {
        if (_desc._colliderName == "")
        {
            Debug.Log("�̸��� �������ּ���");
            return;
        }

        if (_desc._startTransform == null || _desc._endTransform == null)
        {
            Debug.Log("Start, end Transform�� �������ּ���");
            return;
        }

        if (_desc._capsulePrefab.GetComponent<CapsuleCollider>() == null)
        {
            Debug.Log("�ش� �������� ĸ��������Ʈ�� ���������� �ʽ��ϴ�");
            return;
        }

        float betweenLength = Vector3.Distance(_desc._startTransform.position, _desc._endTransform.position);

        if (_desc._transfomrBetweenIsHemi == true)
        {
            //�糡���� ���� ���������� ����ϴ� = �� ������̴ϴ�.
            betweenLength += _desc._radius * 2.0f;
        }
        

        GameObject createdCapsule = Instantiate(_desc._capsulePrefab);
        createdCapsule.transform.SetParent(_desc._startTransform);

        createdCapsule.transform.localScale = Vector3.one;
        createdCapsule.transform.localRotation = Quaternion.identity;
        createdCapsule.transform.localPosition = Vector3.zero;

        Vector3 toSecondDir = (_desc._endTransform.position - _desc._startTransform.position).normalized;
        createdCapsule.transform.position += toSecondDir * betweenLength / 2.0f;

        CapsuleCollider collider = createdCapsule.GetComponent<CapsuleCollider>();

        collider.radius = _desc._radius;
        collider.height = betweenLength;
        collider.center = Vector3.zero;

        float[] dotRets = new float[3]
        {
            Vector3.Dot(_desc._startTransform.right, toSecondDir),
            Vector3.Dot(_desc._startTransform.up, toSecondDir),
            Vector3.Dot(_desc._startTransform.forward, toSecondDir)
        };

        int correctIndex = 0;
        float minDot = -1.0f;

        for (int i = 0; i < 3; i++)
        {
            if (minDot < dotRets[i])
            {
                minDot = dotRets[i];
                correctIndex = i;
            }
        }

        collider.direction = correctIndex;
    }


    private void Awake()
    {
    }

    private void Update()
    {
    }
}
