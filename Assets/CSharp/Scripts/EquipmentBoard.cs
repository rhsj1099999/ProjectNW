using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static ItemAsset;
using static UnityEditor.Progress;

public class EquipmentBoard : MonoBehaviour, IMoveItemStore
{
    [SerializeField] private GameObject _equipmentUIObjectPrefab = null;
    private RectTransform _myRectTransform = null;

    /*-------------
    계속 바뀔 변수들
    -------------*/
    private List<GameObject> _leftWeaponEquipCells = new List<GameObject>();
    private List<GameObject> _rightWeaponEquipCells = new List<GameObject>();
    private List<GameObject> _itemEquipCells = new List<GameObject>();
    private Dictionary<EquipType, GameObject> _equipmentEquipCells = new Dictionary<EquipType, GameObject>();



    private Dictionary<EquipType, GameObject> _currEquippedMesh = new Dictionary<EquipType, GameObject>();
    private Dictionary<EquipType, GameObject> _currEquippedItemUIs = new Dictionary<EquipType, GameObject>();

    public void DeleteOnMe(ItemStoreDesc storeDesc)
    {
        if (storeDesc._itemAsset._EquipType == EquipType.Weapon)
        {
            UnEquipItem_Weapon(storeDesc);
        }
        else
        {
            //UnEquipUI(storeDesc);
            //UnEquipMesh(storeDesc);
        }
    }

    
    
    private void Awake()
    {
        Debug.Assert(_equipmentUIObjectPrefab != null, "프리팹 할당 안함");

        _myRectTransform = transform as RectTransform;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "장착칸이 없습니다");

        BoardCellDesc desc = new BoardCellDesc();
        desc._owner = this;

