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
    //            Debug.Assert(false, "타겟이 중복됩니다" + targetItemInfo.name + "//" + item.ToString());
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
                Debug.Assert(false, "아이템 이름이 중복됩니다" + item._ItemName);
                Debug.Break();
            }

            _itemKeys.Add(item._ItemName, itemKeyRecord);
            _itemAssets.Add(item._ItemKey, item);

            itemKeyRecord++;
        }
    }




    public ItemAsset GetItemInfo(string itemName)
    {
        Debug.Assert(_itemKeys.ContainsKey(itemName) == true, "없는 아이템 인포를 요청했습니다 key : " + itemName);
        int key = _itemKeys[itemName];

        return _itemAssets[key];
    }

    public int GetItemKey(string itemName)
    {
        Debug.Assert(_itemKeys.ContainsKey(itemName) == true, "없는 아이템 인포를 요청했습니다 key : " + itemName);
        return _itemKeys[itemName];
    }

    public ItemAsset GetItemInfo(int itemKey)
    {
        Debug.Assert(_itemAssets.ContainsKey(itemKey) == true, "없는 아이템 인포를 요청했습니다 key : " + itemKey);
        return _itemAssets[itemKey];
    }

    public ItemAsset.ItemType GetItemType(int itemKey)
    {
        Debug.Assert(_itemAssets.ContainsKey(itemKey) == true, "없는 아이템 인포를 요청했습니다 key : " + itemKey);
        return _itemAssets[itemKey]._ItemType;
    }


    private void GetActivatedRenderers(GameObject targetObject, ref Bounds ret, GameObject fromOwner)
    {
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length <= 0)
        {
            //기본 사이즈 설정
            return;
        }

        Bounds firstBound = renderers[0].bounds;
        ret = firstBound;

        foreach (var renderer in renderers)
        {
            if (renderer.enabled == false)
            {
                continue;
            }

            Bounds diffBound = renderer.bounds;
            ret.Encapsulate(diffBound);
        }
    }



    public void DropItemToField(Transform callerTransform, ItemStoreDescBase itemStoreDesc)
    {
        //아이템 생성
        GameObject dropItemGameObject = new GameObject(itemStoreDesc._itemAsset._ItemName);
        dropItemGameObject.transform.position = Vector3.zero;
        dropItemGameObject.transform.rotation = Quaternion.identity;

        GameObject dropItemModel = Instantiate(itemStoreDesc._itemAsset._ItemModel, dropItemGameObject.transform);
        dropItemModel.transform.localPosition = Vector3.zero;
        dropItemModel.transform.localRotation = Quaternion.identity;

        WeaponModelScript weaponModelScript = dropItemModel.GetComponent<WeaponModelScript>();
        if (weaponModelScript != null) 
        {
            weaponModelScript.OffCollider();
        }

        Bounds itemBounds = new Bounds();
        GetActivatedRenderers(dropItemModel, ref itemBounds, callerTransform.gameObject);

        Rigidbody addRigidBody = dropItemGameObject.AddComponent<Rigidbody>();
        {
            addRigidBody.drag = 0.5f;
            addRigidBody.angularDrag = 0.5f;
            addRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            addRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            addRigidBody.includeLayers = 0;
            addRigidBody.excludeLayers = 0;
        }

        CapsuleCollider addCapsuleCollider = dropItemGameObject.AddComponent<CapsuleCollider>();
        {
            Vector3 lengths = new Vector3(itemBounds.size.x, itemBounds.size.y, itemBounds.size.z);
            int heightIndex = 0;
            float maxVal = 0.0f;
            for (int i = 0; i < 3; i++)
            {
                if (maxVal <= lengths[i])
                {
                    maxVal = lengths[i];
                    heightIndex = i;
                }
            }

            addCapsuleCollider.direction = heightIndex;
            addCapsuleCollider.includeLayers = LayerMask.GetMask("StaticNavMeshLayer");
            addCapsuleCollider.excludeLayers = 0;
            addCapsuleCollider.center = itemBounds.center;
            addCapsuleCollider.height = lengths[heightIndex];
            lengths[heightIndex] = 0.0f;
            addCapsuleCollider.radius = lengths.magnitude / 2.0f;
        }



        GameObject dropItemInteraction = new GameObject("Interaction");
        dropItemInteraction.SetActive(false);
        dropItemInteraction.layer = LayerMask.NameToLayer("InteractionableCollider");
        dropItemInteraction.transform.SetParent(dropItemGameObject.transform);
        dropItemInteraction.transform.position = Vector3.zero;
        dropItemInteraction.transform.rotation = Quaternion.identity;

        CapsuleCollider interactionCollider = dropItemInteraction.AddComponent<CapsuleCollider>();
        {
            interactionCollider.direction = addCapsuleCollider.direction;
            interactionCollider.center = addCapsuleCollider.center;
            interactionCollider.height = addCapsuleCollider.height;
            interactionCollider.radius = addCapsuleCollider.radius;
            interactionCollider.isTrigger = true;
        }

        UICall_AcquireItem interactionUIComponent = dropItemInteraction.AddComponent<UICall_AcquireItem>();
        UICall_AcquireItem.UICall_AcquireItemDesc newDesc = new UICall_AcquireItem.UICall_AcquireItemDesc();
        newDesc._itemStoreDesc = itemStoreDesc;
        newDesc._itemTarget = dropItemGameObject;
        newDesc._offCollider = interactionCollider;
        interactionUIComponent.Init(newDesc);
        dropItemInteraction.SetActive(true);



        dropItemGameObject.transform.position = callerTransform.gameObject.transform.position + Vector3.up * 1.5f;
        dropItemGameObject.transform.rotation = callerTransform.gameObject.transform.rotation;

        {
            float itemThrowForce = 1.0f;

            addRigidBody.position = callerTransform.gameObject.transform.position + Vector3.up * 1.5f;
            Vector3 initialForceVector = Quaternion.AngleAxis(-30.0f, callerTransform.gameObject.transform.right) * callerTransform.gameObject.transform.forward * itemThrowForce;
            addRigidBody.AddForce(initialForceVector, ForceMode.Impulse);
        }
    }



    //public ItemAsset_Weapon GetItemSubInfo_Weapon(ItemAsset itemAsset)
    //{
    //    ItemAsset_Weapon asset = null;
    //    _itemSubInfo_WeaponInfo.TryGetValue(itemAsset, out asset);
    //    if (asset == null)
    //    {
    //        Debug.Assert(false, "해당 아이템은 무기정보가 없습니다");
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
    //        Debug.Assert(false, "해당 아이템은 장착 메쉬가없습니다");
    //        Debug.Break();
    //        return null;
    //    }

    //    return asset;
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
