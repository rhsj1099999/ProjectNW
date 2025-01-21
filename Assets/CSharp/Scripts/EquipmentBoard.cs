using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using static ItemAsset;
using static MyUtil;
using static UnityEditor.Progress;
using static ItemUI;

public class EquipmentBoard : BoardUIBaseScript
{
    [SerializeField] private GameObject _equipmentUIObjectPrefab = null;
    private RectTransform _myRectTransform = null;

    /*-------------
    ��� �ٲ� ������
    -------------*/
    private Dictionary<EquipType, BoardUICellBase> _equipCellLinked_Armor = new Dictionary<EquipType, BoardUICellBase>();

    private HashSet<BoardUICellBase> _currActivatedCell = new HashSet<BoardUICellBase>();
    private Dictionary<ItemStoreDescBase, HashSet<BoardUICellBase>> _currActivatedCell_ByItem = new Dictionary<ItemStoreDescBase, HashSet<BoardUICellBase>>();

    private Dictionary<ItemStoreDescBase, List<GameObject>> _currEquippedItemUIObject = new Dictionary<ItemStoreDescBase, List<GameObject>>();
    private Dictionary<ItemStoreDescBase, List<GameObject>> _currEqippedItemMeshObject = new Dictionary<ItemStoreDescBase, List<GameObject>>();

    public override void Init(UIComponent owner)
    {
        _owner = owner;

        Debug.Assert(_equipmentUIObjectPrefab != null, "������ �Ҵ� ����");

        _myRectTransform = transform as RectTransform;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "����ĭ�� �����ϴ�");

        BoardCellDesc desc = new BoardCellDesc();
        desc._owner = this;

