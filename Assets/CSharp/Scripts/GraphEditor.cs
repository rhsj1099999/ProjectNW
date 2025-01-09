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


//    private Vector2 scrollPosition; // 스크롤뷰
//    private Rect nodeRect = new Rect(100, 100, 150, 100); // 노드 영역
//    private Vector2 dragOffset; // 드래그 오프셋
//    private bool isDragging = false; // 드래그 중 여부
//    [SerializeField] private List<Rect> _boxes = new List<Rect>();

//    [MenuItem("Window/Graph Editor")]
//    public static void OpenWindow()
//    {
//        GetWindow<GraphEditorWindow>("Graph Editor");
//    }

//    private void OnGUI()
//    {
//        // 스크롤뷰 시작
//        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

//        // 배경 그리기
//        DrawBackground();

//        // 노드 그리기
//        DrawNode();

//        // 이벤트 처리
//        HandleEvents(Event.current);

//        // 스크롤뷰 끝
//        EditorGUILayout.EndScrollView();


//    }

//    private void DrawBackground()
//    {
//        // 간단한 격자 배경
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

//        // 노드 드래그 처리
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
//        // 컨텍스트 메뉴 생성
//        GenericMenu menu = new GenericMenu();

//        // 메뉴 항목 추가
//        menu.AddItem(new GUIContent("Add Node"), false, AddNode);
//        menu.AddItem(new GUIContent("Delete Node"), false, DeleteNode);
//        menu.AddSeparator(""); // 구분선 추가
//        menu.AddItem(new GUIContent("Reset Graph"), false, ResetGraph);

//        // 메뉴 표시
//        menu.ShowAsContext();
//    }


//    private void AddNode()
//    {
//        //Debug.Log("Add Node clicked at " + mousePosition);
//        // 노드를 추가하는 로직 작성
//        Rect rect = new Rect();
//        rect = nodeRect;
//        _boxes.Add(rect);

//    }

//    private void DeleteNode()
//    {
//        Debug.Log("Delete Node clicked");
//        // 노드를 삭제하는 로직 작성
//    }

//    private void ResetGraph()
//    {
//        Debug.Log("Reset Graph clicked");
//        // 그래프를 초기화하는 로직 작성
//    }
//}