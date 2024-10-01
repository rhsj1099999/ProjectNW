using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnGUIManager : MonoBehaviour
{
    void Update()
    {
    }

    void Start()
    {
    }

    public List<RaycastResult> GetUIElementsUnderMouse()
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

            /*---------------------
             기본 메세지들
            ---------------------*/
            ShowString("디버깅용", ref messageStartRect, 35.0f);

            ShowString("겹친UI 개수 : " + GetUIElementsUnderMouse().Count, ref messageStartRect, 35.0f);
            Vector2 mousePosition = Input.mousePosition;
            ShowString("마우스InputPosition : X|" + mousePosition.x + " Y|" + mousePosition.y, ref messageStartRect, 35.0f);

            foreach (KeyValuePair<string, string> pair in _debugStrings)
            {
                GUI.Label(new Rect(10, 10, 200, 50), "키|-|" + pair.Key + "|-|값:" + pair.Value, _style);
            }
        }
    }

    private void ShowString(string message, ref Rect messagePosition, float yFontSize)
    {
        messagePosition.height = yFontSize;
        GUI.Label(messagePosition, message, _style);
        messagePosition.y += yFontSize;
    }

    private Dictionary<string, string> _debugStrings = new Dictionary<string, string>();

    private static OnGUIManager _instance = null;

    public void AddDebugString(string key, string val)
    {
        if ( _debugStrings.ContainsKey(key) )
        {
            Debug.Assert(_debugStrings[key] == null ); //제목이 이미 있다
            return;
        }

        _debugStrings.Add(key, val);
    }

    public void DeletaDebugString(string key)
    {
        if (_debugStrings.ContainsKey(key) == false)
        {
            return;
        }

        _debugStrings.Remove(key);
    }

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

    private void Awake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);

        _style = new GUIStyle();
        _style.fontSize = 20;
        _style.normal.textColor = Color.white;
    }

    private GUIStyle _style = null;
    [SerializeField] GraphicRaycaster _rayCaster = null;
    [SerializeField] EventSystem _eventSystem = null;
}
