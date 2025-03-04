using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static GraphEditorWithUIToolkit.GraphEditorBackGroundElement;

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

public class HierarchycalWindow : EditorWindow
{
    public enum Layer
    {
        Layer_BackGround,

        Layer_Start,
        Layer_Object_One,
        Layer_Object_Two,
        Layer_Object_Three,
        Layer_Object_Four,
        Layer_End,


        Layer_One,
        Layer_Two,
        Layer_Three,
        Layer_Four,

        End = 11,
    }

    public void AddElement(Layer layer, VisualElement element)
    {
        rootVisualElement.Insert((int)layer, element);
    }
}




public class GraphEditorWithUIToolkit : HierarchycalWindow
{
    [MenuItem("Window/Graph Editor UIToolkit")]
    public static void ShowWindow()
    {
        GraphEditorWithUIToolkit window = GetWindow<GraphEditorWithUIToolkit>();
        window.titleContent = new GUIContent("Graph Editor UIToolkit");
    }


    public class GraphEditorBackGroundElement : MyVisualElement
    {
        public class IOWindowBase : HierarchycalWindow
        {
            public IOWindowBase()
            {
                minSize = new Vector2(500.0f, 300.0f);
                maxSize = new Vector2(500.0f, 300.0f);
            }
        }

        public class SavingWindow : IOWindowBase
        {
            public static void OpenWindow()
            {
                SavingWindow window = GetWindow<SavingWindow>();
                window.titleContent = new GUIContent("Save...");
            }

            private void CreateGUI()
            {
                //비쥬얼 컨테이너 묶음 생성
                {
                    VisualElement backGround = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                        }
                    };
                    rootVisualElement.Add(backGround);


                    VisualElement saveUIContainer = new VisualElement()
                    {
                        style =
                        {
                            borderLeftWidth = 2.0f,
                            borderTopWidth = 2.0f,
                            borderRightWidth = 2.0f,
                            borderBottomWidth = 2.0f,

                            borderLeftColor = Color.gray,
                            borderTopColor = Color.gray,
                            borderRightColor = Color.gray,
                            borderBottomColor = Color.gray,

                            flexGrow = 1,
                            marginLeft = 30.0f,
                            marginRight = 30.0f,
                            marginBottom = 30.0f,
                            marginTop = 30.0f,
                        }
                    };
                    backGround.Add(saveUIContainer);


                    VisualElement nameContaioner = new VisualElement()
                    {
                        style = 
                        {
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    saveUIContainer.Add(nameContaioner);

                    Label nameLabel = new Label("File Name : ");
                    nameContaioner.Add(nameLabel);

                    TextField nameInputField = new TextField()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            flexShrink = 1,
                        }
                    };
                    nameInputField.value = _basicName;
                    nameContaioner.Add(nameInputField);

                    Action saveButtonClicked = () => { };

                    Button saveButton = new Button(saveButtonClicked)
                    {
                        text = "Save",
                        style =
                        {
                            flexGrow = 1,
                        }
                    };
                    saveUIContainer.Add(saveButton);
                }
            }

