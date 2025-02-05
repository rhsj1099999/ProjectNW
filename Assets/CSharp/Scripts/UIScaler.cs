using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
    /*----------------------------------------------------------------
    |NOTI| 이 프로젝트에서 표시될 UI라면 반드시 가지고있어야할 컴포넌트
    보통 프리팹 모드에서 UI 작업을 할때 한가지 규약
    최상단 GameObject는 반드시 이것을 가지고 있어야하며(HUD류 가 아니라면)
    이것은 최대 크기를 정의한다.
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
