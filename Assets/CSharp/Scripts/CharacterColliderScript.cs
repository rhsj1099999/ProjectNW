using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AnimationAttackFrameAsset;

public class CharacterColliderScript : MonoBehaviour
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


    [SerializeField] private CharacterScript _owner = null;

    private Dictionary<ColliderAttachType, GameObject> _colliders = new Dictionary<ColliderAttachType, GameObject>();
    private List<LinkedList<ColliderWorkDesc>> _colliderWorks = new List<LinkedList<ColliderWorkDesc>>();

    private void Awake()
    {
        for (int i = 0; i < (int)ColliderAttachType.ENEND; i++)
        {
            _colliderWorks.Add(new LinkedList<ColliderWorkDesc>());
        }
    }


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
            int layerMask = _owner.CalculateWeaponColliderExcludeLayerMask(type, targetObject);
            collider.excludeLayers = ~layerMask;
        }

        targetObject.SetActive(false);
    }

    public GameObject GetColliderObject(ColliderAttachType type)
    {
        return _colliders[type];
    }

    public void ColliderWork(List<AttackFrameDesc> frameDataAssetList, StateAsset currStateAsset)
    {
        if (frameDataAssetList == null)
        {
            return;
        }

        foreach (AttackFrameDesc desc in frameDataAssetList)
        {
            ColliderAttachType type = desc._attachType;

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

            if (desc._upFrame >= 0.0f)
            {
                float targetFrame = (float)desc._upFrame;
                float animationFPS = currAnimationClip.frameRate;

                //------------------------------------------------------------------
                float animationSpeed = 1.0f; //������ ���ؼ� �ٲ� �� �ִ� ���ɼ��� �ִ�!
                //------------------------------------------------------------------

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._runningCoroutine = StartCoroutine(ActiveColliderCoroutine(colliderWorkDesc));
                colliderWorkDesc._type = type;
                _colliderWorks[(int)type].AddLast(colliderWorkDesc);
            }


            if (desc._underFrame >= 0.0f)
            {
                float targetFrame = (float)desc._underFrame;
                float animationFPS = currAnimationClip.frameRate;

                //------------------------------------------------------------------
                float animationSpeed = 1.0f; //������ ���ؼ� �ٲ� �� �ִ� ���ɼ��� �ִ�!
                //------------------------------------------------------------------

                ColliderWorkDesc colliderWorkDesc = new ColliderWorkDesc();
                colliderWorkDesc._targetTime = (targetFrame / animationFPS) / animationSpeed;
                colliderWorkDesc._runningCoroutine = StartCoroutine(DeActiveColliderCoroutine(colliderWorkDesc));
                colliderWorkDesc._type = type;
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

        _colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
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
                    AnimationAttackManager.Instance.ClearCollider(targetObject);
                    targetObject.SetActive(false);
                }
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].RemoveFirst(); //�������� ����
    }
    
}
