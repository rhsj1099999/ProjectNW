using System;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemAsset", menuName = "Scriptable Object/CreateItemAsset", order = int.MinValue)]
public class ItemAsset : ScriptableObject
{
    public enum EquipType //For BitShift
    {
        None = 0,
        HumanHead = 1 << 0,
        HumanArm = 1 << 1,
        HumanLeg = 1 << 2,
        HumanBody = 1 << 3,
        HumanBackpack = 1 << 4,
        Weapon = 1 << 5,
        UseAndComsumeableByCharacter = 1 << 6,

        All = int.MaxValue
    }
    [SerializeField] private EquipType _equipType;
    public EquipType _EquipType => _equipType;

    public enum WeaponType //캐릭터마다 무기 파지법이 달라질 수 있으니 구체화 해놓지는 않는다
    {
        NotWeapon = 0,

        SmallSword, //단검같은거
        MediumSword, //한손검류
        LargeSword, //대검같은거
        SmallGun,
        MediumGun,
        LargeGun,
    }
    [SerializeField] private WeaponType _weaponType;
    public WeaponType _WeaponType => _weaponType;


    [SerializeField] private string _itemName;
    public string _ItemName => _itemName;

    [SerializeField] private int _itemKey = 0;
    public int _ItemKey => _itemKey;

    [SerializeField] private int _sizeX = 1;
    public int _SizeX => _sizeX;

    [SerializeField] private int _sizeY = 1;
    public int _SizeY => _sizeY;

    [SerializeField] private int _maxStack = 100;
    public int _MaxStack => _maxStack;
}
