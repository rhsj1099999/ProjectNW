using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphEditorWithUIToolkit : EditorWindow
{
    [MenuItem("Window/Graph Editor UIToolkit")]
    public static void ShowExample()
    {
        GraphEditorWithUIToolkit wnd = GetWindow<GraphEditorWithUIToolkit>();
        wnd.titleContent = new GUIContent("Graph Editor UIToolkit");
    }

    public void CreateGUI()
    {
        // 노드 컨테이너
        VisualElement container = new VisualElement
        {
            style =
            {
                flexGrow = 1,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f)
            }
        };

        // 노드 추가
        for (int i = 0; i < 3; i++)
        {
            VisualElement node = CreateNode($"Node {i + 1}");
            node.style.left = 100 * (i + 1);
            node.style.top = 100;
            container.Add(node);
        }

        rootVisualElement.Add(container);
    }

    private VisualElement CreateNode(string title)
    {
        // 노드 요소 생성
        VisualElement node = new VisualElement
        {
            style =
            {
                width = 150,
                height = 100,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                borderBottomWidth = 2,
                borderBottomColor = Color.white,
                borderTopWidth = 2,
                borderTopColor = Color.gray
            }
        };

        // 제목 추가
        Label titleLabel = new Label(title)
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter,
                fontSize = 14,
                color = Color.white
            }
        };
        node.Add(titleLabel);

        return node;
    }
}