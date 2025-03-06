using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static StateGraphAsset;
using System.Linq;
using static StateGraphToolSaveData;
using UnityEditor.Experimental.GraphView;

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
    public void ReadyHierarchy()
    {
        for (int i = 0; i < (int)Layer.End; i++)
        {
            VisualElement hierarchyElement = new VisualElement();
            rootVisualElement.Add(hierarchyElement);
            _hierarchyElement.Add(hierarchyElement);
        }
    }

    public enum Layer
    {
        Layer_BackGround,

        Layer_Object_One,
        Layer_Object_Two,
        Layer_Object_Three,


        Layer_One,
        Layer_Two,
        Layer_Three,



        Layer_EditorHUD_One,
        Layer_EditorHUD_Two,
        Layer_EditorHUD_Three,

        End,
    }

    List<VisualElement> _hierarchyElement = new List<VisualElement>();


    public void AddElement(Layer layer, VisualElement element)
    {
        rootVisualElement.Insert((int)layer, element);
        //_hierarchyElement[(int)layer].Add(element);
    }

    public void RemoveElement(Layer layer, VisualElement element)
    {
        //_hierarchyElement[(int)layer].Remove(element);
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

    private void Clear_All_New_Paper()
    {
        /*------------------------------------------------------------------------
        |NOTI|아직까지는 이 에디터의 핵심정보들은 노드이다. 노드만 다 지우면 깨끗해진다.
        ------------------------------------------------------------------------*/

        foreach (var item in _stateNodes.ToList())
        {
            rootVisualElement.Remove(item);
        }

        _arrowViewMode = ArrowViewMode.ShowAll;
        ReadyHierarchy();
    }



    public class GraphEditorBackGroundElement : MyVisualElement
    {
        public class GraphEditor_SubWindow_IO : HierarchycalWindow
        {
            public GraphEditor_SubWindow_IO()
            {
                minSize = new Vector2(500.0f, 300.0f);
                maxSize = new Vector2(500.0f, 300.0f);
            }

            public virtual void Init(GraphEditorWithUIToolkit graphEditorWindow, VisualElement graphEditorRootElement)
            {
                _graphEditorWindow = graphEditorWindow;
                _graphEditorVisualElement = graphEditorRootElement;
            }

            protected GraphEditorWithUIToolkit _graphEditorWindow = null;
            protected VisualElement _graphEditorVisualElement = null;
        }

        public class SavingWindow : GraphEditor_SubWindow_IO
        {
            public static SavingWindow OpenWindow()
            {
                SavingWindow window = GetWindow<SavingWindow>();
                window.titleContent = new GUIContent("Save...");
                return window;
            }

            public override void Init(GraphEditorWithUIToolkit graphEditorWindow, VisualElement graphEditorRootElement)
            {
                base.Init(graphEditorWindow, graphEditorRootElement);

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
                            flexGrow = 1,
                            flexShrink = 1,
                        }
                    };
                    nameInputField.value = _basicName;
                    nameContaioner.Add(nameInputField);


                    VisualElement pathNameContainer = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    saveUIContainer.Add(pathNameContainer);

                    TextField pathNameInputField = new TextField()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            flexGrow = 1,
                            flexShrink = 1,
                        }
                    };
                    pathNameInputField.value = _basicPathName;


                    Action directoryButtonPushed = () =>
                    {
                        string path = EditorUtility.OpenFolderPanel("폴더 선택", "Assets", "");

                        if (string.IsNullOrEmpty(path) == true)
                        {
                            pathNameInputField.value = _basicPathName;
                        }
                        else
                        {
                            pathNameInputField.value = path;

                            if (pathNameInputField.value.EndsWith("/") == false)
                            {
                                pathNameInputField.value += "/";
                            }
                        }
                    };
                    Button directoryButton = new Button(directoryButtonPushed)
                    {
                        style =
                        {
                            marginRight = 1,
                        },
                        text = "Path..."
                    };
                    pathNameContainer.Add(directoryButton);
                    pathNameContainer.Add(pathNameInputField);



                    Action saveButtonClicked = () =>
                    {
                        StateGraphAsset savedGraphAsset = null;

                        //graphAsset 세이브
                        {
                            if (string.IsNullOrEmpty(nameInputField.value) == true)
                            {
                                return;
                            }

                            if (Directory.Exists(pathNameInputField.value) == false)
                            {
                                Directory.CreateDirectory(pathNameInputField.value);  // 폴더가 없으면 생성
                            }

                            string assetPath = pathNameInputField.value + nameInputField.value + ".asset";

                            bool isFileExist = File.Exists(assetPath);

                            assetPath = FileUtil.GetProjectRelativePath(assetPath);

                            if (isFileExist == true)
                            {
                                //경고문
                                bool overwrite = EditorUtility.DisplayDialog
                                (
                                        "파일이 존재합니다",
                                        "덮어씌웁니까?",
                                        "예",
                                        "아니요"
                                );

                                if (overwrite == false)
                                {
                                    return;
                                }

                                savedGraphAsset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(assetPath);
                            }
                            else
                            {
                                savedGraphAsset = CreateInstance<StateGraphAsset>();
                                AssetDatabase.CreateAsset(savedGraphAsset, assetPath);

                            }

                            savedGraphAsset.InitData(_graphEditorWindow._stateNodes);

                            EditorUtility.SetDirty(savedGraphAsset);

                            AssetDatabase.SaveAssets();

                            AssetDatabase.Refresh();
                        }

                        if (savedGraphAsset == null)
                        {
                            Debug.Assert(false, "savedGraphAsset이 null이다");
                            Debug.Break();
                            return;
                        }

                        //기존 노드 정보들 세이브
                        {
                            //graphAsset 세이브
                            {
                                if (Directory.Exists(_graphEditorDataPath) == false)
                                {
                                    Directory.CreateDirectory(_graphEditorDataPath);  // 폴더가 없으면 생성
                                }

                                if (_graphEditorDataPath.EndsWith("/") == false)
                                {
                                    _graphEditorDataPath += "/";
                                }

                                string assetPath = _graphEditorDataPath + "StateGraphProjects" + ".asset";

                                StateGraphToolSaveData projectData = null;

                                string[] guids = AssetDatabase.FindAssets("t:" + typeof(StateGraphToolSaveData).Name, new[] { _graphEditorDataPath });

                                if (guids.Length > 2)
                                {
                                    Debug.Assert(false, "2개 이상의 프로젝트 데이터가 있다고요? 하나만 있어야합니다");
                                    Debug.Break();
                                    return;
                                }
                                else if (guids.Length <= 0)
                                {
                                    projectData = CreateInstance<StateGraphToolSaveData>();
                                    AssetDatabase.CreateAsset(projectData, assetPath);
                                }
                                else
                                {
                                    projectData = AssetDatabase.LoadAssetAtPath<StateGraphToolSaveData>(assetPath);
                                }
                                StateGraphToolProjectSaveDesc newSaveData = new StateGraphToolProjectSaveDesc()
                                {
                                    _nodeDatas = _graphEditorWindow._stateNodes,
                                    _arrowDatas = _graphEditorWindow._arrows,
                                };

                                projectData.InitData(savedGraphAsset, newSaveData);

                                EditorUtility.SetDirty(projectData);

                                AssetDatabase.SaveAssets();

                                AssetDatabase.Refresh();
                            }
                        }
                    };

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
            private string _basicPathName = "Assets/";
            private string _graphEditorDataPath = "Assets/Editor/StateGraphEditor/ProjectData/Created";
        }

        public class LoadingWindow : GraphEditor_SubWindow_IO
        {
            public static LoadingWindow OpenWindow()
            {
                LoadingWindow window = GetWindow<LoadingWindow>();
                window.titleContent = new GUIContent("Load...");
                return window;
            }

            public override void Init(GraphEditorWithUIToolkit graphEditorWindow, VisualElement graphEditorRootElement)
            {
                base.Init(graphEditorWindow, graphEditorRootElement);

                //비쥬얼 컨테이너 묶음 생성
                {
                    VisualElement backGround = new()
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
                        objectType = typeof(StateGraphAsset),
                        allowSceneObjects = false,
                    };

                    loadUIContainer.Add(_objectField);

                    Action loadButtonClicked = () => 
                    {
                        if (_objectField.value == null)
                        {
                            bool overwrite = EditorUtility.DisplayDialog
                            (
                                "경고",
                                "필드에 그래프를 선택하고 로드해주세요",
                                "예"
                            );

                            return;
                        }


                        //기존 노드 정보들 세이브
                        {
                            //graphAsset 세이브
                            {
                                if (Directory.Exists(_graphEditorDataPath) == false)
                                {
                                    bool overwrite = EditorUtility.DisplayDialog
                                    (
                                        "알림",
                                        "해당 그래프 에셋은 프로젝트가 없습니다. 만들고 저장해주세요.",
                                        "예"
                                    );
                                }

                                if (_graphEditorDataPath.EndsWith("/") == false)
                                {
                                    _graphEditorDataPath += "/";
                                }

                                string assetPath = _graphEditorDataPath + "StateGraphProjects" + ".asset";

                                StateGraphToolSaveData projectData = null;

                                string[] guids = AssetDatabase.FindAssets("t:" + typeof(StateGraphToolSaveData).Name, new[] { _graphEditorDataPath });

                                if (guids.Length > 2)
                                {
                                    Debug.Assert(false, "2개 이상의 프로젝트 데이터가 있다고요? 하나만 있어야합니다");
                                    Debug.Break();
                                    return;
                                }
                                else if (guids.Length <= 0)
                                {
                                    bool overwrite = EditorUtility.DisplayDialog
                                    (
                                        "알림",
                                        "해당 그래프 에셋은 프로젝트가 없습니다. 만들고 저장해주세요.",
                                        "예"
                                    );

                                    return;
                                }

                                projectData = AssetDatabase.LoadAssetAtPath<StateGraphToolSaveData>(assetPath);

                                StateGraphToolProjectLoadDesc existData = projectData.GetLoadDesc((StateGraphAsset)_objectField.value);

                                if (existData == null)
                                {
                                    bool overwrite = EditorUtility.DisplayDialog
                                    (
                                        "알림",
                                        "해당 그래프 에셋은 프로젝트가 없습니다. 만들고 저장해주세요.",
                                        "예"
                                    );

                                    return;
                                }

                                _graphEditorWindow.Clear_All_New_Paper();

                                _graphEditorWindow.LoadProject(existData);
                            }   
                        }
                    };

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
            private string _graphEditorDataPath = "Assets/Editor/StateGraphEditor/ProjectData/Created";
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
                menuEvent.menu.AppendAction("Clear_All", _ => ClearAll());
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
            SavingWindow.OpenWindow().Init(_window, _root);
        }
        private void LoadStateGraph() 
        {
            LoadingWindow.OpenWindow().Init(_window, _root);
        }
        private void ClearAll()
        {
            _window.Clear_All_New_Paper();
        }
    }

    public class GraphEditorViewRadioButton : MyVisualElement
    {
        public GraphEditorViewRadioButton(VisualElement root, GraphEditorWithUIToolkit window) : base(root, window)
        {
            style.position = Position.Absolute;
            style.width = 170.0f;
            style.height = 50.0f;
            style.backgroundColor = Color.gray;
            style.top = 0;
            style.right = 0;


            // 라디오 버튼 그룹 생성
            RadioButtonGroup radioGroup_Button = new RadioButtonGroup()
            {
                style = 
                {
                    position = Position.Absolute,
                    top = 0,
                    right = 0,
                },
            };
            Add(radioGroup_Button);

            RadioButton eachButton_ShowAll = new RadioButton();
            eachButton_ShowAll.value = true;
            radioGroup_Button.Add(eachButton_ShowAll);

            RadioButton eachButton_Hide = new RadioButton();
            radioGroup_Button.Add(eachButton_Hide);

            RadioButton eachButton_Show_OnlyFocused = new RadioButton();
            radioGroup_Button.Add(eachButton_Show_OnlyFocused);

            // 라디오 버튼 추가
            RadioButtonGroup radioGroup_Label = new RadioButtonGroup()
            {
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    right = 20,
                },

            };

            radioGroup_Button.RegisterValueChangedCallback(evt =>
            {
                _window.ArrowShowValueChanged(evt.newValue);
            });

            Add(radioGroup_Label);

            //라벨들 추가
            {
                Label eachButtonLabel_ShowAll = new Label("Show_All")
                {
                    style =
                {
                    unityTextAlign = TextAnchor.UpperRight,
                },
                };
                radioGroup_Label.Add(eachButtonLabel_ShowAll);

                Label eachButtonLabel_Show_OnlyFocused = new Label("Show_OnlyFocused")
                {
                    style =
                {
                    unityTextAlign = TextAnchor.UpperRight,
                },
                };
                radioGroup_Label.Add(eachButtonLabel_Show_OnlyFocused);

                Label eachButtonLabel_Hide = new Label("Hide")
                {
                    style =
                {
                    unityTextAlign = TextAnchor.UpperRight,
                },
                };
                radioGroup_Label.Add(eachButtonLabel_Hide);
            }

            _window.AddElement(Layer.Layer_EditorHUD_One, this);
        }
    }

    public void CreateGUI()
    {
        ReadyHierarchy();

        Label label = new Label("StateGraphTool")
        {
            style =
            {
                position = Position.Absolute,
            }
        };
        AddElement(Layer.Layer_BackGround, label);

        new GraphEditorBackGroundElement(rootVisualElement, this);

        new GraphEditorViewRadioButton(rootVisualElement, this);

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


    /*------------------------------------------------------------------
    |NOTI| 그래프를 읽기 시작할 노드들입니다. 최초 추가시에만 여기 보관됩니다.
    ------------------------------------------------------------------*/
    private HashSet<StateNode> _stateNodes = new HashSet<StateNode>();
    private HashSet<Arrow_Ready> _arrows = new HashSet<Arrow_Ready>();


    public enum ArrowViewMode
    {
        ShowAll,
        Show_OnlyFocused,
        Hide,
        End,
    }

    private ArrowViewMode _arrowViewMode = ArrowViewMode.ShowAll;

    private void ArrowShowValueChanged(int index)
    {
        _arrowViewMode = (ArrowViewMode)index;

        foreach (Arrow_Ready arrow in _arrows)
        {
            bool isShowCheck = IsArrowShow(arrow);

            if (isShowCheck == true)
            {
                arrow.ShowMe();
            }
            else
            {
                arrow.HideMe();
            }
        }
    }

    public bool IsArrowShow(Arrow_Ready arrowReady)
    {
        bool ret = false;
        switch (_arrowViewMode)
        {
            case ArrowViewMode.ShowAll:
                ret = true;
                break;

            case ArrowViewMode.Show_OnlyFocused:
                ret = (arrowReady._ToNode == _focusedNode || arrowReady._FromNode == _focusedNode);
                break;

            case ArrowViewMode.Hide:
                ret = false;
                break;

            default:
                {
                    Debug.Assert(false, "대응이 되지 않습니다");
                    Debug.Break();
                }
                break;
        }
        return ret;
    }

    public void SetFocusedNode(StateNode focusedNode)
    {
        _focusedNode = focusedNode;

        if (_arrowViewMode != ArrowViewMode.Show_OnlyFocused)
        {
            return;
        }

        foreach (Arrow_Ready arrow in _arrows)
        {
            bool isShowCheck = IsArrowShow(arrow);

            if (isShowCheck == true)
            {
                arrow.ShowMe();
            }
            else
            {
                arrow.HideMe();
            }
        }
    }

    public void LoadProject(StateGraphToolProjectLoadDesc loadData)
    {
        foreach (StateGraphToolProjectLoadDesc.LoadData_StateNode stateLoadData in loadData._nodeDatas)
        {
            StateNode node = new StateNode(rootVisualElement, this, stateLoadData._position);
            AddElement(Layer.Layer_Object_One, node);

            {
                node._StateAsset = stateLoadData._asset;
                node._IsEntryState = stateLoadData._isEntry;

                node._entryConditionDatas.Clear();
                node._entryConditionDatas.AddRange(stateLoadData._entryConditions);
            }
            
            //node.
        }

        foreach (StateGraphToolProjectLoadDesc.LoadData_ArrowNode arrowLoadData in loadData._arrowDatas)
        {
            StateNode fromNode = null;
            StateNode toNode = null;

            foreach (StateNode stateNode in _stateNodes)
            {
                if (stateNode._StateAsset == arrowLoadData._fromAsset)
                {
                    fromNode = stateNode;
                    break;
                }
            }

            foreach (StateNode stateNode in _stateNodes)
            {
                if (stateNode._StateAsset == arrowLoadData._toAsset)
                {
                    toNode = stateNode;
                    break;
                }
            }

            if (toNode == null || fromNode == null)
            {
                Debug.Assert(false, "로딩중에 못찾았다고?");
                Debug.Break();
                return;
            }


            Arrow_Ready node = new Arrow_Ready(rootVisualElement, this, fromNode, toNode);

            {
                node.GetCondition().Clear();
                node.GetCondition().AddRange(arrowLoadData._conditions);
            }
        }
    }



    private StateNode _focusedNode = null;


    public void ArrowAdded(Arrow_Ready arrow)
    {
        if (_arrows.Contains(arrow) == true)
        {
            Debug.Assert(false, "이미 있다고?  심각한 오류다");
            Debug.Break();
            return;
        }

        _arrows.Add(arrow);
    }

    public void DeleteArrow(Arrow_Ready arrow)
    {
        if (_arrows.Contains(arrow) == false)
        {
            /*--------------------------------------------------------
            |TODO|지워지는 순서 파악하고 이거 고쳐야한다.
            --------------------------------------------------------*/
            //Debug.Assert(false, "존재하지 않았다고? 심각한 오류다");
            //Debug.Break();
            return;
        }

        _arrows.Remove(arrow);
    }
 

    public void StateNodeAdded(StateNode stateNode)
    {
        if (_stateNodes.Contains(stateNode) == true)
        {
            Debug.Assert(false, "이미 있다고?  심각한 오류다");
            Debug.Break();
            return;
        }

        _stateNodes.Add(stateNode);
    }


    public void DeleteStateNode(StateNode stateNode)
    {
        if (_focusedNode == stateNode)
        {
            _focusedNode = null;
        }

        if (_stateNodes.Contains(stateNode) == false)
        {
            Debug.Assert(false, "존재하지 않았다고? 심각한 오류다");
            Debug.Break();
            return;
        }

        _stateNodes.Remove(stateNode);
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
public class StateNode : MyVisualElement, IConditionExist
{
    private bool _isWindowShow = false;
    private EditorWindow _conditionModifierWindow = null;
    public List<ConditionAssetWrapper> _entryConditionDatas = new List<ConditionAssetWrapper>();

    public List<ConditionAssetWrapper> GetCondition()
    {
        return _entryConditionDatas;
    }
    public void WindowOff()
    {
        _isWindowShow = false;
    }
    public void TurnOnConditionModifyWindow(EditorWindow window)
    {
        _isWindowShow = true;
        _conditionModifierWindow = window;
    }
    public void OnDetach_ConditionModifyWindow()
    {
        if (_isWindowShow == true)
        {
            _conditionModifierWindow.Close();
        }
    }



    public StateNode(VisualElement root, GraphEditorWithUIToolkit window, Vector3 position) : base(root, window)
    {
        Init(position);
    }

    public void Init(Vector3 position)
    {
        //style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.backgroundColor = Color.gray;
        //style.backgroundColor = Color.clear;

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


        //Resizers...
        {
            Resizer resizer_Left = new Resizer(_root, _window, this, _id, Direction_2D.Left, border);
            Resizer resizer_LeftTop = new Resizer(_root, _window, this, _id, Direction_2D.LeftTop, border);
            Resizer resizer_Top = new Resizer(_root, _window, this, _id, Direction_2D.Top, border);
            Resizer resizer_TopRight = new Resizer(_root, _window, this, _id, Direction_2D.TopRight, border);
            Resizer resizer_Right = new Resizer(_root, _window, this, _id, Direction_2D.Right, border);
            Resizer resizer_RightBottom = new Resizer(_root, _window, this, _id, Direction_2D.RightBottom, border);
            Resizer resizer_Bottom = new Resizer(_root, _window, this, _id, Direction_2D.Bottom, border);
            Resizer resizer_BottomLeft = new Resizer(_root, _window, this, _id, Direction_2D.BottomLeft, border);
        }


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
                marginLeft = 0,
                marginRight = 0,
                marginTop = 0,
                marginBottom = 0,

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
        _objectField.RegisterValueChangedCallback(changedValue =>
        {
            string displayName = ((StateAsset)changedValue.newValue).name;
            if (displayName.Contains("State_") == true)
            {
                displayName = displayName.Replace("State_", "");
            }

            _displayNameField.value = displayName;
        });


        objectFieldSet.Add(_objectField);

        VisualElement toggleContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                marginRight = 0,
            }
        };
        objectFieldSet.Add(toggleContainer);

        Label entryToggleLabel = new Label("isEntry : ");
        toggleContainer.Add(entryToggleLabel);

        _isEntryState = new Toggle();
        toggleContainer.Add(_isEntryState);

        Action entryConditionModiClicked = () =>
        {
            if (_isEntryState.value == false)
            {
                return;
            }

            SubEditorWindow_ConditionModify window = EditorWindow.CreateInstance<SubEditorWindow_ConditionModify>();

            window.titleContent = new GUIContent("SubEditorWindow_ConditionModify");
            window.Show();
            window.Init(_entryConditionDatas, this);

            TurnOnConditionModifyWindow(window);
        };
        Button entryConditionModiButton = new Button(entryConditionModiClicked)
        {
            text = "Conditions",
            style =
            {
                flexShrink = 1,
                flexGrow = 1,
                marginRight = 0,
            }
        };
        toggleContainer.Add(entryConditionModiButton);



        _displayNameField = new TextField()
        {
            style =
            {
                marginLeft = 0,
                marginRight = 0,
                marginTop = 0,
                marginBottom = 0,
                flexShrink = 1,
                whiteSpace = WhiteSpace.Normal,
                overflow = Overflow.Hidden,
                maxHeight = 2 * 18,
            },
            multiline = true,
        };
        Add(_displayNameField);


        RegisterCallback<MouseDownEvent>(OnMouseFocusMe);
        RegisterCallback<MouseDownEvent>(OnMouseDown_Move);
        RegisterCallback<MouseUpEvent>(OnMouseUp_Move);
        RegisterCallback<DetachFromPanelEvent>(OnDetach);

        _window.StateNodeAdded(this);

        MoveTo(position);

        _id++;
    }

    private TextField _displayNameField = null;


    public static int _id = 0;

    
    private Dictionary<StateNode, Arrow_Ready> _toStateNodes = new Dictionary<StateNode, Arrow_Ready>();
    public IReadOnlyDictionary<StateNode, Arrow_Ready> _ToStateNodes => _toStateNodes;

    private Dictionary<StateNode, Arrow_Ready> _fromStateNodes = new Dictionary<StateNode, Arrow_Ready>();
    public IReadOnlyDictionary<StateNode, Arrow_Ready> _FromStateNodes => _fromStateNodes;

    private Toggle _isEntryState = null;
    public bool _IsEntryState 
    {
        get { return _isEntryState.value; }
        set { _isEntryState.value = value; }
    }
    private ObjectField _objectField = null;
    public StateAsset _StateAsset 
    {
        get { return (StateAsset)_objectField.value; }
        set { _objectField.value = value; }
    }
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

    private void OnMouseFocusMe(MouseDownEvent mouseDownEvent)
    {
        if (mouseDownEvent.pressedButtons != 4)
        {
            return;
        }

        _window.SetFocusedNode(this);
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

        OnDetach_ConditionModifyWindow();

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

        _window.DeleteStateNode(this);
    }

    public override void DoRayCast(Vector2 position, MyVisualElement caller)
    {
        StateNode stateNode = caller as StateNode;

        if (stateNode == null)
        {
            return;
        }

        if (stateNode == this)
        {
            return;
        }

        if (_fromStateNodes.ContainsKey(stateNode) == true)
        {
            return;
        }

        Arrow_Ready newReadyArrow = new Arrow_Ready(_root, _window, stateNode, this);
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
        _fromStateNodes.Add(from, arrow);
    }

    public void LinkingNode_To(StateNode to, Arrow_Ready arrow)
    {
        _toStateNodes.Add(to, arrow);
    }


    void DeleteMe() 
    {
        _root.Remove(this);
    }



    public void DeleteFriendNode_From_FromArror(StateNode stateNode)
    {
        if (_fromStateNodes.ContainsKey(stateNode) == false)
        {
            Debug.Assert(false, "삭제할건데 없다고?");
            Debug.Break();
            return;
        }

        _fromStateNodes.Remove(stateNode);
    }

    public void DeleteFriendNode_To_FromArror(StateNode stateNode)
    {
        if (_toStateNodes.ContainsKey(stateNode) == false)
        {
            Debug.Assert(false, "삭제할건데 없다고?");
            Debug.Break();
            return;
        }

        _toStateNodes.Remove(stateNode);
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
        style.position = Position.Absolute;
        style.width = 20;
        style.height = 20;

        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);

        _arrowHead = new VisualElement()
        {
            style =
                {
                    position = Position.Absolute,
                    width = 0,
                    height = 0,

                    borderLeftWidth = 10,
                    borderRightWidth = 10,
                    borderBottomWidth = 20,

                    borderLeftColor = Color.clear,
                    borderRightColor = Color.clear,
                    borderBottomColor = Color.green,
                }
        };
        Add(_arrowHead);

        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);
    }

    public void ShowMe()
    {
        style.borderBottomColor = Color.green; // 화살표 색
    }

    public void HideMe()
    {
        style.borderBottomColor = Color.clear; // 화살표 색
    }

    private VisualElement _arrowHead = null;
}


