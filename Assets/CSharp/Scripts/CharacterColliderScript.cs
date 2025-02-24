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
        |NOTI| 공격중에 공속버프가 들어온다면 _targetTime을 수정하세요
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
            Debug.Assert(false, "모델이 있다면 반드시 있어야하는 스크립트 입니다");
            Debug.Break();
        }

        List<WeaponColliderScript> modelBasicColliders = modelDataInitializer.GetModelBasicColliders();

        if (modelBasicColliders.Count <= 0)
        {
            Debug.Assert(false, "모델에 붙어있는 콜라이더가 진짜 하나도 없습니까?");
            Debug.Break();
        }

        foreach (var basicColliderDesc in modelBasicColliders)
        {
            ChangeCollider(basicColliderDesc.GetAttachType(), basicColliderDesc.gameObject);
        }
    }


    public void StateChanged()
    {
        //전부 취소하기
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
        //무기를 장착/해제 하거나 등등할때 콜라이더를 반드시 등록해야합니다..

        /*-------------------------------------------------------------
        |TODO| 단순히 이 작업만으로 끝나지는 않습니다.
        owner가 적군으로 삼은 Layer, tag등에 의해서 ColliderComponent의
        설정이 추가되야합니다.
        -------------------------------------------------------------*/

        /*-------------------------------------------------------------
        |NOTI| 프레임 드랍에 의해 부정확한 충돌이 예상되는 경우.
        충돌 로직을 바꿔야합니다.
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
                Debug.Log("콜라이더가 없다!");
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
                    Debug.Log("콜라이더가 없다!");
                }
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].Remove(workDesc); //다했으니 뺀다
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
                    Debug.Log("콜라이더가 없다!");
                }
                break;
            }

            yield return null;
        }

        _colliderWorks[(int)workDesc._type].Remove(workDesc); //다했으니 뺀다
    }
    
}
