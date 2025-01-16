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
    ��� �ٲ� ������
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
        Debug.Assert(_equipmentUIObjectPrefab != null, "������ �Ҵ� ����");

        _myRectTransform = transform as RectTransform;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "����ĭ�� �����ϴ�");

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

            Debug.Assert(_equipmentEquipCells.ContainsKey(component.GetCellType()) == false, "���� ����Ÿ���� �ߺ��˴ϴ�.");
            _equipmentEquipCells.Add(component.GetCellType(), equipCellObject);
        }
    }




    public void AddItemUsingForcedIndex(ItemStoreDesc storedDesc, int targetX, int targetY) 
    {
        //���� �����ϸ� ���� �Ҹ���
        storedDesc._isRotated = false;
        storedDesc._owner = this;

        if (storedDesc._itemAsset._EquipType == EquipType.Weapon)
        {
            //�������� �Լ� ��
            return;
        }

        if (storedDesc._itemAsset._EquipType == EquipType.UseAndComsumeableByCharacter)
        {
            //����� ���� �Լ� ��
        }

        EquipItem_Mesh(storedDesc, targetX, targetY);
    }

    public bool CheckItemDragDrop(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation)
    {
        startX = 0;
        startY = 0;

        if (storedDesc._itemAsset._EquipType == ItemAsset.EquipType.UseAndComsumeableByCharacter)
        {
            Debug.Assert(false, "����� ���� �����Դϴ�");
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
            //�����Ϸ��� �������߿� ��ġ�°� �ϳ��� ������ ������ ����
            //������� ������� �������µ� �Ӹ��� �̸� �����ϰ� �ִ�.
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
        //    Debug.Assert(false, "��ã�Ҵ�");
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
            //UI�� �ش� Cell�� �ڽ����� �����մϴ�.
            RectTransform equipCellTransform = cell.GetComponent<RectTransform>();
            GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, equipCellTransform);
            ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();
            itemBaseComponent.Initialize(this, storedDesc);

            //ũ��, ��ġ�� ���ֽܳ��ϴ�.
            RectTransform equipmentUIObjectTransform = equipmentUIObject.GetComponent<RectTransform>();
            equipmentUIObjectTransform.sizeDelta = new Vector2(equipCellTransform.rect.width, equipCellTransform.rect.height);
            equipCellTransform.position = equipCellTransform.position;

            _currEquippedItemUIs.Add(cell.GetComponent<EquipmentCell>().GetCellType(), equipmentUIObject);
        }

        //�޽� ����
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

    //    //�޽� ����
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
    //            //Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");
    //            continue;
    //        }

    //        _currEquippedMesh.Remove(item.Key);
    //    }
    //}


    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        /*--------------------------------------------------------------
        |NOTI| �������� �и��͸��ǰ� �ٸ��� ������ ���Ű���(��Ų) ����Դϴ�.
        �׸��� �κ� ���ʵ� �ø��� �Ǹ� �ȵǴµ� �ø��� �Ǵ� ��찡 �ֽ��ϴ�.
        Bound�� ���õ� �������� ã�ƺ����մϴ�.
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

            foreach (var item in equipMeshes) //���� �ϳ���
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
            Debug.Assert(false, "��� UI�� �θ� UIComponent 1�� ���� �������־���մϴ�");
            Debug.Break();
        }

        CharacterAnimatorScript ownerCharacterAnimatorScript = myUIComponent.GetReturnObject().GetComponentInChildren<CharacterAnimatorScript>();

        if (ownerCharacterAnimatorScript == null) 
        {
            Debug.Assert(false, "�ٸ� �� �������� Ȯ���ϱ� ���ؼ� CharacterAnimatorScript�� �ʿ��մϴ�");
            Debug.Break();
        }

        ItemSubInfo_EquipMesh equipmentSubInfo = ItemInfoManager.Instance.GetItemSubInfo_EquipmentMesh(itemAsset);

        Animator prefabAnimator = equipmentSubInfo._EquipmentPrefab.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");

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
