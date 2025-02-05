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
            Debug.Assert(false, "�����տ��� Ÿ�� �̹����� �����ϼ���");
            Debug.Break();
        }
    }


    public void SetImage(Sprite buffSprite)
    {
        if (buffSprite == null)
        {
            Debug.Assert(false, "�̹����� ������? ǥ�õ��� �����Ŷ�� �̹� �������¿��� �������־���� �մϴ�");
            Debug.Break();
        }

        _buffImageComponent.sprite = buffSprite;
    }
}
