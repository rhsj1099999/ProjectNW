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
        public uint _key = 0;
        public bool _isActivated = false;
    }


    private Dictionary<ColliderAttachType, GameObject> _colliders = new Dictionary<ColliderAttachType, GameObject>();
    private List<LinkedList<ColliderWorkDesc>> _colliderWorks = new List<LinkedList<ColliderWorkDesc>>();
    private uint _keyMaker = 0;


    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterColliderScript);

        for (int i = 0; i < (int)ColliderAttachType.ENEND; i++)
        {
            _colliderWorks.Add(new LinkedList<ColliderWorkDesc>());
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
        foreach (LinkedList<ColliderWorkDesc> colliderWorkList in _colliderWorks)
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
                _colliders[type].SetActive(false);
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
            //float animationSpeed = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetFloat("Speed");
            float animationSpeed = _owner.GCST<StatScript>().GetPassiveStat(LevelStatAsset.PassiveStat.AttackSpeedPercentage) / 100.0f;
            //------------------------------------------------------------------

            float time_1 = -1.0f;
            float time_2 = -1.0f;

            
            if (desc._frameUp >= 0.0f)
            {
                float targetFrame = desc._frameUp;
                float animationFPS = currAnimationClip.frameRate;

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._type = type;
                colliderWorkDesc._key = _keyMaker++;
                colliderWorkDesc._runningCoroutine = StartCoroutine(ActiveColliderCoroutine(colliderWorkDesc));
                //_colliderWorks[(int)type].AddLast(colliderWorkDesc);
                time_1 = colliderWorkDesc._targetTime;
            }


            if (desc._frameUnder >= 0.0f)
            {
                float targetFrame = desc._frameUnder;
                float animationFPS = currAnimationClip.frameRate;

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._type = type;
                colliderWorkDesc._key = _keyMaker++;
                colliderWorkDesc._runningCoroutine = StartCoroutine(DeActiveColliderCoroutine(colliderWorkDesc));
                //_colliderWorks[(int)type].AddLast(colliderWorkDesc);
                time_2 = colliderWorkDesc._targetTime;
            }

            if (time_1 > 0.0f && time_2 > 0.0f)
            {
                float delta = time_2 - time_1;
                if (delta <= Time.fixedDeltaTime) 
                {
                    Debug.Assert(false, "�浹�� ����Ȯ�� �� �ֽ��ϴ�");
                }
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
                    if (targetObject.activeSelf == true)
                    {
                        Debug.Assert(false, "�̹� Ȱ��ȭ�� �� �־���");
                    }
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


        //_colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
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
                    if (targetObject.activeSelf == false)
                    {
                        Debug.Assert(false, "�̹� ��Ȱ��ȭ");
                    }
                    AnimationAttackManager.Instance.ClearCollider(targetObject);
                    targetObject.SetActive(false);
                }

                break;
            }

            yield return null;
        }

        //_colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
    }
    
}