public class Arrow_Head_Ready : Arrow_Head
{
    public Arrow_Head_Ready(VisualElement root, GraphEditorWithUIToolkit window, Arrow_Ready arrow, Vector3 mousePosition) : base(root, window, mousePosition)
    {
        _arrow = arrow;

        this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
        {
            menuEvent.menu.AppendAction("Edit_Condition", _ => _arrow.EditCondition());
            menuEvent.menu.AppendAction("Delete_Line", _ => _arrow.DeleteLine());
            menuEvent.menu.AppendAction("Copy_Conditions", _ => _arrow.CopyToClipboard());
            menuEvent.menu.AppendAction("Paste_Conditions", _ => _arrow.PasteFromClipboard());
        }));
    }

    private Arrow_Ready _arrow = null;
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
            _arrowHead.MoveTo(mouseAndStateNodeCenter);
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


//[Serializable]
//public class ConditionAssetWrapper
//{
//    public ConditionAsset _conditionAsset = null;
//    public bool _goal = false;
//}


public interface IConditionExist
{
    public abstract List<ConditionAssetWrapper> GetCondition();
    public abstract void WindowOff();
    public abstract void TurnOnConditionModifyWindow(EditorWindow window);
    public abstract void OnDetach_ConditionModifyWindow();
}

/*----------------------------------------------------
|NOTI|닥스에서 가져왔어요. 분석하면서 만들기-------------
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

            _objectField.RegisterValueChangedCallback(evt =>
            {
                _targetList[(int)_objectField.userData]._conditionAsset = (ConditionAsset)_objectField.value;
            });
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

            _goalToggle.RegisterValueChangedCallback(evt =>
            {
                _targetList[(int)_goalToggle.userData]._goal = _goalToggle.value;
            });
            goalToggleContainer.Add(_goalToggle);
        }

        public void Init(List<ConditionAssetWrapper> targetList)
        {
            _targetList = targetList;
        }

        private ObjectField _objectField = null;
        private Toggle _goalToggle = null;
        private List<ConditionAssetWrapper> _targetList = null;
    }

    private ListView _conditionDataListView = null;
    private VisualElement _buttons = null;
    private List<ConditionAssetWrapper> _conditionData = null;
    private IConditionExist _hasConditionElement = null;

    private void OnDisable()
    {
        _hasConditionElement?.WindowOff();
    }

    public void Init(List<ConditionAssetWrapper> dataTarget, IConditionExist canHaveCondition)
    {
        //데이터 설정
        {
            _conditionData = dataTarget;
            _hasConditionElement = canHaveCondition;
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
                newElement.Init(_conditionData);
                return newElement;
            };

            _conditionDataListView.makeItem = createListElelemt;

            Action<VisualElement, int> dataChanged = (VisualElement element, int index) =>
            {
                ConditionDataVisualElement casted = (ConditionDataVisualElement)element;

                ObjectField conditionAssetField = casted.Q<ObjectField>("_conditionAsset");
                if (conditionAssetField == null)
                {
                    Debug.Assert(false, "conditionAssetField 없다고?");
                    Debug.Break();
                    return;
                }
                conditionAssetField.userData = index;
                conditionAssetField.value = _conditionData[index]._conditionAsset;

                Toggle goalField = casted.Q<Toggle>("_goal");
                if (goalField == null)
                {
                    Debug.Assert(false, "goalField 없다고?");
                    Debug.Break();
                    return;
                }
                goalField.userData = index;
                goalField.value = _conditionData[index]._goal;
                //_conditionDataListView.RefreshItem(index);
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
                _conditionData.Add(new ConditionAssetWrapper());
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
        //_conditionDataListView.Rebuild();
        _conditionDataListView.RefreshItems();
    }
}//----
//----------------------------------------------------



/*--------------------------------------------------
|NOTI| 드래그 완료된, 준비된 화살표
--------------------------------------------------*/
#region Arrow_Ready
public class Arrow_Ready : MyVisualElement, IConditionExist
{
    private bool _isWindowShow = false;
    private EditorWindow _conditionModifierWindow = null;
    public List<ConditionAssetWrapper> GetCondition()
    {
        return _conditionData;
    }

