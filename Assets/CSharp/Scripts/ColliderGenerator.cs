using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct CapsuleColliderDesc
{
    public Transform _startTransform;
    public Transform _endTransform;
    public float _radiusX;
    public float _radiusZ;
    public float _heightRatio;
}



public class ColliderGenerator : MonoBehaviour
{
    //게임이 시작되면 Humanoid 등 사지 컴포넌트를 생성해주는 객체
    //메쉬에다가 Ray Casting을 하기에는 삼각형이 너무 세밀해 연산에 부하가 있다.
    [SerializeField] private List<CapsuleColliderDesc> _colliderDesc = new List<CapsuleColliderDesc>();
    private List<GameObject> _createdColliders = new List<GameObject>();

    

    private void OnValidate()
    {
        //Collider[] existingColliders = GetComponents<Collider>();
        //foreach (var col in existingColliders)
        //{
        //    DestroyImmediate(col); // Editor 모드에서 즉시 제거
        //}
        
        //foreach (var desc in _colliderDesc)
        //{

        //}
    }

    public void GenerateCapsulecollider(CapsuleColliderDesc desc, string name, GameObject capsulePrefab)
    {
        GameObject createdCapsule = Instantiate(capsulePrefab);
        createdCapsule.transform.SetParent(desc._startTransform);

        createdCapsule.transform.position = desc._startTransform.position;
        createdCapsule.transform.rotation = desc._startTransform.rotation;

        Vector3 between = (desc._endTransform != null)
            ? (desc._startTransform.position + desc._endTransform.position) / 2.0f
            : (desc._startTransform.position);

        createdCapsule.transform.position = between;

        //스케일 조정
        Vector3 scaleVector = createdCapsule.transform.localScale;
        scaleVector.y = (desc._endTransform != null)
            ? (Vector2.Distance(desc._startTransform.position, desc._endTransform.position) / 2.0f) * desc._heightRatio
            : desc._heightRatio;
        scaleVector.x = desc._radiusX / 2.0f;
        scaleVector.z = desc._radiusZ / 2.0f;
        createdCapsule.transform.localScale = scaleVector;

        _createdColliders.Add(createdCapsule);
    }


    private void Awake()
    {
        //_animator = GetComponentInChildren<Animator>();
        //_skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        //Debug.Assert(_animator != null, "이 컴포넌트를 사용할려면 Animator가 있어야 합니다");
        //Debug.Assert(_skinnedMeshRenderer != null, "이 컴포넌트를 사용할려면 Animator가 있어야 합니다");

        

        //for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) 
        //{
        //    Transform target = _animator.GetBoneTransform((HumanBodyBones)i);
        //    if (target == null)
        //    {
        //        continue;
        //    }

        //    Vector3 bonePosition = target.position;

        //    GameObject newGameObject = Instantiate(_dubuggingSphere);

        //    newGameObject.name = ((HumanBodyBones)i).ToString();


        //    newGameObject.transform.position = bonePosition;

        //    newGameObject.transform.SetParent(transform);
        //}
    }

    private void Update()
    {
    }
}
