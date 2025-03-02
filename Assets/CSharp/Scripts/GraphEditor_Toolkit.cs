using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.FilterWindow;

public enum Direction_2D
{
    Left,
    LeftTop,
    Top,
    TopRight,
    Right,
    RightBottom,
    Bottom,
    BottomLeft,
    End
}




/*
VisualElement arrow = new VisualElement();
arrow.style.width = 0;
arrow.style.height = 0;
arrow.style.borderLeftWidth = 10;  // 왼쪽 삼각형
arrow.style.borderRightWidth = 10; // 오른쪽 삼각형
arrow.style.borderBottomWidth = 20; // 아래 삼각형 (화살표 모양)
arrow.style.borderLeftColor = Color.clear;
arrow.style.borderRightColor = Color.clear;
arrow.style.borderBottomColor = Color.black; // 화살표 색
arrow.style.position = Position.Absolute; // 원하는 위치 설정

rootVisualElement.Add(arrow);
*/


/*
//schedule.Execute(() =>
//{

//}).Every(50);
*/


public class GraphEditorWithUIToolkit : EditorWindow
{
    [MenuItem("Window/Graph Editor UIToolkit")]
    public static void ShowWindow()
    {
        GraphEditorWithUIToolkit window = GetWindow<GraphEditorWithUIToolkit>();
        window.titleContent = new GUIContent("Graph Editor UIToolkit");
    }

    public void CreateGUI()
    {
        Label label = new Label("StateGraphTool");
        rootVisualElement.Add(label);

        for (int i = 0; i < 3; i++)
        {
            StateNode node = new StateNode(rootVisualElement);
            rootVisualElement.Add(node);
        }
    }
}












#region StateNode
public class StateNode : MyVisualElement
{
    public StateNode(VisualElement root) : base(root)
    {
        pickingMode = PickingMode.Ignore;

        style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

        style.width = 150;
        style.height = 100;

        /*------------------------------------------------------
        |NOTI| 이게 Rel 값이면 하나가 변경될때 다른것도 변경되네요??
        ------------------------------------------------------*/
        style.position = Position.Absolute; //------------------
        //------------------------------------------------------




        int border = 4;

        style.borderLeftWidth = border;
        style.borderTopWidth = border;
        style.borderRightWidth = border;
        style.borderBottomWidth = border;


        Resizer resizer_Left = new Resizer(_root, this, _id, Direction_2D.Left, border);
        Resizer resizer_LeftTop = new Resizer(_root, this, _id, Direction_2D.LeftTop, border);
        Resizer resizer_Top = new Resizer(_root, this, _id, Direction_2D.Top, border);
        Resizer resizer_TopRight = new Resizer(_root, this, _id, Direction_2D.TopRight, border);
        Resizer resizer_Right = new Resizer(_root, this, _id, Direction_2D.Right, border);
        Resizer resizer_RightBottom = new Resizer(_root, this, _id, Direction_2D.RightBottom, border);
        Resizer resizer_Bottom = new Resizer(_root, this, _id, Direction_2D.Bottom, border);
        Resizer resizer_BottomLeft = new Resizer(_root, this, _id, Direction_2D.BottomLeft, border);


        Mover mover = new Mover(_root, this, _id);


        VisualElement contentBox = new VisualElement()
        {
            style =
            {
                flexGrow = 1,
            }
        };
        contentBox.pickingMode = PickingMode.Ignore;
        Add(contentBox);

        contentBox.AddManipulator(new ContextualMenuManipulator(menuEvent =>
        {
            menuEvent.menu.AppendAction("Link Node", _ => LinkNode(this, menuEvent.mousePosition));
            menuEvent.menu.AppendAction("Something Menu1", _ => Menu_Somthing1());
            menuEvent.menu.AppendAction("Something Menu2", _ => Menu_Somthing2());
        }));






        VisualElement objectFieldSet = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                backgroundColor = Color.black,
                flexShrink = 1,
            }
        };
        contentBox.Add(objectFieldSet);

        Label objectFieldLabel = new Label("StateAsset :");
        objectFieldSet.Add(objectFieldLabel);

        _objectField = new ObjectField()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexShrink = 1,
            }
        };
        objectFieldSet.Add(_objectField);

        _id++;
    }


    public static int _id = 0;

    public ObjectField _objectField = null;

    private Arrow_NotReady _arrow_NotReady = null;

    void LinkNode(StateNode startNode, Vector3 mousePosition)
    {
        if (_arrow_NotReady != null)
        {
            Debug.Assert(false, "아까 링킹중이엿나요? 작업이 완료되지 않았나요? null이 아니네요?");
            Debug.Break();
        }

        _arrow_NotReady = new Arrow_NotReady(_root, startNode, mousePosition);
    }

    void Menu_Somthing1() {}
    void Menu_Somthing2() {}
}
#endregion StateNode

