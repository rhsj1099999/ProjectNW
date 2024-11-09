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

    public ComboCommandKeyType _type = ComboCommandKeyType.TargetingFront;
    public float _inputtedTime = 0.0f;
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
    ����Ƽ�� �������� �ʴ� �������� ��ɵ��� �ϴ� Ŭ�����Դϴ�
    -----------------------------------------------------*/
    public class KeyInputDesc
    {
        public KeyPressType _pressType = KeyPressType.None;
        public float _holdedSecond = 0.0f;
        public float _notUsingTime = 0.0f;
        public bool _pressed = false;
        public bool _deleteTrigger = false;
    }

    private float _deleteDescThreshold = 300.0f; //5�е��� ������ �ʾ����� �����Ѵ�
    private bool _isRecordAttackKeyRestrained = false; //True = AttackŰ�� �޴´�. //false = �� �޴´�
    private Dictionary<ComboCommandKeyType, int> _afterAttackKeyRestrained = new Dictionary<ComboCommandKeyType, int>();
    public void SetAttackKeyRestrained(bool target) 
    {
        _afterAttackKeyRestrained.Clear();

        _isRecordAttackKeyRestrained = target;

        if (_isRecordAttackKeyRestrained == true &&
            _comboCommandRecorder.Last.Value._type >= ComboCommandKeyType.LeftClick && _comboCommandRecorder.Last.Value._type <= ComboCommandKeyType.SubRightClick)
        {
            _afterAttackKeyRestrained.Add(_comboCommandRecorder.Last.Value._type, 1);
        }
    }



    private Dictionary<KeyCode, KeyInputDesc> _usingKeyInputDesc = new Dictionary<KeyCode, KeyInputDesc>();
    private List<KeyInputDesc> _currInputDescs = new List<KeyInputDesc>();
    private PlayerScript _playerOnlyOne = null;
    public void LinkPlayer(PlayerScript player) {_playerOnlyOne = player;}
    private LinkedList<ComboCommandKeyDesc> _comboCommandRecorder = new LinkedList<ComboCommandKeyDesc>();
    public LinkedList<ComboCommandKeyDesc> GetComboCommandKeyDescs() { return _comboCommandRecorder; }


    
    //�÷��̾ �޺��� üũ�ϱ� ���Ͽ� ������ �����ϸ� _afterAttackKeyRestrained�̰��� ��´�.


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
            //��������� �����ϴ�.
            _usingKeyInputDesc.Add(target, new KeyInputDesc());
        }
        else
        {
            _usingKeyInputDesc[target]._deleteTrigger = false;
        }
        
        return _usingKeyInputDesc[target];
    }

    private void FixedUpdate()
    {
        ComboKeyCommandUpdate();
    }

    private void Update()
    {
        NormalKeyUpdate();

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
                    //���� �����ӿ� �������� �����ϴ�.
                    keyInput.Value._pressType = KeyPressType.Pressed;
                    keyInput.Value._notUsingTime = 0.0f;
                }
                else
                {
                    //���� �����ӿ� ���Ⱦ����ϴ�.
                    keyInput.Value._pressType = KeyPressType.Hold;
                }
                keyInput.Value._pressed = true;
            }
            else
            {
                if (keyInput.Value._pressed == false)
                {
                    //���� �����ӿ� �������� �����ϴ�.
                    keyInput.Value._pressType = KeyPressType.None;
                    keyInput.Value._notUsingTime += Time.deltaTime;
                    if (keyInput.Value._notUsingTime >= _deleteDescThreshold)
                    {
                        keyInput.Value._deleteTrigger = true;
                    }
                }
                else
                {
                    //���� �����ӿ� ���Ⱦ����ϴ�.
                    keyInput.Value._pressType = KeyPressType.Released;
                    keyInput.Value._holdedSecond = 0.0f;
                    keyInput.Value._pressed = false;
                }
            }
        }
    }

    public bool AttackKeyRestrainedExist(ComboCommandKeyType target)
    {
        return _afterAttackKeyRestrained.ContainsKey(target);
    }

    private void ComboKeyCommandUpdate()
    {
        //w, a, s, d, Click, Right Clikc
        int keyDebugCount = 0;
        ComboCommandKeyType type = ComboCommandKeyType.TargetingFront;

        if (Input.GetKeyDown(KeyCode.Q) == true)
        {
            
            if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                type = ComboCommandKeyType.CtrlLeftClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlLeftClick, Time.time));
                }
                keyDebugCount++;
            }
            else if (Input.GetKey(KeyCode.X) == true)
            {
                type = ComboCommandKeyType.SubLeftClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubLeftClick, Time.time));
                }
                keyDebugCount++;
            }
            else
            {
                type = ComboCommandKeyType.LeftClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.LeftClick, Time.time));
                }
                keyDebugCount++;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) == true)
        {
            if (Input.GetKey(KeyCode.LeftControl) == true)
            {
                type = ComboCommandKeyType.CtrlRightClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlRightClick, Time.time));
                }
                keyDebugCount++;
            }
            else if (Input.GetKey(KeyCode.X) == true)
            {
                type = ComboCommandKeyType.SubRightClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubRightClick, Time.time));
                }
                keyDebugCount++;
            }
            else
            {
                type = ComboCommandKeyType.RightClick;
                if (_isRecordAttackKeyRestrained == true)
                {
                    if (_afterAttackKeyRestrained.ContainsKey(type) == true)
                    {
                        _afterAttackKeyRestrained[type]++;
                    }
                    else
                    {
                        _afterAttackKeyRestrained.Add(type, 0);
                    }
                }
                else
                {
                    _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.RightClick, Time.time));
                }
                keyDebugCount++;
            }
        }

        if (_isRecordAttackKeyRestrained == true)
        {
            return;
        }

        bool isPlayerTargeting = true;

        if (isPlayerTargeting == true)
        {
            if (Input.GetKeyDown(KeyCode.W) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingFront, Time.time));
                keyDebugCount++;
            }

            else if (Input.GetKeyDown(KeyCode.S) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingBack, Time.time));
                keyDebugCount++;
            }

            if (Input.GetKeyDown(KeyCode.A) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingLeft, Time.time));
                keyDebugCount++;
            }

            else if (Input.GetKeyDown(KeyCode.D) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.TargetingRight, Time.time));
                keyDebugCount++;
            }
        }

        Debug.Assert(keyDebugCount < 2, "delta Time�� �������� ���ϴ� Ű �Է¼ӵ��� �����ߴ�");
    }
}
