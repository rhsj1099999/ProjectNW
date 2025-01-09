//using System;
//using UnityEngine;
//using System.Collections.Generic;
//using UnityEditor;
public class GraphEditorWindow
{
    
}
//public class GraphEditorWindow : EditorWindow
//{
//    [Serializable]
//    public class StateAssetWrapper
//    {
//        public StateAsset _stateAsset = null;
//        public bool _isEntryPoint = false;
//    }

//    [Serializable]
//    public class StateLinkDesc
//    {
//        public StateAsset _enterState = null;
//        public List<ConditionAsset> _enterCondition = new List<ConditionAsset>();
//    }


//    private Vector2 scrollPosition; // ��ũ�Ѻ�
//    private Rect nodeRect = new Rect(100, 100, 150, 100); // ��� ����
//    private Vector2 dragOffset; // �巡�� ������
//    private bool isDragging = false; // �巡�� �� ����
//    [SerializeField] private List<Rect> _boxes = new List<Rect>();

//    [MenuItem("Window/Graph Editor")]
//    public static void OpenWindow()
//    {
//        GetWindow<GraphEditorWindow>("Graph Editor");
//    }

//    private void OnGUI()
//    {
//        // ��ũ�Ѻ� ����
//        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

//        // ��� �׸���
//        DrawBackground();

//        // ��� �׸���
//        DrawNode();

//        // �̺�Ʈ ó��
//        HandleEvents(Event.current);

//        // ��ũ�Ѻ� ��
//        EditorGUILayout.EndScrollView();


//    }

//    private void DrawBackground()
//    {
//        // ������ ���� ���
//        for (int x = 0; x < position.width; x += 20)
//            for (int y = 0; y < position.height; y += 20)
//                EditorGUI.DrawRect(new Rect(x, y, 1, 1), Color.gray);
//    }

//    private void DrawNode()
//    {
//        foreach (var node in _boxes)
//        {
//            GUI.Box(node, "Node", EditorStyles.helpBox);
//        }

//        // ��� �巡�� ó��
//        if (isDragging)
//        {
//            nodeRect.position = Event.current.mousePosition + dragOffset;
//            Repaint();
//        }
//    }

//    private void HandleEvents(Event e)
//    {
//        switch (e.type)
//        {
//            case EventType.MouseDown:
//                if (nodeRect.Contains(e.mousePosition))
//                {
//                    isDragging = true;
//                    dragOffset = nodeRect.position - e.mousePosition;
//                }
//                break;

//            case EventType.MouseUp:
//                isDragging = false;
//                break;

//            case EventType.MouseDrag:
//                if (isDragging)
//                {
//                    nodeRect.position = e.mousePosition + dragOffset;
//                    Repaint();
//                }
//                break;

//            case EventType.ContextClick:
//                {
//                    ShowContextMenu();
//                    e.Use();
//                }
//                break;

//            case EventType.MouseMove:
//                {
//                }
//                break;
//        }
//    }


//    private void ShowContextMenu()
//    {
//        // ���ؽ�Ʈ �޴� ����
//        GenericMenu menu = new GenericMenu();

//        // �޴� �׸� �߰�
//        menu.AddItem(new GUIContent("Add Node"), false, AddNode);
//        menu.AddItem(new GUIContent("Delete Node"), false, DeleteNode);
//        menu.AddSeparator(""); // ���м� �߰�
//        menu.AddItem(new GUIContent("Reset Graph"), false, ResetGraph);

//        // �޴� ǥ��
//        menu.ShowAsContext();
//    }


//    private void AddNode()
//    {
//        //Debug.Log("Add Node clicked at " + mousePosition);
//        // ��带 �߰��ϴ� ���� �ۼ�
//        Rect rect = new Rect();
//        rect = nodeRect;
//        _boxes.Add(rect);

//    }

//    private void DeleteNode()
//    {
//        Debug.Log("Delete Node clicked");
//        // ��带 �����ϴ� ���� �ۼ�
//    }

//    private void ResetGraph()
//    {
//        Debug.Log("Reset Graph clicked");
//        // �׷����� �ʱ�ȭ�ϴ� ���� �ۼ�
//    }
//}