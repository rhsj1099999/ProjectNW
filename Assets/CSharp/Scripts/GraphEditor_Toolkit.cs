using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
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

        VisualElement rootElementBackGroud = new VisualElement()
        {
            style =
            {
                flexGrow = 1,
                backgroundColor = Color.clear,
            }
        };
        rootVisualElement.Add(rootElementBackGroud);


        for (int i = 0; i < 3; i++)
        {
            StateNode node = new StateNode(rootVisualElement, this);
            rootVisualElement.Add(node);
        }

        //for (int i = 0; i < 1; i++)
        //{
        //    TestNode node = new TestNode(rootVisualElement);
        //    rootVisualElement.Add(node);
        //}
    }

    public void RaycastElement(Vector2 position, MyVisualElement caller)
    {
        VisualElement pickedElement = rootVisualElement.panel.Pick(position);

        MyVisualElement casted = pickedElement as MyVisualElement;

        if (casted == null)
        {
            return;
        }

        casted.DoRayCast(position, caller);
    }
}


public class LinkedStateNodeDesc
{
    public LinkedStateNodeDesc(StateNode target, Arrow_Ready arrow)
    {
        _target = target;
        _arrow = arrow;
    }
    public StateNode _target = null;
    public Arrow_Ready _arrow = null;
}



#region MyVisualElement
public class MyVisualElement : VisualElement
{
    public MyVisualElement(VisualElement root, GraphEditorWithUIToolkit window)
    {
        _root = root;
        _window = window;

        schedule.Execute(() =>
        {
            _actions?.Invoke();

        }).Every(25);
    }

    
    protected VisualElement _root = null;
    protected GraphEditorWithUIToolkit _window = null;
    protected Action _actions = null;

    public virtual void Update() {}
    public virtual void DoRayCast(Vector2 position, MyVisualElement caller) { }


    public void MoveTo_IfArrow(Vector2 position)
    {
        Vector2 localPosition = position - _root.worldBound.position;

        float centerX = style.borderLeftWidth.value + style.borderRightWidth.value;
        float centerY = style.borderBottomWidth.value / 2.0f;
        Vector2 borderCenter = new Vector2(centerX, centerY);

        transform.position = localPosition - borderCenter;
    }

    public void MoveTo(Vector2 position)
    {
        transform.position = ConvertToWindowPosition(position) - new Vector2(style.width.value.value / 2.0f, style.height.value.value / 2.0f);
    }
    public void MoveTo_LT(Vector2 position)
    {
        transform.position = ConvertToWindowPosition(position);
    }
    public void MoveTo_RT(Vector2 position)
    {
        transform.position = ConvertToWindowPosition(position) - new Vector2(style.width.value.value, 0.0f);
    }
    public void MoveTo_LB(Vector2 position)
    {
        transform.position = ConvertToWindowPosition(position) - new Vector2(0.0f, style.height.value.value);
    }
    public void MoveTo_RB(Vector2 position)
    {
        transform.position = ConvertToWindowPosition(position) - new Vector2(style.width.value.value, style.height.value.value);
    }

    public void N_MoveTo(Vector2 position)
    {
        transform.position = (position) - new Vector2(style.width.value.value / 2.0f, style.height.value.value / 2.0f);
    }
    public void N_MoveTo_LT(Vector2 position)
    {
        transform.position = (position);
    }
    public void N_MoveTo_RT(Vector2 position)
    {
        transform.position = (position) - new Vector2(style.width.value.value, 0.0f);
    }
    public void N_MoveTo_LB(Vector2 position)
    {
        transform.position = (position) - new Vector2(0.0f, style.height.value.value);
    }
    public void N_MoveTo_RB(Vector2 position)
    {
        transform.position = (position) - new Vector2(style.width.value.value, style.height.value.value);
    }


