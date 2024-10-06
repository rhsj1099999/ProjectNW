using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Animations.Rigging;

public struct RiggingDataDesc
{
    public bool _isSocket;
    public HumanBodyBones _boneIndex;
    public WeightedTransformArray _sourceObjects;
    public MultiAimConstraintData.Axis _aimAxis;
    public MultiAimConstraintData.Axis _upAxis;
    public MultiAimConstraintData.WorldUpType _worldUpType;
    public bool _constrainedX;
    public bool _constrainedY;
    public bool _constrainedZ;
    public Vector3 _offset;
    public Vector2 _limits;
}


public class RiggingPublisher : MonoBehaviour
{
    /*----------------------------------------------------------------------------------
     * Owner 가 가지고 있는 리깅 정보를 Unity 뼈 정보로 해석한다음에 똑같이 붙여줄 스크립트
     * 장비 붙이기를 구현하다보니 자식 모델들에게 리깅을 하기 위해서 만들었다.
     * 
     * ****************************************************************
     * 장비들이 장비를 입을 캐릭터랑 똑같은 뼈구조라면 이거 안만들어도 된다*
     * ****************************************************************
    ----------------------------------------------------------------------------------*/

    [SerializeField] private GameObject _ownerSkeleton = null;
    [SerializeField] private GameObject _ownerRig = null;
    [SerializeField] private Animator _ownerAnimator = null;

    private List<RiggingDataDesc> _riggeds = new List<RiggingDataDesc>();

    private void Awake()
    {
        Debug.Assert( _ownerSkeleton != null, "owner Skeleton이 null이여서는 안된다");

        Debug.Assert(_ownerAnimator != null, "owner Animator이 null이여서는 안된다");

        Rig ownerRig = _ownerRig.GetComponent<Rig>();

        Debug.Assert(ownerRig != null, "Rig가 없는 오브젝트에서 동작하려 한다");

        for (int i = 0; i < ownerRig.transform.childCount; i++)
        {
            MultiAimConstraint childsMultiAimConstraint = ownerRig.transform.GetChild(i).gameObject.GetComponent<MultiAimConstraint>();

            Debug.Assert(childsMultiAimConstraint != null, "Aim 구조가 아닌데 동작하려 한다");

            Transform constrainedTransform = childsMultiAimConstraint.data.constrainedObject.transform;


            RiggingDataDesc dataDesc = new RiggingDataDesc();

            HumanBodyBones mappedBone = FindBodyBone(constrainedTransform);
            if (mappedBone == HumanBodyBones.LastBone)
            {
                //보통의 경우라면 소켓의 경우 여기에 걸린다. (소켓의 부모로 한번더 탐색)
                dataDesc._isSocket = true;

                Transform socketParentTransform = constrainedTransform.transform.parent;

                mappedBone = FindBodyBone(socketParentTransform);

                Debug.Assert(mappedBone != HumanBodyBones.LastBone, "소켓의 소켓인 경우입니까?");
            }
            dataDesc._boneIndex = mappedBone;
            dataDesc._offset = childsMultiAimConstraint.data.offset;
            dataDesc._worldUpType = childsMultiAimConstraint.data.worldUpType;
            dataDesc._upAxis = childsMultiAimConstraint.data.upAxis;
            dataDesc._aimAxis = childsMultiAimConstraint.data.aimAxis;
            dataDesc._sourceObjects = childsMultiAimConstraint.data.sourceObjects;
            dataDesc._constrainedX = childsMultiAimConstraint.data.constrainedXAxis;
            dataDesc._constrainedY = childsMultiAimConstraint.data.constrainedYAxis;
            dataDesc._constrainedZ = childsMultiAimConstraint.data.constrainedZAxis;
            dataDesc._limits = childsMultiAimConstraint.data.limits;
            _riggeds.Add(dataDesc);
        }
    }

    private HumanBodyBones FindBodyBone(Transform target)
    {
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            if (target == _ownerAnimator.GetBoneTransform((HumanBodyBones)i))
            {
                return (HumanBodyBones)i;
            }
        }

