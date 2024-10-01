using UnityEngine;



public struct ItemInfo
{
    public enum EquipType
    {
        None = 0,
        HumanHead,
        HumanArm,
        HumanLeg,
        HumanBody,
        RifleWeapon,
        HandgunWeapon,
        End,
    }

    public int _itemKey;
    public int _sizeX;
    public int _sizeY;

    public bool _isStackAble;
    public int _maxStack;

    public Sprite _sprite;

    public EquipType equipType;
    public Mesh _mesh;
};
