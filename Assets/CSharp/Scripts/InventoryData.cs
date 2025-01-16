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

//    public Vector2 _position;       //����� ��ġ 
//    public ItemInfo _info;          //����
//    public int _count;              //����
//    public int _storedIndex;        //����� ĭ
//    public bool _isRotated;
//    public InventoryBoard _owner;

//}

public class InventoryData : MonoBehaviour
{
    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;

    /*-------------
    ��� �ٲ� ������
    -------------*/
    private int _blank = 0;
    private bool[,] _blankDispaly;
    private Dictionary<int/*Ű*/, Dictionary<int/*����� ĭ*/, ItemStoreDesc>> _items = new Dictionary<int, Dictionary<int, ItemStoreDesc>>();  //�������� �ִ��� ������ Ȯ�ο�
    private Dictionary<int/*����� ĭ*/, GameObject> _itemUIs = new Dictionary<int, GameObject>();                   //������ ����ִ� ������ ������ ���


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
        Dictionary<int, ItemStoreDesc> itemKeyCategory = _items[storedDesc._itemAsset._ItemKey]; //��� �ϳ��� ����־���� �Ѵ�
        itemKeyCategory.Remove(storedDesc._storedIndex);
    }


    public bool CheckInventorySpace_MustOpt(ItemAsset itemInfo, ref int targetX, ref int targetY, ref bool isRotated, int startX = 0, int startY = 0)
    {
        //|TODO| = �ʹ� BruteForce�� ����ȭ�� �ʿ��ϴ�

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
