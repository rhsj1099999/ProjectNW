using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HUDPositioner)), CanEditMultipleObjects]
public class HUDPositionerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 Inspector UI 그리기


        if (GUILayout.Button("SyncAnchor To UI Point"))
        {
            foreach (var obj in targets)
            {
                HUDPositioner component = (HUDPositioner)obj;
                SyncAnchorToMyPosition(component);
            }
        }
    }

    Vector3[] GetScreenCorners(RectTransform rectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        Vector3[] screenCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);
        }

        return screenCorners;
    }

    private void SyncAnchorToMyPosition(HUDPositioner uiComponent)
    {

        Canvas mainCanvasComponent = uiComponent.GetComponentInParent<Canvas>();

        RectTransform canvasRectTransform = (RectTransform)mainCanvasComponent.transform;

        RectTransform rectTransform = uiComponent.GetComponent<RectTransform>();

        Vector3[] screenCorners = GetScreenCorners(rectTransform);

        float screenWidth = canvasRectTransform.rect.width;
        float screenHeight = canvasRectTransform.rect.height;

        Vector2 anchorMin = new Vector2(screenCorners[0].x / screenWidth, screenCorners[0].y / screenHeight);
        Vector2 anchorMax = new Vector2(screenCorners[2].x / screenWidth, screenCorners[2].y / screenHeight);

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        EditorUtility.SetDirty(rectTransform); // 변경 사항 Dirty 플래그 설정
        
    }
}
