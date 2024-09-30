using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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

public class InventoryBoard : MonoBehaviour
{

    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;
    [SerializeField] private GameObject _cellPrefab = null;
    [SerializeField] private GameObject _itemUIPrefab = null;

    private RectTransform _myRectTransform = null;

    /*-------------
    ��� �ٲ� ������
    -------------*/
    private int _itemCount = 0; //ID�� ���� ���ϰ�
    private int _blank = 0;
    private bool[,] _blankDispaly;
    private List<GameObject> _cells = new List<GameObject>();
    
    private Dictionary<int/*Ű*/, Dictionary<int/*����� ĭ*/, ItemStoreDesc>>      _items = new Dictionary<int, Dictionary<int, ItemStoreDesc>>();  //�������� �ִ��� ������ Ȯ�ο�
    private Dictionary<int/*����� ĭ*/, GameObject>                                _itemUIs = new Dictionary<int, GameObject>();                   //������ ����ִ� ������ ������ ���


    public void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            //if (this == null) // ������Ʈ�� ��ȿ���� Ȯ��
            //{
            //    return;
            //}

            //_myRectTransform.sizeDelta = new Vector2(_cols * 20, _rows * 20); // n�� ���� ũ�� ����
        };
    }


    private void Awake()
    {
        _myRectTransform = transform as RectTransform;

        if (_cellPrefab == null)
        {
            Debug.Log("�� ������ �Ҵ����");
            return;
        }
        
        _myRectTransform.sizeDelta = new Vector2(20 * _cols, 20 * _rows);

        InventoryCellDesc cellDesc = new InventoryCellDesc();
        cellDesc._owner = this;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                GameObject cellObject = Instantiate(_cellPrefab, transform);

                RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                Vector2 mySize = _myRectTransform.rect.size;
                Vector2 cellPosition = new Vector2((-mySize.x / 2) + (10) + (20 * j), (mySize.y / 2) - (10) - (20 * i));
                cellRectTransform.anchoredPosition = cellPosition;

                InventoryCell cellComponent = cellObject.GetComponent<InventoryCell>();
                Debug.Assert(cellComponent != null, "cellComponent�� ���� �� ����");
                cellComponent.Initialize(ref cellDesc);
                
                cellObject.SetActive(false);
                _cells.Add(cellObject);
            }
        }

        _blank = _cols * _rows;
        _blankDispaly = new bool[_rows, _cols];

        /*-------------------
        �׽�Ʈ�� �̸� 3ĭ ����ä���
        -------------------*/
        //_blankDispaly[0,0] = true;
        //_blankDispaly[0,1] = true;
        //_blankDispaly[1, 0] = true;
    }


    private void OnEnable()
    {
        foreach (var cell in _cells)
        {
            cell.SetActive(true);
        }
    }


    void Start()
    {
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U) == true)
        {
            ItemInfo testItemInfo = new ItemInfo();

            testItemInfo._sprite = null;
            testItemInfo._isStackAble = true;
            testItemInfo._itemKey = 1;
            testItemInfo._sizeX = 1;
            testItemInfo._sizeY = 1;

            AddItem(testItemInfo);
        }

        if (Input.GetKeyDown(KeyCode.G) == true)
        {
            ItemInfo testItemInfo = new ItemInfo();

            testItemInfo._sprite = null;
            testItemInfo._isStackAble = true;
            testItemInfo._itemKey = 2;
            testItemInfo._sizeX = 1;
            testItemInfo._sizeY = 3;

            AddItem(testItemInfo);
        }

        if (Input.GetKeyDown(KeyCode.P) == true)
        {
            ItemInfo testItemInfo = new ItemInfo();

            testItemInfo._sprite = null;
            testItemInfo._isStackAble = true;
            testItemInfo._itemKey = 3;
            testItemInfo._sizeX = 2;
            testItemInfo._sizeY = 3;

            AddItem(testItemInfo);
        }

        if (Input.GetKeyDown(KeyCode.N) == true)
        {
            ItemInfo testItemInfo = new ItemInfo();

            testItemInfo._sprite = null;
            testItemInfo._isStackAble = true;
            testItemInfo._itemKey = 4;
            testItemInfo._sizeX = 2;
            testItemInfo._sizeY = 2;

            AddItem(testItemInfo);
        }

        if (Input.GetKeyDown(KeyCode.M) == true)
        {
            ItemInfo testItemInfo = new ItemInfo();

            testItemInfo._sprite = null;
            testItemInfo._isStackAble = true;
            testItemInfo._itemKey = 5;
            testItemInfo._sizeX = 1;
            testItemInfo._sizeY = 2;

            AddItem(testItemInfo);
        }
    }

    public bool CheckItemDragDrop(InventoryTransitionDesc transitionDesc, ref int startX, ref int startY)
    {
        if ((transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY) > _blank) { return false; } //���ʿ� �� ������ ����

        Vector2 currPosition = Input.mousePosition;
        Vector2 boardSize = new Vector2(_myRectTransform.rect.width, _myRectTransform.rect.height);
        Vector2 boardStartPosition = new Vector2(_myRectTransform.position.x + (-boardSize.x / 2), _myRectTransform.position.y + (boardSize.y / 2));
        Vector2 delta = currPosition - boardStartPosition;
        int IndexX = (int)(delta.x / 20);
        int IndexY = (int)(-delta.y / 20);


        int itemSizeX = transitionDesc._itemInfo._sizeX;
        int itemSizeY = transitionDesc._itemInfo._sizeY;
        {
            //���콺�� ���� ���¿��� ȸ���� �����Դϱ�?
            //itemSizeX, itemSizeY ����
        }


        //�� ĭ�� �������� ���� �˻� �Լ�
        if (IndexX + itemSizeX > _cols ||
            IndexY + itemSizeY > _rows)
        {
            return false;
        }

        for (int i = 0; i < itemSizeY; i++)
        {
            for (int j = 0; j < itemSizeX; j++)
            {
                if (_blankDispaly[i + IndexY, j + IndexX] == true)
                {
                    return false;
                }
            }
        }

        startX = IndexX;
        startY = IndexY;

        return true;
    }

    public void AddItemDragDrop(InventoryTransitionDesc transitionDesc)
    {
        int startX = -1;
        int startY = -1;
        
        bool isRotated = false;

        if (CheckItemDragDrop(transitionDesc, ref startX, ref startY) == false)
        {
            return; //�ش� ���콺 ���������δ� �������� ���� �� ����.
        }

        {
            //�������� �߰��ϴ� �Լ� //�巡�� ����� ������ �⺻������ ���� �ʴ´�.
            GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);
            {
                if (itemUI == null)
                {
                    Debug.Assert(itemUI != null, "itemUI ���� ����");
                }

                RectTransform itemUIRectTransform = itemUI.GetComponent<RectTransform>();
                //�������
                itemUIRectTransform.sizeDelta = new Vector2(transitionDesc._itemInfo._sizeX * 20, transitionDesc._itemInfo._sizeY * 20);

                //��ġ����
                Vector2 cellIndexToMyPosition = new Vector2(-_myRectTransform.rect.size.x / 2 + 10, _myRectTransform.rect.size.y / 2 - 10);
                cellIndexToMyPosition.x += (startX * 20);
                cellIndexToMyPosition.y -= (startY * 20);

                Vector2 itemUISize = new Vector2(transitionDesc._itemInfo._sizeX * 20, transitionDesc._itemInfo._sizeY * 20);
                Vector2 itemUISizeDelta = new Vector2(itemUISize.x / 2 - 10, -itemUISize.y / 2 + 10);

                if (isRotated == true)
                {
                    itemUIRectTransform.Rotate(new Vector3(0.0f, 0.0f, -1.0f), 90);

                    float temp = itemUISizeDelta.x;
                    itemUISizeDelta.x = -itemUISizeDelta.y;
                    itemUISizeDelta.y = temp;
                }

                itemUIRectTransform.anchoredPosition = cellIndexToMyPosition + itemUISizeDelta;

                if (transitionDesc._itemInfo._sprite != null)
                {
                    itemUI.GetComponent<Image>().sprite = transitionDesc._itemInfo._sprite;
                }
            }

            //���� �˻� �� ���� ã������ ��������
            {
                _blank -= transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY;
            }

            //���ڰ���
            int rows = (isRotated == true) ? transitionDesc._itemInfo._sizeX : transitionDesc._itemInfo._sizeY;
            int cols = (isRotated == true) ? transitionDesc._itemInfo._sizeY : transitionDesc._itemInfo._sizeX;
            int targetX_modified = (isRotated == true) ? startX - 1 : startY;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    _blankDispaly[startY + y, targetX_modified + x] = true;
                }
            }

            //Ž���� ����ü ����
            int inventoryIndex = _cols * startY + targetX_modified;

            if (_items.ContainsKey(transitionDesc._itemInfo._itemKey) == false) //�߰������� ����.
            {
                _items.Add(transitionDesc._itemInfo._itemKey, new Dictionary<int, ItemStoreDesc>());
            }

            Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey];

            //���� �ϳ��� ���� 1. ����ī��Ʈ�� �Ѿ ���Ӱ� �־��ִ���, 2. �������� ���� �ϳ��� ���� ���ʿ�����

            ItemStoreDesc storeDesc = new ItemStoreDesc();
            storeDesc._count = 0;
            storeDesc._storedIndex = inventoryIndex;
            storeDesc._isRotated = isRotated;
            storeDesc._owner = this;
            storeDesc._info = transitionDesc._itemInfo;
            itemKeyCategory.Add(inventoryIndex, storeDesc);

            ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();
            if (itemBaseComponent != null)
            {
                itemBaseComponent.Initialize(this, storeDesc);
            }

            _itemUIs.Add(inventoryIndex, itemUI);
        }
    }


    public void AddItem(ItemInfo info, int itemCount = 1)
    {
        if ((info._sizeX * info._sizeY) > _blank) { return; } //���ʿ� �� ������ ����

        {
            //������ ���ð����� �������� �ִ°�� = ���� �ְ� �Լ�����
            bool jobFinished = false;

            if (_items.ContainsKey(info._itemKey) == true &&
                _items[info._itemKey].Count > 0 &&
                info._isStackAble == true)
            {

                foreach (KeyValuePair<int, ItemStoreDesc>? item in _items[info._itemKey])
                {
                    item.Value.Value.PlusItem();
                    jobFinished = true;
                    break;
                }

                if (jobFinished == true)
                {
                    return; 
                    //���ð��� ���� �ȳѾ �� �־��ٸ� �Լ��� �����ϰ�
                    //�װ� �ƴ϶��, ���а��� üũ�ؼ� ������ �ֱ� (�ؿ����� ����������)
                }
            }
        }


        int targetX = -1;
        int targetY = -1;
        bool isRotated = false;

        bool isPushAble = CheckInventorySpace_MustOpt(ref info, ref targetX, ref targetY, ref isRotated);

        if (isPushAble == false) { return; }

        //������ �ִ�.

        //������Ʈ �غ�
        GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);
        {
            if (itemUI == null)
            {
                Debug.Assert(itemUI != null, "itemUI ���� ����");
            }

            RectTransform itemUIRectTransform = itemUI.GetComponent<RectTransform>();
            //�������
            itemUIRectTransform.sizeDelta = new Vector2(info._sizeX * 20, info._sizeY * 20);

            //��ġ����
            Vector2 cellIndexToMyPosition = new Vector2(-_myRectTransform.rect.size.x/2 + 10, _myRectTransform.rect.size.y/2 - 10);
            cellIndexToMyPosition.x += (targetX * 20);
            cellIndexToMyPosition.y -= (targetY * 20);

            Vector2 itemUISize = new Vector2(info._sizeX * 20, info._sizeY * 20);
            Vector2 itemUISizeDelta = new Vector2(itemUISize.x / 2 - 10, -itemUISize.y / 2 + 10);

            if (isRotated == true)
            {
                itemUIRectTransform.Rotate(new Vector3(0.0f,0.0f,-1.0f), 90);

                float temp = itemUISizeDelta.x;
                itemUISizeDelta.x = -itemUISizeDelta.y;
                itemUISizeDelta.y = temp;
            }

            itemUIRectTransform.anchoredPosition = cellIndexToMyPosition + itemUISizeDelta;

            if (info._sprite != null)
            {
                itemUI.GetComponent<Image>().sprite = info._sprite;
            }
        }

        //���� �˻� �� ���� ã������ ��������
        {
            _blank -= info._sizeX * info._sizeY; 
        }

        //���ڰ���
        int rows = (isRotated == true) ? info._sizeX : info._sizeY;
        int cols = (isRotated == true) ? info._sizeY : info._sizeX;
        int targetX_modified = (isRotated == true) ? targetX - 1 : targetX;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                _blankDispaly[targetY + y, targetX_modified + x] = true;
            }
        }

        //Ž���� ����ü ����
        int inventoryIndex = _cols * targetY + targetX_modified;
        
        if (_items.ContainsKey(info._itemKey) == false) //�߰������� ����.
        {
            _items.Add(info._itemKey, new Dictionary<int, ItemStoreDesc>());
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[info._itemKey];

        //���� �ϳ��� ���� 1. ����ī��Ʈ�� �Ѿ ���Ӱ� �־��ִ���, 2. �������� ���� �ϳ��� ���� ���ʿ�����

        ItemStoreDesc storeDesc = new ItemStoreDesc();
        storeDesc._count = itemCount;
        storeDesc._storedIndex = inventoryIndex;
        storeDesc._isRotated = isRotated;
        storeDesc._owner = this;
        storeDesc._info = info;
        itemKeyCategory.Add(inventoryIndex, storeDesc);

        ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();
        if (itemBaseComponent != null)
        {
            itemBaseComponent.Initialize(this, storeDesc);
        }

        _itemUIs.Add(inventoryIndex, itemUI);
    }


    public void DeleteItemUseForDragItem(InventoryTransitionDesc transitionDesc)
    {
        //�������� �巡�� ��� ������ �Ѳ����� �ű�� �Լ� = ���δ� ���ٰ��̴�.

        if (_items.ContainsKey(transitionDesc._itemInfo._itemKey) == false)
        {
            Debug.Assert(false, "���� �������� ������ϰ��ִ�");
            return;
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey]; //��� �ϳ��� ����־���� �Ѵ�

        if (itemKeyCategory.ContainsKey(transitionDesc._fromIndex) == false)
        {
            Debug.Assert(false, "���� �������� ������ϰ��ִ�. �̹� �����Ƴ�?");
            return;
        }

        //ĭ �� ����
        {
            _blank += transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY;
        }

        //���� ����
        {
            bool isRotated = itemKeyCategory[transitionDesc._fromIndex]._isRotated;

            int rows = (isRotated == true) ? transitionDesc._itemInfo._sizeX : transitionDesc._itemInfo._sizeY;
            int cols = (isRotated == true) ? transitionDesc._itemInfo._sizeY : transitionDesc._itemInfo._sizeX;

            int targetRow = (int)(transitionDesc._fromIndex / _rows);
            int targetCol = transitionDesc._fromIndex - (targetRow * _cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _blankDispaly[targetRow + i, targetCol + j] = false;
                }
            }
        }

        RemoveItemUsingCellIndex(transitionDesc);
    }


    private void RemoveItemUsingCellIndex(InventoryTransitionDesc transitionDesc)
    {
        Destroy(_itemUIs[transitionDesc._fromIndex]);
        _itemUIs.Remove(transitionDesc._fromIndex);
        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey]; //��� �ϳ��� ����־���� �Ѵ�
        itemKeyCategory.Remove(transitionDesc._fromIndex);
    }








    public bool CheckInventorySpace_MustOpt(ref ItemInfo itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        //|TODO| = �ʹ� BruteForce�� ����ȭ�� �ʿ��ϴ�

        bool isFind = true;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                int cellIndex = (i * _cols) + j; //����׿�

                if (_blankDispaly[i, j] == true)
                {
                    continue;
                }

                {//���� ��ü�� �˻�
                    if (i + itemInfo._sizeY <= _rows && j + itemInfo._sizeX <= _cols)
                    {
                        for (int y = 0; y < itemInfo._sizeY; y++)
                        {
                            for (int x = 0; x < itemInfo._sizeX; x++)
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

                    if (i + itemInfo._sizeX <= _rows && j + itemInfo._sizeY <= _cols)
                    {
                        for (int y = 0; y < itemInfo._sizeX; y++)
                        {
                            for (int x = 0; x < itemInfo._sizeY; x++)
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
                            targetX = j + (itemInfo._sizeX - 1);
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
}
