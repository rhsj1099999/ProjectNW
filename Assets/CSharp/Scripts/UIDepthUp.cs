using UnityEngine;

public interface IUIDepthUp
{
    virtual void DepthTop(Canvas canvas)
    {
        Debug.Assert(canvas != null, "canvas �� null�� �� ����.");

        canvas.overrideSorting = true;
    }

    virtual void DepthDown(Canvas canvas) 
    {
        Debug.Assert(canvas != null, "canvas �� null�� �� ����.");

        canvas.overrideSorting = false;
    }
}
