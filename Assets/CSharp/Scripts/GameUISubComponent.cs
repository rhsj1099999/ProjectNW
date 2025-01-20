using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameUISubComponent : MonoBehaviour
{
    protected UIComponent _owner = null;
    public UIComponent _Owner => _owner;

    public abstract void Init(UIComponent owner);
}