    public Vector2 GetPosition_C()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value / 2.0f, style.height.value.value / 2.0f);
        return ConvertToWindowPosition_R(position);
    }
    public Vector2 GetPosition_LT()
    {
        return ConvertToWindowPosition_R(transform.position);
    }
    public Vector2 GetPosition_RT()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value, 0.0f);
        return ConvertToWindowPosition_R(position);
    }
    public Vector2 GetPosition_RB()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value, style.height.value.value);
        return ConvertToWindowPosition_R(position);
    }
    public Vector2 GetPosition_LB()
    {
        Vector2 position = transform.position;
        position += new Vector2(0.0f, style.height.value.value);
        return ConvertToWindowPosition_R(position);
    }

    public Vector2 N_GetPosition_C()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value / 2.0f, style.height.value.value / 2.0f);
        return position;
    }
    public Vector2 N_GetPosition_LT()
    {
        return transform.position;
    }
    public Vector2 N_GetPosition_RT()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value, 0.0f);
        return transform.position;
    }
    public Vector2 N_GetPosition_RB()
    {
        Vector2 position = transform.position;
        position += new Vector2(style.width.value.value, style.height.value.value);
        return position;
    }
    public Vector2 N_GetPosition_LB()
    {
        Vector2 position = transform.position;
        position += new Vector2(0.0f, style.height.value.value);
        return position;
    }


    public Vector2 ConvertToWindowPosition(Vector2 position)
    {
        Vector2 localPosition = position - _root.worldBound.position;
        return localPosition;
    }
    public Vector2 ConverToPanelPosition(Vector2 position)
    {
        Vector2 mousePosition = position;
        Vector2 mousePositionCorrected = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
        mousePositionCorrected = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePositionCorrected);
        return mousePositionCorrected;
    }
    public Vector2 ConverToRootLocalPosition(Vector2 position)
    {
        //Vector2 mousePosition = position;
        //Vector2 mousePositionCorrected = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
        //Vector2 localMousePos = _root.WorldToLocal(mousePositionCorrected);
        Vector2 localMousePos = _root.WorldToLocal(position);
        return localMousePos;
    }

    public Vector2 ConvertToWindowPosition_R(Vector2 position)
    {
        Vector2 localPosition = position + _root.worldBound.position;
        return localPosition;
    }

    public Vector2 ConvertMouspositionToEventPosition(Vector2 position)
    {
        //Rect worldBounds = _root.worldBound;

        //float uiX = position.x - worldBounds.xMin;
        //float uiY = worldBounds.yMax - position.y;

        //return new Vector2(uiX, uiY);

        

        // 1. EditorWindow의 position 정보 가져오기
        Rect windowRect = _window.position;

        // 2. 현재 EditorWindow 내에서의 상대 좌표로 변환
        float xInWindow = position.x - windowRect.x;
        float yInWindow = position.y - windowRect.y;

        // 3. UI Toolkit의 worldBound를 기준으로 좌표 변환
        Rect worldBounds = _root.worldBound;

        // 4. DPI 스케일 적용 (Editor 스케일 영향 보정)
        float scale = EditorGUIUtility.pixelsPerPoint;
        xInWindow /= scale;
        yInWindow /= scale;

        // 5. UI Toolkit의 좌표계로 변환 (Y축 뒤집기)
        float uiX = xInWindow - worldBounds.xMin;
        float uiY = worldBounds.yMax - yInWindow;

        return new Vector2(uiX, uiY);
    }

    public void Register_AbsCallBack(EventCallback<MouseMoveEvent> mouseMoveAction)
    {
        _root.RegisterCallback(mouseMoveAction);
    }
    public void Register_AbsCallBack(EventCallback<PointerMoveEvent> pointerMoveAction)
    {
        _root.RegisterCallback(pointerMoveAction);
    }
    public void Register_AbsCallBack(EventCallback<MouseDownEvent> mouseDownAction)
    {
        _root.RegisterCallback(mouseDownAction);
    }

    public void UnRegister_AbsCallBack(EventCallback<MouseMoveEvent> mouseMoveAction)
    {
        _root.UnregisterCallback(mouseMoveAction);
    }
    public void UnRegister_AbsCallBack(EventCallback<PointerMoveEvent> pointerMoveAction)
    {
        _root.UnregisterCallback(pointerMoveAction);
    }
    public void UnRegister_AbsCallBack(EventCallback<MouseDownEvent> mouseDownAction)
    {
        _root.UnregisterCallback(mouseDownAction);
    }

}
#endregion MyVisualElement





#region TestNode
public class TestNode : MyVisualElement
{
    public enum EventMode
    {
        Event_MouseMove,
        Event_PointerMove,
        Event_Update,
        End,
    }

