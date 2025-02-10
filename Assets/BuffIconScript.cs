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
            Debug.Assert(false, "�����տ��� Ÿ�� �̹����� �����ϼ���");
            Debug.Break();
        }

        if (_buffImageComponent_BackGroundShade == null)
        {
            Debug.Assert(false, "�����տ��� ��� �׸��� �̹����� �����ϼ���");
            Debug.Break();
        }

        if (_countUI == null)
        {
            Debug.Assert(false, "�����տ��� ī���� UI�� �����Ͻʼ�");
            Debug.Break();
        }
    }


    public void SetImage(Sprite buffSprite, RuntimeBuffAsset buffAsset)
    {
        if (buffSprite == null)
        {
            Debug.Assert(false, "�̹����� ������? ǥ�õ��� �����Ŷ�� �̹� �������¿��� �������־���� �մϴ�");
            Debug.Break();
        }
        _myRuntimeBuffAsset = buffAsset;
        _buffImageComponent.sprite = buffSprite;
        _buffImageComponent_BackGroundShade.sprite = buffSprite;

        _countUI.SetCount(_myRuntimeBuffAsset._Count);
        //���������� ���ε�
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
