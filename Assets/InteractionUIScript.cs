using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUIScript : MonoBehaviour
{
    [SerializeField] private Image _imageComponent = null;
    [SerializeField] private TextMeshProUGUI _textComponent = null;

    private void Awake()
    {
        if (_imageComponent == null)
        {
            Debug.Assert(false, "�ν����Ϳ��� Image�� �����ϼ���");
            Debug.Break();
        }

        if (_textComponent == null)
        {
            Debug.Assert(false, "�ν����Ϳ��� Text�� �����ϼ���");
            Debug.Break();
        }
    }


    public void SetUIListData(Sprite sprite, string text)
    {
        _textComponent.text = text;
        _imageComponent.sprite = sprite;
    }
}
