using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
    /*----------------------------------------------------------------
    |NOTI| �� ������Ʈ���� ǥ�õ� UI��� �ݵ�� �������־���� ������Ʈ
    ���� ������ ��忡�� UI �۾��� �Ҷ� �Ѱ��� �Ծ�
    �ֻ�� GameObject�� �ݵ�� �̰��� ������ �־���ϸ�(HUD�� �� �ƴ϶��)
    �̰��� �ִ� ũ�⸦ �����Ѵ�.
    ----------------------------------------------------------------*/

    RectTransform _myRectTransform = null;

    private void Awake()
    {
        _myRectTransform = (RectTransform)transform;
    }

    public Vector2 GetAnchoredSize()
    {
        if (_myRectTransform == null)
        {
            _myRectTransform = GetComponent<RectTransform>();
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector2 size = new Vector2(
            (_myRectTransform.anchorMax.x - _myRectTransform.anchorMin.x) * screenSize.x,
            (_myRectTransform.anchorMax.y - _myRectTransform.anchorMin.y) * screenSize.y
        );

        return size;
    }

}
