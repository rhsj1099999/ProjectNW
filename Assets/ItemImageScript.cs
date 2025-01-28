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
            Debug.Assert(false, "Image ���� ����� �ݵ�� �����ϼ���");
            Debug.Break();
        }

        _targetUIComponent = _targetObject.GetComponent<Image>();
        if (_targetUIComponent == null)
        {
            Debug.Assert(false, "_targetObject �� Image ������Ʈ�� ����� �ȵ˴ϴ�");
            Debug.Break();
        }
    }

    public void Init(Sprite itemSprite)
    {
        _targetUIComponent.sprite = itemSprite;
    }
}
