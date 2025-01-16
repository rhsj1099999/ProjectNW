using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;







public class ItemStoreDesc
{
    //�������� �����ϰų� �κ��丮�� �����ϸ�, �� Ŭ������ ���μ� �����Ѵ�.
    //��ġ ����, ȸ�����־����� ����� �����ִ�.

    private ItemStoreDesc() { }
    public ItemStoreDesc
        (
        ItemAsset itemAsset,
        int count,
        int stroredIndex,
        bool isRotated,
        IMoveItemStore fromInstance
        )
    {
        _itemAsset = itemAsset;
        _count = count;
        _storedIndex = stroredIndex;
        _isRotated = isRotated;
        _owner = fromInstance;
    }


    public UInt64 _absoluteKey = 0;

    public ItemAsset _itemAsset;    //����
    public int _storedIndex;        //����� ĭ(���â�̸� ���� 0)
    public int _count;              //����
    public bool _isRotated;         //ȸ���� ä�� ������־���?

    public IMoveItemStore _owner;   //���⼭�κ��� �Դ�.
}

public class ItemInfoManager : SubManager<ItemInfoManager>
{
    [SerializeField] List<ItemSubInfo_EquipMesh>    _itemSubInfo_EquipMesh_Init = new List<ItemSubInfo_EquipMesh>();
    [SerializeField] List<ItemSubInfo_UIInfo>       _itemSubInfo_UIInfo_Init = new List<ItemSubInfo_UIInfo>();
    [SerializeField] List<ItemSubInfo_WeaponInfo>   _itemSubInfo_WeaponInfo_Init = new List<ItemSubInfo_WeaponInfo>();
    [SerializeField] List<ItemSubInfo_UsingInfo>    _itemSubInfo_UsingInfo_Init = new List<ItemSubInfo_UsingInfo>();

    Dictionary<ItemAsset, ItemSubInfo_EquipMesh>    _itemSubInfo_EquipMesh = new Dictionary<ItemAsset, ItemSubInfo_EquipMesh>();
    Dictionary<ItemAsset, ItemSubInfo_UIInfo>       _itemSubInfo_UIInfo = new Dictionary<ItemAsset, ItemSubInfo_UIInfo>();
    Dictionary<ItemAsset, ItemSubInfo_WeaponInfo>   _itemSubInfo_WeaponInfo = new Dictionary<ItemAsset, ItemSubInfo_WeaponInfo>();
    Dictionary<ItemAsset, ItemSubInfo_UsingInfo>    _itemSubInfo_UsingInfo = new Dictionary<ItemAsset, ItemSubInfo_UsingInfo>();

    //Dictionary<Type, Dictionary<ItemAsset, ItemSubInfo>> _itemSubInfo_Total = new Dictionary<Type, Dictionary<ItemAsset, ItemSubInfo>>();

    [SerializeField] List<ItemAsset>                _itemAssets_Init = new List<ItemAsset>();
    Dictionary<int, ItemAsset>                      _itemInfo = new Dictionary<int, ItemAsset>();
    

    public override void SubManagerInit()
    {
        SingletonAwake();

        {
            InitItemSubInfo(_itemSubInfo_EquipMesh_Init, _itemSubInfo_EquipMesh);
            InitItemSubInfo(_itemSubInfo_UIInfo_Init, _itemSubInfo_UIInfo);
            InitItemSubInfo(_itemSubInfo_WeaponInfo_Init, _itemSubInfo_WeaponInfo);
            InitItemSubInfo(_itemSubInfo_UsingInfo_Init, _itemSubInfo_UsingInfo);
        }

        //������ ������ �����Ѵ�
        {
            InitItem();
        }
    }

    private void InitItemSubInfo<T>(List<T> initContainer, Dictionary<ItemAsset, T> targetContainer) 
        where T : ItemSubInfo
    {
        foreach (T item in initContainer)
        {
            ItemAsset targetItemInfo = item._UsingThisItemAssets;

            if (targetContainer.ContainsKey(targetItemInfo) == true)
            {
                Debug.Assert(false, "Ÿ���� �ߺ��˴ϴ�" + targetItemInfo.name + "//" + item.ToString());
            }

            targetContainer.Add(targetItemInfo, item);

            //if (!_itemSubInfo_Total.ContainsKey(typeof(T)))
            //{
            //    _itemSubInfo_Total[typeof(T)] = targetContainer.ToDictionary(
            //        pair => pair.Key,
            //        pair => (ItemSubInfo)pair.Value
            //    );
            //}
            //else
            //{
            //    Debug.Assert(false, $"Type {typeof(T)} already exists in _itemSubInfo_Total");
            //}
        }
    }





