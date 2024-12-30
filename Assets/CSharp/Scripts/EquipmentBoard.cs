using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static ItemInfo;

public class EquipmentBoard : MonoBehaviour, IMoveItemStore
{
    //[SerializeField] private GameObject _itemUIPrefab = null;
    [SerializeField] private GameObject _equipmentUIObjectPrefab = null;
    private RectTransform _myRectTransform = null;

    /*-------------
    ��� �ٲ� ������
    -------------*/
    private List<GameObject> _leftWeaponEquipCells = new List<GameObject>();
    private List<GameObject> _rightWeaponEquipCells = new List<GameObject>();
    private List<GameObject> _itemEquipCells = new List<GameObject>();
    private Dictionary<EquipType, GameObject> _equipCellUIs = new Dictionary<EquipType, GameObject>();
    private Dictionary<EquipType, GameObject> _currEquippedMesh = new Dictionary<EquipType, GameObject>();
    private Dictionary<EquipType, GameObject> _currEquippedItemUIs = new Dictionary<EquipType, GameObject>();

    public void DeleteOnMe(ItemStoreDesc storeDesc)
    {
        if (storeDesc._info._equipType == EquipType.Weapon)
        {
            UnEquipItem_Weapon(storeDesc);
        }
        else
        {
            UnEquipUI(storeDesc);
            UnEquipMesh(storeDesc);
        }
    }

    
    
    private void Awake()
    {
        Debug.Assert(_equipmentUIObjectPrefab != null, "������ �Ҵ� ����");

        _myRectTransform = transform as RectTransform;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "����ĭ�� �����ϴ�");

        EquipmentCellDesc desc;
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

