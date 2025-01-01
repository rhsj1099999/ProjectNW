using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameCharacterSubScript : MonoBehaviour
{
    //Character Script가 필요로 하는 서브 컴포넌트들.
    protected Type _myType = null;
    protected CharacterScript _owner = null;

    public void Test() { Debug.Log("상속받은 컴포넌트입니다"); }

    public abstract void Init(CharacterScript owner);
    public abstract void SubScriptStart();

    public Type GetMyRealType() 
    {
        if (_myType == null)
        {
            Debug.Assert(false, "실제 타입을 정의하지 않았습니다. Init 에서 정의하세여");
            Debug.Break();
        }
        return _myType;
    }


}
