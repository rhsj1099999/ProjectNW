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
}
