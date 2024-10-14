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
            // �κ��丮 �г��� Ȱ��ȭ�� �� ������ ������ �߰�
            CreateInventorySlots();
        }
    }

    private void CreateInventorySlots()
    {
        // ���� ������ ���� (������ ����)
        foreach (Transform child in _inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // ������ ��� �� ���� ���� ���� ����
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
