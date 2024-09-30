using UnityEngine;

public interface IUIDepthUp
{
    virtual void DepthTop(Canvas canvas)
    {
        Debug.Assert(canvas != null, "canvas 는 null일 수 없다.");

        canvas.overrideSorting = true;
    }

    virtual void DepthDown(Canvas canvas) 
    {
        Debug.Assert(canvas != null, "canvas 는 null일 수 없다.");

        canvas.overrideSorting = false;
    }
}
