using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffTypes
{
    StateBuff,
    StateBlocking, //가드자세를 취함으로써 얻을 수 있는 버프
    StateDrinkingPotion, //물약을 마시는 상태라서 얻을 수 있는 버프
}

public class BuffScript
{
    public float _buffActivatedTime = 0.0f;
    public float _buffRoughness = 0.0f;

    public void StartBuff()
    {

    }

    public void EndBuff()
    {

    }

    public GameObject CallFunc()
    {
        return null;
    }
}