            private string _basicName = "StateGraph_";
        }

        public class LoadingWindow : IOWindowBase
        {
            public static void OpenWindow()
            {
                LoadingWindow window = GetWindow<LoadingWindow>();
                window.titleContent = new GUIContent("Load...");
            }

            private void CreateGUI()
            {
                //비쥬얼 컨테이너 묶음 생성
                {
                    VisualElement backGround = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                        }
                    };
                    rootVisualElement.Add(backGround);


                    VisualElement loadUIContainer = new VisualElement()
                    {
                        style =
                        {
                            borderLeftWidth = 2.0f,
                            borderTopWidth = 2.0f,
                            borderRightWidth = 2.0f,
                            borderBottomWidth = 2.0f,

                            borderLeftColor = Color.gray,
                            borderTopColor = Color.gray,
                            borderRightColor = Color.gray,
                            borderBottomColor = Color.gray,

                            flexGrow = 1,

                            marginLeft = 30.0f,
                            marginRight = 30.0f,
                            marginBottom = 30.0f,
                            marginTop = 30.0f,
                        }
                    };
                    backGround.Add(loadUIContainer);


                    _objectField = new ObjectField()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                        },
                        objectType = typeof(StateAsset),
                        allowSceneObjects = false,
                    };

                    loadUIContainer.Add(_objectField);

                    Action loadButtonClicked = () => { };

                    Button loadButton = new Button(loadButtonClicked)
                    {
                        text = "Load",
                        style =
                        {
                            flexGrow = 1,
                        }
                    };
                    loadUIContainer.Add(loadButton);
                }
            }

            private ObjectField _objectField = null;
        }

        public GraphEditorBackGroundElement(VisualElement root, GraphEditorWithUIToolkit window) : base(root, window)
        {
            style.flexGrow = 1;
            style.backgroundColor = Color.clear;

            this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
            {
                menuEvent.menu.AppendAction("Create StateNode", _ => CreateStateNode(menuEvent.originalMousePosition));
                menuEvent.menu.AppendAction("Save_StateGraph", _ => SaveStateGraph());
                menuEvent.menu.AppendAction("Load_StateGraph", _ => LoadStateGraph());
            }));

            RegisterCallback<MouseDownEvent>(OnMouseDown);

            window.AddElement(Layer.Layer_BackGround, this);
        }

        private Vector2 _lastMousePosition = Vector2.zero;

        private void OnMouseDown(MouseDownEvent mouseDownEvent)
        {
            _lastMousePosition = mouseDownEvent.mousePosition;
        }

        private void CreateStateNode(Vector2 mousePosition)
        {
            StateNode node = new StateNode(_root, _window, _lastMousePosition);
            _window.AddElement(Layer.Layer_Object_One, node);
        }
        private void SaveStateGraph() 
        {
            SavingWindow.OpenWindow();
        }
        private void LoadStateGraph() 
        {
            LoadingWindow.OpenWindow();
        }
    }

    public void CreateGUI()
    {
        Label label = new Label("StateGraphTool");
        AddElement(Layer.Layer_BackGround, label);

        new GraphEditorBackGroundElement(rootVisualElement, this);

        for (int i = 0; i < 3; i++)
        {
            StateNode node = new StateNode(rootVisualElement, this, Vector2.zero);
            AddElement(Layer.Layer_Object_One, node);
        }
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

    //protected Dictionary<Type, List<EventCallback<EventBase>>> _safeStops = new Dictionary<Type, List<EventCallback<EventBase>>>();
    //protected void AddSafeStop(Type eventType, EventCallback<EventBase> func)
    //{
    //    List<EventCallback<EventBase>> targetList = _safeStops.GetOrAdd(eventType);
    //    targetList.Add(func);
    //}


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


    public Vector2 GetPosition_RotatedOffset_C(float radius, float angle)
    {
        Vector2 offset = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        return GetPosition_C() + offset;
    }
    public Vector2 GetPosition_RotatedOffset_LT(float radius, float angle)
    {
        return GetPosition_LT() + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }
    public Vector2 GetPosition_RotatedOffset_RT(float radius, float angle)
    {
        return GetPosition_RT() + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }
    public Vector2 GetPosition_RotatedOffset_RB(float radius, float angle)
    {
        return GetPosition_RB() + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }
    public Vector2 GetPosition_RotatedOffset_LB(float radius, float angle)
    {
        return GetPosition_LB() + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
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





#region Move_Extension
public static class MoveExtensions
{
    public static void Default_OnMouseDown_MoveEx(this VisualElement moveable, ref bool trigger) {trigger = true;}
    public static void Default_OnMouseUp_MoveEx(this VisualElement moveable, ref bool trigger) {trigger = false;}
    public static void Default_OnMouseMove_MoveEx(this VisualElement moveable, MouseMoveEvent mouseMoveEvent, ref bool trigger)
    {
        if (trigger == false)
        {
            return;
        }

        moveable.transform.position += new Vector3(mouseMoveEvent.mouseDelta.x, mouseMoveEvent.mouseDelta.y, 0.0f);
    }
}
#endregion Move_Extension






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
    public StateNode(VisualElement root, GraphEditorWithUIToolkit window, Vector3 position) : base(root, window)
    {
        //style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.backgroundColor = Color.white;

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

        this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
        {
            menuEvent.menu.AppendAction("Link Node", _ => LinkNode(this, menuEvent.mousePosition));
            menuEvent.menu.AppendAction("Delete Node", _ => DeleteMe());
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
        Add(objectFieldSet);

        _objectField = new ObjectField()
        {
            objectType = typeof(StateAsset),
            allowSceneObjects = false,
            style =
            {
                flexDirection = FlexDirection.Row,
                flexShrink = 1,
            }
        };
        
        objectFieldSet.Add(_objectField);



        RegisterCallback<MouseDownEvent>(OnMouseDown_Move);
        RegisterCallback<MouseUpEvent>(OnMouseUp_Move);
        RegisterCallback<DetachFromPanelEvent>(OnDetach);

        MoveTo(position);
        _id++;
    }





    public static int _id = 0;

    private Dictionary<StateNode, Arrow_Ready> _toStateNodes = new Dictionary<StateNode, Arrow_Ready>();
    private Dictionary<StateNode, Arrow_Ready> _fromStateNodes = new Dictionary<StateNode, Arrow_Ready>();

    public ObjectField _objectField = null;
    private Arrow_NotReady _arrow_NotReady = null;
    
    private bool _rayCastReady = false;


    private bool _trigger_move = false;

    private void OnMouseDown_RayCast(MouseDownEvent mouseDown)
    {
        _window.RaycastElement(mouseDown.mousePosition, this);
        _rayCastReady = false;
        UnRegister_AbsCallBack(OnMouseDown_RayCast);

        _root.Remove(_arrow_NotReady);
        _arrow_NotReady = null;
    }

    private void OnMouseDown_Move(MouseDownEvent mouseDownEvent)
    {
        this.Default_OnMouseDown_MoveEx(ref _trigger_move);
        RegisterCallback<MouseMoveEvent>(OnMouseMove_Move);
    }

    private void OnMouseMove_Move(MouseMoveEvent mouseMoveEvent)
    {
        this.Default_OnMouseMove_MoveEx(mouseMoveEvent, ref _trigger_move);
    }

    private void OnMouseUp_Move(MouseUpEvent mouseUpEvent)
    {
        this.Default_OnMouseUp_MoveEx(ref _trigger_move);
        UnregisterCallback<MouseMoveEvent>(OnMouseMove_Move);
    }

    private void OnDetach(DetachFromPanelEvent detachEvent)
    {
        if (_rayCastReady == true)
        {
            UnRegister_AbsCallBack(OnMouseDown_RayCast);
        }

        //내가 갈 수 있는 노드들의 Arrow들 삭제
        {
            foreach (KeyValuePair<StateNode, Arrow_Ready> pair in _toStateNodes)
            {
                pair.Key.DeleteFriendNode_From(this);

            }
            _toStateNodes.Clear();
        }

        //나에게로 올 수 있는 노드들의 Arrow들 삭제
        {
            foreach (KeyValuePair<StateNode, Arrow_Ready> pair in _fromStateNodes)
            {
                pair.Key.DeleteFriendNode_To(this);
            }
            _toStateNodes.Clear();
        }
    }

    public override void DoRayCast(Vector2 position, MyVisualElement caller)
    {
        StateNode stateNode = caller as StateNode;

        if (stateNode == null)
        {
            return;
        }

        Arrow_Ready newReadyArrow = new Arrow_Ready(_root, _window, stateNode, this);

        LinkingNode_From(stateNode, newReadyArrow);
        stateNode.LinkingNode_To(this, newReadyArrow);
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

    



    public void LinkingNode_From(StateNode from, Arrow_Ready arrow)
    {
        //LinkedStateNodeDesc newLinked = new LinkedStateNodeDesc(from, arrow);
        _fromStateNodes.Add(from, arrow);
    }

    public void LinkingNode_To(StateNode to, Arrow_Ready arrow)
    {
        LinkedStateNodeDesc newLinked = new LinkedStateNodeDesc(to, arrow);
        _toStateNodes.Add(to, arrow);
    }


    void DeleteMe() 
    {
        _root.Remove(this);
    }


    private void DeleteFriendNode_From(StateNode stateNode)
    {
        Arrow_Ready deleteArrow = null;
        _fromStateNodes.TryGetValue(stateNode, out deleteArrow);

        if (deleteArrow == null)
        {
            Debug.Assert(false, "삭제할건데 없다고?");
            Debug.Break();
            return;
        }

        _root.Remove(deleteArrow);

        _fromStateNodes.Remove(stateNode);
    }

    private void DeleteFriendNode_To(StateNode stateNode)
    {
        Arrow_Ready deleteArrow = null;
        _toStateNodes.TryGetValue(stateNode, out deleteArrow);

        if (deleteArrow == null)
        {
            Debug.Assert(false, "삭제할건데 없다고?");
            Debug.Break();
            return;
        }

        _root.Remove(deleteArrow);

        _toStateNodes.Remove(stateNode);
    }

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

        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);
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
        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);

        ////중간에 꽃힐 화살표 비쥬얼 엘리먼트 생성
        {
            _arrowHead = new Arrow_Head(_root, _window, Vector2.zero);
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


[Serializable]
public class ConditionDataWrapper
{
    public ConditionAsset _conditionAsset = null;
    public bool _goal = false;
}


/*--------------------------------------------------
|NOTI| 드래그 완료된, 준비된 화살표
--------------------------------------------------*/
#region Arrow_Ready
public class Arrow_Ready : MyVisualElement
{
    /*----------------------------------------------------
    |NOTI| 닥스에서 가져왔어요 분석하면서 만들기-------------
    ----------------------------------------------------*/
    public class ListViewExample : HierarchycalWindow
    {
        // Gradient used for the HP color indicator.
        private Gradient hpGradient;
        private GradientColorKey[] hpColorKey;
        private GradientAlphaKey[] hpAlphaKey;
        // Sets up the gradient.
        private void SetGradient()
        {
            hpGradient = new Gradient();

            // HP at 0%: Red. At 10%: Dark orange. At 40%: Yellow. At 100%: Green.
            hpColorKey = new GradientColorKey[4];
            hpColorKey[0] = new GradientColorKey(Color.red, 0f);
            hpColorKey[1] = new GradientColorKey(new Color(1f, 0.55f, 0f), 0.1f); // Dark orange
            hpColorKey[2] = new GradientColorKey(Color.yellow, 0.4f);
            hpColorKey[3] = new GradientColorKey(Color.green, 1f);

            // Alpha is always full.
            hpAlphaKey = new GradientAlphaKey[2];
            hpAlphaKey[0] = new GradientAlphaKey(1f, 0f);
            hpAlphaKey[1] = new GradientAlphaKey(1f, 1f);
            hpGradient.SetKeys(hpColorKey, hpAlphaKey);
        }




        public static void OpenWindow()
        {
            GetWindow<ListViewExample>().Show();
        }




        // ListView is kept for easy reference.
        private ListView listView;

        [Serializable]
        public class CharacterInfo
        {
            public string name;
            public int maxHp;
            public int currentHp;
        }
        private List<CharacterInfo> items;














        public void CreateGUI()
        {
            SetGradient();

            // Create and populate the list of CharacterInfo objects.
            const int itemCount = 50;
            items = new List<CharacterInfo>(itemCount);

            //단순히 데이터 보관용 컨테이너에 저장만 합니다...50번 반복합니다.
            {
                for (int i = 1; i <= itemCount; i++)
                {
                    CharacterInfo character = new CharacterInfo { name = $"Character {i}", maxHp = 100 };
                    character.currentHp = character.maxHp;
                    items.Add(character);
                }
            }

            // The ListView calls this to add visible items to the scroller.
            Func<VisualElement> makeItem = () =>
            {
                var characterInfoVisualElement = new CharacterInfoVisualElement();
                var slider = characterInfoVisualElement.Q<SliderInt>(name: "hp");
                slider.RegisterValueChangedCallback(evt =>
                {
                    var hpColor = characterInfoVisualElement.Q<VisualElement>("hpColor");
                    var i = (int)slider.userData;
                    var characterInfo = items[i];
                    characterInfo.currentHp = evt.newValue;
                    SetHp(slider, hpColor, characterInfo);
                });
                return characterInfoVisualElement;
            };


            // The ListView calls this if a new item becomes visible when the item first appears on the screen, 
            // when a user scrolls, or when the dimensions of the scroller are changed.
            Action<VisualElement, int> bindItem = (e, i) => BindItem(e as CharacterInfoVisualElement, i);

            // Height used by the ListView to determine the total height of items in the list.
            int itemHeight = 55;

            // Use the constructor with initial values to create the ListView.
            listView = new ListView(items, itemHeight, makeItem, bindItem);
            listView.reorderable = false;
            listView.style.flexGrow = 1f; // Fills the window, at least until the toggle below.
            listView.showBorder = true;
            rootVisualElement.Add(listView);

            // Add a toggle to switch the reorderable property of the ListView.
            var reorderToggle = new Toggle("Reorderable");
            reorderToggle.style.marginTop = 10f;
            reorderToggle.value = false;
            reorderToggle.RegisterValueChangedCallback(evt => listView.reorderable = evt.newValue);
            rootVisualElement.Add(reorderToggle);
        }



        // Bind the data (characterInfo) to the display (elem).
        private void BindItem(CharacterInfoVisualElement elem, int i)
        {
            CharacterInfo characterInfo = items[i];

            var label = elem.Q<Label>(name: "nameLabel");
            label.text = characterInfo.name;

            var slider = elem.Q<SliderInt>(name: "hp");
            slider.userData = i;

            var hpColor = elem.Q<VisualElement>("hpColor");
            SetHp(slider, hpColor, characterInfo);
        }

        private void SetHp(SliderInt slider, VisualElement colorIndicator, CharacterInfo characterInfo)
        {
            slider.highValue = characterInfo.maxHp;
            slider.SetValueWithoutNotify(characterInfo.currentHp);
            float ratio = (float)characterInfo.currentHp / characterInfo.maxHp;
            colorIndicator.style.backgroundColor = hpGradient.Evaluate(ratio);
        }

        // This class inherits from VisualElement to display and modify data to and from a CharacterInfo.
        public class CharacterInfoVisualElement : VisualElement
        {
            // Use Constructor when the ListView uses makeItem and returns a VisualElement to be 
            // bound to a CharacterInfo data class.
            public CharacterInfoVisualElement()
            {
                var root = new VisualElement();

                // The code below to style the ListView is for demo purpose. It's better to use a USS file
                // to style a visual element. 
                root.style.paddingTop = 3f;
                root.style.paddingRight = 0f;
                root.style.paddingBottom = 15f;
                root.style.paddingLeft = 3f;
                root.style.borderBottomColor = Color.gray;
                root.style.borderBottomWidth = 1f;
                var nameLabel = new Label() { name = "nameLabel" };
                nameLabel.style.fontSize = 14f;
                var hpContainer = new VisualElement();
                hpContainer.style.flexDirection = FlexDirection.Row;
                hpContainer.style.paddingLeft = 15f;
                hpContainer.style.paddingRight = 15f;
                hpContainer.Add(new Label("HP:"));
                var hpSlider = new SliderInt { name = "hp", lowValue = 0, highValue = 100 };
                hpSlider.style.flexGrow = 1f;
                hpContainer.Add(hpSlider);
                var hpColor = new VisualElement();
                hpColor.name = "hpColor";
                hpColor.style.height = 15f;
                hpColor.style.width = 15f;
                hpColor.style.marginRight = 5f;
                hpColor.style.marginBottom = 5f;
                hpColor.style.marginLeft = 5f;
                hpColor.style.backgroundColor = Color.black;
                hpContainer.Add(hpColor);
                root.Add(nameLabel);
                root.Add(hpContainer);
                Add(root);
            }
        }


    } //-------------------
    //----------------------------------------------------
    public class SubEditorWindow_ConditionModify : HierarchycalWindow
    {
        public class ConditionDataVisualElement : VisualElement
        {
            public ConditionDataVisualElement() 
            {
                style.borderTopWidth = 3f;
                style.borderLeftWidth = 3f;
                style.borderRightWidth = 3f;
                style.borderBottomWidth = 3f;

                style.borderTopColor = Color.gray;
                style.borderLeftColor = Color.gray;
                style.borderRightColor = Color.gray;
                style.borderBottomColor = Color.gray;

                _objectField = new ObjectField()
                {
                    name = "_conditionAsset",
                    objectType = typeof(ConditionAsset),
                    allowSceneObjects = false,
                };
                Add(_objectField);

                VisualElement goalToggleContainer = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    }
                };
                Add(goalToggleContainer);

                Label goalToggleName = new Label("Goal : ")
                {
                    style =
                    {
                        alignContent = Align.Center,
                    },

                    name = "_nameLabel_goal",
                };
                goalToggleContainer.Add(goalToggleName);

                _goalToggle = new Toggle()
                {
                    style =
                    {
                        alignContent = Align.Center,
                    },

                    name = "_goal",
                    value = false,
                };
                goalToggleContainer.Add(_goalToggle);
            }

            private ObjectField _objectField = null;
            private Toggle _goalToggle = null;
        }

        private ListView _conditionDataListView = null;
        private VisualElement _buttons = null;
        private List<ConditionDataWrapper> _conditionData = null;
        private Arrow_Ready _arrow_ready = null;

        private void OnDisable()
        {
            _arrow_ready?.WindowOff();
        }

        public void Init(List<ConditionDataWrapper> dataTarget, Arrow_Ready fromArrow)
        {
            //데이터 설정
            {
                _conditionData = dataTarget;
                _arrow_ready = fromArrow;
            }
            

            //제목 및 배경 설정
            {
                Label label = new Label("ConditionModifier");
                AddElement(Layer.Layer_BackGround, label);

                VisualElement rootElementBackGroud = new VisualElement()
                {
                    style =
                    {
                        flexGrow = 1,
                        backgroundColor = Color.clear,
                        position = Position.Absolute,
                    }

                };

                rootElementBackGroud.pickingMode = PickingMode.Ignore;
                AddElement(Layer.Layer_BackGround, rootElementBackGroud);
            }

            //리스트 뷰 세팅
            {
                _conditionDataListView = new ListView();

                _conditionDataListView.itemsSource = _conditionData;

                Func<ConditionDataVisualElement> createListElelemt = () =>
                {
                    ConditionDataVisualElement newElement = new ConditionDataVisualElement();
                    return newElement;
                };
                _conditionDataListView.makeItem = createListElelemt;


                Action<VisualElement, int> dataChanged = (VisualElement element, int index) =>
                {
                    ConditionDataWrapper target = _conditionData[index];
                    if (index >= _conditionData.Count)
                    {
                        Debug.Assert(false, "데이터 개수를 넘은 인덱스? 심각하다");
                        Debug.Break();
                        return;
                    }
                    if (index < 0)
                    {
                        Debug.Assert(false, "음수의 인덱스? 심각하다");
                        Debug.Break();
                        return;
                    }

                    ConditionDataVisualElement casted = (ConditionDataVisualElement)element;

                    ObjectField conditionAssetField = casted.Q<ObjectField>("_conditionAsset");
                    if (conditionAssetField == null)
                    {
                        Debug.Assert(false, "conditionAssetField 없다고?");
                        Debug.Break();
                        return;
                    }
                    target._conditionAsset = (ConditionAsset)conditionAssetField.value;

                    Toggle goalField = casted.Q<Toggle>("_goal");
                    if (goalField == null)
                    {
                        Debug.Assert(false, "goalField 없다고?");
                        Debug.Break();
                        return;
                    }
                    target._goal = goalField.value;
                };

                _conditionDataListView.bindItem = dataChanged;
                _conditionDataListView.fixedItemHeight = 50;
                _conditionDataListView.reorderable = true;
                _conditionDataListView.showBorder = true;

                AddElement(Layer.Layer_Object_One, _conditionDataListView);
            }

            //버튼들
            {
                _buttons = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        height = 50,
                        width = 100,
                    }
                };
                AddElement(Layer.Layer_Object_Two, _buttons);

                Action plusButtonClickAction = () =>
                {
                    _conditionData.Add(new ConditionDataWrapper());
                    RefreshUI();
                };
                Button plusButton = new Button(plusButtonClickAction)
                {
                    style =
                    {
                        width = 40,
                        height = 40,
                    },
                    text = "+",
                };
                _buttons.Add(plusButton);


                Action minusButtonClickAction = () =>
                {
                    if (_conditionData.Count <= 0)
                    {
                        return;
                    }
                    _conditionData.RemoveAt(_conditionData.Count - 1);
                    RefreshUI();
                };
                Button minusButton = new Button(minusButtonClickAction)
                {
                    style =
                    {
                        width = 40,
                        height = 40,
                    },
                    text = "-",
                };
                _buttons.Add(minusButton);
            }
        }

        private void RefreshUI()
        {
            _conditionDataListView.Rebuild();
        }
    }//----
    //----------------------------------------------------





    public Arrow_Ready(VisualElement root, GraphEditorWithUIToolkit window, StateNode fromNode, StateNode toNode) : base(root, window)
    {
        _fromNode = fromNode;
        _toNode = toNode;

        //style 설정
        {
            style.position = Position.Absolute;
            style.backgroundColor = Color.green;
            style.height = 2.0f;
        }

        ////중간에 꽃힐 화살표 비쥬얼 엘리먼트 생성
        {
            _arrowHead = new Arrow_Head(_root, _window, Vector2.zero);
        }

        _actions += ArrowPositionUpdate;

        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);

        RegisterCallback<DetachFromPanelEvent>(OnDetach);
        RegisterCallback<MouseDownEvent>(OnMouseDown_ModifyCondition);
    }


    private bool _isWindowShow = false;
    private StateNode _fromNode = null;
    private StateNode _toNode = null;
    private Arrow_Head _arrowHead = null;
    private List<ConditionDataWrapper> _conditionData = new List<ConditionDataWrapper>();
    private EditorWindow _conditionModifierWindow = null;


    /*----------------------------------------------------
    |TODO| 매 프레임마다 움직이지 않아도 이걸 업데이트 하네요?
    전에 만들어둔 노드 위치가 변경될때만 하는게 좋을거같아요.
    ----------------------------------------------------*/
    private void ArrowPositionUpdate()
    {
        Vector2 direction = _toNode.GetPosition_C() - _fromNode.GetPosition_C();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector2 fromNodeRatedOffsetPosition = _fromNode.GetPosition_RotatedOffset_C(15.0f, (angle + 90.0f) * Mathf.Deg2Rad);
        Vector2 toNodeRatedOffsetPosition = _toNode.GetPosition_RotatedOffset_C(15.0f, (angle + 180.0f) * Mathf.Deg2Rad);

        style.width = Vector2.Distance(fromNodeRatedOffsetPosition, toNodeRatedOffsetPosition);

        Vector2 centerPosition = (fromNodeRatedOffsetPosition + toNodeRatedOffsetPosition) / 2.0f;



        //화살표 몸통이 업데이트 된다
        {
            MoveTo(centerPosition);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        //화살표 중간 삼각형이 업데이트 된다
        {
            _arrowHead.MoveTo_IfArrow(centerPosition);
            _arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }



    public void WindowOff()
    {
        _isWindowShow = false;
    }

    private void OnMouseDown_ModifyCondition(MouseDownEvent mouseDownEvent)
    {
        if (_isWindowShow == true)
        {
            return;
        }

        SubEditorWindow_ConditionModify window = EditorWindow.CreateInstance<SubEditorWindow_ConditionModify>();

        window.titleContent = new GUIContent("SubEditorWindow_ConditionModify");
        window.Show();
        window.Init(_conditionData, this);

        _isWindowShow = true;
        _conditionModifierWindow = window;
    }

    private void OnDetach(DetachFromPanelEvent detachEvent)
    {
        _actions = null;

        if (_isWindowShow == true)
        {
            _conditionModifierWindow.Close();
        }

        if (_arrowHead != null)
        {
            _root.Remove(_arrowHead);
            _arrowHead = null;
        }
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


//#region Mover
//public class Mover : ChildElement
//{
//    public Mover(VisualElement root, GraphEditorWithUIToolkit window, VisualElement parent, int id) : base(root, window, parent, id)
//    {
//        style.left = 2;
//        style.top = 2;
//        style.right = 2;
//        style.bottom = 2;
//        style.backgroundColor = Color.white;



//        RegisterCallback<MouseDownEvent>(evt =>
//        {
//            _isDragging = true;
//            //this.CaptureMouse();
//            AfterMouseDown();
//        });


//        RegisterCallback<MouseUpEvent>(evt =>
//        {
//            _isDragging = false;
//            //this.ReleaseMouse();
//            AfterMouseUp();
//        });

//        _parent.Add(this);
//    }

//    private bool _isDragging = false;

//    private void AfterMouseDown()
//    {
//        RegisterCallback<MouseMoveEvent>(OnMouseMove);
//    }

//    private void OnMouseMove(MouseMoveEvent moveEvent)
//    {
//        if (_isDragging == false)
//        {
//            return;
//        }

//        _parent.transform.position += new Vector3(moveEvent.mouseDelta.x, moveEvent.mouseDelta.y, 0.0f);
//    }

//    private void AfterMouseUp()
//    {
//        UnregisterCallback<MouseMoveEvent>(OnMouseMove);
//    }
//}
//#endregion Mover



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
            AfterMouseDown();
        });



        RegisterCallback<MouseUpEvent>(evt =>
        {
            _isResizing = false;
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
