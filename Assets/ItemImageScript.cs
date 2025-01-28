using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemImageScript : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject = null;
    private Image _targetUIComponent = null;
    

    private void Awake()
    {
        if (_targetObject == null)
        {
            Debug.Assert(false, "Image 설정 대상을 반드시 지정하세요");
            Debug.Break();
        }

        _targetUIComponent = _targetObject.GetComponent<Image>();
        if (_targetUIComponent == null)
        {
            Debug.Assert(false, "_targetObject 에 Image 컴포넌트가 없어서는 안됩니다");
            Debug.Break();
        }
    }

    public void Init(Sprite itemSprite)
    {
        _targetUIComponent.sprite = itemSprite;
    }
}
