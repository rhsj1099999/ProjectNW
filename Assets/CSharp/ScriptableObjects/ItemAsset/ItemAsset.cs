using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemAsset", menuName = "Scriptable Object/Create_ItemAsset", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset : ScriptableObject
{
    public enum ItemType //For BitShift
    {
        None,
        Bullet,
        Magazine,
        Equip,
    }
    [SerializeField] private ItemType _itemType;
    public ItemType _ItemType => _itemType;

    



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


    //Key -> ItemInfoManager 에서 순서대로 자동맵핑되게
    //웬만하면 아이템을 이름으로 찾지 않을까?
    public int _ItemKey = 0; 

    //[SerializeField] private int _itemKey = 0;
    //public int _ItemKey => _itemKey;

    [SerializeField] private string _itemName;
    public string _ItemName => _itemName;

    [SerializeField] private int _sizeX = 1;
    public int _SizeX => _sizeX;

    [SerializeField] private int _sizeY = 1;
    public int _SizeY => _sizeY;

    [SerializeField] private int _maxStack = 100;
    public int _MaxStack => _maxStack;

    [SerializeField] private GameObject _itemModel = null;
    public GameObject _ItemModel => _itemModel;

    //바닥에 뿌릴 수 있습니까? => 모델이 존재해야한다
    //모델이 없으면 기본모델을 정의한다.
    [SerializeField] private bool _fieldExistAble = true;
    public bool _FieldExistAble => _fieldExistAble;

    [SerializeField] private Image _itemImage = null;
    public Image _ItemImage => _itemImage;
}