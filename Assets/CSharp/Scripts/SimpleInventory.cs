using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleInventory : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _inventoryPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleInventory();
        }
    }


    private void ToggleInventory()
    {
        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);

        if (_inventoryPanel.activeSelf)
        {
            // 인벤토리 패널을 활성화할 때 아이템 슬롯을 추가
            CreateInventorySlots();
        }
    }

    private void CreateInventorySlots()
    {
        // 기존 슬롯을 제거 (재사용을 위해)
        foreach (Transform child in _inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 지정된 행과 열 수에 따라 슬롯 생성
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                Instantiate(_itemSlotPrefab, _inventoryPanel.transform);
            }
        }
    }

    [SerializeField] private GameObject _inventoryPanel = null;
    [SerializeField] private GameObject _itemSlotPrefab = null;
    [SerializeField] private int _rows = 4;
    [SerializeField] private int _cols = 4;
}