    public enum UpdateMode
    {
        MoveMode_C,
        MoveMode_LT,
        MoveMode_RT,
        MoveMode_RB,
        MoveMode_LB,
        End,
    }
    public enum PositionMode
    {
        Position_Start,
        Position_Position,
        Position_LocalPosition,
        Position_OriginalPosition,
        Position_End,

        End,
    }

    private void OnKeyDown(KeyDownEvent keyDownEvent)
    {
        switch (keyDownEvent.keyCode)
        {
            case KeyCode.Q:
                {
                    _updateMode = UpdateMode.MoveMode_C;
                }
                break;
            case KeyCode.W:
                {
                    _updateMode = UpdateMode.MoveMode_LT;
                }
                break;
            case KeyCode.E:
                {
                    _updateMode = UpdateMode.MoveMode_RT;
                }
                break;
            case KeyCode.R:
                {
                    _updateMode = UpdateMode.MoveMode_RB;
                }
                break;
            case KeyCode.T:
                {
                    _updateMode = UpdateMode.MoveMode_LB;
                }
                break;




            case KeyCode.Alpha1:
                {
                    if (_evnetMode == EventMode.Event_Update || _evnetMode == EventMode.End)
                    {
                        _positionMode = PositionMode.End;
                        break;
                    }

                    _positionMode = PositionMode.Position_Position;
                }
                break;
            case KeyCode.Alpha2:
                {
                    if (_evnetMode == EventMode.Event_Update || _evnetMode == EventMode.End)
                    {
                        _positionMode = PositionMode.End;
                        break;
                    }

                    _positionMode = PositionMode.Position_LocalPosition;
                }
                break;
            case KeyCode.Alpha3:
                {
                    if (_evnetMode == EventMode.Event_Update || _evnetMode == EventMode.End)
                    {
                        _positionMode = PositionMode.End;
                        break;
                    }

                    _positionMode = PositionMode.Position_OriginalPosition;
                }
                break;



            case KeyCode.P:
                {
                    _actions = null;
                    UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                    UnregisterCallback<PointerMoveEvent>(OnPointerMove);

                    _evnetMode = EventMode.Event_PointerMove;
                    RegisterCallback<PointerMoveEvent>(OnPointerMove);
                }
                break;
            case KeyCode.M:
                {
                    _actions = null;
                    UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                    UnregisterCallback<PointerMoveEvent>(OnPointerMove);

                    _evnetMode = EventMode.Event_MouseMove;
                    RegisterCallback<MouseMoveEvent>(OnMouseMove);
                }
                break;
            case KeyCode.U:
                {
                    _actions = null;
                    UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                    UnregisterCallback<PointerMoveEvent>(OnPointerMove);

                    _evnetMode = EventMode.Event_Update;
                    _actions += Update;
                }
                break;
            case KeyCode.F:
                {
                    _isConvert = !_isConvert;
                }
                break;



            default:
                {

                }
                break;
        }
    }

    public TestNode(VisualElement root, GraphEditorWithUIToolkit window) : base(root, window)
    {
        style.width = 150;
        style.height = 100;
        style.backgroundColor = Color.white;
        focusable = true;

        style.position = Position.Absolute;

        Focus();
        RegisterCallback<KeyDownEvent>(OnKeyDown);


        schedule.Execute(() =>
        {
            _actions?.Invoke();
        }).Every(25);

    }

    private EventMode _evnetMode = EventMode.End;
    private UpdateMode _updateMode = UpdateMode.End;
    private PositionMode _positionMode = PositionMode.End;
    private bool _isConvert = false;

