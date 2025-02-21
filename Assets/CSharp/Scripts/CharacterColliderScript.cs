using MagicaCloth2;
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
        |NOTI| n�� ���Ŀ� (Ȱ�� / ��Ȱ��ȭ)�� ���ִ� ����ü�̴�
        �����߿� ���ӹ����� ���´ٸ� _targetTime�� �����ϼ���
        ----------------------------------------------------------*/
        public ColliderAttachType _type = ColliderAttachType.ENEND;

        public Coroutine _runningCoroutine = null;

        public float _targetTime = -1.0f;
        public float _currTime = 0.0f;

        public uint _key = 0;
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

                //GameObject colliderObject = null;

                //_colliders.TryGetValue(type, out colliderObject);
                //if (colliderObject != null)
                //{
                //    _colliders[type].SetActive(false);
                //    WeaponColliderManager.Instance.ClearCollider(colliderObject);
                //}
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

        if (_colliderWorks[(int)type].Count > 0)
        {
            foreach (ColliderWorkDesc workDesc in _colliderWorks[(int)type])
            {
                StopCoroutine(workDesc._runningCoroutine);
            }

            _colliderWorks[(int)type].Clear();
        }

        _colliders[type] = targetObject;

        Collider collider = targetObject.GetComponent<Collider>();

        collider.includeLayers = _owner.CalculateWeaponColliderIncludeLayerMask();

        targetObject.SetActive(false);
    }

    public GameObject GetColliderObject(ColliderAttachType type)
    {
        return _colliders[type];
    }


    private void CalculateColliderAttachType(AEachFrameData desc, ref ColliderAttachType retOut)
    {
        retOut = desc._colliderAttachType;

        if (_owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBool("IsMirroring") == true)
        {
            switch (retOut)
            {
                case ColliderAttachType.HumanoidLeftHand:
                    retOut = ColliderAttachType.HumanoidRightHand;
                    break;
                case ColliderAttachType.HumanoidRightHand:
                    retOut = ColliderAttachType.HumanoidLeftHand;
                    break;
                case ColliderAttachType.HumanoidLeftLeg:
                    retOut = ColliderAttachType.HumanoidRightLeg;
                    break;
                case ColliderAttachType.HumanoidRightLeg:
                    retOut = ColliderAttachType.HumanoidLeftLeg;
                    break;
                case ColliderAttachType.HumanoidHead:
                    break;
                case ColliderAttachType.HumanoidRightHandWeapon:
                    retOut = ColliderAttachType.HumanoidLeftHandWeapon;
                    break;
                case ColliderAttachType.HumanoidLeftHandWeapon:
                    retOut = ColliderAttachType.HumanoidRightHandWeapon;
                    break;
                case ColliderAttachType.ENEND:
                    break;
                default:
                    break;
            }
        }
    }




    public void ColliderWork(List<AEachFrameData> frameDataAssetList, StateAsset currStateAsset)
    {
        if (frameDataAssetList == null)
        {
            return;
        }

        foreach (AEachFrameData desc in frameDataAssetList)
        {
            ColliderAttachType type = ColliderAttachType.ENEND;
            CalculateColliderAttachType(desc, ref type);
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

            if (desc._frameUp >= 0.0f)
            {
                float targetFrame = desc._frameUp;
                float animationFPS = currAnimationClip.frameRate;

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._type = type;
                colliderWorkDesc._key = _keyMaker++;
                colliderWorkDesc._runningCoroutine = StartCoroutine(ActiveColliderCoroutine(colliderWorkDesc));
                _colliderWorks[(int)type].AddLast(colliderWorkDesc);
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
                _colliderWorks[(int)type].AddLast(colliderWorkDesc);
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
                //�� �ݶ���� ��� = �� �����Ӹ��� Overlap Box üũ ���� ����
                break;
            }

            yield return null;
        }


        _colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
    }


    private IEnumerator DeActiveColliderCoroutine(ColliderWorkDesc workDesc)
    {
        while (true)
        {
            workDesc._currTime += Time.deltaTime;

            if (workDesc._currTime >= workDesc._targetTime)
            {
                //�� �ݶ���� �������
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
    }
    
}