#region MyVisualElement
public class MyVisualElement : VisualElement
{
    public MyVisualElement(VisualElement root)
    {
        _root = root;
    }

    protected VisualElement _root = null;
}
#endregion MyVisualElement


/*--------------------------------------------------
|NOTI| 시작점에서 드래그 하는중인, 준비되지 않은 화살표
--------------------------------------------------*/
#region Arrow_NotReady
public class Arrow_NotReady : MyVisualElement
{
    public class Arrow_Body_NotReady : MyVisualElement
    {
        public Arrow_Body_NotReady(VisualElement root, StateNode startNode, Arrow_NotReady arrow_NotReady, Vector3 mousePosition) : base(root)
        {
            _startNode = startNode;

            style.width = 15;
            style.height = 45;

            style.borderBottomColor = Color.white;
            style.position = Position.Absolute;

            RegisterCallback<PointerMoveEvent>(UpdateArrowBodySize);
            arrow_NotReady.Add(this);
        }

        private StateNode _startNode = null;
        private List<Action> _actions = new List<Action>();

        private void UpdateArrowBodySize(PointerMoveEvent pointerMoveEvent)
        {
            Debug.Log("이벤트 실행중");

            Vector2 worldBoundPosition = (_startNode.worldBound.position + new Vector2(pointerMoveEvent.position.x, pointerMoveEvent.position.y)) / 2.0f;
            style.left = worldBoundPosition.x;
            style.right = worldBoundPosition.y;

            //Vector2 direction = pointerMoveEvent.position - _startNode.worldBound.position;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //worldBound.rotation = Quaternion.Euler(0, 0, angle);
        }


    }

    public Arrow_NotReady(VisualElement root, StateNode startNode, Vector3 mousePosition) : base(root)
    {
        _startNode = startNode;
        style.position = Position.Absolute;
        style.backgroundColor = Color.green;

        Vector3 debug_StartNodePosition = _startNode.worldBound.position;
        Vector3 debug_MousePosition = mousePosition;

        float length = 0.0f;
        {
            length = Vector3.Distance(_startNode.worldBound.position, mousePosition);

            Vector2 worldBoundPosition = (_startNode.worldBound.position + new Vector2(mousePosition.x, mousePosition.y)) / 2.0f;
            style.left = worldBoundPosition.x;
            style.right = worldBoundPosition.y;

            //worldBound.position = (_startNode.worldBound.position + mousePosition) / 2.0f;

            //Vector2 direction = mousePosition - _startNode.worldBound.position;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //worldBound.rotation = Quaternion.Euler(0, 0, angle);
        }

        style.width = length / 2.0f;
        style.height = 20.0f;

        //style.width = 30.0f;
        //style.height = 30.0f;


        //_arrowBody_NotReady = new Arrow_Body_NotReady(_root, startNode, this, mousePosition);
        //this.CaptureMouse();
        RegisterCallback<MouseMoveEvent>(UpdateArrowBodySize);
        _root.Add(this);
    }


    private Arrow_Body_NotReady _arrowBody_NotReady = null;
    private StateNode _startNode = null;
    private Vector3 _currEndPosition = Vector3.zero;
    private List<Action> _actions = new List<Action>();

