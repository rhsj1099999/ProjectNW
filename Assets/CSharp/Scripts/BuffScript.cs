using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffTypes
{
    StateBuff,
    StateBlocking, //�����ڼ��� �������ν� ���� �� �ִ� ����
    StateDrinkingPotion, //������ ���ô� ���¶� ���� �� �ִ� ����
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
