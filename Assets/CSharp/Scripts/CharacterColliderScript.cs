using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using static AnimationAttackFrameAsset;

public class CharacterColliderScript : MonoBehaviour
{
    public class ColliderWorkDesc
    {
        /*----------------------------------------------------------
        |NOTI| 공격중에 공속버프가 들어온다면 _targetTime을 수정하세요
        ----------------------------------------------------------*/
        public ColliderAttachType _type = ColliderAttachType.ENEND;

        public float _targetTime = -1.0f;
        public float _currTime = 0.0f;
        public Coroutine _runningCoroutine = null;
    }

    [Serializable]
    public class BasicColliderDesc
    {
        public ColliderAttachType _type = ColliderAttachType.ENEND;
        public GameObject _basicTarget = null;
    }

    [SerializeField] private CharacterScript _owner = null;
    [SerializeField] private GameObject _ownerModelObject = null;
    [SerializeField] private List<BasicColliderDesc> _basicColliders = null;
    [SerializeField] private Animator _animator = null;

    private Dictionary<ColliderAttachType, GameObject> _colliders = new Dictionary<ColliderAttachType, GameObject>();
    private List<LinkedList<ColliderWorkDesc>> _colliderWorks = new List<LinkedList<ColliderWorkDesc>>();

    private void Awake()
    {
        for (int i = 0; i < (int)ColliderAttachType.ENEND; i++)
        {
            _colliderWorks.Add(new LinkedList<ColliderWorkDesc>());
        }

        foreach (var basicColliderDesc in _basicColliders)
        {
            ChangeCollider(basicColliderDesc._type, basicColliderDesc._basicTarget);
        }
    }

    public void StateChanged()
    {
        //전부 취소하기
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
        //무기를 장착/해제 하거나 등등할때 콜라이더를 반드시 등록해야합니다..
        _colliders[type] = targetObject;

        /*-------------------------------------------------------------
        |TODO| 단순히 이 작업만으로 끝나지는 않습니다.
        owner가 적군으로 삼은 Layer, tag등에 의해서 ColliderComponent의
        설정이 추가되야합니다.
        -------------------------------------------------------------*/


        /*-------------------------------------------------------------
        |TODO| 프레임 드랍에 의해 부정확학 충돌이 예상되는 경우.
        충돌 로직을 바꿔야합니다.
        CastAttack
        -------------------------------------------------------------*/

        Collider collider = targetObject.GetComponent<Collider>();

        if (collider != null)
        {
            int layerMask = _owner.CalculateWeaponColliderExcludeLayerMask(type, targetObject);
            collider.excludeLayers = ~layerMask;
        }
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

            if (_animator.GetBool("IsMirroring") == true)
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
                        break;
                    case ColliderAttachType.HumanoidRightLeg:
                        break;
                    case ColliderAttachType.HumanoidLeftHead:
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
                continue;
            }

            AnimationClip currAnimationClip = currStateAsset._myState._stateAnimationClip;

            if (desc._upFrame >= 0.0f)
            {
                float targetFrame = (float)desc._upFrame;
                float animationFPS = currAnimationClip.frameRate;

                //------------------------------------------------------------------
                float animationSpeed = 1.0f; //버프에 의해서 바뀔 수 있는 가능성이 있다!
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
                float animationSpeed = 1.0f; //버프에 의해서 바뀔 수 있는 가능성이 있다!
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
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].RemoveFirst(); //다했으니 뺀다
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

        _colliderWorks[(int)workDesc._type].RemoveFirst(); //다했으니 뺀다
    }
    
}