    private void UpdateArrowBodySize(MouseMoveEvent mouseMove)
    {
        float length = Vector3.Distance(_startNode.worldBound.position, mouseMove.mousePosition);

        Vector3 de_mouse = mouseMove.mousePosition;
        Vector3 de_startNode = _startNode.worldBound.position;

        

        Vector2 worldBoundPosition = (_startNode.worldBound.position + mouseMove.mousePosition) / 2.0f;
        transform.position = worldBoundPosition;


        //worldBound.position = (_startNode.worldBound.position + new Vector3(pointerMoveEvent.originalMousePosition.x, pointerMoveEvent.originalMousePosition.y, 0.0f)) / 2.0f;

        Vector2 direction = mouseMove.mousePosition - _startNode.worldBound.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        style.width = length / 2.0f;
        style.height = 20.0f;


        //style.width = 30.0f;
        //style.height = 30.0f;

    }

}
#endregion Arrow_NotReady


/*--------------------------------------------------
|NOTI| 드래그 완료된, 준비된 화살표
--------------------------------------------------*/
#region Arrow
public class Arrow : MyVisualElement
{
    public Arrow(VisualElement root, StateNode startNode, StateNode endNode) : base(root)
    {
        _startNode = startNode;
        _endNode = endNode;
    }

    private StateNode _startNode = null;
    private StateNode _endNode = null;
}
#endregion  Arrow



#region ChildElement
public class ChildElement : MyVisualElement
{
    protected ChildElement(VisualElement root, VisualElement parent, int id) : base(root)
    {
        _parent = parent;
        _id = id;

        style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        style.position = Position.Absolute; //style.flexGrow = 1;
    }

    protected VisualElement _parent = null;
    protected int _id = 0;
}
#endregion ChildElement




#region Mover
public class Mover : ChildElement
{
    public Mover(VisualElement root, VisualElement parent, int id) : base(root, parent, id)
    {
        style.left = 2;
        style.top = 2;
        style.right = 2;
        style.bottom = 2;
        style.backgroundColor = Color.white;



        RegisterCallback<MouseDownEvent>(evt =>
        {
            _isDragging = true;
            this.CaptureMouse();
            AfterMouseDown();
        });


        RegisterCallback<MouseUpEvent>(evt =>
        {
            _isDragging = false;
            this.ReleaseMouse();
            AfterMouseUp();
        });

        _parent.Add(this);
    }

    private bool _isDragging = false;

    private void AfterMouseDown()
    {
        RegisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    private void OnMouseMove(MouseMoveEvent moveEvent)
    {
        if (_isDragging == false)
        {
            return;
        }

        _parent.transform.position += new Vector3(moveEvent.mouseDelta.x, moveEvent.mouseDelta.y, 0.0f);
    }

    private void AfterMouseUp()
    {
        UnregisterCallback<MouseMoveEvent>(OnMouseMove);
    }
}
#endregion Mover



#region Resizer
public class Resizer : ChildElement
{
    private bool _isResizing = false;

    private Direction_2D _myDirection = Direction_2D.End;
    private int _myHeight = 0;

