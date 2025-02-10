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


    [SerializeField] private SceneChangeDoor_InField _infieldDoorScript = null;

    private void Awake()
    {
        if (_infieldDoorScript == null)
        {
            Debug.Assert(false, "�ν����Ϳ��� Door Script�� �����ϼ���");
            Debug.Break();
        }
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
            Debug.Log("�÷��̾ �ε�����");
        }

        _infieldDoorScript.DoorCall();
    }
}
