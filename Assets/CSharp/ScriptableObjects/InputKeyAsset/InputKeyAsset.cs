using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "InputKeyAsset", menuName = "Scriptable Object/CreateInputKeyAsset", order = int.MinValue)]
public class InputKeyAsset : ScriptableObject
{
    [SerializeField] public string _keyName = "None";
    [SerializeField] public KeyCode _keyCode = KeyCode.None;

    public bool GetKeyState(KeyPressType targetPressType)
    {
        switch (targetPressType) 
        {
            default:
                return false;

            case KeyPressType.Pressed:
                {
                    if (Input.GetKeyDown(_keyCode) == true)
                    {
                        return true;
                    }
                    return false;
                }

            case KeyPressType.Hold:
                {
                    if (Input.GetKey(_keyCode) == true)
                    {
                        return true;
                    }
                    return false;
                }

            case KeyPressType.Released:
                {
                    if (Input.GetKeyUp(_keyCode) == true)
                    {
                        return true;
                    }
                    return false;
                }

            case KeyPressType.None:
                {
                    if (Input.GetKey(_keyCode) == true)
                    {
                        return false;
                    }
                    return true;
                }
        }

    }
}
