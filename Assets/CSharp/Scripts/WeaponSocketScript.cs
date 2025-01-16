using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponSocketScript : MonoBehaviour
{
    public enum SideType
    {
        Left,
        Right,
        Middle,
    }
    public SideType _sideType = SideType.Middle;
    public List<ItemAsset.WeaponType> _equippableWeaponTypes = new List<ItemAsset.WeaponType>();
    public Animator _ownerAnimator = null;

    private void Awake()
    {
        Debug.Assert(_equippableWeaponTypes.Count > 0, "장착할 수 있는 무기가 없는데 소켓입니까?");
        CalculateSide();
    }

    private void CalculateSide()
    {
        _ownerAnimator = GetComponentInParent<Animator>();
        Debug.Assert(_ownerAnimator != null, "Socket Component는 부모에 Animator가 있어야 합니다");

        Transform ownerTransform = transform.parent;

        bool isFind = false;

        for (HumanBodyBones i = HumanBodyBones.Hips; i < HumanBodyBones.LastBone; i++)
        {
            isFind = true;

            if (ownerTransform == _ownerAnimator.GetBoneTransform(i))
            {
                string enumToString = i.ToString().ToLower();

                if (enumToString.Contains("left") == true)
                {
                    _sideType = SideType.Left;
                    break;
                }

                if (enumToString.Contains("right") == true)
                {
                    _sideType = SideType.Right;
                    break;
                }

                else
                {
                    _sideType = SideType.Middle;
                    break;
                }
            }
        }

        Debug.Assert(isFind == true, "부모뼈가 Animator 에 맞지 않습니다");
    }
}
