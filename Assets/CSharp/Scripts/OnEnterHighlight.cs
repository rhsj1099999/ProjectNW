using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnEnterHighlight : MonoBehaviour
{
    void Awake()
    {
        _imageComponent = GetComponent<Image>();

        if (_imageComponent != null )
        {
            Debug.Assert(_imageComponent != null, "이미지 컴포넌트가 있어야하는 스크립트");
        }

        _originalColor = _imageComponent.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    _imageComponent.color = Color.green;
    //}
    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    _imageComponent.color = _originalColor;
    //}


    // Update is called once per frame
    void Update()
    {
        
    }

    private Image _imageComponent = null; 
    private Color _originalColor = Color.white; 
}
