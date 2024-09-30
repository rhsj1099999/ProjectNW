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

    public Vector2 _position;       //저장된 위치 
    public ItemInfo _info;          //인포
    public int _count;              //개수
    public int _storedIndex;        //저장된 칸
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
    계속 바뀔 변수들
    -------------*/
    private int _itemCount = 0; //ID로 같이 쓰일것
    private int _blank = 0;
    private bool[,] _blankDispaly;
    private List<GameObject> _cells = new List<GameObject>();
    
    private Dictionary<int/*키*/, Dictionary<int/*저장된 칸*/, ItemStoreDesc>>      _items = new Dictionary<int, Dictionary<int, ItemStoreDesc>>();  //아이템이 있는지 없는지 확인용
    private Dictionary<int/*저장된 칸*/, GameObject>                                _itemUIs = new Dictionary<int, GameObject>();                   //실제로 들어있는 아이템 렌더링 담당


    public void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            //if (this == null) // 오브젝트가 유효한지 확인
            //{
            //    return;
            //}

            //_myRectTransform.sizeDelta = new Vector2(_cols * 20, _rows * 20); // n에 따라 크기 변경
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

        /*-------------------
        테스트용 미리 3칸 억지채우기
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
        if ((transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY) > _blank) { return false; } //애초에 들어갈 공간이 없다

        Vector2 currPosition = Input.mousePosition;
        Vector2 boardSize = new Vector2(_myRectTransform.rect.width, _myRectTransform.rect.height);
        Vector2 boardStartPosition = new Vector2(_myRectTransform.position.x + (-boardSize.x / 2), _myRectTransform.position.y + (boardSize.y / 2));
        Vector2 delta = currPosition - boardStartPosition;
        int IndexX = (int)(delta.x / 20);
        int IndexY = (int)(-delta.y / 20);


        int itemSizeX = transitionDesc._itemInfo._sizeX;
        int itemSizeY = transitionDesc._itemInfo._sizeY;
        {
            //마우스로 집은 상태에서 회전된 상태입니까?
            //itemSizeX, itemSizeY 스왑
        }


        //그 칸을 기준으로 여백 검사 함수
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
            return; //해당 마우스 포지션으로는 아이템을 넣을 수 없다.
        }

        {
            //아이템을 추가하는 함수 //드래그 드롭은 스택을 기본적으로 하지 않는다.
            GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);
            {
                if (itemUI == null)
                {
                    Debug.Assert(itemUI != null, "itemUI 생성 실패");
                }

                RectTransform itemUIRectTransform = itemUI.GetComponent<RectTransform>();
                //사이즈변경
                itemUIRectTransform.sizeDelta = new Vector2(transitionDesc._itemInfo._sizeX * 20, transitionDesc._itemInfo._sizeY * 20);

                //위치변경
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

            //다음 검사 시 빨리 찾기위한 공간갱신
            {
                _blank -= transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY;
            }

            //격자갱신
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

            //탐색용 구조체 갱신
            int inventoryIndex = _cols * startY + targetX_modified;

            if (_items.ContainsKey(transitionDesc._itemInfo._itemKey) == false) //추가된적이 없다.
            {
                _items.Add(transitionDesc._itemInfo._itemKey, new Dictionary<int, ItemStoreDesc>());
            }

            Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey];

            //둘중 하나의 경우다 1. 스택카운트가 넘어서 새롭게 넣어주던가, 2. 동일종류 템이 하나도 없고 최초였던가

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
        if ((info._sizeX * info._sizeY) > _blank) { return; } //애초에 들어갈 공간이 없다

        {
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
                    //스택가능 개수 안넘어서 잘 넣었다면 함수를 종료하고
                    //그게 아니라면, 여분공간 체크해서 아이템 넣기 (밑에로직 공통적으로)
                }
            }
        }


        int targetX = -1;
        int targetY = -1;
        bool isRotated = false;

        bool isPushAble = CheckInventorySpace_MustOpt(ref info, ref targetX, ref targetY, ref isRotated);

        if (isPushAble == false) { return; }

        //여분이 있다.

        //오브젝트 준비
        GameObject itemUI = Instantiate(_itemUIPrefab, _myRectTransform);
        {
            if (itemUI == null)
            {
                Debug.Assert(itemUI != null, "itemUI 생성 실패");
            }

            RectTransform itemUIRectTransform = itemUI.GetComponent<RectTransform>();
            //사이즈변경
            itemUIRectTransform.sizeDelta = new Vector2(info._sizeX * 20, info._sizeY * 20);

            //위치변경
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

        //다음 검사 시 빨리 찾기위한 공간갱신
        {
            _blank -= info._sizeX * info._sizeY; 
        }

        //격자갱신
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

        //탐색용 구조체 갱신
        int inventoryIndex = _cols * targetY + targetX_modified;
        
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


    public void DeleteItemUseForDragItem(InventoryTransitionDesc transitionDesc)
    {
        //아이템을 드래그 드롭 했을때 한꺼번에 옮기는 함수 = 전부다 없앨것이다.

        if (_items.ContainsKey(transitionDesc._itemInfo._itemKey) == false)
        {
            Debug.Assert(false, "없는 아이템을 지우려하고있다");
            return;
        }

        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey]; //적어도 하나는 들어있었어야 한다

        if (itemKeyCategory.ContainsKey(transitionDesc._fromIndex) == false)
        {
            Debug.Assert(false, "없는 아이템을 지우려하고있다. 이미 삭제됐나?");
            return;
        }

        //칸 수 갱신
        {
            _blank += transitionDesc._itemInfo._sizeX * transitionDesc._itemInfo._sizeY;
        }

        //여백 갱신
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
        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[transitionDesc._itemInfo._itemKey]; //적어도 하나는 들어있었어야 한다
        itemKeyCategory.Remove(transitionDesc._fromIndex);
    }








    public bool CheckInventorySpace_MustOpt(ref ItemInfo itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        //|TODO| = 너무 BruteForce다 최적화가 필요하다

        bool isFind = true;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
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