    private void InitItem()
    {
        foreach (ItemAsset item in _itemAssets_Init)
        {
            if (_itemInfo.ContainsKey(item._ItemKey) == true)
            {
                Debug.Assert(false, "Ÿ���� �ߺ��˴ϴ� ������ Ű : " + item._ItemKey);
            }

            _itemInfo.Add(item._ItemKey, item);
        }
    }







    public ItemAsset GetItemInfo(int itemKey)
    {
        Debug.Assert(_itemInfo.ContainsKey(itemKey) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemKey);
        return _itemInfo[itemKey];
    }

    public ItemSubInfo_WeaponInfo GetItemSubInfo_Weapon(ItemAsset itemAsset)
    {
        ItemSubInfo_WeaponInfo asset = null;
        _itemSubInfo_WeaponInfo.TryGetValue(itemAsset, out asset);
        if (asset == null)
        {
            Debug.Assert(false, "�ش� �������� ���������� �����ϴ�");
            Debug.Break();
            return null;
        }

        return asset;
    }



    public ItemSubInfo_EquipMesh GetItemSubInfo_EquipmentMesh(ItemAsset itemInfo)
    {
        ItemSubInfo_EquipMesh asset = null;
        _itemSubInfo_EquipMesh.TryGetValue(itemInfo, out asset);
        if (asset == null)
        {
            Debug.Assert(false, "�ش� �������� ���� �޽��������ϴ�");
            Debug.Break();
            return null;
        }

        return asset;
    }




    //public GameObject GetEquipmentPrefab(string prefabName)
    //{
    //    //Debug.Assert(_equipmentInfo.ContainsKey(prefabName) == true, "�������� �ʴ� �������� ��û�մϴ�. ���ҽ� ��ȭ�� �־����ϱ�?");
    //    //return _equipmentInfo[prefabName];
    //    return null;
    //}




    //private void InitItem_TestCode_MustDel()
    //{
    //    /*-----------------------------------------------------------------
    //    |TODO| �ܺο��� �Ľ��ϴ°� �����غ��� //�ӽ÷� ������ ������ ����� �Լ�
    //    -----------------------------------------------------------------*/

    //    ItemInfo testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "����1";
    //    testItemInfo._itemKey = 30;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 1;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanHead;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(3);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "�尩1";
    //    testItemInfo._itemKey = 31;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanArm;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(6);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);





    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "��������1";
    //    testItemInfo._itemKey = 32;
    //    testItemInfo._sizeX = 3;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanBody;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(9);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "���Ű���1";
    //    testItemInfo._itemKey = 33;
    //    testItemInfo._sizeX = 5;
    //    testItemInfo._sizeY = 5;
    //    testItemInfo._equipType = ItemInfo.EquipType.All;
    //    testItemInfo._meshObjectName = "Vanguard";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(0);
    //    testItemInfo._equipMeshIndicies.Add(1);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "���Ű���2";
    //    testItemInfo._itemKey = 34;
    //    testItemInfo._sizeX = 4;
    //    testItemInfo._sizeY = 4;
    //    testItemInfo._equipType = ItemInfo.EquipType.All;
    //    testItemInfo._meshObjectName = "Paladin";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(0);
    //    testItemInfo._equipMeshIndicies.Add(1);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "����ظ�1";
    //    testItemInfo._itemKey = 35;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "�Ѽհ�1";
    //    testItemInfo._itemKey = 36;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "����1";
    //    testItemInfo._itemKey = 37;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 1;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);


    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "������1";
    //    testItemInfo._itemKey = 38;
    //    testItemInfo._sizeX = 4;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "����1";
    //    testItemInfo._itemKey = 39;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);










    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "Noel_Skin";
    //    testItemInfo._itemKey = 40;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 4;
    //    testItemInfo._equipType = ItemInfo.EquipType.All;
    //    testItemInfo._meshObjectName = "Noel";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(0);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);








    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "��Ŭ";
    //    testItemInfo._itemKey = 41;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);







    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "Beidou_Skin";
    //    testItemInfo._itemKey = 42;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 5;
    //    testItemInfo._equipType = ItemInfo.EquipType.All;
    //    testItemInfo._meshObjectName = "Beidou";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(0);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "��������Ŭ";
    //    testItemInfo._itemKey = 43;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);

    //}



    public override void SubManagerUpdate()
    {
    }

    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerLateUpdate()
    {
    }

    public override void SubManagerStart()
    {
    }
}
