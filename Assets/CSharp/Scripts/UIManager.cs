using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InventoryUIDesc
{
    public int _rows;
    public int _cols;

    public Dictionary<int/*키*/, Dictionary<int/*저장된 칸*/, ItemStoreDesc>> _items;
    public Dictionary<int/*저장된 칸*/, GameObject> _itemUIs;
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
        //UI 매니저에게 화면에 띄울 UI를 전달하는 함수

        //1. 메인 캔버스의 자식으로 UI를 생성한다
    }

    public void TurnOffUI(GameObject uiPrefab)
    {
        //UI 매니저에게 비활성화 할 UI를 전달하는 함수

        //파괴하면 정보가 사라짐
    }



    private static UIManager _instance = null;
}
