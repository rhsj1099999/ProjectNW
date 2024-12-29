using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class InventoryBoard : MonoBehaviour, IMoveItemStore
{

    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;
    [SerializeField] private GameObject _cellPrefab = null;
    [SerializeField] private GameObject _itemUIPrefab = null;

    private RectTransform _myRectTransform = null;
    /*-------------
    계속 바뀔 변수들
    -------------*/
    private int _blank = 0;
    private bool[,] _blankDispaly;
    private List<GameObject> _cells = new List<GameObject>();
    
    private Dictionary<int/*키*/, Dictionary<int/*저장된 칸*/, ItemStoreDesc>>      _items = new Dictionary<int, Dictionary<int, ItemStoreDesc>>();  //아이템이 있는지 없는지 확인용
    private Dictionary<int/*저장된 칸*/, GameObject>                                _itemUIs = new Dictionary<int, GameObject>();                   //실제로 들어있는 아이템 렌더링 담당


    public void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this == null) // 오브젝트가 유효한지 확인
            {
                return;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                return;
            }

            rectTransform.sizeDelta = new Vector2(_cols * 20, _rows * 20); // n에 따라 크기 변경
        };
    }

    private void Awake()
    {
        _myRectTransform = transform as RectTransform;

        if (_cellPrefab == null)
        {
            Debug.Log("셀 프리팹 할당안함");
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
                Debug.Assert(cellComponent != null, "cellComponent는 널일 수 없다");
                cellComponent.Initialize(ref cellDesc);
                
                cellObject.SetActive(false);
                _cells.Add(cellObject);
            }
        }

        _blank = _cols * _rows;
        _blankDispaly = new bool[_rows, _cols];
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
        TestCode();

        DebugCells();
    }

    public GameObject getCell(int index)
    {
        if (index >= _cells.Count)
        {
            Debug.Assert(false, "getCell에서 인덱스를 벗어났다");
            return null;
        }
        return _cells[index];
    }

    public GameObject getItem(int index)
    {
        if (index >= _itemUIs.Count)
        {
            Debug.Assert(false, "getItem에서 인덱스를 벗어났다");
            return null;
        }
        return _itemUIs[index];
    }



    public bool CheckItemDragDrop(ItemStoreDesc storedDesc, ref int startX, ref int startY, ItemBase callerItem)
    {
        /*---------------------------------------------------------------------------
        |TODO| 동일 아이템이면 겹친 칸만큼 보정해줘야한다
        ---------------------------------------------------------------------------*/
        //if ((storedDesc._info._sizeX * storedDesc._info._sizeY) > _blank) { return false; } //애초에 들어갈 공간이 없다

        Vector2 currPosition = Input.mousePosition;
        Vector2 boardSize = new Vector2(_myRectTransform.rect.width, _myRectTransform.rect.height);
        Vector2 boardStartPosition = new Vector2(_myRectTransform.position.x + (-boardSize.x / 2), _myRectTransform.position.y + (boardSize.y / 2));
        Vector2 delta = currPosition - boardStartPosition;
        int IndexX = (int)(delta.x / 20);
        int IndexY = (int)(-delta.y / 20);

        int itemSizeX = (callerItem.GetRotated() == false) ? storedDesc._info._sizeX : storedDesc._info._sizeY;
        int itemSizeY = (callerItem.GetRotated() == false) ? storedDesc._info._sizeY : storedDesc._info._sizeX;
        
        if (IndexX + itemSizeX > _cols || IndexY + itemSizeY > _rows)
        {
            return false;
        }


        HashSet<int> sameItemIndex = new HashSet<int>();


        /*---------------------------------------------------------------------------
        |TODO|  비교조건 수정해라 (참일때 하는 일은 템옮기기일때 동일아이템이 움직였을때 겹친칸 continue용도
        ---------------------------------------------------------------------------*/
        if (
            callerItem != null && 
            (storedDesc._owner is InventoryBoard) && 
            (InventoryBoard)storedDesc._owner == this &&
            _itemUIs.Count > 0 &&
            _itemUIs[storedDesc._storedIndex].GetComponent<ItemBase>() == callerItem
            )
        {
            int existingSizeX = (storedDesc._isRotated == false) ? storedDesc._info._sizeX: storedDesc._info._sizeY;
            int existingSizeY = (storedDesc._isRotated == false) ? storedDesc._info._sizeY : storedDesc._info._sizeX;

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




    private GameObject CreateInventoryItem(ItemInfo info, int targetX, int targetY, bool isAdditionalRotated = false)
    {
        GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);

        if (itemUI == null)
        {
            Debug.Assert(itemUI != null, "itemUI 생성 실패");
            return null;
        }

        RectTransform itemUIRectTransform = itemUI.GetComponent<RectTransform>();
        //사이즈변경
        itemUIRectTransform.sizeDelta = new Vector2(info._sizeX * 20, info._sizeY * 20);

        //위치변경
        Vector2 cellIndexToMyPosition = new Vector2(-_myRectTransform.rect.size.x / 2 + 10, _myRectTransform.rect.size.y / 2 - 10);
        cellIndexToMyPosition.x += (targetX * 20);
        cellIndexToMyPosition.y -= (targetY * 20);

        Vector2 itemUISize = new Vector2(info._sizeX * 20, info._sizeY * 20);
        Vector2 itemUISizeDelta = new Vector2(itemUISize.x / 2, -itemUISize.y / 2);
        Vector3 itemOffset = new Vector3(-10, 10, 0);

        itemUIRectTransform.anchoredPosition = cellIndexToMyPosition + itemUISizeDelta + new Vector2(itemOffset.x, itemOffset.y);


        if (isAdditionalRotated == true)
        {
            int index = (targetY * _cols) + targetX;
            Vector2 cellPosition = new Vector2(_cells[index].transform.position.x, _cells[index].transform.position.y) + new Vector2(itemOffset.x, itemOffset.y);
            

            itemUIRectTransform.RotateAround(cellPosition, new Vector3(0.0f, 0.0f, 1.0f), 90);

            itemUIRectTransform.anchoredPosition -= new Vector2(0.0f, info._sizeX * 20);
        }


        if (info._sprite != null)
        {
            itemUI.GetComponent<Image>().sprite = info._sprite;
        }

        return itemUI;
    }

    public void AddItemUsingForcedIndex(ItemStoreDesc storedDesc, int targetX, int targetY, bool isAdditionalRotated = false)
    {
        //드래그 드랍으로 아이템을 넣을때 인덱스가 결정돼있는 상태,
        GameObject itemUI = CreateInventoryItem(storedDesc._info, targetX, targetY, isAdditionalRotated);

        //다음 검사 시 빨리 찾기위한 공간갱신
        _blank -= storedDesc._info._sizeX * storedDesc._info._sizeY;

        //격자갱신
        int rows = (isAdditionalRotated == true) ? storedDesc._info._sizeX : storedDesc._info._sizeY;
        int cols = (isAdditionalRotated == true) ? storedDesc._info._sizeY : storedDesc._info._sizeX;
        int targetX_modified = (isAdditionalRotated == true) ? targetX - 1 : targetX;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                _blankDispaly[targetY + y, targetX + x] = true;
            }
        }

        //탐색용 구조체 갱신
        int inventoryIndex = _cols * targetY + targetX;

        if (_items.ContainsKey(storedDesc._info._itemKey) == false) //추가된적이 없다.
        {
            _items.Add(storedDesc._info._itemKey, new Dictionary<int, ItemStoreDesc>());
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[storedDesc._info._itemKey];

        //둘중 하나의 경우다 1. 스택카운트가 넘어서 새롭게 넣어주던가, 2. 동일종류 템이 하나도 없고 최초였던가

        ItemStoreDesc storeDesc = new ItemStoreDesc();
        storeDesc._count = 0;
        storeDesc._storedIndex = inventoryIndex;
        storeDesc._isRotated = isAdditionalRotated;
        storeDesc._owner = this;
        storeDesc._info = storedDesc._info;
        itemKeyCategory.Add(inventoryIndex, storeDesc);

        ItemBase itemBaseComponent = itemUI.GetComponent<ItemBase>();
        if (itemBaseComponent != null)
        {
            itemBaseComponent.Initialize(this, storeDesc);
        }

        Debug.Assert(_itemUIs.ContainsKey(inventoryIndex) == false, "겹치려고 하고있다");
        _itemUIs.Add(inventoryIndex, itemUI);
    }


    public void AddItemAutomatic(ItemInfo info, int itemCount = 1)
    {
        if ((info._sizeX * info._sizeY) > _blank) { return; } //애초에 들어갈 공간이 없다

        //기존에 스택가능한 아이템이 있는경우 = 빨리 넣고 함수종료
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
            }
        }


        int targetX = -1;
        int targetY = -1;
        bool isRotated = false;

        bool isPushAble = CheckInventorySpace_MustOpt(ref info, ref targetX, ref targetY, ref isRotated);

        if (isPushAble == false) { return; }

        //여분이 있다.

        //오브젝트 준비
        GameObject itemUI = CreateInventoryItem(info, targetX, targetY, isRotated);

        //다음 검사 시 빨리 찾기위한 공간갱신
        _blank -= info._sizeX * info._sizeY;

        //격자갱신
        int rows = (isRotated == true) ? info._sizeX : info._sizeY;
        int cols = (isRotated == true) ? info._sizeY : info._sizeX;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                _blankDispaly[targetY + y, targetX + x] = true;
            }
        }

        //탐색용 구조체 갱신
        int inventoryIndex = _cols * targetY + targetX;
        
        if (_items.ContainsKey(info._itemKey) == false) //추가된적이 없다.
        {
            _items.Add(info._itemKey, new Dictionary<int, ItemStoreDesc>());
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[info._itemKey];

        //둘중 하나의 경우다 1. 스택카운트가 넘어서 새롭게 넣어주던가, 2. 동일종류 템이 하나도 없고 최초였던가
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


    public void DeleteOnMe(ItemStoreDesc storedDesc) // : IMoveItemStore
    {
        //아이템을 드래그 드롭 했을때 한꺼번에 옮기는 함수 = 전부다 없앨것이다.

        if (_items.ContainsKey(storedDesc._info._itemKey) == false)
        {
            Debug.Assert(false, "없는 아이템을 지우려하고있다");
            return;
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[storedDesc._info._itemKey]; //적어도 하나는 들어있었어야 한다

        if (itemKeyCategory.ContainsKey(storedDesc._storedIndex) == false)
        {
            Debug.Assert(false, "없는 아이템을 지우려하고있다. 이미 삭제됐나?");
            return;
        }

        //칸 수 갱신
        {
            _blank += storedDesc._info._sizeX * storedDesc._info._sizeY;
        }

        //여백 갱신
        {
            bool isRotated = itemKeyCategory[storedDesc._storedIndex]._isRotated;

            int rows = (isRotated == true) ? storedDesc._info._sizeX : storedDesc._info._sizeY;
            int cols = (isRotated == true) ? storedDesc._info._sizeY : storedDesc._info._sizeX;

            int targetRow = (int)(storedDesc._storedIndex / _cols);
            int targetCol = storedDesc._storedIndex - (targetRow * _cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    _blankDispaly[targetRow + i, targetCol + j] = false;
                }
            }
        }

        RemoveItemUsingCellIndex(storedDesc);
    }

    public void SuccessCall(GameObject uiObject)
    {

    }


    private void RemoveItemUsingCellIndex(ItemStoreDesc storedDesc)
    {
        _itemUIs.Remove(storedDesc._storedIndex);
        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[storedDesc._info._itemKey]; //적어도 하나는 들어있었어야 한다
        itemKeyCategory.Remove(storedDesc._storedIndex);
    }


    public bool CheckInventorySpace_MustOpt(ref ItemInfo itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        /*------------------------------------------
        |TODO| = 너무 BruteForce다 최적화가 필요하다
        ------------------------------------------*/
        bool isFind = true;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                isFind = true;
                int cellIndex = (i * _cols) + j; //디버그용

                if (_blankDispaly[i, j] == true)
                {
                    continue;
                }

                {//인포 자체로 검사
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
                            //가로로 긴 형상으로 검사한 형태
                            targetX = j;
                            targetY = i;
                            isRotated = false;
                            return true;
                        }
                    }
                }


                {//90도 시계방향으로 돌려서 검사
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
                            //targetX = j + (itemInfo._sizeX - 1);
                            //targetY = i;
                            //isRotated = true;
                            //return true;

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
        if (Input.GetKeyDown(KeyCode.Alpha7) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(30));
        }

        if (Input.GetKeyDown(KeyCode.Alpha8) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(31));
        }

        if (Input.GetKeyDown(KeyCode.Alpha9) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(32));
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(33));
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(34));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(35));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(36));
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
