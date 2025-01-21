using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;









public class ItemInfoManager : SubManager<ItemInfoManager>
{
    [SerializeField] List<ItemAsset> _itemAssets_Init = new List<ItemAsset>();
    Dictionary<string, int> _itemKeys = new Dictionary<string, int>();
    Dictionary<int, ItemAsset> _itemAssets = new Dictionary<int, ItemAsset>();

    public override void SubManagerInit()
    {
        SingletonAwake();

        {
            //InitItemSubInfo(_itemSubInfo_WeaponInfo_Init, _itemSubInfo_WeaponInfo);
        }

        //������ ������ �����Ѵ�
        {
            InitItem();
        }
    }

    //private void InitItemSubInfo<T>(List<T> initContainer, Dictionary<ItemAsset, T> targetContainer) 
    //    where T : ItemSubInfo
    //{
    //    foreach (T item in initContainer)
    //    {
    //        ItemAsset targetItemInfo = item._UsingThisItemAssets;

    //        if (targetContainer.ContainsKey(targetItemInfo) == true)
    //        {
    //            Debug.Assert(false, "Ÿ���� �ߺ��˴ϴ�" + targetItemInfo.name + "//" + item.ToString());
    //        }

    //        targetContainer.Add(targetItemInfo, item);
    //    }
    //}





    private void InitItem()
    {
        int itemKeyRecord = 0;

        foreach (ItemAsset item in _itemAssets_Init)
        {
            item._ItemKey = itemKeyRecord;

            if (_itemKeys.ContainsKey(item._ItemName) == true)
            {
                Debug.Assert(false, "������ �̸��� �ߺ��˴ϴ�" + item._ItemName);
                Debug.Break();
            }

            _itemKeys.Add(item._ItemName, itemKeyRecord);
            _itemAssets.Add(item._ItemKey, item);

            itemKeyRecord++;
        }
    }




    public ItemAsset GetItemInfo(string itemName)
    {
        Debug.Assert(_itemKeys.ContainsKey(itemName) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemName);
        int key = _itemKeys[itemName];

        return _itemAssets[key];
    }

    public int GetItemKey(string itemName)
    {
        Debug.Assert(_itemKeys.ContainsKey(itemName) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemName);
        return _itemKeys[itemName];
    }

    public ItemAsset GetItemInfo(int itemKey)
    {
        Debug.Assert(_itemAssets.ContainsKey(itemKey) == true, "���� ������ ������ ��û�߽��ϴ� key : " + itemKey);
        return _itemAssets[itemKey];
    }

    //public ItemAsset_Weapon GetItemSubInfo_Weapon(ItemAsset itemAsset)
    //{
    //    ItemAsset_Weapon asset = null;
    //    _itemSubInfo_WeaponInfo.TryGetValue(itemAsset, out asset);
    //    if (asset == null)
    //    {
    //        Debug.Assert(false, "�ش� �������� ���������� �����ϴ�");
    //        Debug.Break();
    //        return null;
    //    }

    //    return asset;
    //}



    //public ItemAsset_EquipMesh GetItemSubInfo_EquipmentMesh(ItemAsset itemInfo)
    //{
    //    ItemAsset_EquipMesh asset = null;
    //    _itemSubInfo_EquipMesh.TryGetValue(itemInfo, out asset);
    //    if (asset == null)
    //    {
    //        Debug.Assert(false, "�ش� �������� ���� �޽��������ϴ�");
    //        Debug.Break();
    //        return null;
    //    }

    //    return asset;
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
