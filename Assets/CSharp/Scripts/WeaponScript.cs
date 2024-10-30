using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public PlayerScript _owner = null;
    public ItemInfo _itemInfo = null;
    public ItemInfo.WeaponType _weaponType = ItemInfo.WeaponType.NotWeapon;
    public AnimationClip _handlingIdleAnimation = null;

    public GameObject _itemPrefab = null;
    public string _itemPrefabRoute = null;

    public Transform _socketTranform = null;

    public Vector3 _pivotRotation = Vector3.zero;
    public Vector3 _pivotPosition = Vector3.zero;

    private void Awake()
    {
        /*-----------------------------------------------------------------------------------------------------------------
        _pivotPosition, _pivotRotation = 무기마다 들고있는 고유 피벗 프리팹 인스펙터 창에서 미리 설정해둔다
        -----------------------------------------------------------------------------------------------------------------*/
        _pivotPosition = transform.position;
        _pivotRotation = transform.rotation.eulerAngles;
    }

    virtual public void FollowSocketTransform()
    {
        transform.position = _pivotPosition + _socketTranform.position;
        transform.rotation = Quaternion.Euler(_pivotRotation) * _socketTranform.rotation;
    }

    virtual public void TurnOnAim()
    {

    }

    virtual public void TurnOffAim()
    {

    }

    virtual public void Equip(PlayerScript itemOwner, Transform followTransform)
    {
        _owner = itemOwner;
        _socketTranform = followTransform;
    }

    virtual public void UnEquip()
    {
    }
}