    private void OnPointerMove(PointerMoveEvent pointerMoveEvent)
    {
        if (_positionMode < PositionMode.Position_Start || _positionMode > PositionMode.Position_End)
        {
            return;
        }

        Vector2 targetPosition = new Vector2();

        switch (_positionMode)
        {
            case PositionMode.Position_Position:
                targetPosition = pointerMoveEvent.position;
                break;
            case PositionMode.Position_LocalPosition:
                targetPosition = pointerMoveEvent.localPosition;
                break;
            case PositionMode.Position_OriginalPosition:
                targetPosition = pointerMoveEvent.originalMousePosition;
                break;
        }

        if (_isConvert == true)
        {
            targetPosition = ConvertToWindowPosition(targetPosition);
        }

        switch (_updateMode)
        {
            case UpdateMode.MoveMode_C:
                N_MoveTo(targetPosition);
                break;
            case UpdateMode.MoveMode_LT:
                N_MoveTo_LT(targetPosition);
                break;
            case UpdateMode.MoveMode_RT:
                N_MoveTo_RT(targetPosition);
                break;
            case UpdateMode.MoveMode_RB:
                N_MoveTo_RB(targetPosition);
                break;
            case UpdateMode.MoveMode_LB:
                N_MoveTo_RB(targetPosition);
                break;
        }
    }
    public void OnMouseMove(MouseMoveEvent mouseEvent)
    {
        if (_positionMode < PositionMode.Position_Start || _positionMode > PositionMode.Position_End)
        {
            return;
        }

        Vector2 targetPosition = new Vector2();

        switch (_positionMode)
        {
            case PositionMode.Position_Position:
                targetPosition = mouseEvent.mousePosition;
                break;
            case PositionMode.Position_LocalPosition:
                targetPosition = mouseEvent.localMousePosition;
                break;
            case PositionMode.Position_OriginalPosition:
                targetPosition = mouseEvent.originalMousePosition;
                break;
        }

        if (_isConvert == true)
        {
            targetPosition = ConvertToWindowPosition(targetPosition);
        }

        switch (_updateMode)
        {
            case UpdateMode.MoveMode_C:
                N_MoveTo(targetPosition);
                break;
            case UpdateMode.MoveMode_LT:
                N_MoveTo_LT(targetPosition);
                break;
            case UpdateMode.MoveMode_RT:
                N_MoveTo_RT(targetPosition);
                break;
            case UpdateMode.MoveMode_RB:
                N_MoveTo_RB(targetPosition);
                break;
            case UpdateMode.MoveMode_LB:
                N_MoveTo_LB(targetPosition);
                break;
        }
    }
    public override void Update()
    {
        Vector2 targetPosition = Mouse.current.position.value;


        

        if (_isConvert == true)
        {
            //targetPosition = ConvertToWindowPosition(targetPosition);
            //targetPosition = ConverToPanelPosition(targetPosition);
            targetPosition = ConverToRootLocalPosition(targetPosition);
        }

        switch (_updateMode)
        {
            case UpdateMode.MoveMode_C:
                N_MoveTo(targetPosition);
                break;
            case UpdateMode.MoveMode_LT:
                N_MoveTo_LT(targetPosition);
                break;
            case UpdateMode.MoveMode_RT:
                N_MoveTo_RT(targetPosition);
                break;
            case UpdateMode.MoveMode_RB:
                N_MoveTo_RB(targetPosition);
                break;
            case UpdateMode.MoveMode_LB:
                N_MoveTo_LB(targetPosition);
                break;
        }
    }
}
#endregion TestNode




#region StateNode
public class StateNode : MyVisualElement
{
    public StateNode(VisualElement root, GraphEditorWithUIToolkit window) : base(root, window)
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


        Resizer resizer_Left = new Resizer(_root, _window, this, _id, Direction_2D.Left, border);
        Resizer resizer_LeftTop = new Resizer(_root, _window, this, _id, Direction_2D.LeftTop, border);
        Resizer resizer_Top = new Resizer(_root, _window, this, _id, Direction_2D.Top, border);
        Resizer resizer_TopRight = new Resizer(_root, _window, this, _id, Direction_2D.TopRight, border);
        Resizer resizer_Right = new Resizer(_root, _window, this, _id, Direction_2D.Right, border);
        Resizer resizer_RightBottom = new Resizer(_root, _window, this, _id, Direction_2D.RightBottom, border);
        Resizer resizer_Bottom = new Resizer(_root, _window, this, _id, Direction_2D.Bottom, border);
        Resizer resizer_BottomLeft = new Resizer(_root, _window, this, _id, Direction_2D.BottomLeft, border);


        Mover mover = new Mover(_root, _window, this, _id);


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


        RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }


    private void OnDetach(DetachFromPanelEvent detachEvent)
    {
        if (_rayCastReady == true)
        {
            UnRegister_AbsCallBack(OnMouseDown_RayCast);
        }
    }


    public static int _id = 0;

    private HashSet<LinkedStateNodeDesc> _toStateNodes = new HashSet<LinkedStateNodeDesc>();
    private HashSet<LinkedStateNodeDesc> _fromStateNodes = new HashSet<LinkedStateNodeDesc>();

    public ObjectField _objectField = null;
    private Arrow_NotReady _arrow_NotReady = null;
    private bool _rayCastReady = false;

    public override void DoRayCast(Vector2 position, MyVisualElement caller)
    {
        StateNode stateNode = caller as StateNode;

        if (stateNode == null)
        {
            return;
        }

        LinkingNode_From(stateNode);
        stateNode.LinkingNode_To(this);
    }


    void LinkNode(StateNode startNode, Vector3 mousePosition)
    {
        if (_arrow_NotReady != null)
        {
            Debug.Assert(false, "아까 링킹중이엿나요? 작업이 완료되지 않았나요? null이 아니네요?");
            Debug.Break();
        }

        _arrow_NotReady = new Arrow_NotReady(_root, _window, startNode, mousePosition);
        _rayCastReady = true;
        Register_AbsCallBack(OnMouseDown_RayCast);
    }


    private void OnMouseDown_RayCast(MouseDownEvent mouseDown)
    {
        _window.RaycastElement(mouseDown.mousePosition, this);
        _rayCastReady = false;
        UnRegister_AbsCallBack(OnMouseDown_RayCast);

        _root.Remove(_arrow_NotReady);
        _arrow_NotReady = null;
    }

    public void LinkingNode_From(StateNode from)
    {
        Arrow_Ready newReadyArrow = new Arrow_Ready(_root, _window, from, this);
        _root.Insert(0, newReadyArrow);
        LinkedStateNodeDesc newLinked = new LinkedStateNodeDesc(from, newReadyArrow);

        _fromStateNodes.Add(newLinked);
    }

    public void LinkingNode_To(StateNode to)
    {
        Arrow_Ready newReadyArrow = new Arrow_Ready(_root, _window, to, this);
        _root.Insert(0, newReadyArrow);
        LinkedStateNodeDesc newLinked = new LinkedStateNodeDesc(to, newReadyArrow);

        _toStateNodes.Add(newLinked);
    }

    void Menu_Somthing1() {}
    void Menu_Somthing2() {}
}
#endregion StateNode




#region Arrow_Head
public class Arrow_Head : MyVisualElement
{
    public Arrow_Head(VisualElement root, GraphEditorWithUIToolkit window, Vector3 mousePosition) : base(root, window)
    {
        style.width = 0;
        style.height = 0;
        style.position = Position.Absolute; // 원하는 위치 설정

        /*----------------------------------------------------
        |TODO| 삼각형 회전이 이상해요
        ----------------------------------------------------*/

        style.borderLeftWidth = 10;  // 왼쪽 삼각형
        style.borderRightWidth = 10; // 오른쪽 삼각형
        style.borderBottomWidth = 20; // 아래 삼각형 (화살표 모양)

        style.borderLeftColor = Color.clear;
        style.borderRightColor = Color.clear;
        style.borderBottomColor = Color.green; // 화살표 색
    }
}
#endregion Arrow_Head


/*--------------------------------------------------
|NOTI| 시작점에서 드래그 하는중인, 준비되지 않은 화살표
--------------------------------------------------*/
#region Arrow_NotReady
public class Arrow_NotReady : MyVisualElement
{
    public Arrow_NotReady(VisualElement root, GraphEditorWithUIToolkit window, StateNode startNode, Vector3 mousePosition) : base(root, window)
    {
        _startNode = startNode;
        style.position = Position.Absolute;
        style.backgroundColor = Color.green;
        style.height = 2.0f;

        Register_AbsCallBack(OnMouseMove);
        _root.Insert(0, this);

        ////중간에 꽃힐 화살표 비쥬얼 엘리먼트 생성
        {
            _arrowHead = new Arrow_Head(_root, _window, Vector2.zero);
            _root.Insert(0, _arrowHead);
        }

        RegisterCallback<DetachFromPanelEvent>(OnDetached);
    }

