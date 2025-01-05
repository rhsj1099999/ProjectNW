using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CurtainCallType
{
    SimpleColorFadeInOut,
    FocusedPolygoneScaler,
    END,
}

public class CurtainCallControlDesc
{
    public float _runningTime = 0.0f;
    public bool _target = false;
}


public abstract class CurtainCallBase : MonoBehaviour
{
    [SerializeField] protected CurtainCallType _myCurtailCallType = CurtainCallType.END;

    protected bool _isOn = false;
    protected Coroutine _runningCoroutine = null;
    protected float _runningTimeACC = 0.0f;

    public void Active(CurtainCallControlDesc desc)
    {
        ActiveInitial(desc);
        _runningCoroutine = StartCoroutine(YourOverrideActive(desc));
    }

    public CurtainCallType GetCurtainCallType()
    {
        return _myCurtailCallType;
    }

    protected virtual void Awake()
    {
        SetMyType();

        if (_myCurtailCallType == CurtainCallType.END)
        {
            Debug.Assert(false, "커튼콜 타입이 End 이다" + this.name);
            Debug.Break();
            return;
        }
    }

    public abstract Type GetMyDescType();
    public abstract CurtainCallControlDesc GetMyControllingDescType();
    protected abstract void SetMyType();
    protected abstract void ActiveInitial(CurtainCallControlDesc desc);
    protected abstract IEnumerator YourOverrideActive(CurtainCallControlDesc desc);
}
