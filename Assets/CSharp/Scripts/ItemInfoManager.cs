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

    public Vector2 _position;       //����� ��ġ 
    public ItemInfo _info;          //����
    public int _count;              //����
    public int _storedIndex;        //����� ĭ
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

    public ItemInfo? GetItemInfo(int itemKey)
    {
        Debug.Assert(_items.ContainsKey(itemKey) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemKey);

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
        testItemInfo._equipMeshIndicies = new List<int>(3);
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
        testItemInfo._equipMeshIndicies = new List<int>(6);
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
        testItemInfo._equipMeshIndicies = new List<int>(9);
        _items.Add(testItemInfo._itemKey, testItemInfo);
    }
}