    private void OnDetached(DetachFromPanelEvent detachEvent)
    {
        UnRegister_AbsCallBack(OnMouseMove);
        _root.Remove(_arrowHead);
    }


    private Arrow_Head _arrowHead = null;
    private StateNode _startNode = null;

    private void OnMouseMove(MouseMoveEvent mouseMove)
    {
        Vector2 startNodeCenterPositionConverted = _startNode.GetPosition_C();
        Vector2 mousePositionConverted = mouseMove.mousePosition;

        style.width = Vector3.Distance(startNodeCenterPositionConverted, mousePositionConverted);

        Vector2 mouseAndStateNodeCenter = (mousePositionConverted + startNodeCenterPositionConverted) / 2.0f;

        Vector2 direction = mousePositionConverted - startNodeCenterPositionConverted;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //화살표 몸통이 업데이트 된다
        {
            MoveTo(mouseAndStateNodeCenter);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        //화살표 중간 삼각형이 업데이트 된다
        {
            _arrowHead.MoveTo_IfArrow(mouseAndStateNodeCenter);
            _arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }




    /*----------------------------------------------------------------------------------------------
    |NOTI| Mouse.current.position의 값과 Event의 position값은 다르다. 이유를 찾고 고치면 더 쉽게 작업가능
    ----------------------------------------------------------------------------------------------*/
    private void UpdateArrowPosition_Pointer(PointerMoveEvent pointerMove)
    {
        Vector2 startNodeCenterPositionConverted = _startNode.GetPosition_C();
        Vector2 mousePositionConverted = pointerMove.position;

        float length = Vector3.Distance(startNodeCenterPositionConverted, mousePositionConverted);
        style.width = length;


        Vector2 mouseAndStateNodeCenter = (mousePositionConverted + startNodeCenterPositionConverted) / 2.0f;

        MoveTo(mouseAndStateNodeCenter);

        Vector2 direction = mousePositionConverted - startNodeCenterPositionConverted;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }  //-------------------
    public override void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        Vector2 startNodeCenterPositionConverted = _startNode.GetPosition_C();
        Vector2 mousePositionConverted = ConvertMouspositionToEventPosition(Mouse.current.position.value);
        

        

        float length = Vector3.Distance(startNodeCenterPositionConverted, mousePositionConverted);
        style.width = length + 5.0f;


        Vector2 mouseAndStateNodeCenter = (mousePositionConverted + startNodeCenterPositionConverted) / 2.0f;

        MoveTo(mouseAndStateNodeCenter);

        Vector2 direction = mousePositionConverted - startNodeCenterPositionConverted;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }  //------------------------------------------------------------
    //----------------------------------------------------------------------------------------------
}
#endregion Arrow_NotReady


/*--------------------------------------------------
|NOTI| 드래그 완료된, 준비된 화살표
--------------------------------------------------*/
#region Arrow_Ready
public class Arrow_Ready : MyVisualElement
{
    public Arrow_Ready(VisualElement root, GraphEditorWithUIToolkit window, StateNode fromNode, StateNode toNode) : base(root, window)
    {
        //style 설정
        {
            style.backgroundColor = Color.green;
        }

        _fromNode = fromNode;
        _toNode = toNode;

        _actions += ArrowPositionUpdate;
    }

    private StateNode _fromNode = null;
    private StateNode _toNode = null;
    private Arrow_Head _arrow_Head = null;


    /*----------------------------------------------------
    |TODO| 매 프레임마다 움직이지 않아도 이걸 업데이트 하네요?
    전에 만들어둔 노드 위치가 변경될때만 하는게 좋을거같아요.
    ----------------------------------------------------*/
    private void ArrowPositionUpdate()
    {

    }

    public void ReadyToDelete()
    {
        _actions = null;
    }
}
#endregion Arrow_Ready



#region ChildElement
public class ChildElement : MyVisualElement
{
    protected ChildElement(VisualElement root, GraphEditorWithUIToolkit window, VisualElement parent, int id) : base(root, window)
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
    public Mover(VisualElement root, GraphEditorWithUIToolkit window, VisualElement parent, int id) : base(root, window, parent, id)
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

    public Resizer(VisualElement root, GraphEditorWithUIToolkit window, VisualElement parent, int id, Direction_2D myDirection, int myHeight) : base(root, window, parent, id)
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
