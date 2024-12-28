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

    public override void UICall_Off()
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

        UIManager.Instance.TurnOnUI(_chestInventory);

        _openedRotation = _originalRotation * Quaternion.Euler(_maxOpenDegree, 0.0f, 0.0f);
        Quaternion goalRotation = _openedRotation;
        Quaternion startQauternion = _originalRotation;
        _rotationSpeed = Quaternion.Angle(startQauternion, goalRotation) / _openTime;
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
    }

    public override void UICall()
    {

        if (_isChestOpened == false)
        {
            OpenChest();
        }
        else
        {
            CloseChest();
        }

        _currRotatingTime = 0.0f;

        if (_isChestRotating == false)
        {
            StartCoroutine(OpenChestCoroutine());
        }

        return;
    }

    private void Awake()
    {
        _originalRotation = _chestLid.transform.rotation;
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
