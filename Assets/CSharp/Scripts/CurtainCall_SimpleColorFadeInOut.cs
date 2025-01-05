using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurtainCallControl_SimpleColor : CurtainCallControlDesc
{
    public Vector3 _color = Vector3.one;
}

public class CurtainCall_SimpleColorFadeInOut : CurtainCallBase
{
    private Image _imageComponent = null;

    public override Type GetMyDescType() { return typeof(CurtainCallControlDesc); }

    public override CurtainCallControlDesc GetMyControllingDescType()
    {
        return new CurtainCallControl_SimpleColor();
    }

    protected override void SetMyType()
    {
        _myCurtailCallType = CurtainCallType.SimpleColorFadeInOut;
    }

    protected override void Awake()
    {
        base.Awake();

        _imageComponent = GetComponentInChildren<Image>();

        if ( _imageComponent == null )
        {
            Debug.Assert(false, "Image 컴포넌트가 없습니다");
            Debug.Break();
            return;
        }
    }

    protected override void ActiveInitial(CurtainCallControlDesc desc)
    {
        CurtainCallControl_SimpleColor casted = (CurtainCallControl_SimpleColor)desc;

        if (_runningCoroutine != null) 
        {
            StopCoroutine(_runningCoroutine);
            _runningCoroutine = null;
        }

        float alpha = (desc._target == true)
            ? 0.0f
            : 1.0f;

        Vector4 nextColor = new Vector4(casted._color.x, casted._color.y, casted._color.z, alpha);

        _imageComponent.color = nextColor;
    }


    protected override IEnumerator YourOverrideActive(CurtainCallControlDesc desc)
    {
        CurtainCallControl_SimpleColor casted = (CurtainCallControl_SimpleColor)desc;

        _runningTimeACC = 0.0f;

        while (true) 
        {
            _runningTimeACC += Time.deltaTime;

            _runningTimeACC = Math.Clamp(_runningTimeACC, 0.0f, desc._runningTime);

            float alphaPercentage = _runningTimeACC / desc._runningTime;

            Color imageColor = _imageComponent.color;

            imageColor.a = (desc._target == true)
                ? 1.0f - alphaPercentage
                : alphaPercentage;

            _imageComponent.color = imageColor;

            if (_runningTimeACC >= casted._runningTime)
            {
                _runningTimeACC = 0.0f;
                _isOn = desc._target;
                break;
            }

            yield return null;
        }
    }
}