            if (cellType != EquipType.Weapon)
            {
                Debug.Assert(_equipCellUIs.ContainsKey(component.GetCellType()) == false, "���� ����Ÿ���� �ߺ��˴ϴ�.");
                _equipCellUIs.Add(component.GetCellType(), equipCellObject);
            }
        }
    }



    public List<GameObject> GetRestEquipmetItems(GameObject callerUI)
    {
        List<GameObject> restEquipments = new List<GameObject>();
        foreach (var item in _equipCellUIs)
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

        GameObject weaponPrefab = ItemInfoManager.Instance.GetWeaponPrefab(storeDesc._info._itemName);

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        myUIComponent.GetReturnObject().GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, weaponPrefab);
    }





    public void UnEquipItem_Weapon(ItemStoreDesc storeDesc)
    {
        GameObject targetObject = null;
        EquipmentCell equipcellComponent = null;
        foreach (var uiObject in _rightWeaponEquipCells)
        {
            equipcellComponent = uiObject.GetComponent<EquipmentCell>();
            if (equipcellComponent.GetItemStoreDesc() == storeDesc)
            {
                targetObject = uiObject;
                break;
            }
        }

        if (targetObject == null)
        {
            foreach (var uiObject in _leftWeaponEquipCells)
            {
                equipcellComponent = uiObject.GetComponent<EquipmentCell>();
                if (equipcellComponent.GetItemStoreDesc() == storeDesc)
                {
                    targetObject = uiObject;
                    break;
                }
            }
        }

        if (targetObject == null)
        {
            Debug.Assert(false, "��ã�Ҵ�");
            Debug.Break();
        }


        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        bool isRight = targetObject.name.Contains("Right");

        int index = targetObject.name.Last() - 49;

        myUIComponent.GetReturnObject().GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, null);

        equipcellComponent.ClearItemStoreDesc();
    }





    public bool EquipItem(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {
        if (storeDesc._info._equipType == EquipType.UseAndComsumeableByCharacter)
        {
            Debug.Assert(false, "����� ���� �����Դϴ�");
            Debug.Break();
            return false;
        }


        if (storeDesc._info._equipType == EquipType.Weapon)
        {
            storeDesc._owner.DeleteOnMe(storeDesc);
            EquipItem_Weapon(storeDesc, callerEquipCell);
            callerEquipCell.GetComponent<EquipmentCell>().SetItemStoreDesc(storeDesc);
            return true;
        }

        List<GameObject> cells = GetCells(storeDesc);

        foreach (var cell in cells)
        {
            if (_currEquippedItemUIs.ContainsKey(cell.GetComponent<EquipmentCell>().GetCellType()) == true) return false;
        }

        storeDesc._owner.DeleteOnMe(storeDesc);

        foreach (var cell in cells)
        {
            GameObject equipmentUIObject = (storeDesc._info._equipType == EquipType.All)
                ? Instantiate(_equipmentUIObjectPrefab, _equipCellUIs[cell.GetComponent<EquipmentCell>().GetCellType()].gameObject.transform)
                : Instantiate(_equipmentUIObjectPrefab, callerEquipCell.transform);

            RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();
            RectTransform cellRectTransform = cell.GetComponent<RectTransform>();
            equipmentUIRectTransform.sizeDelta = new Vector2(cellRectTransform.rect.width, cellRectTransform.rect.height);
            equipmentUIRectTransform.position = cellRectTransform.position;

            ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

            storeDesc._owner = this;

            itemBaseComponent.Initialize(this, storeDesc);

            _currEquippedItemUIs.Add(cell.GetComponent<EquipmentCell>().GetCellType(), equipmentUIObject);
        }

        //�޽� ����
        {
            EquipItemMesh(storeDesc);
        }

        foreach (var item in _equipCellUIs)
        {
            if ((item.Key & storeDesc._info._equipType) != EquipType.None)
            {
                item.Value.GetComponent<EquipmentCell>().SetItemStoreDesc(storeDesc);
            }
        }

        return true;
    }

    private void UnEquipUI(ItemStoreDesc storeDesc)
    {
        var keys = _currEquippedItemUIs.Keys.ToList();
        foreach (var key in keys)
        {
            if ((key & storeDesc._info._equipType) == EquipType.None)
            {
                continue;
            }

            _equipCellUIs[key].GetComponent<EquipmentCell>().ClearItemStoreDesc();
            _currEquippedItemUIs.Remove(key);
        }
    }

    private void UnEquipMesh(ItemStoreDesc storeDesc)
    {
        if (IsSameSkelaton(storeDesc._info) == true)
        {
            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];
            Destroy(mesh);
        }
        else
        {
            UIComponent myUIComponent = GetComponentInParent<UIComponent>();
            GameObject uiReturnOwner = myUIComponent.GetReturnObject();
            CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();
            ownerCharacterAnimatorScript.ResetCharacterModel();
        }

        foreach (var item in _equipCellUIs)
        {
            if ((item.Key & storeDesc._info._equipType) == EquipType.None)
            {
                //Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");
                continue;
            }

            _currEquippedMesh.Remove(item.Key);
        }
    }


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

        if (IsSameSkelaton(storeDesc._info) == true)
        {
            List<GameObject> equipMeshes = ItemInfoManager.Instance.GetMeshes(storeDesc._info);

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
            equippedMesh = ownerCharacterAnimatorScript.ModelChange(ItemInfoManager.Instance.GetEquipmentPrefab(storeDesc._info._meshObjectName));
        }

        foreach (var item in _equipCellUIs)
        {
            if ((item.Key & storeDesc._info._equipType) != EquipType.None)
            {
                _currEquippedMesh.Add(item.Key, equippedMesh);
            }
        }
    }

    private bool IsSameSkelaton(ItemInfo itemInfo)
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

        GameObject prefabObject = ItemInfoManager.Instance.GetEquipmentPrefab(itemInfo._meshObjectName);
        Animator prefabAnimator = prefabObject.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");

        Avatar prefabAvatar = prefabAnimator.avatar;

        return ownerCharacterAnimatorScript.IsSameSkeleton(prefabAvatar);
    }

    private List<GameObject> GetCells(ItemStoreDesc storeDesc)
    {
        List<GameObject> retCells = new List<GameObject>();

        foreach (KeyValuePair<EquipType, GameObject> cell in _equipCellUIs)
        {
            if ((cell.Key & storeDesc._info._equipType) != EquipType.None)
            {
                retCells.Add(cell.Value);
            }
        }

        return retCells;
    }
}
