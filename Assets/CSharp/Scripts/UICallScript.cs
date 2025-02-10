using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UICallScript : MonoBehaviour
{
    protected InteractionUIListScript _addedList = null;

    public abstract void UICall(InteractionUIListScript caller);
    public abstract void UICall_Off(InteractionUIListScript caller);

    private void OnDestroy()
    {
        if (_addedList != null)
        {
            _addedList.RemoveList(this);
        }
    }
}
