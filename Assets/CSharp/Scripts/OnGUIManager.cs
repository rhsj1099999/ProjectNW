using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnGUIManager : SubManager
{
    private static OnGUIManager _instance = null;

    private GUIStyle _style = null;
    private List<int> _frameDebug = new List<int>();
    private int _currentFrame = 0;
    private float _currentTimeAcc = 0;
    private Dictionary<string, string> _debugStrings = new Dictionary<string, string>();
    [SerializeField] private float _fontSize = 10.0f;
    private float _lineSize = 5.0f;

    public static OnGUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<OnGUIManager>();
                singletonObject.name = typeof(OnGUIManager).ToString();

                DontDestroyOnLoad(singletonObject);
            }

            return _instance;
        }
    }

    public override void SubManagerAwake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);

        _style = new GUIStyle();
        _style.fontSize = (int)_fontSize;
        _style.normal.textColor = Color.red;
    }

    override public void SubManagerUpdate()
    {
        _currentTimeAcc += Time.deltaTime;
        _style.fontSize = (int)_fontSize;

        if (_currentTimeAcc > 0.2f)
        {
            if (_frameDebug.Count >= 5)
            {
                _frameDebug.Remove(_frameDebug[0]);
            }

            _frameDebug.Add((int)(1.0f / Time.deltaTime));

            if (_frameDebug.Count >= 5)
            {
                int total = 0;
                foreach (int i in _frameDebug)
                {
                    total += i;
                }
                _currentFrame = total / 5;
            }

            _currentTimeAcc = 0.0f;
        }
    }

    private List<RaycastResult> GetUIElementsUnderMouse()
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RayCastAll(ref raycastResults);
        }

        return raycastResults;
    }


    private void OnGUI()
    {
        if (_style != null)
        {
            Rect messageStartRect = new Rect(10, 10, 200, -99999); //Height 무관하다
            float totalHeight = _fontSize + _lineSize;

            //기본 메세지들
            {
                ShowString("디버깅용", ref messageStartRect, totalHeight);
                //ShowString("겹친UI 개수 : " + GetUIElementsUnderMouse().Count, ref messageStartRect, totalHeight);
                Vector2 mousePosition = Input.mousePosition;
                ShowString("마우스InputPosition : X|" + mousePosition.x + " Y|" + mousePosition.y, ref messageStartRect, totalHeight);
                ShowString("현재프레임 : " + _currentFrame + "FPS", ref messageStartRect, totalHeight);
            }

            //추가한 메세지들
            foreach (KeyValuePair<string, string> pair in _debugStrings)
            {
                ShowString(pair.Key + pair.Value, ref messageStartRect, totalHeight);
            }
        }
    }

    private void ShowString(string message, ref Rect messagePosition, float yFontSize)
    {
        messagePosition.height = yFontSize;
        GUI.Label(messagePosition, message, _style);
        messagePosition.y += yFontSize;
    }



    public void AddDebugString(string key, string val)
    {
        if ( _debugStrings.ContainsKey(key) == true)
        {
            _debugStrings[key] = val;
            return;
        }

        _debugStrings.Add(key, val);
    }

    public void DeleteDebugString(string key)
    {
        if (_debugStrings.ContainsKey(key) == false)
        {
            return;
        }

        _debugStrings.Remove(key);
    }




}
