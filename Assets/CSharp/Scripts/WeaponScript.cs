using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationOverrideDesc
{
    public AnimationClip _animationClip = null;
    public AvatarMask _targetMask = null;
}

[Serializable]
public class AnimationOverridePair
{
    public StateAsset _targetAsset = null;
    public AnimationOverrideDesc _animationOverrideDesc = null;
}



public class WeaponScript : MonoBehaviour
{
    public enum HoldAbleType
    {
        None = 0,
        OneHand = 1 << 1,
        TwoHand = 1 << 2,
    }

    public HoldAbleType _holdAbleType = HoldAbleType.None;
    public bool _onlyTwoHand = false;
    public ItemInfo _itemInfo = null;
    public PlayerScript _owner = null;

    public AvatarMask _weaponAvatarMask = null; // ??



    public AnimationClip _handlingIdleAnimation = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;

    public GameObject _itemPrefab = null;
    public string _itemPrefabRoute = null;
    public Transform _socketTranform = null;
    public Vector3 _pivotRotation = Vector3.zero;
    public Vector3 _pivotPosition = Vector3.zero;

    [SerializeField] private List<AnimationOverridePair> _animationOverrideList = new List<AnimationOverridePair>();
    private Dictionary<StateAsset, AnimationOverrideDesc> _animationOverrideDic = new Dictionary<StateAsset, AnimationOverrideDesc>();


    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        _pivotPosition, _pivotRotation = 무기마다 들고있는 고유 피벗 프리팹 인스펙터 창에서 미리 설정해둔다
        -----------------------------------------------------------------------------------------------------------------*/
        _pivotPosition = transform.position;
        _pivotRotation = transform.rotation.eulerAngles;

        /*-----------------------------------------------------------------------------------------------------------------
        |NOTI| 아이템 프리팹은 기본 PIVOT을 들고있다.
        무기의 위치는 자식 Transform으로 결정돠면 안된다 : (IK를 이용할 가능성 때문에)
        -----------------------------------------------------------------------------------------------------------------*/
    }


    public void ReadyAnimationOverrideDic_Debug(List<StateAsset> targetStataAssets, List<AnimationOverrideDesc> targetAnimationClips)
    {
        for (int i = 0; i < targetAnimationClips.Count; ++i)
        {
            _animationOverrideDic.Add(targetStataAssets[i], targetAnimationClips[i]);
        }
    }


    virtual public void FollowSocketTransform()
    {
        transform.position = _pivotPosition + _socketTranform.position;
        transform.rotation = Quaternion.Euler(_pivotRotation) * _socketTranform.rotation;
    }


    virtual public void Equip(PlayerScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        _socketTranform = followTransform;
    }


    virtual public AnimationOverrideDesc FindAnimationOverride(StateAsset currStateAsset) 
    {
        if (_animationOverrideDic.ContainsKey(currStateAsset) == false)
        {
            return null;
        }

        return _animationOverrideDic[currStateAsset];
    }


    virtual public void TurnOnAim() { }
    virtual public void TurnOffAim() { }
    virtual public void UnEquip() { }
}
