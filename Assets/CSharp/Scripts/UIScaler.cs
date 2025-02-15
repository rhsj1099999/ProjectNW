using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Vector4 GetAnchorMinMax()
    {
        if (_myRectTransform == null)
        {
            _myRectTransform = GetComponent<RectTransform>();
        }

        Vector4 ret = new Vector4
        (
            _myRectTransform.anchorMin.x,
            _myRectTransform.anchorMin.y,
            _myRectTransform.anchorMax.x,
            _myRectTransform.anchorMax.y
        );

        return ret;
    }

    public Vector2 GetAnchoredSize()
    {
        if (_myRectTransform == null)
        {
            _myRectTransform = GetComponent<RectTransform>();
        }

        Canvas mainCanvasComponent = UIManager.Instance.Get2DCanvs().GetComponent<Canvas>();
        //CanvasScaler canvasScaler = UIManager.Instance.GetMainCanvasObject().GetComponent<CanvasScaler>();

        RectTransform canvasRectTransform = (RectTransform)mainCanvasComponent.transform;
        
        float screenWidth = canvasRectTransform.rect.width;
        float screenHeight = canvasRectTransform.rect.height;

        Vector2 size = new Vector2
        (
            (_myRectTransform.anchorMax.x - _myRectTransform.anchorMin.x) * screenWidth,
            (_myRectTransform.anchorMax.y - _myRectTransform.anchorMin.y) * screenHeight
        );

        return size;
    }

}
