using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AnimationFrameDataAsset;

public class CharacterColliderScript : GameCharacterSubScript
{
    public class ColliderWorkDesc
    {
        /*----------------------------------------------------------
        |NOTI| �����߿� ���ӹ����� ���´ٸ� _targetTime�� �����ϼ���
        ----------------------------------------------------------*/
        public ColliderAttachType _type = ColliderAttachType.ENEND;

        public float _targetTime = -1.0f;
        public float _currTime = 0.0f;
        public Coroutine _runningCoroutine = null;
    }


    private Dictionary<ColliderAttachType, GameObject> _colliders = new Dictionary<ColliderAttachType, GameObject>();
    private List<HashSet<ColliderWorkDesc>> _colliderWorks = new List<HashSet<ColliderWorkDesc>>();


    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterColliderScript);

        for (int i = 0; i < (int)ColliderAttachType.ENEND; i++)
        {
            _colliderWorks.Add(new HashSet<ColliderWorkDesc>());
        }
    }


    public override void SubScriptStart() {}


    public void InitModelCollider(GameObject targetModel)
    {
        CharacterModelDataInitializer modelDataInitializer = targetModel.GetComponentInChildren<CharacterModelDataInitializer>();

        if (modelDataInitializer == null)
        {
            Debug.Assert(false, "���� �ִٸ� �ݵ�� �־���ϴ� ��ũ��Ʈ �Դϴ�");
            Debug.Break();
        }

        List<WeaponColliderScript> modelBasicColliders = modelDataInitializer.GetModelBasicColliders();

        if (modelBasicColliders.Count <= 0)
        {
            Debug.Assert(false, "�𵨿� �پ��ִ� �ݶ��̴��� ��¥ �ϳ��� �����ϱ�?");
            Debug.Break();
        }

        foreach (var basicColliderDesc in modelBasicColliders)
        {
            ChangeCollider(basicColliderDesc.GetAttachType(), basicColliderDesc.gameObject);
        }
    }


    public void StateChanged()
    {
        //���� ����ϱ�
        foreach (HashSet<ColliderWorkDesc> colliderWorkList in _colliderWorks)
        {
            if (colliderWorkList == null)
            {
                continue;
            }

            ColliderAttachType type = ColliderAttachType.ENEND;

            foreach (ColliderWorkDesc colliderWork in colliderWorkList)
            {
                type = colliderWork._type;
                StopCoroutine(colliderWork._runningCoroutine);
            }

            GameObject colliderObject = null;
            _colliders.TryGetValue(type, out colliderObject);

            if (colliderObject != null)
            {
                colliderObject.SetActive(false);
                AnimationAttackManager.Instance.ClearCollider(colliderObject);
            }

            colliderWorkList.Clear();
        }
    }


    public void ChangeCollider(ColliderAttachType type, GameObject targetObject)
    {
        //���⸦ ����/���� �ϰų� ����Ҷ� �ݶ��̴��� �ݵ�� ����ؾ��մϴ�..

        /*-------------------------------------------------------------
        |TODO| �ܼ��� �� �۾������� �������� �ʽ��ϴ�.
        owner�� �������� ���� Layer, tag� ���ؼ� ColliderComponent��
        ������ �߰��Ǿ��մϴ�.
        -------------------------------------------------------------*/

        /*-------------------------------------------------------------
        |NOTI| ������ ����� ���� ����Ȯ�� �浹�� ����Ǵ� ���.
        �浹 ������ �ٲ���մϴ�.
        -------------------------------------------------------------*/

        _colliders[type] = targetObject;

        Collider collider = targetObject.GetComponent<Collider>();

        if (collider != null)
        {
            collider.includeLayers = _owner.CalculateWeaponColliderIncludeLayerMask();
        }

        targetObject.SetActive(false);
    }

    public GameObject GetColliderObject(ColliderAttachType type)
    {
        return _colliders[type];
    }

    public void ColliderWork(List<AEachFrameData> frameDataAssetList, StateAsset currStateAsset)
    {
        if (frameDataAssetList == null)
        {
            return;
        }

        foreach (AEachFrameData desc in frameDataAssetList)
        {
            ColliderAttachType type = desc._colliderAttachType;

            if (_owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBool("IsMirroring") == true)
            {
                switch (type)
                {
                    case ColliderAttachType.HumanoidLeftHand:
                        type = ColliderAttachType.HumanoidRightHand;
                        break;
                    case ColliderAttachType.HumanoidRightHand:
                        type = ColliderAttachType.HumanoidLeftHand;
                        break;
                    case ColliderAttachType.HumanoidLeftLeg:
                        type = ColliderAttachType.HumanoidRightLeg;
                        break;
                    case ColliderAttachType.HumanoidRightLeg:
                        type = ColliderAttachType.HumanoidLeftLeg;
                        break;
                    case ColliderAttachType.HumanoidHead:
                        break;
                    case ColliderAttachType.HumanoidRightHandWeapon:
                        type = ColliderAttachType.HumanoidLeftHandWeapon;
                        break;
                    case ColliderAttachType.HumanoidLeftHandWeapon:
                        type = ColliderAttachType.HumanoidRightHandWeapon;
                        break;
                    case ColliderAttachType.ENEND:
                        break;
                    default:
                        break;
                }
            }

            if (_colliders.ContainsKey(type) == false)
            {
                Debug.Log("�ݶ��̴��� ����!");
                continue;
            }

            AnimationClip currAnimationClip = currStateAsset._myState._stateAnimationClip;


            //------------------------------------------------------------------
            float animationSpeed = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetFloat("Speed");
            //------------------------------------------------------------------


            if (desc._frameUp >= 0.0f)
            {
                float targetFrame = (float)desc._frameUp;
                float animationFPS = currAnimationClip.frameRate;

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._runningCoroutine = StartCoroutine(ActiveColliderCoroutine(colliderWorkDesc));
                colliderWorkDesc._type = type;
                _colliderWorks[(int)type].Add(colliderWorkDesc);
            }


            if (desc._frameUnder >= 0.0f)
            {
                float targetFrame = (float)desc._frameUnder;
                float animationFPS = currAnimationClip.frameRate;

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._runningCoroutine = StartCoroutine(DeActiveColliderCoroutine(colliderWorkDesc));
                colliderWorkDesc._type = type;
                _colliderWorks[(int)type].Add(colliderWorkDesc);
            }
        }
    }

    

    private IEnumerator ActiveColliderCoroutine(ColliderWorkDesc workDesc)
    {
        while (true)
        {
            workDesc._currTime += Time.deltaTime;

            if (workDesc._currTime >= workDesc._targetTime)
            {
                GameObject targetObject = null;
                _colliders.TryGetValue(workDesc._type, out targetObject);
                if (targetObject != null) 
                {
                    targetObject.SetActive(true);
                }
                else
                {
                    Debug.Log("�ݶ��̴��� ����!");
                }
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].Remove(workDesc); //�������� ����
    }


    private IEnumerator DeActiveColliderCoroutine(ColliderWorkDesc workDesc)
    {
        while (true)
        {
            workDesc._currTime += Time.deltaTime;

            if (workDesc._currTime >= workDesc._targetTime)
            {
                GameObject targetObject = null;
                _colliders.TryGetValue(workDesc._type, out targetObject);
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                    AnimationAttackManager.Instance.ClearCollider(targetObject);
                }
                else
                {
                    Debug.Log("�ݶ��̴��� ����!");
                }
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].Remove(workDesc); //�������� ����
    }
    
}