        foreach (EquipmentCell component in components) 
        {
            EquipType cellType = component.GetCellType();

            component.Initialize(desc);

            GameObject equipCellObject = component.gameObject;

            if (cellType == EquipType.Weapon)
            {
                List<GameObject> pushTarget = (equipCellObject.name.Contains("Right") == true)
                    ? _rightWeaponEquipCells
                    : _leftWeaponEquipCells;
                
                pushTarget.Add(equipCellObject);

                continue;
            }

            if (cellType == EquipType.UseAndComsumeableByCharacter)
            {
                _itemEquipCells.Add(equipCellObject);
                continue;
            }

            Debug.Assert(_equipmentEquipCells.ContainsKey(component.GetCellType()) == false, "셀의 장착타입이 중복됩니다.");
            _equipmentEquipCells.Add(component.GetCellType(), equipCellObject);
        }
    }




    public void AddItemUsingForcedIndex(ItemStoreDesc storedDesc, int targetX, int targetY) 
    {
        //장착 성공하면 여기 불린다
        storedDesc._isRotated = false;
        storedDesc._owner = this;

        if (storedDesc._itemAsset._EquipType == EquipType.Weapon)
        {
            //무기장착 함수 콜
            return;
        }

        if (storedDesc._itemAsset._EquipType == EquipType.UseAndComsumeableByCharacter)
        {
            //사용템 장착 함수 콜
        }

        EquipItem_Mesh(storedDesc, targetX, targetY);
    }

    public bool CheckItemDragDrop(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        startX = 0;
        startY = 0;

        if (storedDesc._itemAsset._EquipType == ItemAsset.EquipType.UseAndComsumeableByCharacter)
        {
            Debug.Assert(false, "사용템 장착 순서입니다");
            Debug.Break();
            return false;
        }


        if (storedDesc._itemAsset._EquipType == ItemAsset.EquipType.Weapon)
        {
            return true;
        }

        List<GameObject> cells = GetCells(storedDesc);

        foreach (var cell in cells)
        {
            //장착하려는 아이템중에 겹치는게 하나라도 있으면 무조건 종료
            //예를들어 전신장비 입으려는데 머리를 미리 장착하고 있다.
            if (_currEquippedItemUIs.ContainsKey(cell.GetComponent<EquipmentCell>().GetCellType()) == true)
            {
                return false;
            }
        }

        return true;
    }



    public List<GameObject> GetRestEquipmetItems(GameObject callerUI)
    {
        List<GameObject> restEquipments = new List<GameObject>();
        foreach (var item in _equipmentEquipCells)
        {
            GameObject childObject = item.Value.transform.GetChild(0).gameObject;

            if (childObject == callerUI.gameObject)
            {
                continue;
            }

            restEquipments.Add(childObject);
        }
        return restEquipments;
    }

    public void EquipItem_Weapon(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {
        GameObject equipmentUIObject =  Instantiate(_equipmentUIObjectPrefab, callerEquipCell.transform);

        RectTransform equipmentUIRectTransform = (RectTransform)equipmentUIObject.transform;
        RectTransform cellRectTransform = (RectTransform)callerEquipCell.transform;

        equipmentUIRectTransform.sizeDelta = new Vector2(cellRectTransform.rect.width, cellRectTransform.rect.height);
        equipmentUIRectTransform.position = cellRectTransform.position;

        ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

        storeDesc._owner = this;

        itemBaseComponent.Initialize(this, storeDesc);

        bool isRight = callerEquipCell.gameObject.name.Contains("Right");

        int index = callerEquipCell.gameObject.name.Last() - 49;

        GameObject weaponPrefab = ItemInfoManager.Instance.GetItemSubInfo_Weapon(storeDesc._itemAsset)._WeaponPrefab;

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        myUIComponent.GetReturnObject().GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, weaponPrefab);
    }

    public void UnEquipItem_Weapon(ItemStoreDesc storeDesc)
    {
        //GameObject targetObject = null;
        //EquipmentCell equipcellComponent = null;
        //foreach (var uiObject in _rightWeaponEquipCells)
        //{
        //    equipcellComponent = uiObject.GetComponent<EquipmentCell>();
        //    if (equipcellComponent.GetItemStoreDesc() == storeDesc)
        //    {
        //        targetObject = uiObject;
        //        break;
        //    }
        //}

        //if (targetObject == null)
        //{
        //    foreach (var uiObject in _leftWeaponEquipCells)
        //    {
        //        equipcellComponent = uiObject.GetComponent<EquipmentCell>();
        //        if (equipcellComponent.GetItemStoreDesc() == storeDesc)
        //        {
        //            targetObject = uiObject;
        //            break;
        //        }
        //    }
        //}

        //if (targetObject == null)
        //{
        //    Debug.Assert(false, "못찾았다");
        //    Debug.Break();
        //}


        //UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        //bool isRight = targetObject.name.Contains("Right");

        //int index = targetObject.name.Last() - 49;

        //myUIComponent.GetReturnObject().GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, null);

        //equipcellComponent.ClearItemStoreDesc();
    }


    private void EquipItem_Mesh(ItemStoreDesc storedDesc, int targetX, int targetY)
    {
        List<GameObject> cells = GetCells(storedDesc);

        foreach (var cell in cells)
        {
            //UI를 해당 Cell의 자식으로 생성합니다.
            RectTransform equipCellTransform = cell.GetComponent<RectTransform>();
            GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, equipCellTransform);
            ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();
            itemBaseComponent.Initialize(this, storedDesc);

            //크기, 위치를 낑겨넣습니다.
            RectTransform equipmentUIObjectTransform = equipmentUIObject.GetComponent<RectTransform>();
            equipmentUIObjectTransform.sizeDelta = new Vector2(equipCellTransform.rect.width, equipCellTransform.rect.height);
            equipCellTransform.position = equipCellTransform.position;

            _currEquippedItemUIs.Add(cell.GetComponent<EquipmentCell>().GetCellType(), equipmentUIObject);
        }

        //메쉬 장착
        {
            EquipItemMesh(storedDesc);
        }

        //foreach (var item in _equipCellUIs)
        //{
        //    if ((item.Key & storeDesc._itemAsset._EquipType) != EquipType.None)
        //    {
        //        item.Value.GetComponent<EquipmentCell>().SetItemStoreDesc(storeDesc);
        //    }
        //}
    }



    //public bool EquipItem(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    //{
    //    List<GameObject> cells = GetCells(storeDesc);

    //    foreach (var cell in cells)
    //    {
    //        if (_currEquippedItemUIs.ContainsKey(cell.GetComponent<EquipmentCell>().GetCellType()) == true) return false;
    //    }

    //    storeDesc._owner.DeleteOnMe(storeDesc);

    //    foreach (var cell in cells)
    //    {
    //        GameObject equipmentUIObject = (storeDesc._itemAsset._EquipType == EquipType.All)
    //            ? Instantiate(_equipmentUIObjectPrefab, _equipCellUIs[cell.GetComponent<EquipmentCell>().GetCellType()].gameObject.transform)
    //            : Instantiate(_equipmentUIObjectPrefab, callerEquipCell.transform);

    //        RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();
    //        RectTransform cellRectTransform = cell.GetComponent<RectTransform>();
    //        equipmentUIRectTransform.sizeDelta = new Vector2(cellRectTransform.rect.width, cellRectTransform.rect.height);
    //        equipmentUIRectTransform.position = cellRectTransform.position;

    //        ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

    //        storeDesc._owner = this;

    //        itemBaseComponent.Initialize(this, storeDesc);

    //        _currEquippedItemUIs.Add(cell.GetComponent<EquipmentCell>().GetCellType(), equipmentUIObject);
    //    }

    //    //메쉬 장착
    //    {
    //        EquipItemMesh(storeDesc);
    //    }

    //    foreach (var item in _equipCellUIs)
    //    {
    //        if ((item.Key & storeDesc._itemAsset._EquipType) != EquipType.None)
    //        {
    //            item.Value.GetComponent<EquipmentCell>().SetItemStoreDesc(storeDesc);
    //        }
    //    }

    //    return true;
    //}

    //private void UnEquipUI(ItemStoreDesc storeDesc)
    //{
    //    var keys = _currEquippedItemUIs.Keys.ToList();
    //    foreach (var key in keys)
    //    {
    //        if ((key & storeDesc._itemAsset._EquipType) == EquipType.None)
    //        {
    //            continue;
    //        }

    //        _equipCellUIs[key].GetComponent<EquipmentCell>().ClearItemStoreDesc();
    //        _currEquippedItemUIs.Remove(key);
    //    }
    //}

    //private void UnEquipMesh(ItemStoreDesc storeDesc)
    //{
    //    if (IsSameSkelaton(storeDesc._itemAsset) == true)
    //    {
    //        GameObject mesh = _currEquippedMesh[storeDesc._itemAsset._EquipType];
    //        Destroy(mesh);
    //    }
    //    else
    //    {
    //        UIComponent myUIComponent = GetComponentInParent<UIComponent>();
    //        GameObject uiReturnOwner = myUIComponent.GetReturnObject();
    //        CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();
    //        ownerCharacterAnimatorScript.ResetCharacterModel();
    //    }

    //    foreach (var item in _equipCellUIs)
    //    {
    //        if ((item.Key & storeDesc._itemAsset._EquipType) == EquipType.None)
    //        {
    //            //Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "장착하지 않았는데 장착해제합니다??");
    //            continue;
    //        }

    //        _currEquippedMesh.Remove(item.Key);
    //    }
    //}


    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        /*--------------------------------------------------------------
        |NOTI| 뼈구조가 밀리터리맨과 다르면 무조건 전신갑옷(스킨) 취급입니다.
        그리고 부분 값옷들 컬링이 되면 안되는데 컬링이 되는 경우가 있습니다.
        Bound와 관련된 문제인지 찾아봐야합니다.
        --------------------------------------------------------------*/

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        GameObject uiReturnOwner = myUIComponent.GetReturnObject();

        CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();

        GameObject originalAnimatorGameObject = ownerCharacterAnimatorScript.GetCurrActivatedModelObject();

        GameObject equippedMesh = null;

        ItemSubInfo_EquipMesh equipInfo = ItemInfoManager.Instance.GetItemSubInfo_EquipmentMesh(storeDesc._itemAsset);

        if (IsSameSkelaton(storeDesc._itemAsset) == true)
        {
            List<GameObject> equipMeshes = equipInfo._EquipmentMeshes;

            foreach (var item in equipMeshes) //보통 하나임
            {
                equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = originalAnimatorGameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                skinnedMeshRenderer.updateWhenOffscreen = true;
            }
        }
        else
        {
            equippedMesh = ownerCharacterAnimatorScript.ModelChange(equipInfo._EquipmentPrefab);
        }

        //foreach (var item in _equipCellUIs)
        //{
        //    if ((item.Key & storeDesc._itemAsset._EquipType) != EquipType.None)
        //    {
        //        _currEquippedMesh.Add(item.Key, equippedMesh);
        //    }
        //}
    }

    private bool IsSameSkelaton(ItemAsset itemAsset)
    {
        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        if (myUIComponent == null)
        {
            Debug.Assert(false, "모든 UI는 부모에 UIComponent 1개 만을 가지고있어야합니다");
            Debug.Break();
        }

        CharacterAnimatorScript ownerCharacterAnimatorScript = myUIComponent.GetReturnObject().GetComponentInChildren<CharacterAnimatorScript>();

        if (ownerCharacterAnimatorScript == null) 
        {
            Debug.Assert(false, "다른 뼈 구조인지 확인하기 위해선 CharacterAnimatorScript가 필요합니다");
            Debug.Break();
        }

        ItemSubInfo_EquipMesh equipmentSubInfo = ItemInfoManager.Instance.GetItemSubInfo_EquipmentMesh(itemAsset);

        Animator prefabAnimator = equipmentSubInfo._EquipmentPrefab.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "입으려는 장비Prefab은 반드시 Animator를 가지고 있어야 합니다");

        return ownerCharacterAnimatorScript.IsSameSkeleton(equipmentSubInfo._EquipmentAvatar);
    }

    private List<GameObject> GetCells(ItemStoreDesc storeDesc)
    {
        List<GameObject> retCells = new List<GameObject>();

        foreach (KeyValuePair<EquipType, GameObject> cell in _equipmentEquipCells)
        {
            if ((cell.Key & storeDesc._itemAsset._EquipType) != EquipType.None)
            {
                retCells.Add(cell.Value);
            }
        }

        return retCells;
    }
}
