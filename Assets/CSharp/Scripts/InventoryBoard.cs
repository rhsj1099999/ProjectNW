using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ItemUI;


public class InventoryBoard : BoardUIBaseScript
{
    private RectTransform _myRectTransform = null;

    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;

    [SerializeField] private GameObject _cellPrefab = null;
    [SerializeField] private GameObject _itemUIPrefab = null;

    /*-------------
    ��� �ٲ� ������
    -------------*/

    private int _blank = 0;
    private bool[,] _blankDispaly;
    private List<GameObject> _cells = new List<GameObject>();

    //�������� �ִ��� ������ Ȯ�ο�
    private Dictionary<int/*Ű*/, SortedDictionary<int/*����� ĭ*/, ItemStoreDescBase>> _items = new Dictionary<int, SortedDictionary<int, ItemStoreDescBase>>();
    public Dictionary<int, SortedDictionary<int, ItemStoreDescBase>> _Items => _items;

    //������ ���� UI �����
    private Dictionary<ItemStoreDescBase/*��������*/, GameObject/*���ۿ� UI*/> _itemUIs = new Dictionary<ItemStoreDescBase, GameObject>();


    public void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            RectTransform inventoryCellTransform = (RectTransform)_cellPrefab.transform;

            if (this == null) // ������Ʈ�� ��ȿ���� Ȯ��
            {
                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                return;
            }

