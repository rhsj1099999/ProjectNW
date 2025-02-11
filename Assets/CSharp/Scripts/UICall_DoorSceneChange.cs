using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICall_SceneChange : UICallScript
{
    [Serializable]
    public class FirstItemCreateDesc
    {
        public string _itemName = "None";
        public int _count = 0;
    }

    [SerializeField] private Sprite _uiImage = null;
    [SerializeField] private SceneChangeDoor_InField _infieldDoorScript = null;

    private void Awake()
    {
        if (_uiImage == null)
        {
            Debug.Assert(false, "인스펙터에서 UIImage를 설정하세요");
        }

        if (_infieldDoorScript == null)
        {
            Debug.Assert(false, "인스펙터에서 Door Script를 설정하세요");
            Debug.Break();
        }

        _uiData = new InteractionUIData();
        _uiData._sprite = _uiImage;
        _uiData._message = "들어가기.";
    }


    public override void UICall_Off(InteractionUIListScript caller)
    {
        if (_addedList == caller)
        {
            _addedList = null;
        }

        _infieldDoorScript.DoorCall();
    }

    public override void UICall(InteractionUIListScript caller)
    {
        _addedList = caller;

        _infieldDoorScript.SceneChange();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("플레이어가 부딪혔다");
        }

        _infieldDoorScript.DoorCall();
    }
}
