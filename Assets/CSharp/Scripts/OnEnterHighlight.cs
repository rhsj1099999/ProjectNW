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
            Debug.Assert(_imageComponent != null, "�̹��� ������Ʈ�� �־���ϴ� ��ũ��Ʈ");
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
