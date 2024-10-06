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
     * Owner �� ������ �ִ� ���� ������ Unity �� ������ �ؼ��Ѵ����� �Ȱ��� �ٿ��� ��ũ��Ʈ
     * ��� ���̱⸦ �����ϴٺ��� �ڽ� �𵨵鿡�� ������ �ϱ� ���ؼ� �������.
     * 
     * ****************************************************************
     * ������ ��� ���� ĳ���Ͷ� �Ȱ��� ��������� �̰� �ȸ��� �ȴ�*
     * ****************************************************************
    ----------------------------------------------------------------------------------*/

    [SerializeField] private GameObject _ownerSkeleton = null;
    [SerializeField] private GameObject _ownerRig = null;
    [SerializeField] private Animator _ownerAnimator = null;

    private List<RiggingDataDesc> _riggeds = new List<RiggingDataDesc>();

    private void Awake()
    {
        Debug.Assert( _ownerSkeleton != null, "owner Skeleton�� null�̿����� �ȵȴ�");

        Debug.Assert(_ownerAnimator != null, "owner Animator�� null�̿����� �ȵȴ�");

        Rig ownerRig = _ownerRig.GetComponent<Rig>();

        Debug.Assert(ownerRig != null, "Rig�� ���� ������Ʈ���� �����Ϸ� �Ѵ�");

        for (int i = 0; i < ownerRig.transform.childCount; i++)
        {
            MultiAimConstraint childsMultiAimConstraint = ownerRig.transform.GetChild(i).gameObject.GetComponent<MultiAimConstraint>();

            Debug.Assert(childsMultiAimConstraint != null, "Aim ������ �ƴѵ� �����Ϸ� �Ѵ�");

            Transform constrainedTransform = childsMultiAimConstraint.data.constrainedObject.transform;


            RiggingDataDesc dataDesc = new RiggingDataDesc();

            HumanBodyBones mappedBone = FindBodyBone(constrainedTransform);
            if (mappedBone == HumanBodyBones.LastBone)
            {
                //������ ����� ������ ��� ���⿡ �ɸ���. (������ �θ�� �ѹ��� Ž��)
                dataDesc._isSocket = true;

                Transform socketParentTransform = constrainedTransform.transform.parent;

                mappedBone = FindBodyBone(socketParentTransform);

                Debug.Assert(mappedBone != HumanBodyBones.LastBone, "������ ������ ����Դϱ�?");
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
         |TODO| �θ�� �־����� �ڽ����� �־����� depth check �Լ� �ѹ� �����غ���
        ---------------------------------------------------------------------*/
        //if (false/*�θ�� �־����� Depth Ȯ�� �Լ�*/)
        //{
        //    callerSkeletonObject == �޽�, ���̷����� ���� �θ�. ���̷��� ��ü�� ������ �� ��
        //    Debug.Assert(false, "�θ� ������Ʈ�� �־���մϴ�");
        //}


        //1. callerSkeletonObject�� RigBuilder Component�� ������ش�
        RigBuilder createdRigBuilder = callerSkeletonObject.AddComponent<RigBuilder>();

        //2. callerSkeletonObject �� Rig ������Ʈ�� ������ְ� Rig Object�� Rig Component�� �����Ѵ�
        GameObject createChildRigObject = new GameObject("Rig");
        createChildRigObject.transform.SetParent(callerSkeletonObject.transform);
        Rig createdRigComponent = createChildRigObject.AddComponent<Rig>();

        //3. ������� RigObject�� �� ����Ʈ���� ��ȸ�ϸ� ������Ʈ�� ������ָ�, ������ ������Ʈ�� MultiAimConstrained Component�� ������ش�.
        int index = 0;
        foreach (var desc in _riggeds)
        {
            GameObject createdEachRiggingObject = new GameObject("Child" + index);
            createdEachRiggingObject.transform.SetParent(createChildRigObject.transform);
            MultiAimConstraint createdMultiAimConstaint = createdEachRiggingObject.AddComponent<MultiAimConstraint>();


            /*---------------------------------------------------------------------
             |TODO| ����ü�� �����ϴ� �� ������ �Ѳ����� �� �� ������
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

        //4. ������� callerSkeletonObject�� RigBuilder Component�� RigLayer�� Ȯ���Ѵ�
        RigLayer newRigLayer = new RigLayer(createdRigComponent, true); // �� ��° �Ű������� Ȱ��ȭ ����
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
        MultiAimConstraintData.Axis axisRet = MultiAimConstraintData.Axis.X; // �⺻���� X�� ����
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