            rectTransform.sizeDelta = new Vector2(_cols * inventoryCellTransform.rect.width, _rows * inventoryCellTransform.rect.height); // n�� ���� ũ�� ����
        };
    }

    public override void Init(UIComponent owner)
    {
        _owner = owner;

        _myRectTransform = transform as RectTransform;

        if (_cellPrefab == null)
        {
            Debug.Log("�� ������ �Ҵ����");
            return;
        }

        RectTransform inventoryCellTransform = (RectTransform)_cellPrefab.transform;
        float cellWidth = inventoryCellTransform.rect.width;
        float cellHeight = inventoryCellTransform.rect.height;

        _myRectTransform.sizeDelta = new Vector2(cellWidth * _cols, cellHeight * _rows);

        BoardCellDesc cellDesc = new BoardCellDesc();
        cellDesc._owner = this;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                GameObject cellObject = Instantiate(_cellPrefab, transform);

                RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                Vector2 mySize = _myRectTransform.rect.size;
                Vector2 cellPosition = new Vector2((-mySize.x / 2) + (cellWidth/2.0f) + (cellWidth * j), (mySize.y / 2) - (cellHeight/2.0f) - (cellHeight * i));
                cellRectTransform.anchoredPosition = cellPosition;

                InventoryCell cellComponent = cellObject.GetComponent<InventoryCell>();
                Debug.Assert(cellComponent != null, "cellComponent�� ���� �� ����");
                cellComponent.Initialize(cellDesc);

                int index = (i * _cols) + j;

                cellComponent.SetCellIndex(index);
                _cells.Add(cellObject);
            }
        }

        _blank = _cols * _rows;
        _blankDispaly = new bool[_rows, _cols];
    }

    void Update()
    {
        TestCode();

        DebugCells();
    }

    public override List<GameObject> GetItemUIs(ItemStoreDescBase storeDesc)
    {
        List<GameObject> ret = new List<GameObject>();
        ret.Add(_itemUIs[storeDesc]);
        return ret;
    }


    public GameObject getCell(int index)
    {
        if (index >= _cells.Count)
        {
            Debug.Assert(false, "getCell���� �ε����� �����");
            return null;
        }
        return _cells[index];
    }





    public GameObject getItem(ItemStoreDescBase storeDesc)
    {
        return _itemUIs[storeDesc];
    }



    public override bool CheckItemDragDrop(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation, BoardUICellBase caller)
    {
        Vector2 currPosition = Input.mousePosition;
        Vector2 boardSize = new Vector2(_myRectTransform.rect.width, _myRectTransform.rect.height);
        Vector2 boardStartPosition = new Vector2(_myRectTransform.position.x + (-boardSize.x / 2), _myRectTransform.position.y + (boardSize.y / 2));
        Vector2 delta = currPosition - boardStartPosition;

        InventoryCell inventoryCell = (InventoryCell)caller;


        int IndexX = inventoryCell._CellIndex % _cols;
        int IndexY = inventoryCell._CellIndex / _cols;

        int itemSizeX = (grabRotation == false) ? storedDesc._itemAsset._SizeX : storedDesc._itemAsset._SizeY;
        int itemSizeY = (grabRotation == false) ? storedDesc._itemAsset._SizeY : storedDesc._itemAsset._SizeX;
        
        if (IndexX + itemSizeX > _cols || IndexY + itemSizeY > _rows)
        {
            return false;
        }


        HashSet<int> sameItemIndex = new HashSet<int>();


        if (_itemUIs.ContainsKey(storedDesc) == true )
        {
            /*----------------------------------------------------
            |NOTI| �� �κ��丮���� �� �κ��丮�� ������ ����Դϴ�
            HashSet�� ������ ĭ���� �����մϴ�
            ----------------------------------------------------*/

            int existingSizeX = (storedDesc._isRotated == false) ? storedDesc._itemAsset._SizeX : storedDesc._itemAsset._SizeY;
            int existingSizeY = (storedDesc._isRotated == false) ? storedDesc._itemAsset._SizeY : storedDesc._itemAsset._SizeX;

            int existingIndexY = storedDesc._storedIndex / _cols;
            int existingIndexX = storedDesc._storedIndex - (existingIndexY * _cols);

            for (int i = 0; i < existingSizeY; i++)
            {
                for (int j = 0; j < existingSizeX; j++)
                {
                    int skipIndex = ((i + existingIndexY) * _cols) + (j + existingIndexX);
                    sameItemIndex.Add(skipIndex);
                }
            }
        }

        for (int i = 0; i < itemSizeY; i++)
        {
            for (int j = 0; j < itemSizeX; j++)
            {
                if (_blankDispaly[i + IndexY, j + IndexX] == true)
                {
                    if (sameItemIndex.Count > 0)
                    {
                        int index = ((i + IndexY) * _cols) + (j + IndexX);
                        if (sameItemIndex.Contains(index) == true)
                        {
                            continue;
                        }
                    }
                    return false;
                }
            }
        }

        startX = IndexX;
        startY = IndexY;

        return true;
    }


    //������ ����� ItemUI�� ����� �Լ�...
    private GameObject CreateInventoryItem(ItemAsset info, int targetX, int targetY, int storedIndex, int count, bool isAdditionalRotated)
    {
        GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);

        RectTransform inventoryCellTransform = (RectTransform)_cellPrefab.transform;
        float cellWidth = inventoryCellTransform.rect.width;
        float cellHeight = inventoryCellTransform.rect.height;

        RectTransform itemUIRectTransform = itemUI.transform as RectTransform;
        itemUIRectTransform.localPosition = Vector3.zero;
        itemUIRectTransform.localRotation = Quaternion.identity;

        //�������
        itemUIRectTransform.sizeDelta = new Vector2(info._SizeX * cellHeight, info._SizeY * cellHeight);

        //��ġ����
        Vector2 cellIndexToMyPosition = new Vector2(-_myRectTransform.rect.size.x / 2 + (cellWidth/2.0f), _myRectTransform.rect.size.y / 2 - (cellHeight/2.0f));
        cellIndexToMyPosition.x += (targetX * cellWidth);
        cellIndexToMyPosition.y -= (targetY * cellHeight);

        Vector2 itemUISize = new Vector2(info._SizeX * cellWidth, info._SizeY * cellHeight);
        Vector2 itemUISizeDelta = new Vector2(itemUISize.x / 2, -itemUISize.y / 2);
        Vector3 itemOffset = new Vector3(-cellWidth / 2.0f, cellHeight/2.0f, 0);

        itemUIRectTransform.anchoredPosition = cellIndexToMyPosition + itemUISizeDelta + new Vector2(itemOffset.x, itemOffset.y);

        if (isAdditionalRotated == true)
        {
            int index = (targetY * _cols) + targetX;
            RectTransform cellRectTransform = _cells[index].transform as RectTransform;
            itemUIRectTransform.RotateAround(cellRectTransform.position, cellRectTransform.forward, 90.0f);
            itemUIRectTransform.anchoredPosition -= new Vector2(0.0f, (info._SizeX - 1) * cellWidth);
        }

        return itemUI;
    }




    /*----------------------------------------------------
    |NOTI| �κ��丮 ���� -> �κ��丮 ���� �� ���
    �����ϰ� ������, �ִ¼��� �� �Լ��� ȣ��ƴ�.
    ----------------------------------------------------*/
    public override void AddItemUsingForcedIndex(ItemStoreDescBase storedDesc, int targetX, int targetY, BoardUICellBase caller)
    {
        int inventoryIndex = _cols * targetY + targetX;
        storedDesc._storedIndex = inventoryIndex;
        storedDesc._owner = this;
        UpdateBlank(true, storedDesc);

        GameObject itemUI = CreateInventoryItem(storedDesc._itemAsset, targetX, targetY, inventoryIndex, storedDesc._Count, storedDesc._isRotated);
        itemUI.GetComponent<ItemUI>().Initialize(storedDesc);

        SortedDictionary<int, ItemStoreDescBase> sameKeyItems = null;
        _items.TryGetValue(storedDesc._itemAsset._ItemKey, out sameKeyItems);
        if (sameKeyItems == null) 
        {
            //�ش� Key�� Item�� �����߰� �ƴ�.
            _items.Add(storedDesc._itemAsset._ItemKey, new SortedDictionary<int, ItemStoreDescBase>());
        }

        sameKeyItems = _items[storedDesc._itemAsset._ItemKey];

        if (sameKeyItems.ContainsKey(storedDesc._storedIndex) == true)
        {
            Debug.Assert(false, "�ڸ��� ��ġ���� �ϰ��ִ�");
            Debug.Break();
        }

        sameKeyItems.Add(inventoryIndex, storedDesc);


        if (_itemUIs.ContainsKey(storedDesc) == true)
        {
            Debug.Assert(false, "�ش� ���������� �̹� UI�� �����ƴ�");
            Debug.Break();
        }

        _itemUIs.Add(storedDesc, itemUI);
    }











    public bool CheckItemStackAble(ItemAsset itemInfo, out ItemStoreDescBase storeDescTarget, int itemCount)
    {
        storeDescTarget = null;

        int itemMaxStack = itemInfo._MaxStack;

        if (itemMaxStack <= 1)
        {
            //�� �������� �������� �����
            return false;
        }

        SortedDictionary<int, ItemStoreDescBase> currSameKeyItems = null;
        _items.TryGetValue(itemInfo._ItemKey, out currSameKeyItems);

        if (currSameKeyItems == null) 
        {
            //�ִ��� �� �������� �κ��丮�� �����
            return false;
        }

        foreach (KeyValuePair<int, ItemStoreDescBase> indexStoreDescPair in currSameKeyItems)
        {
            ItemStoreDescBase storeDesc = indexStoreDescPair.Value;

            int currItemCount = storeDesc._Count;

            if (currItemCount + itemCount <= itemMaxStack) 
            {
                storeDescTarget = storeDesc;
                return true;
            }
        }

        return false;
    }



    public void AddItemAutomatic(ItemAsset info, int itemCount)
    {
        if ((info._SizeX * info._SizeY) > _blank) { return; } //���ʿ� �� ������ ����

        //������ ���ð����� �������� �ִ°�� = ���� �ְ� �Լ�����
        {
            ItemStoreDescBase targetStoreDesc = null;
            if (CheckItemStackAble(info, out targetStoreDesc, itemCount) == true)
            {
                targetStoreDesc.PlusItemCount(itemCount);
                return;
            }
        }


        int targetX = -1;
        int targetY = -1;
        bool isRotated = false;

        //������ ������ ����.
        if (CheckInventorySpace_MustOpt(info, ref targetX, ref targetY, ref isRotated) == false)
        {
            return;
        }

        //�κ��丮 ���� X,Y �ε���, ȸ������ �����ƴ�.

        int inventoryIndex = _cols * targetY + targetX;

        //������Ʈ �غ�
        GameObject itemUI = CreateInventoryItem(info, targetX, targetY, inventoryIndex, itemCount, isRotated);

        //���� �˻� �� ���� ã������ ��������
        _blank -= info._SizeX * info._SizeY;

        //���ڰ���
        int rows = (isRotated == true) ? info._SizeX : info._SizeY;
        int cols = (isRotated == true) ? info._SizeY : info._SizeX;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                _blankDispaly[targetY + y, targetX + x] = true;
            }
        }

        //Ž���� ����ü ����
        
        if (_items.ContainsKey(info._ItemKey) == false) //�߰������� ����.
        {
            _items.Add(info._ItemKey, new SortedDictionary<int, ItemStoreDescBase>());
        }

        SortedDictionary<int, ItemStoreDescBase> itemKeyCategory = _items[info._ItemKey];

        ItemStoreDescBase storeDesc = null;
        {
            if (info._ItemType == ItemAsset.ItemType.Magazine)
            {
                storeDesc = new ItemStoreDesc_Magazine(info, itemCount, inventoryIndex, isRotated, this, null);
            }
            else if (info._ItemType == ItemAsset.ItemType.Equip)
            {
                if (info._EquipType == ItemAsset.EquipType.Weapon)
                {
                    ItemAsset_Weapon weaponInfo = info as ItemAsset_Weapon;

                    if (weaponInfo._WeaponType >= ItemAsset_Weapon.WeaponType.SmallGun && weaponInfo._WeaponType <= ItemAsset_Weapon.WeaponType.LargeGun) 
                    {
                        //Equip -> Weapon �ε�
                        storeDesc = new ItemStoreDesc_Weapon_Gun(info, itemCount, inventoryIndex, isRotated, this);
                    }
                    else
                    {
                        //Equip -> Weapon -> Gun�ε�
                        storeDesc = new ItemStoreDesc_Weapon(info, itemCount, inventoryIndex, isRotated, this);
                    }
                }
                else
                {
                    //Equip -> Armor �ε�
                    storeDesc = new ItemStoreDescBase(info, itemCount, inventoryIndex, isRotated, this);
                }
            }
            else
            {
                storeDesc = new ItemStoreDescBase(info, itemCount, inventoryIndex, isRotated, this);
            }
        }
        

        itemKeyCategory.Add(inventoryIndex, storeDesc);

        ItemUI itemBaseComponent = itemUI.GetComponent<ItemUI>();

        if (itemBaseComponent != null)
        {
            itemBaseComponent.Initialize(storeDesc);
        }

        _itemUIs.Add(storeDesc, itemUI);
    }


    private void UpdateBlank(bool target, ItemStoreDescBase storedDesc)
    {
        //ĭ �� ����
        {
            _blank += storedDesc._itemAsset._SizeX * storedDesc._itemAsset._SizeY;
        }

        //���� ����
        {
            bool isRotated = storedDesc._isRotated;

            int rows = (isRotated == true) ? storedDesc._itemAsset._SizeX : storedDesc._itemAsset._SizeY;
            int cols = (isRotated == true) ? storedDesc._itemAsset._SizeY : storedDesc._itemAsset._SizeX;

            int targetRow = (int)(storedDesc._storedIndex / _cols);
            int targetCol = storedDesc._storedIndex - (targetRow * _cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _blankDispaly[targetRow + i, targetCol + j] = target;
                }
            }
        }
    }



    public override void DeleteOnMe(ItemStoreDescBase storedDesc)
    {
        //���ڰ� ���ŵȴ�.
        UpdateBlank(false, storedDesc);

        _itemUIs.Remove(storedDesc);

        if (_items.ContainsKey(storedDesc._itemAsset._ItemKey) == false)
        {
            Debug.Assert(false, "�߰������� ���� �������� ������Ѵ�");
            return;
        }

        SortedDictionary<int, ItemStoreDescBase> sameItemsByItemKey = _items[storedDesc._itemAsset._ItemKey];

        if (sameItemsByItemKey.ContainsKey(storedDesc._storedIndex) == false)
        {
            Debug.Assert(false, "�ش� �ڸ����� �� �������� ���µ� ����� �Ѵ�");
            return;
        }
        
        sameItemsByItemKey.Remove(storedDesc._storedIndex);
    }


    public bool CheckInventorySpace_MustOpt(ItemAsset itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        /*------------------------------------------
        |TODO| = �ʹ� BruteForce�� ����ȭ�� �ʿ��ϴ�
        ------------------------------------------*/
        bool isFind = true;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                isFind = true;
                int cellIndex = (i * _cols) + j; //����׿�

                if (_blankDispaly[i, j] == true)
                {
                    continue;
                }

                {//���� ��ü�� �˻�
                    if (i + itemInfo._SizeY <= _rows && j + itemInfo._SizeX <= _cols)
                    {
                        for (int y = 0; y < itemInfo._SizeY; y++)
                        {
                            for (int x = 0; x < itemInfo._SizeX; x++)
                            {
                                if (_blankDispaly[i + y, j + x] == true)
                                {
                                    isFind = false;
                                    break;
                                }
                            }

                            if (isFind == false)
                            {
                                break;
                            }
                        }

                        if (isFind == true)
                        {
                            //���η� �� �������� �˻��� ����
                            targetX = j;
                            targetY = i;
                            isRotated = false;
                            return true;
                        }
                    }
                }


                {//90�� �ð�������� ������ �˻�
                    isFind = true;

                    if (i + itemInfo._SizeX <= _rows && j + itemInfo._SizeY <= _cols)
                    {
                        for (int y = 0; y < itemInfo._SizeX; y++)
                        {
                            for (int x = 0; x < itemInfo._SizeY; x++)
                            {
                                if (_blankDispaly[i + y, j + x] == true)
                                {
                                    isFind = false;
                                    break;
                                }
                            }

                            if (isFind == false)
                            {
                                break;
                            }
                        }

                        if (isFind == true)
                        {
                            targetX = j;
                            targetY = i;
                            isRotated = true;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void TestCode()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("PaladinArmor"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("VanguardArmor"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("NoelArmor"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("BeidouArmor"), 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SoldierVest"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SoldierGlove"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SoldierHat"), 1);
        }


        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SimpleSword"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("BeidouSword"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("Hammer"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SimpleShield"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("HeizoKnuckle"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SimpleKnuckle"), 1);
        }
        

        if (Input.GetKeyDown(KeyCode.Alpha3) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("M16"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("AK47"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("D_Eagle"), 1);
        }



        if (Input.GetKeyDown(KeyCode.Alpha4) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("źâ_5ź_10��"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("źâ_7ź_45��"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("źâ_9ź_15��"), 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("�Ѿ�_5ź_����0"), 7);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("�Ѿ�_7ź_����0"), 7);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("�Ѿ�_9ź_����0"), 7);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("WolfMound"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("BeidouArmor"), 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("BeidouArmor"), 1);
        }


        if (Input.GetKeyDown(KeyCode.Alpha8) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("RedPotion"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("BluePotion"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("StaminaPotion"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("SPPotion"), 1);
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo("HeistPotion_LV0"), 1);
        }
    }




    private void DebugCells()
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                int debuggingCellIndex = (i * _cols) + j;

                InventoryCell cellComponent = _cells[debuggingCellIndex].GetComponent<InventoryCell>();

                if (_blankDispaly[i, j] == true)
                {
                    cellComponent.TurnOn();
                }
                if (_blankDispaly[i, j] == false)
                {
                    cellComponent.TurnOff();
                }
            }
        }
    }


}



//public void MoveItemSameInventory(ItemStoreDescBase storedDesc, int targetX, int targetY)
//{
//    //���� �˻� �� ���� ã������ ��������
//    {
//        UpdateBlank(false, storedDesc);
//        int inventoryIndex = _cols * targetY + targetX;
//        storedDesc._storedIndex = inventoryIndex;
//        UpdateBlank(true, storedDesc);
//    }
//}


//public void MoveItemDiffrentInventory(ItemStoreDescBase storedDesc, int targetX, int targetY)
//{
//    int inventoryIndex = _cols * targetY + targetX;
//    storedDesc._storedIndex = inventoryIndex;
//    UpdateBlank(true, storedDesc);

//    GameObject itemUI = CreateInventoryItem(storedDesc._itemAsset, targetX, targetY, inventoryIndex, storedDesc._count, storedDesc._isRotated);
//    itemUI.GetComponent<ItemUI>().Initialize(this, storedDesc);

//    _items.Add(storedDesc._itemAsset._ItemKey, new Dictionary<int, ItemStoreDescBase>());

//    Dictionary<int, ItemStoreDescBase> itemKeyCategory = _items[storedDesc._itemAsset._ItemKey];

//    itemKeyCategory.Add(inventoryIndex, storedDesc);

//    Debug.Assert(_itemUIs.ContainsKey(storedDesc) == false, "��ġ���� �ϰ��ִ�");
//    _itemUIs.Add(storedDesc, itemUI);
//}