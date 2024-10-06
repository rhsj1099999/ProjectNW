using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;

public struct ItemInfo
{
    public enum EquipType
    {
        None = 0,
        HumanHead,
        HumanArm,
        HumanLeg,
        HumanBody,
        HumanBackpack,
        RifleWeapon,
        HandgunWeapon,
        End,
        AllEquipment,
    }


    public int _itemKey;
    public string _itemName;
    public int _sizeX;
    public int _sizeY;

    public bool _isStackAble;
    public int _maxStack;

    public Sprite _sprite;

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
    public InventoryBoard _owner;

}

public class ItemInfoManager : MonoBehaviour
{
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

    private void Awake()
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


    Dictionary<string, List<GameObject>> _equipmentObject = new Dictionary<string, List<GameObject>>();
    Dictionary<int, ItemInfo> _items = new Dictionary<int, ItemInfo>();

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

    public ItemInfo? GetItemInfo(int itemKey)
    {
        Debug.Assert(_items.ContainsKey(itemKey) == true, "없는 아이템 인포를 요청했습니다 key : " + itemKey);

        return _items[itemKey];
    }



    private void InitEquipments()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("IgnoreResources/EquipmentModels");

        foreach (GameObject prefab in loadedPrefabs)
        {
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
        testItemInfo._equipMeshIndicies = new List<int>(3);
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
        testItemInfo._equipMeshIndicies = new List<int>(6);
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
        testItemInfo._equipMeshIndicies = new List<int>(9);
        _items.Add(testItemInfo._itemKey, testItemInfo);
    }
}
