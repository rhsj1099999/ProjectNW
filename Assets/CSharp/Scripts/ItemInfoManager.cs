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
    //아이템을 장착하거나 인벤토리에 보유하면, 이 클래스로 감싸서 보관한다.
    //위치 정보, 회전해있었는지 등등을 갖고있다.

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

    public ItemAsset _itemAsset;    //인포
    public int _storedIndex;        //저장된 칸(장비창이면 거의 0)
    public int _count;              //개수
    public bool _isRotated;         //회전된 채로 저장돼있었나?

    public IMoveItemStore _owner;   //여기서로부터 왔다.
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

        //아이템 인포를 형성한다
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
                Debug.Assert(false, "타겟이 중복됩니다" + targetItemInfo.name + "//" + item.ToString());
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
                Debug.Assert(false, "타겟이 중복됩니다 아이템 키 : " + item._ItemKey);
            }

            _itemInfo.Add(item._ItemKey, item);
        }
    }







    public ItemAsset GetItemInfo(int itemKey)
    {
        Debug.Assert(_itemInfo.ContainsKey(itemKey) == true, "없는 아이템 인포를 요청했습니다 key : " + itemKey);
        return _itemInfo[itemKey];
    }

    public ItemSubInfo_WeaponInfo GetItemSubInfo_Weapon(ItemAsset itemAsset)
    {
        ItemSubInfo_WeaponInfo asset = null;
        _itemSubInfo_WeaponInfo.TryGetValue(itemAsset, out asset);
        if (asset == null)
        {
            Debug.Assert(false, "해당 아이템은 무기정보가 없습니다");
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
            Debug.Assert(false, "해당 아이템은 장착 메쉬가없습니다");
            Debug.Break();
            return null;
        }

        return asset;
    }




    //public GameObject GetEquipmentPrefab(string prefabName)
    //{
    //    //Debug.Assert(_equipmentInfo.ContainsKey(prefabName) == true, "존재하지 않는 프리팹을 요청합니다. 리소스 변화가 있었습니까?");
    //    //return _equipmentInfo[prefabName];
    //    return null;
    //}




    //private void InitItem_TestCode_MustDel()
    //{
    //    /*-----------------------------------------------------------------
    //    |TODO| 외부에서 파싱하는걸 생각해볼것 //임시로 아이템 데이터 만드는 함수
    //    -----------------------------------------------------------------*/

    //    ItemInfo testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "군모1";
    //    testItemInfo._itemKey = 30;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 1;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanHead;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(3);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "장갑1";
    //    testItemInfo._itemKey = 31;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanArm;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(6);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);





    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "군용조끼1";
    //    testItemInfo._itemKey = 32;
    //    testItemInfo._sizeX = 3;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.HumanBody;
    //    testItemInfo._meshObjectName = "BasicCharacter";
    //    testItemInfo._equipMeshIndicies = new List<int>();
    //    testItemInfo._equipMeshIndicies.Add(9);
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "전신갑옷1";
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
    //    testItemInfo._itemName = "전신갑옷2";
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
    //    testItemInfo._itemName = "양손해머1";
    //    testItemInfo._itemKey = 35;
    //    testItemInfo._sizeX = 2;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);




    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "한손검1";
    //    testItemInfo._itemKey = 36;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 3;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "권총1";
    //    testItemInfo._itemKey = 37;
    //    testItemInfo._sizeX = 1;
    //    testItemInfo._sizeY = 1;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);


    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "라이플1";
    //    testItemInfo._itemKey = 38;
    //    testItemInfo._sizeX = 4;
    //    testItemInfo._sizeY = 2;
    //    testItemInfo._equipType = ItemInfo.EquipType.Weapon;
    //    _itemInfo.Add(testItemInfo._itemKey, testItemInfo);



    //    testItemInfo = new ItemInfo();
    //    testItemInfo._itemName = "방패1";
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
    //    testItemInfo._itemName = "너클";
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
    //    testItemInfo._itemName = "헤이조너클";
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
