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
            Debug.Assert(false, "인스펙터에서 Image를 지정하세요");
            Debug.Break();
        }

        if (_textComponent == null)
        {
            Debug.Assert(false, "인스펙터에서 Text를 지정하세요");
            Debug.Break();
        }
    }


    public void SetUIListData(Sprite sprite, string text)
    {
        _textComponent.text = text;
        _imageComponent.sprite = sprite;
    }
}
