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

    public Vector2 _position;       //저장된 위치 
    public ItemInfo _info;          //인포
    public int _count;              //개수
    public int _storedIndex;        //저장된 칸
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


    public List<GameObject> GetMeshes(ItemInfo itemInfo) //전신장비들은 비트연산이 하나라도 있으면 세트템을 줄거다
    {
        Debug.Assert(_equipmentObject.ContainsKey(itemInfo._meshObjectName) == true, "아이템 인포에 meshObject가 없습니다");

        List<GameObject> equipMeshes = _equipmentObject[itemInfo._meshObjectName];

        List<GameObject> targetMeshes = new List<GameObject>();

        foreach (var index in itemInfo._equipMeshIndicies)
        {
            if (index >= equipMeshes.Count)
            {
                Debug.Log("인덱스를 벗어난 메쉬를 요청합니다. 모델의 수정이 있었습니까?");
            }

            targetMeshes.Add(equipMeshes[index]);
        }

        return targetMeshes;
    }

    public ItemInfo GetItemInfo(int itemKey)
    {
        Debug.Assert(_items.ContainsKey(itemKey) == true, "없는 아이템 인포를 요청했습니다 key : " + itemKey);

        return _items[itemKey];
    }


    public GameObject GetEquipmentPrefab(string prefabName)
    {
        Debug.Assert(_equipmentPrefabs.ContainsKey(prefabName) == true, "존재하지 않는 프리팹을 요청합니다. 리소스 변화가 있었습니까?");
        return _equipmentPrefabs[prefabName];
    }



    private void InitEquipments()
    {
        //asset 폴더 내에 기존 메쉬들을 등록하는 함수 

        //Dictionary<string, List<GameObject>> _equipmentObject

        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("EquipmentModels");

        foreach (GameObject prefab in loadedPrefabs)
        {
            Debug.Assert(_equipmentPrefabs.ContainsKey(prefab.name) == false, "프리팹 원본이 중복됩니다");

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
        //임시로 아이템 데이터 만드는 함수
        /*---------------------------------
        |TODO| 외부에서 파싱하는걸 생각해볼것 
        ---------------------------------*/

        ItemInfo testItemInfo = new ItemInfo();
        testItemInfo._itemName = "군모1";
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
        testItemInfo._itemName = "장갑1";
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
        testItemInfo._itemName = "군용조끼1";
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
        testItemInfo._itemName = "전신갑옷1";
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