        foreach (EquipmentCell component in components)
        {
            component.Initialize(desc);

            EquipType cellType = component.GetCellType();
            if (cellType >= EquipType.HumanHead && cellType <= EquipType.HumanBody)
            {
                _equipCellLinked_Armor.Add(cellType, component);
            }
        }
    }

    public override void DeleteOnMe(ItemStoreDescBase storeDesc)
    {
        {
            if (storeDesc._itemAsset._EquipType == EquipType.Weapon)
            {
                //���� ���� ���� �Լ�
                UnEquipItem_Weapon(storeDesc);
            }
            else if (storeDesc._itemAsset._EquipType == EquipType.UseAndComsumeableByCharacter)
            {
                //����� ���� ���� �Լ�
            }
            else
            {
                UnEquipMesh(storeDesc);
            }
        }

        UnEquipUI(storeDesc);
    }


    public override void AddItemUsingForcedIndex(ItemStoreDescBase storedDesc, int targetX, int targetY, BoardUICellBase caller) 
    {
        //���� �����ϸ� ���� �Ҹ���
        storedDesc._isRotated = false;
        storedDesc._owner = this;

        EquipUI(storedDesc, caller);

        {
            if (storedDesc._itemAsset._EquipType == EquipType.Weapon)
            {
                //�������� �Լ� ��
                EquipItem_Weapon(storedDesc, caller);
                return;
            }

            if (storedDesc._itemAsset._EquipType == EquipType.UseAndComsumeableByCharacter)
            {
                //����� ���� �Լ� ��
                return;
            }

            EquipItemMesh(storedDesc);
        }
    }



    private void EquipUI(ItemStoreDescBase storedDesc, BoardUICellBase caller)
    {
        HashSet<BoardUICellBase> cells = CalculateTargetEquipcells_Equip(storedDesc, caller);

        foreach (var cell in cells)
        {
            //UI�� �ش� Cell�� �ڽ����� �����մϴ�.
            RectTransform equipCellTransform = cell.GetComponent<RectTransform>();
            GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, equipCellTransform);
            ItemUI itemBaseComponent = equipmentUIObject.GetComponent<ItemUI>();
            itemBaseComponent.Initialize(storedDesc);

            //ũ��, ��ġ�� ���ֽܳ��ϴ�.
            RectTransform equipmentUIObjectTransform = equipmentUIObject.GetComponent<RectTransform>();
            equipmentUIObjectTransform.sizeDelta = new Vector2(equipCellTransform.rect.width, equipCellTransform.rect.height);
            equipmentUIObjectTransform.position = equipCellTransform.position;

            _currActivatedCell.Add(cell);

            HashSet<BoardUICellBase> currActivateCellSameItem = _currActivatedCell_ByItem.GetOrAdd(storedDesc);
            currActivateCellSameItem.Add(cell);

            List<GameObject> createdUIs = _currEquippedItemUIObject.GetOrAdd(storedDesc);
            createdUIs.Add(equipmentUIObject);
        }
    }

    public override bool CheckItemDragDrop(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation, BoardUICellBase caller)
    {
        startX = 0;
        startY = 0;

        if (storedDesc._itemAsset._EquipType == EquipType.UseAndComsumeableByCharacter)
        {
            Debug.Assert(false, "����� ���� �����Դϴ� �̱���!");
            Debug.Break();
            return false;
        }

        if (_currActivatedCell.Contains(caller) == true)
        {
            return false;
        }

        HashSet<BoardUICellBase> cells = CalculateTargetEquipcells_Equip(storedDesc, caller);

        foreach (var cell in cells)
        {
            //�����Ϸ��� �������߿� ��ġ�°� �ϳ��� ������ ������ ���� => �Ӹ��� �����ϰ� �ִµ� ������� �԰��ִ� �׷���..
            if (_currActivatedCell.Contains(cell) == true) {return false;}
        }

        return true;
    }

    public void EquipItem_Weapon(ItemStoreDescBase storeDesc, BoardUICellBase caller)
    {
        bool isRight = caller.gameObject.name.Contains("Right");

        int index = caller.gameObject.name.Last() - 49;

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        myUIComponent.GetUIControllingComponent().gameObject.GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, storeDesc._itemAsset as ItemAsset_Weapon);
    }

    public void UnEquipItem_Weapon(ItemStoreDescBase storeDesc)
    {
        //������ �� ���⸦ �����ϰ� �ִ� ��
        HashSet<BoardUICellBase> targetCells = null; 
        _currActivatedCell_ByItem.TryGetValue(storeDesc, out targetCells);
        if (targetCells == null)
        {
            Debug.Assert(false, "���⸦ ���������� ����??");
            Debug.Break();
        }

        if (targetCells.Count >= 2)
        {
            Debug.Assert(false, "���⸦ 2ĭ �̻� �����ߴ�???");
            Debug.Break();
        }

        BoardUICellBase targetCell = targetCells.First();

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        bool isRight = targetCell.gameObject.name.Contains("Right");

        int index = targetCell.gameObject.name.Last() - 49;

        myUIComponent.GetUIControllingComponent().gameObject.GetComponentInChildren<PlayerScript>().SetWeapon(isRight, index, null);
    }

    private void UnEquipUI(ItemStoreDescBase storeDesc)
    {
        HashSet<BoardUICellBase> targets = CalculateTargetEquipcells_UnEquip(storeDesc);
        foreach (BoardUICellBase target in targets) 
        {
            _currActivatedCell.Remove(target);
        }


        _currActivatedCell_ByItem.Remove(storeDesc);
        
        {
            List<GameObject> createdItemUISameStoreDesc = _currEquippedItemUIObject.GetOrAdd(storeDesc);
            foreach (GameObject gObj in createdItemUISameStoreDesc)
            {
                ItemUI component = gObj.GetComponent<ItemUI>();
                if (component != null)
                {
                    StartCoroutine(component.DestroyCoroutine());
                }
            }
            _currEquippedItemUIObject.Remove(storeDesc);
        }

    }

    private void UnEquipMesh(ItemStoreDescBase storeDesc)
    {
        if (IsSameSkelaton(storeDesc._itemAsset) == true)
        {
            List<GameObject> meshes = _currEqippedItemMeshObject[storeDesc];
            foreach (GameObject mesh in meshes)
            {
                Destroy(mesh);
            }
            _currEqippedItemMeshObject.Remove(storeDesc);
        }
        else
        {
            UIComponent myUIComponent = GetComponentInParent<UIComponent>();
            GameObject uiReturnOwner = myUIComponent.GetUIControllingComponent().gameObject;
            CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();
            ownerCharacterAnimatorScript.ResetCharacterModel();
        }
    }


    private void EquipItemMesh(ItemStoreDescBase storeDesc)
    {
        /*--------------------------------------------------------------
        |NOTI| �������� �и��͸��ǰ� �ٸ��� ������ ���Ű���(��Ų) ����Դϴ�.
        �׸��� �κ� ���ʵ� �ø��� �Ǹ� �ȵǴµ� �ø��� �Ǵ� ��찡 �ֽ��ϴ�.
        Bound�� ���õ� �������� ã�ƺ����մϴ�.
        --------------------------------------------------------------*/

        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        GameObject uiReturnOwner = myUIComponent.GetUIControllingComponent().gameObject;

        CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();

        GameObject originalAnimatorGameObject = ownerCharacterAnimatorScript.GetCurrActivatedModelObject();

        ItemAsset_EquipMesh equipInfo = storeDesc._itemAsset as ItemAsset_EquipMesh;

        if (IsSameSkelaton(storeDesc._itemAsset) == true)
        {
            List<GameObject> equipMeshes = equipInfo._EquipmentMeshes;

            foreach (var item in equipMeshes) //���� �ϳ���
            {
                GameObject equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = originalAnimatorGameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                skinnedMeshRenderer.updateWhenOffscreen = true;

                List<GameObject> equippedMeshSameStoreDesc = _currEqippedItemMeshObject.GetOrAdd(storeDesc);
                equippedMeshSameStoreDesc.Add(equippedMesh);
            }
        }
        else
        {
            ownerCharacterAnimatorScript.ModelChange(equipInfo._EquipmentPrefab);
        }

    }

    private bool IsSameSkelaton(ItemAsset itemAsset)
    {
        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        if (myUIComponent == null)
        {
            Debug.Assert(false, "��� UI�� �θ� UIComponent 1�� ���� �������־���մϴ�");
            Debug.Break();
        }

        CharacterAnimatorScript ownerCharacterAnimatorScript = myUIComponent.GetUIControllingComponent().gameObject.GetComponentInChildren<CharacterAnimatorScript>();

        if (ownerCharacterAnimatorScript == null) 
        {
            Debug.Assert(false, "�ٸ� �� �������� Ȯ���ϱ� ���ؼ� CharacterAnimatorScript�� �ʿ��մϴ�");
            Debug.Break();
        }

        ItemAsset_EquipMesh equipmentSubInfo = itemAsset as ItemAsset_EquipMesh;

        Animator prefabAnimator = equipmentSubInfo._EquipmentPrefab.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");

        return ownerCharacterAnimatorScript.IsSameSkeleton(equipmentSubInfo._EquipmentAvatar);
    }

    private HashSet<BoardUICellBase> CalculateTargetEquipcells_Equip(ItemStoreDescBase storeDesc, BoardUICellBase caller)
    {
        HashSet<BoardUICellBase> retCells = new HashSet<BoardUICellBase>();

        //�����Ϸ��°� Armor ��
        if ((storeDesc._itemAsset._EquipType >= EquipType.HumanHead && storeDesc._itemAsset._EquipType <= EquipType.HumanBackpack) ||
            storeDesc._itemAsset._EquipType >= EquipType.All)
        {
            foreach (KeyValuePair<EquipType, BoardUICellBase> cell in _equipCellLinked_Armor)
            {
                if ((cell.Key & storeDesc._itemAsset._EquipType) != EquipType.None)
                {
                    retCells.Add(cell.Value);
                }
            }
        }

        if (retCells.Contains(caller) == false)
        {
            retCells.Add(caller);
        }
        
        return retCells;
    }

    private HashSet<BoardUICellBase> CalculateTargetEquipcells_UnEquip(ItemStoreDescBase storeDesc)
    {
        //�� ������ ������ ������ ������ �ֽ��ϱ�?
        return _currActivatedCell_ByItem.GetOrAdd(storeDesc);
    }
}
