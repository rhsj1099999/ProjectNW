using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconScript : MonoBehaviour
{
    [SerializeField] private Image _buffImageComponent = null;

    private void Awake()
    {
        if (_buffImageComponent == null)
        {
            Debug.Assert(false, "프리팹에서 타겟 이미지를 설정하세여");
            Debug.Break();
        }
    }


    public void SetImage(Sprite buffSprite)
    {
        if (buffSprite == null)
        {
            Debug.Assert(false, "이미지가 없나요? 표시되지 않을거라면 이미 버프에셋에서 설정돼있었어야 합니다");
            Debug.Break();
        }

        _buffImageComponent.sprite = buffSprite;
    }
}
