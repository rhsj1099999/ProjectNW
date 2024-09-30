using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InventoryUIDesc
{
    public int _rows;
    public int _cols;

    public Dictionary<int/*Ű*/, Dictionary<int/*����� ĭ*/, ItemStoreDesc>> _items;
    public Dictionary<int/*����� ĭ*/, GameObject> _itemUIs;
}

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static UIManager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Update()
    {
        
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) 
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void TurnOnUI(GameObject uiPrefab)
    {
        //UI �Ŵ������� ȭ�鿡 ��� UI�� �����ϴ� �Լ�

        //1. ���� ĵ������ �ڽ����� UI�� �����Ѵ�
    }

    public void TurnOffUI(GameObject uiPrefab)
    {
        //UI �Ŵ������� ��Ȱ��ȭ �� UI�� �����ϴ� �Լ�

        //�ı��ϸ� ������ �����
    }



    private static UIManager _instance = null;
}
