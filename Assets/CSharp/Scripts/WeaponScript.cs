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
    ////���� ������Ʈ�� ���� ������Ʈ�� �Ѿ�°�, '�޺� ����' �����



    public State NextStateCheck()
    {
        //�÷��̾�� ���⿡�� �ܼ��� ��Ŭ��, ��Ŭ�� �� ���۸� �Ѱ��ٰ���

        //���Ⱑ �˾Ƽ� �Ǵ��ؼ� ���� ������Ʈ�� �Ѱ���� �Ѵ�.
        //���߿� ������ ���߰���, ���� �޺����� ���


        //���Ⱑ ��ȹ�� ���¸� ���� ���߿� ��ĵ ����� ��û�ϸ�?

        return null;
    }



    public virtual State CalculateNextState()
    {
        return null;
    }



    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| ������ �������� �⺻ PIVOT�� ����ִ�.
        ������ ��ġ�� �ڽ� Transform���� �����¸� �ȵȴ� : (IK�� �̿��� ���ɼ� ������)
        ���� _pivotPosition, _pivotRotation = ���⸶�� ����ִ� ���� �ǹ� ������ �ν����� â���� �̸� �����صд�
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
