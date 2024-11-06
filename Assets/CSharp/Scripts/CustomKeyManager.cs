using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComboCommandKeyType
{
    TargetingFront,
    TargetingBack,
    TargetingLeft,
    TargetingRight,
    LeftClick,
    RightClick,
    CtrlLeftClick,
    CtrlRightClick,
    SubLeftClick,
    SubRightClick,
};

public class ComboCommandKeyDesc
{
    public ComboCommandKeyDesc(ComboCommandKeyType type, float time)
    {
        _type = type;
        _inputtedTime = time;
    }

    ComboCommandKeyType _type = ComboCommandKeyType.TargetingFront;
    float _inputtedTime = 0.0f;
}

public enum KeyPressType
{
    Pressed,
    Hold,
    Released,
    None,
};

public class CustomKeyManager : MonoBehaviour
{
    /*-----------------------------------------------------
    유니티가 지원하지 않는 부차적인 기능들을 하는 클래스입니다
    -----------------------------------------------------*/


    private float _deleteDescThreshold = 300.0f; //5분동안 사용되지 않았으면 제거한다

    public class KeyInputDesc
    {
        public KeyPressType _pressType = KeyPressType.None;
        public float _holdedSecond = 0.0f;
        public float _notUsingTime = 0.0f;
        public bool _pressed = false;
        public bool _deleteTrigger = false;
    }

    private Dictionary<KeyCode, KeyInputDesc> _usingKeyInputDesc = new Dictionary<KeyCode, KeyInputDesc>();
    private List<KeyInputDesc> _currInputDescs = new List<KeyInputDesc>();
    private PlayerScript _playerOnlyOne = null;
    public void LinkPlayer(PlayerScript player) {_playerOnlyOne = player;}
    private LinkedList<ComboCommandKeyDesc> _comboCommandRecorder = new LinkedList<ComboCommandKeyDesc>();
    public LinkedList<ComboCommandKeyDesc> GetComboCommandKeyDescs() { return _comboCommandRecorder; }

    private static CustomKeyManager _instance = null;
    public static CustomKeyManager Instance
    {
        get 
        { 
            if (_instance == null)
            {
                GameObject newGameObject = new GameObject("CustomKeyManager");
                DontDestroyOnLoad(newGameObject);
                CustomKeyManager component = newGameObject.AddComponent<CustomKeyManager>();
                _instance = component;
            }
            return _instance; 
        }
    }
    

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public KeyInputDesc GetKeyInputDesc(KeyCode target)
    {
        if (_usingKeyInputDesc.ContainsKey(target) == false)
        {
            //사용한적이 없습니다.
            _usingKeyInputDesc.Add(target, new KeyInputDesc());
        }
        else
        {
            _usingKeyInputDesc[target]._deleteTrigger = false;
        }
        
        return _usingKeyInputDesc[target];
    }

    private void Update()
    {
        NormalKeyUpdate();
        ComboKeyCommandUpdate();
    }



    private void NormalKeyUpdate()
    {
        foreach (KeyValuePair<KeyCode, KeyInputDesc> keyInput in _usingKeyInputDesc)
        {
            if (keyInput.Value._deleteTrigger == true)
            {
                continue;
            }

            if (Input.GetKey(keyInput.Key) == true)
            {
                keyInput.Value._holdedSecond += Time.deltaTime;

                if (keyInput.Value._pressed == false)
                {
                    //직전 프레임에 눌린적이 없습니다.
                    keyInput.Value._pressType = KeyPressType.Pressed;
                    keyInput.Value._notUsingTime = 0.0f;
                }
                else
                {
                    //직전 프레임에 눌렸었습니다.
                    keyInput.Value._pressType = KeyPressType.Hold;
                }
                keyInput.Value._pressed = true;
            }
            else
            {
                if (keyInput.Value._pressed == false)
                {
                    //직전 프레임에 눌린적이 없습니다.
                    keyInput.Value._pressType = KeyPressType.None;
                    keyInput.Value._notUsingTime += Time.deltaTime;
                    if (keyInput.Value._notUsingTime >= _deleteDescThreshold)
                    {
                        keyInput.Value._deleteTrigger = true;
                    }
                }
                else
                {
                    //직전 프레임에 눌렸었습니다.
                    keyInput.Value._pressType = KeyPressType.Released;
                    keyInput.Value._holdedSecond = 0.0f;
                    keyInput.Value._pressed = false;
                }
            }
        }
    }


    private void ComboKeyCommandUpdate()
    {
        //w, a, s, d, Click, Right Clikc
        int keyDebugCount = 0;


        if (Input.GetKeyDown(KeyCode.Q) == true)
        {
            if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlLeftClick, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(KeyCode.X) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubLeftClick, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.LeftClick, Time.time));
                keyDebugCount++;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) == true)
        {
            if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlRightClick, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(KeyCode.X) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubRightClick, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.RightClick, Time.time));
                keyDebugCount++;
            }
        }

        bool isPlayerTargeting = false;

        if (isPlayerTargeting == true)
        {
            if (Input.GetKeyDown(KeyCode.W) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingFront, Time.time));
                keyDebugCount++;
            }

            if (Input.GetKeyDown(KeyCode.S) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingBack, Time.time));
                keyDebugCount++;
            }

            if (Input.GetKeyDown(KeyCode.A) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingLeft, Time.time));
                keyDebugCount++;
            }

            if (Input.GetKeyDown(KeyCode.D) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingRight, Time.time));
                keyDebugCount++;
            }
        }

        Debug.Assert(keyDebugCount < 2, "delta Time이 구분하지 못하는 키 입력속도에 도달했다");
    }
}
