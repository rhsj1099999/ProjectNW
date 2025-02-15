using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
