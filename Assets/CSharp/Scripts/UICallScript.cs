using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UICallScript : MonoBehaviour
{
    public abstract void UICall(InteractionUIListScript caller);
    public abstract void UICall_Off(InteractionUIListScript caller);
}
