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
        Debug.Assert(_equippableWeaponTypes.Count > 0, "������ �� �ִ� ���Ⱑ ���µ� �����Դϱ�?");
        CalculateSide();
    }

    private void CalculateSide()
    {
        _ownerAnimator = GetComponentInParent<Animator>();
        Debug.Assert(_ownerAnimator != null, "Socket Component�� �θ� Animator�� �־�� �մϴ�");

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

        Debug.Assert(isFind == true, "�θ���� Animator �� ���� �ʽ��ϴ�");
    }
}
