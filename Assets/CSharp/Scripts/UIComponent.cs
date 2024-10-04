using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Debug.Assert( _canvas != null, "Canvas는 널일 수 없다" );
    }

    public void HideUI()
    {
        _canvas.enabled = false;
    }

    public void ShowUI()
    {
        _canvas.enabled = true;
    }

    private Canvas _canvas = null;
}
