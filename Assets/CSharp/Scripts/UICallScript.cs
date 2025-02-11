using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UICallScript : MonoBehaviour
{
    public class InteractionUIData
    {
        public Sprite _sprite = null;
        public string _message = "None";
    }

    protected InteractionUIListScript _addedList = null;
    protected InteractionUIData _uiData = null;
    public InteractionUIData _UIData => _uiData;

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
