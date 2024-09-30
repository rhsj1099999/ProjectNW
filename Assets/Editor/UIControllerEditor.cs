using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryBoard))]
public class UIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 UI 표시
        DrawDefaultInspector();

        // 타겟 스크립트 참조
        InventoryBoard uiController = (InventoryBoard)target;

        // n이 변경될 때마다 RectTransform 업데이트
        if (GUILayout.Button("Update RectTransform"))
        {
            //uiController.TestSize();
        }
    }
}