    public void WindowOff()
    {
        _isWindowShow = false;
    }

    public void TurnOnConditionModifyWindow(EditorWindow window)
    {
        _isWindowShow = true;
        _conditionModifierWindow = window;
    }

    public void OnDetach_ConditionModifyWindow()
    {
        if (_isWindowShow == true)
        {
            _conditionModifierWindow.Close();
        }
    }


    public Arrow_Ready(VisualElement root, GraphEditorWithUIToolkit window, StateNode fromNode, StateNode toNode) : base(root, window)
    {
        Init(fromNode, toNode);
    }

    public void Init(StateNode fromNode, StateNode toNode)
    {
        _fromNode = fromNode;
        _toNode = toNode;


        _toNode.LinkingNode_From(_fromNode, this);
        _fromNode.LinkingNode_To(_toNode, this);


        //style 설정
        {
            style.position = Position.Absolute;
            style.backgroundColor = Color.green;
            style.height = 2.0f;
        }

        //중간에 꽃힐 화살표 비쥬얼 엘리먼트 생성
        {
            _arrowHead = new Arrow_Head_Ready(_root, _window, this, Vector2.zero);
        }


        _window.AddElement(HierarchycalWindow.Layer.Layer_Object_One, this);

        _window.ArrowAdded(this);


        this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
        {
            menuEvent.menu.AppendAction("Edit_Condition", _ => EditCondition());
            menuEvent.menu.AppendAction("Delete_Line", _ => DeleteLine());
            menuEvent.menu.AppendAction("Copy_Conditions", _ => CopyToClipboard());
            menuEvent.menu.AppendAction("Paste_Conditions", _ => PasteFromClipboard());
        }));

        RegisterCallback<DetachFromPanelEvent>(OnDetach);

        if (_window.IsArrowShow(this) == false)
        {
            HideMe();
        }

        //그래도 일단 한번 업데이트는 해준다
        {
            ArrowPositionUpdate();
        }

        _actions += ArrowPositionUpdate;
    }


    private StateNode _fromNode = null;
    public StateNode _FromNode => _fromNode;

    private StateNode _toNode = null;
    public StateNode _ToNode => _toNode;

    private Arrow_Head_Ready _arrowHead = null;

    private List<ConditionAssetWrapper> _conditionData = new List<ConditionAssetWrapper>();
    public List<ConditionAssetWrapper> _ConditionDatas => _conditionData;

    private bool CheckInvocation(Action target)
    {
        foreach (var invokList in _actions.GetInvocationList())
        {
            if (invokList.Method == target.Method)
            {
                return true;
            }
        }
        return false;
    }

    public void ShowMe()
    {
        {
            //날 화면상에 보여준다
            //if (CheckInvocation(ArrowPositionUpdate) == true)
            //{
            //    Debug.Assert(false, "이미 화면에 보여주고있었나요? 심각한 오류다");
            //    Debug.Break();
            //    return;
            //}
        }

        _actions += ArrowPositionUpdate;
        style.backgroundColor = Color.green;

        _arrowHead.ShowMe();
    }

    public void HideMe()
    {
        {
            //날 화면상에 보여준다
            //if (CheckInvocation(ArrowPositionUpdate) == false)
            //{
            //    Debug.Assert(false, "이미 안보여주고있었나요? 심각한 오류다");
            //    Debug.Break();
            //    return;
            //}
        }

        _actions -= ArrowPositionUpdate;
        style.backgroundColor = Color.clear;

        _arrowHead.HideMe();
    }




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
            _arrowHead.MoveTo(centerPosition);
            _arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }




    public void EditCondition()
    {
        if (_isWindowShow == true)
        {
            return;
        }

        SubEditorWindow_ConditionModify window = EditorWindow.CreateInstance<SubEditorWindow_ConditionModify>();

        window.titleContent = new GUIContent("SubEditorWindow_ConditionModify");
        window.Show();
        window.Init(_conditionData, this);

        TurnOnConditionModifyWindow(window);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }


    public void CopyToClipboard()
    {
        string json = JsonUtility.ToJson(new Wrapper<ConditionAssetWrapper> { Items = _conditionData });
        GUIUtility.systemCopyBuffer = json;
    }

    public List<T> GetDataFromClipboard<T>()
    {
        string json = GUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(json) == true)
        {
            return new List<T>();
        }

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }


    public void PasteFromClipboard()
    {
        List<ConditionAssetWrapper> beforeList = new List<ConditionAssetWrapper>();
        beforeList.Clear();
        beforeList.AddRange(_conditionData);


        List<ConditionAssetWrapper> coppied = GetDataFromClipboard<ConditionAssetWrapper>();
        if (coppied.Count <= 0)
        {
            return;
        }

        _conditionData.Clear();
        _conditionData.AddRange(coppied);
    }


    public void DeleteLine()
    {
        _toNode.DeleteFriendNode_From_FromArror(_fromNode);
        _fromNode.DeleteFriendNode_To_FromArror(_toNode);
        _root.Remove(this);
    }

    private void OnDetach(DetachFromPanelEvent detachEvent)
    {
        _window.DeleteArrow(this);

        _actions = null;

        OnDetach_ConditionModifyWindow();

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
