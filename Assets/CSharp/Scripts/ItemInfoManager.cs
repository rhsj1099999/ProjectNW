using System.Collections.Generic;
using UnityEngine;



public class ItemInfo
{
    public enum EquipType //For BitShift
    {
        None = 0,
        HumanHead = 1 << 0,
        HumanArm = 1 << 1,
        HumanLeg = 1 << 2,
        HumanBody = 1 << 3,
        HumanBackpack = 1 << 4,
        All = int.MaxValue
    }

    public enum WeaponType //ĳ���͸��� ���� �������� �޶��� �� ������ ��üȭ �س����� �ʴ´�
    {
        NotWeapon = 0,

        SmallSword, //�ܰ˰�����
        MediumSword, //�Ѽհ˷�
        LargeSword, //��˰�����
        SmallGun,
        MediumGun,
        LargeGun,
    }
    

    public int _itemKey;
    public string _itemName;
    public int _sizeX;
    public int _sizeY;

    public bool _isStackAble;
    public int _maxStack;

    public Sprite _sprite;

    public WeaponType _weaponType;
    public EquipType _equipType;
    public string _meshObjectName;
    public List<int> _equipMeshIndicies;
};



public struct ItemStoreDesc
{
    public ItemStoreDesc(ItemInfo info, int count, Vector2 position)
    {
        _info = info;
        _count = count;
        _position = position;
        _storedIndex = 0;
        _isRotated = false;
        _owner = null;
    }

    public void PlusItem(int count = 1)
    {
        _count++;
    }

    public Vector2 _position;       //����� ��ġ 
    public ItemInfo _info;          //����
    public int _count;              //����
    public int _storedIndex;        //����� ĭ
    public bool _isRotated;
    public IMoveItemStore _owner;

}

public class ItemInfoManager : SubManager
{
    Dictionary<string, GameObject>       _equipmentPrefabs = new Dictionary<string, GameObject>();
    Dictionary<string, List<GameObject>> _equipmentObject = new Dictionary<string, List<GameObject>>();
    Dictionary<int, ItemInfo> _items = new Dictionary<int, ItemInfo>();

    private static ItemInfoManager _instance = null;

    public static ItemInfoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("ItemInfoMananger");
                _instance = gameObject.AddComponent<ItemInfoManager>();
                DontDestroyOnLoad(gameObject);
            }
            return _instance;
        }
    }

    public override void SubManagerAwake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }

        _instance = this;

        DontDestroyOnLoad(_instance.gameObject);

        InitEquipments();

        InitItem_TestCode_MustDel();
    }



    public GameObject GetModelMesh(ItemInfo itemInfo)
    {
        return null;
    }


    public List<GameObject> GetMeshes(ItemInfo itemInfo) //���������� ��Ʈ������ �ϳ��� ������ ��Ʈ���� �ٰŴ�
    {
        Debug.Assert(_equipmentObject.ContainsKey(itemInfo._meshObjectName) == true, "������ ������ meshObject�� �����ϴ�");

        List<GameObject> equipMeshes = _equipmentObject[itemInfo._meshObjectName];

        List<GameObject> targetMeshes = new List<GameObject>();

        foreach (var index in itemInfo._equipMeshIndicies)
        {
            if (index >= equipMeshes.Count)
            {
                Debug.Log("�ε����� ��� �޽��� ��û�մϴ�. ���� ������ �־����ϱ�?");
            }

            targetMeshes.Add(equipMeshes[index]);
        }

        return targetMeshes;
    }

    public ItemInfo GetItemInfo(int itemKey)
    {
        Debug.Assert(_items.ContainsKey(itemKey) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemKey);

        return _items[itemKey];
    }


    public GameObject GetEquipmentPrefab(string prefabName)
    {
        Debug.Assert(_equipmentPrefabs.ContainsKey(prefabName) == true, "�������� �ʴ� �������� ��û�մϴ�. ���ҽ� ��ȭ�� �־����ϱ�?");
        return _equipmentPrefabs[prefabName];
    }



    private void InitEquipments()
    {
        //asset ���� ���� ���� �޽����� ����ϴ� �Լ� 

        //Dictionary<string, List<GameObject>> _equipmentObject

        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("EquipmentModels");

        foreach (GameObject prefab in loadedPrefabs)
        {
            Debug.Assert(_equipmentPrefabs.ContainsKey(prefab.name) == false, "������ ������ �ߺ��˴ϴ�");

            _equipmentPrefabs.Add(prefab.name, prefab);

            SkinnedMeshRenderer[] components = prefab.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (_equipmentObject.ContainsKey(prefab.name) == false)
            {
                _equipmentObject.Add(prefab.name, new List<GameObject>());
            }

            for (int i = 0; i < components.Length; ++i)
            {
                _equipmentObject[prefab.name].Add(components[i].gameObject);
            }
        }
    }





    private void InitItem_TestCode_MustDel()
    {
        //�ӽ÷� ������ ������ ����� �Լ�
        /*---------------------------------
        |TODO| �ܺο��� �Ľ��ϴ°� �����غ��� 
        ---------------------------------*/

        ItemInfo testItemInfo = new ItemInfo();
        testItemInfo._itemName = "����1";
        testItemInfo._sprite = null;
        testItemInfo._isStackAble = true;
        testItemInfo._itemKey = 30;
        testItemInfo._sizeX = 1;
        testItemInfo._sizeY = 1;
        testItemInfo._equipType = ItemInfo.EquipType.HumanHead;
        testItemInfo._meshObjectName = "EquipTest";
        testItemInfo._equipMeshIndicies = new List<int>();
        testItemInfo._equipMeshIndicies.Add(3);
        _items.Add(testItemInfo._itemKey, testItemInfo);


        testItemInfo = new ItemInfo();
        testItemInfo._itemName = "�尩1";
        testItemInfo._sprite = null;
        testItemInfo._isStackAble = true;
        testItemInfo._itemKey = 31;
        testItemInfo._sizeX = 1;
        testItemInfo._sizeY = 2;
        testItemInfo._equipType = ItemInfo.EquipType.HumanArm;
        testItemInfo._meshObjectName = "EquipTest";
        testItemInfo._equipMeshIndicies = new List<int>();
        testItemInfo._equipMeshIndicies.Add(6);
        _items.Add(testItemInfo._itemKey, testItemInfo);


        testItemInfo = new ItemInfo();
        testItemInfo._itemName = "��������1";
        testItemInfo._sprite = null;
        testItemInfo._isStackAble = true;
        testItemInfo._itemKey = 32;
        testItemInfo._sizeX = 3;
        testItemInfo._sizeY = 3;
        testItemInfo._equipType = ItemInfo.EquipType.HumanBody;
        testItemInfo._meshObjectName = "EquipTest";
        testItemInfo._equipMeshIndicies = new List<int>();
        testItemInfo._equipMeshIndicies.Add(9);
        _items.Add(testItemInfo._itemKey, testItemInfo);




        testItemInfo = new ItemInfo();
        testItemInfo._itemName = "���Ű���1";
        testItemInfo._sprite = null;
        testItemInfo._isStackAble = true;
        testItemInfo._itemKey = 33;
        testItemInfo._sizeX = 5;
        testItemInfo._sizeY = 7;
        testItemInfo._equipType = ItemInfo.EquipType.All;
        testItemInfo._meshObjectName = "VanguardTest";
        testItemInfo._equipMeshIndicies = new List<int>();
        testItemInfo._equipMeshIndicies.Add(0);
        testItemInfo._equipMeshIndicies.Add(1);
        _items.Add(testItemInfo._itemKey, testItemInfo);
    }
}
