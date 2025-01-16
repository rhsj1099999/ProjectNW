using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public struct ItemStoreDesc
//{
//    public ItemStoreDesc(ItemInfo info, int count, Vector2 position)
//    {
//        _info = info;
//        _count = count;
//        _position = position;
//        _storedIndex = 0;
//        _isRotated = false;
//        _owner = null;
//    }

//    public void PlusItem(int count = 1)
//    {
//        _count++;
//    }

//    public Vector2 _position;       //저장된 위치 
//    public ItemInfo _info;          //인포
//    public int _count;              //개수
//    public int _storedIndex;        //저장된 칸
//    public bool _isRotated;
//    public InventoryBoard _owner;

//}

public class InventoryData : MonoBehaviour
{
    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;

    /*-------------
    계속 바뀔 변수들
    -------------*/
    private int _blank = 0;
    private bool[,] _blankDispaly;
    private Dictionary<int/*키*/, Dictionary<int/*저장된 칸*/, ItemStoreDesc>> _items = new Dictionary<int, Dictionary<int, ItemStoreDesc>>();  //아이템이 있는지 없는지 확인용
    private Dictionary<int/*저장된 칸*/, GameObject> _itemUIs = new Dictionary<int, GameObject>();                   //실제로 들어있는 아이템 렌더링 담당


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        _blank = _cols * _rows;
        _blankDispaly = new bool[_rows, _cols];
    }













    private void RemoveItemUsingCellIndex(ItemStoreDesc storedDesc)
    {
        Destroy(_itemUIs[storedDesc._storedIndex]);
        _itemUIs.Remove(storedDesc._storedIndex);
        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[storedDesc._itemAsset._ItemKey]; //적어도 하나는 들어있었어야 한다
        itemKeyCategory.Remove(storedDesc._storedIndex);
    }


    public bool CheckInventorySpace_MustOpt(ItemAsset itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        //|TODO| = 너무 BruteForce다 최적화가 필요하다

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
                            targetX = j + (itemInfo._SizeX - 1);
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
