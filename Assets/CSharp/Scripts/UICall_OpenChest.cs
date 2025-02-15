using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICall_OpenChest : UICallScript
{
    [SerializeField] private bool _isChestOpened = false;
    [SerializeField] private bool _isChestRotating = false;
    [SerializeField] private GameObject _chestInventory = null;

    [SerializeField] private float _maxOpenDegree = 100.0f;
    [SerializeField] private float _openTime = 1.0f;

    [SerializeField] private GameObject _chestLid = null;
    private Quaternion _originalRotation = Quaternion.identity;
    private Quaternion _openedRotation = Quaternion.identity;
    private float _currRotatingTime = 0.0f;
    private float _rotationSpeed = 0.0f;



    [Serializable]
    public class FirstItemCreateDesc
    {
        public string _itemName = "None";
        public int _count = 0;
    }

    
    [SerializeField] private List<FirstItemCreateDesc> _fixedCreateTryItemList = new List<FirstItemCreateDesc>();

    [SerializeField] private Sprite _uiImage = null;

    private void Awake()
    {
        if (_uiImage == null)
        {
            Debug.Assert(false, "인스펙터에서 UIImage를 설정하세요");
        }

        _originalRotation = _chestLid.transform.rotation;

        _uiData = new InteractionUIData();
        _uiData._sprite = _uiImage;
        _uiData._message = "상자 열기.";
    }


    private void Start()
    {
        InventoryBoard boardComponent = _chestInventory.GetComponentInChildren<InventoryBoard>();

        foreach (FirstItemCreateDesc itemDesc in _fixedCreateTryItemList)
        {
            boardComponent.AddItemAutomatic(ItemInfoManager.Instance.GetItemInfo(itemDesc._itemName), itemDesc._count);
        }
    }


    public override void UICall_Off(InteractionUIListScript caller)
    {
        CloseChest();
    }

    public void OpenChest()
    {
        if (_isChestOpened == true)
        {
            return;
        }

        _isChestOpened = true;

        UIManager.Instance.TurnOnUI(_chestInventory, UIManager.LayerOrder.InventorySomethingElse);

        _openedRotation = _originalRotation * Quaternion.Euler(_maxOpenDegree, 0.0f, 0.0f);
        Quaternion goalRotation = _openedRotation;
        Quaternion startQauternion = _originalRotation;
        _rotationSpeed = Quaternion.Angle(startQauternion, goalRotation) / _openTime;

        _currRotatingTime = 0.0f;

        if (_isChestRotating == false)
        {
            StartCoroutine(OpenChestCoroutine());
        }
    }

    public void CloseChest()
    {
        if (_isChestOpened == false)
        {
            return;
        }

        _isChestOpened = false;

        UIManager.Instance.TurnOffUI(_chestInventory);

        _openedRotation = _originalRotation * Quaternion.Euler(_maxOpenDegree, 0.0f, 0.0f);
        Quaternion goalRotation = _originalRotation;
        Quaternion startQauternion = _openedRotation;
        _rotationSpeed = Quaternion.Angle(startQauternion, goalRotation) / _openTime;

        _currRotatingTime = 0.0f;

        if (_isChestRotating == false)
        {
            StartCoroutine(OpenChestCoroutine());
        }
    }

    public override void UICall(InteractionUIListScript caller)
    {
        if (_isChestOpened == false)
        {
            OpenChest();
        }
        else
        {
            CloseChest();
        }

        return;
    }



    private IEnumerator OpenChestCoroutine()
    {
        _isChestRotating = true;

        while (true) 
        {
            _currRotatingTime += Time.deltaTime;

            if (_currRotatingTime >= _openTime)
            {
                _currRotatingTime = 0.0f;

                _chestLid.transform.rotation = (_isChestOpened == true)
                    ? _openedRotation
                    : _originalRotation;

                break;
            }

            Quaternion targetRotation = (_isChestOpened == true)
                ? _openedRotation
                : _originalRotation;

            _chestLid.transform.rotation = Quaternion.Slerp(_chestLid.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            yield return null;
        }

        _isChestRotating = false;
    }
}
