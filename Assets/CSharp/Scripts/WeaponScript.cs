using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public Transform _socketTranform = null;
    public Vector3 _pivotRotation = Vector3.zero;
    public Vector3 _pivotPosition = Vector3.zero;


    
    public PlayerScript _owner = null;
    public AnimationClip _handlingIdleAnimation = null;



    public bool _onlyTwoHand = false;
    public ItemInfo _itemInfo = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;



    //public List<StateAsset> _weaponStateAssets = new List<StateAsset>();
    //private List<State> _weaponState = new List<State>();
    ////이제 스테이트가 다음 스테이트로 넘어가는게, '콤보 어택' 기능임



    public State NextStateCheck()
    {
        //플레이어는 무기에게 단순히 좌클릭, 우클릭 등 조작만 넘겨줄거임

        //무기가 알아서 판단해서 다음 스테이트를 넘겨줘야 한다.
        //공중에 있으면 공중공격, 다음 콤보공격 등등


        //무기가 계획된 상태를 연출 도중에 평캔 등등을 요청하면?

        return null;
    }



    public virtual State CalculateNextState()
    {
        return null;
    }



    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| 아이템 프리팹은 기본 PIVOT을 들고있다.
        무기의 위치는 자식 Transform으로 결정돠면 안된다 : (IK를 이용할 가능성 때문에)
        따라서 _pivotPosition, _pivotRotation = 무기마다 들고있는 고유 피벗 프리팹 인스펙터 창에서 미리 설정해둔다
        -----------------------------------------------------------------------------------------------------------------*/
        _pivotPosition = transform.position;
        _pivotRotation = transform.rotation.eulerAngles;

        //foreach (var stateAsset in _weaponStateAssets) 
        //{
        //    State newState = new State(stateAsset);

        //    _weaponState.Add(newState);
        //}

        //foreach (State state in _weaponState)
        //{
        //    state.LinkingStates(ref _weaponState);
        //}
    }

    protected virtual void LateUpdate()
    {
        FollowSocketTransform();
    }

    virtual public void FollowSocketTransform()
    {
        transform.position = _pivotPosition + _socketTranform.position;
        transform.rotation = Quaternion.Euler(_pivotRotation) * _socketTranform.rotation;

        //Quaternion parentRotation = transform.parent.rotation;
        //Quaternion adjustedPivotRotation = parentRotation * Quaternion.Euler(_pivotRotation);
        //transform.rotation = _socketTranform.rotation * adjustedPivotRotation;
    }



    virtual public void Equip(PlayerScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        _socketTranform = followTransform;
    }



    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }



    //virtual public AnimationOverrideDesc FindAnimationOverride(StateAsset currStateAsset) 
    //{
    //    if (_animationOverrideDic.ContainsKey(currStateAsset) == false)
    //    {
    //        return null;
    //    }

    //    return _animationOverrideDic[currStateAsset];
    //}

    //public void ReadyAnimationOverrideDic_Debug(List<StateAsset> targetStataAssets, List<AnimationOverrideDesc> targetAnimationClips)
    //{
    //    for (int i = 0; i < targetAnimationClips.Count; ++i)
    //    {
    //        _animationOverrideDic.Add(targetStataAssets[i], targetAnimationClips[i]);
    //    }
    //}
}
