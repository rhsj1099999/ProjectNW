using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    [SerializeField] GameObject _owner = null;

    private Canvas _canvas = null;
}