    public Resizer(VisualElement root, VisualElement parent, int id, Direction_2D myDirection, int myHeight) : base(root, parent, id)
    {
        _myDirection = myDirection;
        _myHeight = myHeight;

        switch (_myDirection)
        {
            case Direction_2D.Left:
                {
                    style.backgroundColor = Color.green;

                    style.width = _myHeight;
                    style.flexDirection = FlexDirection.Column;

                    style.left = -_myHeight;

                    style.top = _myHeight / 2;
                    style.bottom = _myHeight / 2;
                }
                break;
            case Direction_2D.LeftTop:
                {
                    style.backgroundColor = Color.red;

                    style.width = _myHeight;
                    style.height = _myHeight;


                    style.left = -_myHeight;
                    style.top = -_myHeight;
                }
                break;
            case Direction_2D.Top:
                {
                    style.backgroundColor = Color.green;

                    style.height = _myHeight;
                    style.flexDirection = FlexDirection.Row;

                    style.top = -_myHeight;

                    style.left = _myHeight / 2;
                    style.right = _myHeight / 2;
                }
                break;
            case Direction_2D.TopRight:
                {
                    style.backgroundColor = Color.red;

                    style.width = _myHeight;
                    style.height = _myHeight;


                    style.right = -_myHeight;
                    style.top = -_myHeight;
                }
                break;
            case Direction_2D.Right:
                {
                    style.backgroundColor = Color.green;

                    style.width = _myHeight;
                    style.flexDirection = FlexDirection.Column;

                    style.right = -_myHeight;

                    style.top = _myHeight / 2;
                    style.bottom = _myHeight / 2;
                }
                break;
            case Direction_2D.RightBottom:
                {
                    style.backgroundColor = Color.red;

                    style.width = _myHeight;
                    style.height = _myHeight;


                    style.right = -_myHeight;
                    style.bottom = -_myHeight;
                }
                break;
            case Direction_2D.Bottom:
                {
                    style.backgroundColor = Color.green;

                    style.height = _myHeight;
                    style.flexDirection = FlexDirection.Row;

                    style.bottom = -_myHeight;

                    style.left = _myHeight / 2;
                    style.right = _myHeight / 2;
                }
                break;
            case Direction_2D.BottomLeft:
                {
                    style.backgroundColor = Color.red;

                    style.width = _myHeight;
                    style.height = _myHeight;


                    style.left = -_myHeight;
                    style.bottom = -_myHeight;
                }
                break;
            case Direction_2D.End:
                {
                    Debug.Assert(false, "대응이 되지 않는다");
                    Debug.Break();
                }
                break;
            default:
                break;
        }


        {
            /*-------------------------------------------------
            |TODO| 커서 어떻게 바꾸냐고1!!!
            -------------------------------------------------*/
            //resizeHandle.style.cursor =
            //UnityEngine.UIElements.Cursor cursor2 = new UnityEngine.UIElements.Cursor();
            //{ defaultCursorId = (int)MouseCursor.ResizeHorizontal };
        }

        RegisterCallback<MouseDownEvent>(evt =>
        {
            _isResizing = true;
            this.CaptureMouse();
            AfterMouseDown();
        });



        RegisterCallback<MouseUpEvent>(evt =>
        {
            _isResizing = false;
            this.ReleaseMouse();
            AfterMouseUp();
        });

        _parent.Add(this);
    }


    private void AfterMouseDown()
    {
        RegisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    private void AfterMouseUp() 
    {
        UnregisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    private void OnMouseMove(MouseMoveEvent mouseMoveEvent)
    {
        if (_isResizing == false)
        {
            Debug.Log("크기가 변경이 호출됐지만 씹혔다" + _id);
            return;
        }

        Debug.Log("크기가 변경됐다" + _id);

        if (_myDirection == Direction_2D.Left || _myDirection == Direction_2D.Right)
        {
            float mouseDelta = (_myDirection == Direction_2D.Left)
            ? mouseMoveEvent.mouseDelta.x
            : -mouseMoveEvent.mouseDelta.x;

            _parent.style.width = _parent.style.width.value.value + Mathf.Abs(mouseDelta);
            Vector2 nextposition = _parent.transform.position + new Vector3(mouseDelta / 2.0f, 0.0f, 0.0f);
            _parent.transform.position = nextposition;
        }

        else if (_myDirection == Direction_2D.Top || _myDirection == Direction_2D.Bottom)
        {
            float mouseDelta = (_myDirection == Direction_2D.Top)
            ? mouseMoveEvent.mouseDelta.y
            : -mouseMoveEvent.mouseDelta.y;

            _parent.style.height = _parent.style.height.value.value + Mathf.Abs(mouseDelta);
            Vector2 nextposition = _parent.transform.position + new Vector3(0.0f, mouseDelta / 2.0f, 0.0f);
            _parent.transform.position = nextposition;
        }
    }
}
#endregion Resizer
