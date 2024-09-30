using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryBoard))]
public class UIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �⺻ �ν����� UI ǥ��
        DrawDefaultInspector();

        // Ÿ�� ��ũ��Ʈ ����
        InventoryBoard uiController = (InventoryBoard)target;

        // n�� ����� ������ RectTransform ������Ʈ
        if (GUILayout.Button("Update RectTransform"))
        {
            //uiController.TestSize();
        }
    }
}
