using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyPressType
{
    Pressed,
    Hold,
    Released,
    None,
};

public enum ComboCommandKeyType
{
    TargetingFront,
    TargetingBack,
    TargetingLeft,
    TargetingRight,

    LeftClick,
    RightClick,
    UltClikc,
    EleClikc,

    CtrlLeftClick,
    CtrlRightClick,
    CtrlUltClick,
    CtrlEleClick,

    SubLeftClick,
    SubRightClick,
    SubUltClick,
    SubEleClick,

    LeftUp,
    RightUp,
    UltUp,
    EleUp,

    CtrlLeftUp,
    CtrlRightUp,
    CtrlUltUp,
    CtrlEleUp,

    SubLeftUp,
    SubRightUp,
    SubUltUp,
    SubEleUp,
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



public class CustomKeyManager : SubManager<CustomKeyManager>
{
    /*-----------------------------------------------------
    유니티가 지원하지 않는 부차적인 기능들을 하는 클래스입니다
    -----------------------------------------------------*/
    public class KeyInputDesc
    {
        public KeyPressType _pressType = KeyPressType.None;
        public float _holdedSecond = 0.0f;
        public float _notUsingTime = 0.0f;
        public bool _pressed = false;
        public bool _deleteTrigger = false;
    }

    private float _deleteDescThreshold = 300.0f; //5분동안 사용되지 않았으면 제거한다




    private Dictionary<KeyCode, KeyInputDesc> _usingKeyInputDesc = new Dictionary<KeyCode, KeyInputDesc>();
    private List<KeyInputDesc> _currInputDescs = new List<KeyInputDesc>();
    private PlayerScript _playerOnlyOne = null;
    public void LinkPlayer(PlayerScript player) {_playerOnlyOne = player;}
    private LinkedList<ComboCommandKeyDesc> _comboCommandRecorder = new LinkedList<ComboCommandKeyDesc>();
    public LinkedList<ComboCommandKeyDesc> GetComboCommandKeyDescs() { return _comboCommandRecorder; }
    public void ClearKeyRecord() 
    { 
        _comboCommandRecorder.Clear();
        _attackKeyTry = false;
    }

    private bool _attackKeyTry = false;
    public bool _AttackKeyTry => _attackKeyTry;

    private KeyCode _leftClick = KeyCode.Mouse0;
    private KeyCode _rightClick = KeyCode.Mouse1;
    private KeyCode _eleUse = KeyCode.E;
    private KeyCode _ultUse = KeyCode.Q;

    private KeyCode _subUseWith = KeyCode.X;
    private KeyCode _ctrlUseWith = KeyCode.LeftControl;



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



    private void ComboKeyCommandUpdate2()
    {
        //w, a, s, d, Click, Right Clikc

        int keyDebugCount = 0;

        if (UIManager.Instance.IsConsumeInput() == true)
        {
            return;
        }

        if (Input.GetKeyDown(_leftClick) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlLeftClick, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
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
        else if (Input.GetKeyUp(_leftClick) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlLeftUp, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubLeftUp, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.LeftUp, Time.time));
                keyDebugCount++;
            }
        }

        if (Input.GetKeyDown(_rightClick) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlRightClick, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
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
        else if (Input.GetKeyUp(_rightClick) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlRightUp, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubRightUp, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.RightUp, Time.time));
                keyDebugCount++;
            }
        }

        if (Input.GetKeyDown(_ultUse) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlUltClick, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubUltClick, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.UltClikc, Time.time));
                keyDebugCount++;
            }
        }
        else if (Input.GetKeyUp(_ultUse) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlUltUp, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubUltUp, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.UltUp, Time.time));
                keyDebugCount++;
            }
        }

        if (Input.GetKeyDown(_eleUse) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlEleUp, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubEleClick, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.EleClikc, Time.time));
                keyDebugCount++;
            }
        }
        else if (Input.GetKeyUp(_eleUse) == true)
        {
            _attackKeyTry = true;

            if (Input.GetKey(_ctrlUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.CtrlEleUp, Time.time));
                keyDebugCount++;
            }
            else if (Input.GetKey(_subUseWith) == true)
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.SubEleUp, Time.time));
                keyDebugCount++;
            }
            else
            {
                _comboCommandRecorder.AddLast(new ComboCommandKeyDesc(ComboCommandKeyType.EleUp, Time.time));
                keyDebugCount++;
            }
        }


        if (true/*플레이어가 락온중이라면*/)
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
    }

    public override void SubManagerInit()
    {
        SingletonAwake();
    }

    public override void SubManagerUpdate()
    {
        NormalKeyUpdate();
        ComboKeyCommandUpdate2();
    }

    public override void SubManagerFixedUpdate() {}
    public override void SubManagerLateUpdate() {}
    public override void SubManagerStart() {}
}
