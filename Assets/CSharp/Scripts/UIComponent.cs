using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Debug.Assert( _canvas != null, "Canvas�� ���� �� ����" );
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
