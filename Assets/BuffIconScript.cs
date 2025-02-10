using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StatScript;

public class BuffIconScript : MonoBehaviour
{
    [SerializeField] private Image _buffImageComponent = null;
    [SerializeField] private Image _buffImageComponent_BackGroundShade = null;
    [SerializeField] private BuffCountUI _countUI = null;

    private RuntimeBuffAsset _myRuntimeBuffAsset = null;

    private void Awake()
    {
        if (_buffImageComponent == null)
        {
            Debug.Assert(false, "프리팹에서 타겟 이미지를 설정하세여");
            Debug.Break();
        }

        if (_buffImageComponent_BackGroundShade == null)
        {
            Debug.Assert(false, "프리팹에서 배경 그림자 이미지를 설정하세요");
            Debug.Break();
        }

        if (_countUI == null)
        {
            Debug.Assert(false, "프리팹에서 카운팅 UI를 설정하십셔");
            Debug.Break();
        }
    }


    public void SetImage(Sprite buffSprite, RuntimeBuffAsset buffAsset)
    {
        if (buffSprite == null)
        {
            Debug.Assert(false, "이미지가 없나요? 표시되지 않을거라면 이미 버프에셋에서 설정돼있었어야 합니다");
            Debug.Break();
        }
        _myRuntimeBuffAsset = buffAsset;
        _buffImageComponent.sprite = buffSprite;
        _buffImageComponent_BackGroundShade.sprite = buffSprite;

        _countUI.SetCount(_myRuntimeBuffAsset._Count);
        //델리게이터 바인딩
        {
            _myRuntimeBuffAsset.AddDelegate(_countUI.SetCount);
        }

        SetImageClockCounter();
    }



    private void Update()
    {
        SetImageClockCounter();
    }

    private void SetImageClockCounter()
    {
        if (_myRuntimeBuffAsset._fromAsset._Duration < 0)
        {
            return;
        }
        _buffImageComponent.fillAmount = 1.0f - (_myRuntimeBuffAsset._timeACC / _myRuntimeBuffAsset._fromAsset._Duration);
    }
}
