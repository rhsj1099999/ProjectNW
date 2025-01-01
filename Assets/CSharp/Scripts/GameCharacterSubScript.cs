using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameCharacterSubScript : MonoBehaviour
{
    //Character Script�� �ʿ�� �ϴ� ���� ������Ʈ��.
    protected Type _myType = null;
    protected CharacterScript _owner = null;

    public void Test() { Debug.Log("��ӹ��� ������Ʈ�Դϴ�"); }

    public abstract void Init(CharacterScript owner);
    public abstract void SubScriptStart();

    public Type GetMyRealType() 
    {
        if (_myType == null)
        {
            Debug.Assert(false, "���� Ÿ���� �������� �ʾҽ��ϴ�. Init ���� �����ϼ���");
            Debug.Break();
        }
        return _myType;
    }


}