        return HumanBodyBones.LastBone;
    }

    public void PublishRigging(GameObject callerSkeletonObject, Animator callerAnimator)
    {
        /*---------------------------------------------------------------------
         |TODO| 부모로 넣었는지 자식으로 넣었는지 depth check 함수 한번 생각해볼것
        ---------------------------------------------------------------------*/
        //if (false/*부모로 넣었는지 Depth 확인 함수*/)
        //{
        //    callerSkeletonObject == 메쉬, 스켈레톤의 공통 부모. 스켈레톤 자체를 넣지는 말 것
        //    Debug.Assert(false, "부모 오브젝트를 넣어야합니다");
        //}


        //1. callerSkeletonObject에 RigBuilder Component를 만들어준다
        RigBuilder createdRigBuilder = callerSkeletonObject.AddComponent<RigBuilder>();

        //2. callerSkeletonObject 에 Rig 오브젝트를 만들어주고 Rig Object에 Rig Component를 생성한다
        GameObject createChildRigObject = new GameObject("Rig");
        createChildRigObject.transform.SetParent(callerSkeletonObject.transform);
        Rig createdRigComponent = createChildRigObject.AddComponent<Rig>();

        //3. 만들어진 RigObject에 위 리스트들을 순회하며 오브젝트를 만들어주며, 각각의 오브젝트에 MultiAimConstrained Component를 만들어준다.
        int index = 0;
        foreach (var desc in _riggeds)
        {
            GameObject createdEachRiggingObject = new GameObject("Child" + index);
            createdEachRiggingObject.transform.SetParent(createChildRigObject.transform);
            MultiAimConstraint createdMultiAimConstaint = createdEachRiggingObject.AddComponent<MultiAimConstraint>();


            /*---------------------------------------------------------------------
             |TODO| 구조체를 대입하는 이 동작을 한꺼번에 할 수 없을까
            ---------------------------------------------------------------------*/
            if (desc._isSocket == true)
            {
                Transform socketParent = callerAnimator.GetBoneTransform(desc._boneIndex);
                GameObject socket = new GameObject("socket");
                socket.transform.SetParent(socketParent);
                createdMultiAimConstaint.data.constrainedObject = socket.transform;
            }
            else
            {
                createdMultiAimConstaint.data.constrainedObject = callerAnimator.GetBoneTransform(desc._boneIndex);
            }
            createdMultiAimConstaint.data.sourceObjects = desc._sourceObjects;

            {
                MultiAimConstraintData.Axis aimAxis = CalculateClosetAxis(new Vector3(0.0f, 0.0f, 1.0f), createdMultiAimConstaint.data.constrainedObject.transform);
                createdMultiAimConstaint.data.aimAxis = aimAxis;
                MultiAimConstraintData.Axis upAxis = CalculateClosetAxis(new Vector3(0.0f, 1.0f, 0.0f), createdMultiAimConstaint.data.constrainedObject.transform);
                createdMultiAimConstaint.data.upAxis = upAxis;
                createdMultiAimConstaint.data.offset = CalculateOffsetVector
                    (
                    desc._aimAxis,
                    desc._upAxis,
                    aimAxis,
                    upAxis,
                    desc._offset
                    );

            }
            createdMultiAimConstaint.data.worldUpType = desc._worldUpType;
            createdMultiAimConstaint.data.constrainedXAxis = desc._constrainedX;
            createdMultiAimConstaint.data.constrainedYAxis = desc._constrainedY;
            createdMultiAimConstaint.data.constrainedZAxis = desc._constrainedZ;
            createdMultiAimConstaint.data.limits = desc._limits;
            index++;
        }

        //4. 만들어진 callerSkeletonObject에 RigBuilder Component에 RigLayer를 확정한다
        RigLayer newRigLayer = new RigLayer(createdRigComponent, true); // 두 번째 매개변수는 활성화 여부
        createdRigBuilder.layers.Add(newRigLayer);
        createdRigBuilder.Build();
    }









    private Vector3 CalculateOffsetVector(
        MultiAimConstraintData.Axis originalAim,
        MultiAimConstraintData.Axis originalUp,
        MultiAimConstraintData.Axis targetAim,
        MultiAimConstraintData.Axis targetUp,
        Vector3 originalOffset)
    {
        Vector3[] dirs = 
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        Vector3 orginalLookVector = dirs[(int)originalAim];
        Vector3 orginalUpVector = dirs[(int)originalUp];
        Vector3 targetLookVector = dirs[(int)targetAim];
        Vector3 targetUpVector = dirs[(int)targetUp];

        Quaternion rotation = Quaternion.LookRotation(targetLookVector, targetUpVector) * Quaternion.Inverse(Quaternion.LookRotation(orginalLookVector, orginalUpVector));

        return rotation * originalOffset;
    }









    private MultiAimConstraintData.Axis CalculateClosetAxis(Vector3 worldTarget, Transform targetTransform)
    {
        MultiAimConstraintData.Axis axisRet = MultiAimConstraintData.Axis.X; // 기본으로 X로 설정
        worldTarget = worldTarget.normalized;
        float dotMax = -Mathf.Infinity;

        float dotResult = 0.0f;
        Vector3 targetVector = Vector3.zero;

        targetVector = targetTransform.right.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            dotMax = dotResult;
            axisRet = MultiAimConstraintData.Axis.X;
        }

        targetVector = -targetTransform.right.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            dotMax = dotResult;
            axisRet = MultiAimConstraintData.Axis.X_NEG;
        }

        targetVector = targetTransform.up.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            dotMax = dotResult;
            axisRet = MultiAimConstraintData.Axis.Y;
        }

        targetVector = -targetTransform.up.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            dotMax = dotResult;
            axisRet = MultiAimConstraintData.Axis.Y_NEG;
        }

        targetVector = targetTransform.forward.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            dotMax = dotResult;
            axisRet = MultiAimConstraintData.Axis.Z;
        }

        targetVector = -targetTransform.forward.normalized;
        dotResult = Vector3.Dot(targetVector, worldTarget);
        if (dotMax < dotResult)
        {
            axisRet = MultiAimConstraintData.Axis.Z_NEG;
        }

        return axisRet;
    }
}
