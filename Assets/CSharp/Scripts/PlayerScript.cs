using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;


public class CoroutineLock
{
    public bool _isEnd = false;
}


public enum AnimatorLayerTypes
{
    FullBody = 0,
    LeftHand,
    RightHand,
    Head,
    Body,
    LeftLeg,
    RightLeg,
    End,
}


public enum WeaponGrabFocus
{
    Normal,
    RightHandFocused,
    LeftHandFocused,
    DualGrab,
}


public class AnimatorBlendingDesc
{
    public AnimatorBlendingDesc(AnimatorLayerTypes type, Animator ownerAnimator)
    {
        _targetPart = type;
        _ownerAnimator = ownerAnimator;
    }

    AnimatorLayerTypes _targetPart = AnimatorLayerTypes.FullBody;
    public float _blendTarget = 0.0f;
    public float _blendTarget_Sub = 0.0f;
    public Animator _ownerAnimator = null;
    public bool _isUsingFirstLayer = false;



    public enum BlendingCoroutineType
    {
        ActiveNextLayer,
        ActiveAllLayer,
        DeactiveAllLayer,
    }

    public IEnumerator ChangeNextLayerWeightSubCoroutine(float transitionSpeed, BlendingCoroutineType workType, CoroutineLock coroutineLockNullable)
    {
        int targetHandMainLayerIndex = (int)_targetPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;
        bool signal = false;
        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            switch (workType)
            {
                default:
                    {
                        Debug.Break();
                    }
                    break;

                case BlendingCoroutineType.ActiveNextLayer:
                    {
                        mainLayerBlendDelta = (_isUsingFirstLayer == true)
                            ? transitionSpeed * Time.deltaTime
                            : -transitionSpeed * Time.deltaTime;

                        subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

                        _blendTarget += mainLayerBlendDelta;
                        _blendTarget_Sub += subLayerBlendDelta;

                        _blendTarget = Mathf.Clamp(_blendTarget, 0.0f, 1.0f);
                        _blendTarget_Sub = Mathf.Clamp(_blendTarget_Sub, 0.0f, 1.0f);

                        _ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, _blendTarget);
                        _ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, _blendTarget_Sub);

                        float target = (_isUsingFirstLayer == true)
                            ? _blendTarget
                            : _blendTarget_Sub;

                        if (target >= 1.0f)
                        {
                            signal = true;
                        }
                    }
                    break;

                case BlendingCoroutineType.ActiveAllLayer:
                    {
                        mainLayerBlendDelta = transitionSpeed * Time.deltaTime;
                        subLayerBlendDelta = mainLayerBlendDelta;

                        _blendTarget += mainLayerBlendDelta;
                        _blendTarget_Sub += subLayerBlendDelta;

                        _blendTarget = Mathf.Clamp(_blendTarget, 0.0f, 1.0f);
                        _blendTarget_Sub = Mathf.Clamp(_blendTarget_Sub, 0.0f, 1.0f);

                        _ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, _blendTarget);
                        _ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, _blendTarget_Sub);

                        float minTarget = Mathf.Min(_blendTarget, _blendTarget_Sub);
                        if (minTarget >= 1.0f)
                        {
                            signal = true;
                        }
                    }
                    break;

                case BlendingCoroutineType.DeactiveAllLayer:
                    {
                        mainLayerBlendDelta = -transitionSpeed * Time.deltaTime;
                        subLayerBlendDelta = mainLayerBlendDelta;

                        _blendTarget += mainLayerBlendDelta;
                        _blendTarget_Sub += subLayerBlendDelta;

                        _blendTarget = Mathf.Clamp(_blendTarget, 0.0f, 1.0f);
                        _blendTarget_Sub = Mathf.Clamp(_blendTarget_Sub, 0.0f, 1.0f);

                        _ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, _blendTarget);
                        _ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, _blendTarget_Sub);

                        float maxTarget = Mathf.Max(_blendTarget, _blendTarget_Sub);
                        if (maxTarget <= 0.0f)
                        {
                            signal = true;
                        }
                    }
                    break;
            }

            if (signal == true)
            {
                break;
            }

            yield return null;
        }

        if (coroutineLockNullable != null)
        {
            coroutineLockNullable._isEnd = true;
        }
    }
}



public class PlayerScript : MonoBehaviour, IHitable
{
    /*----------------------------------
    HitAble Section
    ----------------------------------*/
    public void DealMe(int damage, GameObject caller)
    {
        Debug.Assert(_stateContoller != null, "StateController�� null�̿����� �ȵȴ�");

        RepresentStateType representType = RepresentStateType.Hit_Fall;

        //�������� ������ �󸶳� ������ �¾Ҵ��� ���
        {
            if (damage < 2)
            {
                representType = RepresentStateType.Hit_L;
            }
            else if (damage < 5)
            {
                representType = RepresentStateType.Hit_H;
            }
            else
            {
                representType = RepresentStateType.Hit_Fall;
            }
        }

        //StateController���� ���� ���� ����
        _stateContoller.TryChangeState(representType);
    }




    //��ǥ ������Ʈ
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> �̰� �ٸ� ������Ʈ�� ������(���� ��������ִ°� EquipmentBoard Ȥ�� Inventory)
    [SerializeField] private List<GameObject> _tempLeftWeaponPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> _tempRightWeaponPrefabs = new List<GameObject>();

    private KeyCode _changeRightHandWeaponHandlingKey = KeyCode.B;
    private KeyCode _changeLeftHandWeaponHandlingKey = KeyCode.V;
    private KeyCode _useItemKeyCode1 = KeyCode.N;
    private KeyCode _useItemKeyCode2 = KeyCode.M;
    private KeyCode _useItemKeyCode3 = KeyCode.Comma;
    private KeyCode _useItemKeyCode4 = KeyCode.Period;

    private int _currLeftWeaponIndex = 0;
    private int _currRightWeaponIndex = 0;
    private int _tempMaxWeaponSlot = 3;


    private GameObject _tempCurrLeftWeapon = null;
    private GameObject _tempCurrRightWeapon = null;

    public GameObject GetLeftWeapon() { return _tempCurrLeftWeapon; }
    public GameObject GetRightWeapon() { return _tempCurrRightWeapon; }

    private WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }


    private bool _tempUsingRightHandWeapon = false; //�ֱٿ� ����� ���Ⱑ �������Դϱ�?
    public bool GetLatestWeaponUse() { return _tempUsingRightHandWeapon; }
    public void SetLatestWeaponUse(bool isRightHandWeapon)
    {
        _tempUsingRightHandWeapon = isRightHandWeapon;
    }

    //����� ĳ���͸޽��� �ִϸ����͸� �����ֱ� ������ �ִϸ����͸� �����ִ� ���ӿ�����Ʈ�� ĳ���� �޽���
    private GameObject _characterModelObject = null; //�ִϸ����ʹ� �갡 �����ִ�
    public Animator GetAnimator() { return _animator; }


    //Aim System
    private AimScript2 _aimScript = null;
    private bool _isAim = false;
    private bool _isTargeting = false;
    public bool GetIsTargeting() { return _isTargeting; }


    //Animator Secton -> �̰� �ٸ� ������Ʈ�� ������
    private Animator _animator = null;
    private AnimatorOverrideController _overrideController = null;

    private List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    [SerializeField] private List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();

        //Animation_TotalBlendSection
        private bool _corutineStarted = false;
        private float _blendTarget = 0.0f;
        [SerializeField] private float _transitionSpeed = 5.0f;

        //Animation_WeaponBlendSection
        private float _blendTarget_Weapon = 0.0f;
        [SerializeField] private float _transitionSpeed_Weapon = 20.0f;



    private AnimationClip _currAnimClip = null;
    private List<bool> _currentBusyAnimatorLayer = new List<bool>();
    [SerializeField] private int _currentBusyAnimatorLayer_BitShift = 0;
    private int _currentLayerIndex = 0;
    private int _maxLayer = 2;


    [SerializeField] private AnimationClip _tempWeaponHandling_NoPlay = null;


    private void Awake()
    {
        _inputController = GetComponent<InputController>();
        Debug.Assert( _inputController != null, "��ǲ ��Ʈ�ѷ��� ����");

        _characterMoveScript2 = GetComponent<CharacterMoveScript2>();
        Debug.Assert(_characterMoveScript2 != null, "CharacterMove ������Ʈ ����");

        _stateContoller = GetComponent<StateContoller>();
        Debug.Assert(_stateContoller != null, "StateController�� ����");

        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "StateController�� ����");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
        _characterModelObject = _animator.gameObject;

        /*-----------------------------------------------------------------
        |NOTI| �� ���� �Ӹ� �ƴ϶� �׳� ���������� �����ؼ��� TPS �ý����� �غ��Ѵ�
        -----------------------------------------------------------------*/
        ReadyAimSystem();
        _aimScript.enabled = false;


        for (int i = 0; i < (int)AnimatorLayerTypes.End; i++)
        {
            _partBlendingDesc.Add(null);
            _currentBusyAnimatorLayer.Add(false);
        }

        foreach (var type in _usingBodyPart)
        {
            if (_partBlendingDesc[(int)type] != null)
            {
                continue;
            }

            _partBlendingDesc[(int)type] = new AnimatorBlendingDesc(type, _animator);
        }
    }

    public List<bool> GetCurrentBusyAnimatorLayer()
    {
        return _currentBusyAnimatorLayer;
    }

    public int GetCurrentBusyAnimatorLayer_BitShift()
    {
        return _currentBusyAnimatorLayer_BitShift;
    }

    private void Start()
    {
        State currState = _stateContoller.GetCurrState();
        AnimationClip currAnimationClip = currState.GetStateDesc()._stateAnimationClip;
        _currAnimClip = currAnimationClip;
    }

    private void Update()
    {
        //Ÿ�ӵ����
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.L))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.O))  // RŰ�� ������ ���� �ӵ��� �������� ����
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        //�ӽ� Hit ����� �ڵ�
        {
            if (Input.GetKeyDown(KeyCode.H) == true)
            {
                DealMe(1, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.J) == true)
            {
                DealMe(4, this.gameObject);
                // Hit_L�� ����
            }

            if (Input.GetKeyDown(KeyCode.K) == true)
            {
                DealMe(100, this.gameObject);
                // Hit_Fall�� ����
            }
        }

        //�ӽ� ������ �ڵ�
        {
            ChangeWeaponHandling();
        }

        //�ӽ� ���⺯�� �ڵ�
        {
            WeaponChangeCheck2();
        }

        //�ӽ� ������ ��� �ڵ�
        {
            UseItemCheck();
        }

        //Temp Aim
        if (_aimScript != null && _aimScript.enabled == true)
        {
            //bool isAimed = Input.GetButton("Fire2");

            //if (isAimed != _isAim)
            //{
            //    _isAim = isAimed;

            //    if (isAimed == true)
            //    {
            //        _aimScript.OnAimState();

            //        if (_currRightWeaponIndex == 1)
            //        {
            //            _tempRightWeapons[1].TurnOnAim();
            //        }
            //    }
            //    else
            //    {
            //        _aimScript.OffAimState();

            //        if (_tempRightWeapons[_currRightWeaponIndex] != null)
            //        {
            //            _tempRightWeapons[_currRightWeaponIndex].TurnOffAim();
            //        }
            //    }
            //}
        }

        //���� ���� ������Ʈ
        {
            _stateContoller.DoWork();
        }

        //�⺻������ �߷��� ��� ������Ʈ �Ѵ�
        {
            _characterMoveScript2.GravityUpdate();
            _characterMoveScript2.ClearLatestVelocity();
        }
    }


    public void StateChanged()
    {
        ChangeAnimation(_stateContoller.GetCurrState());
    }

    private void ReadyAimSystem()
    {
        if (_aimScript == null)
        {
            _aimScript = transform.gameObject.AddComponent<AimScript2>();
        }
        _aimScript.enabled = true;
    }


    public WeaponScript GetWeaponScript(bool isRightHand)
    {
        GameObject weaponPrefab = (isRightHand == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        if (weaponPrefab == null) 
        {
            return null;
        }

        return weaponPrefab.GetComponent<WeaponScript>();
    }


    public void ChangeAnimation(State nextState)
    {
        /*----------------------------------------------------
        |NOTI| ��� �ִϸ��̼��� RightHand �������� ��ȭ�ƽ��ϴ�.
        ------------------------------------------------------*/

        AnimationClip targetClip = nextState.GetStateDesc()._stateAnimationClip;

        //�����
        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "������� �ִϸ��̼��� �Ҿ���Ƚ��ϴ�");
        }

        //�����ȯ �۾�
        {
            _currentLayerIndex++;
            if (_currentLayerIndex >= _maxLayer)
            {
                _currentLayerIndex = _currentLayerIndex % _maxLayer;
            }

            string nextNodeName = "None";

            if (_currentLayerIndex == 0)
            {
                _overrideController[MyUtil._motionChangingAnimationNames[0]] = targetClip;
                nextNodeName = "State1";
            }
            else
            {
                _overrideController[MyUtil._motionChangingAnimationNames[1]] = targetClip;
                nextNodeName = "State2";
            }

            _animator.Play(nextNodeName, _currentLayerIndex, 0.0f);

            //Start Coroutine
            if (_corutineStarted == false)
            {
                //_corutineStarted = true;
                StartCoroutine("SwitchingBlendCoroutine");
            }

            _currAnimClip = targetClip;
        }

        //���� ���̾� ����, Mirror ���� (���� �ִϸ��̼��̶��)

        //WeaponLayerChange(nextState);
    }


    private IEnumerator SwitchingBlendCoroutine()
    {
        _corutineStarted = true;

        //Layer �ε��� ����
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0�� ���̾ ����ؾ��ϳ�
                ? Time.deltaTime * -_transitionSpeed   //0�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 0.0
                : Time.deltaTime * _transitionSpeed;  //1�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 1.0

            _blendTarget += blendDelta;

            _blendTarget = Mathf.Clamp(_blendTarget, 0.0f, 1.0f);

            _animator.SetLayerWeight(1, _blendTarget);

            if (_blendTarget >= 1.0f || _blendTarget <= 0.0f)
            {
                break;
            }

            yield return null;
        }

        _corutineStarted = false;
    }






    private void WeaponLayerChange(State currState)
    {
        //�� �ϴ� �ִϸ��̼��� �ٲ�� ���⿡ ���ɴϴ�
        //if (_currentBusyAnimatorLayer_BitShift > 0)
        //{
        //    return;
        //}

        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
        int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
            ? (int)AnimatorLayerTypes.LeftHand * 2
            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
        int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
            ? (int)AnimatorLayerTypes.RightHand * 2
            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

        bool isMirrored = (_tempUsingRightHandWeapon == false);

        switch (_tempGrabFocusType)
        {
            case WeaponGrabFocus.Normal:
                {
                    //���� �ִϸ��̼��̴�
                    if (currState.GetStateDesc()._isAttackState == true)
                    {
                        int usingHandLayer = (_tempUsingRightHandWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (_tempUsingRightHandWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                        GameObject oppositeWeapon = (_tempUsingRightHandWeapon == true)
                            ? _tempCurrLeftWeapon
                            : _tempCurrRightWeapon;

                        if (oppositeWeapon == null) //�޼չ��⸦ ������� �ʴٸ�
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
                        }
                        else
                        {
                            if (oppositeWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
                            {
                                _overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
                            }
                            _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
                        }

                        _animator.SetBool("IsMirroring", isMirrored);
                    }
                    //���� �ִϸ��̼��� �ƴϴ�
                    else
                    {
                        //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
                        float leftHandLayerWeight = (_tempCurrLeftWeapon == null ||_tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
                        float rightHandLayerWeight = (_tempCurrRightWeapon == null || _tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) 
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

                        //_animator.SetBool("IsMirroring", false);
                    }
                }
                break;

            case WeaponGrabFocus.RightHandFocused:
            case WeaponGrabFocus.LeftHandFocused:
                {
                    float layerWeight = (currState.GetStateDesc()._isAttackState == true)
                        ? 0.0f
                        : 1.0f;

                    _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
                    _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

                    if (currState.GetStateDesc()._isAttackState == true)
                    {
                        _animator.SetBool("IsMirroring", isMirrored);
                    }
                    else
                    {
                        //_animator.SetBool("IsMirroring", false);
                    }
                }
                break;

            case WeaponGrabFocus.DualGrab:
                {
                    Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
                }
                break;
        }
    }


    public void WeaponChange_Animation(AnimationClip nextAnimation, AnimatorLayerTypes targetBody, bool isZeroFrame)
    {
        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];

        targetPart._isUsingFirstLayer = !(targetPart._isUsingFirstLayer);

        int layerStartIndex = (int)targetBody * 2;

        int layerIndex = (targetPart._isUsingFirstLayer == true)
            ? layerStartIndex
            : layerStartIndex + 1;

        string nextNodeName = (targetPart._isUsingFirstLayer == true)
            ? "State1"
            : "State2";

        AnimationClip targetclip = (isZeroFrame == true)
            ? ResourceDataManager.Instance.GetZeroFrameAnimation(nextAnimation.name)
            : nextAnimation;

        _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = targetclip;

        _animator.Play(nextNodeName, layerIndex, 0.0f);
    }














    // �ڷ�ƾ ���� �Լ�
    public void StartCoroutineWithCallback(IEnumerator coroutine, Action<object[]> onComplete, params object[] parameters)
    {
        StartCoroutine(RunCoroutineWithCallback(coroutine, onComplete, parameters));
    }

    // ���������� ���ε� �ڷ�ƾ
    private IEnumerator RunCoroutineWithCallback(IEnumerator coroutine, Action<object[]> onComplete, object[] parameters)
    {
        yield return StartCoroutine(coroutine); // �ڷ�ƾ�� �Ϸ�� ������ ���
        onComplete?.Invoke(parameters); // �ڷ�ƾ �Ϸ� �� ���� �Լ� ����
    }



    //private void testFunc()
    //{
    //    // ���� �Ű����� �ݹ� ���ε�
    //    StartCoroutineWithCallback(ExampleCoroutine(), (args) =>
    //    {
    //        Debug.Log($"Callback executed with parameters: {string.Join(", ", args)}");
    //    }, "Hello", 123, true, 45.67f);
    //}







    

















    private void StartCoroutine_WithCallBack(IEnumerator coroutine, Action<object[]> callBackFunc, object[] parameters, CoroutineLock coroutineLockNullable)
    {
        StartCoroutine(CoroutineWrapper(coroutine, callBackFunc, parameters, coroutineLockNullable));
    }

    private IEnumerator CoroutineWrapper(IEnumerator coroutine, Action<object[]> callBackFunc, object[] parameters, CoroutineLock coroutineLockNullable)
    {
        yield return StartCoroutine(coroutine);

        if (callBackFunc != null)
        {
            callBackFunc.Invoke(parameters);
        }

        if (coroutineLockNullable != null)
        {
            coroutineLockNullable._isEnd = true;
        }
    }
    






















    // ���� �ڷ�ƾ
    private IEnumerator ExampleCoroutine()
    {
        Debug.Log("Coroutine started.");
        yield return new WaitForSeconds(2); // 2�� ���
        Debug.Log("Coroutine ended.");
    }












































































    private void UpdateBusyAnimatorLayers(int willBusyLayers_BitShift, bool isOn)
    {
        if (isOn == true)
        {
            _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift | willBusyLayers_BitShift);
        }
        else
        {
            _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~willBusyLayers_BitShift);
        }
    }




















    private void WeaponChangeCheck2()
    {
        GameObject nextWeaponPrefab = null;
        bool weaponChangeTry = false;
        bool tempIsRightHandWeapon = false;

        if (Input.GetKeyDown(KeyCode.R))
        {
            //�޼� ���� �������� ��ȯ
            weaponChangeTry = true;

            _currLeftWeaponIndex++;
            if (_currLeftWeaponIndex >= _tempMaxWeaponSlot)
            {
                _currLeftWeaponIndex = _currLeftWeaponIndex % _tempMaxWeaponSlot;
            }
            nextWeaponPrefab = _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            //������ ���� �������� ��ȯ
            weaponChangeTry = true;

            _currRightWeaponIndex++;
            if (_currRightWeaponIndex >= _tempMaxWeaponSlot)
            {
                _currRightWeaponIndex = _currRightWeaponIndex % _tempMaxWeaponSlot;
            }
            nextWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

            tempIsRightHandWeapon = true;
        }

        if (weaponChangeTry == false) //���� ��ȯ�� �õ����� �ʾҴ�. �ƹ��ϵ� �Ͼ�� �������̴�.
        {
            return;
        }

        int willUsingAnimatorLayer = 0;

        //����� �ִϸ��̼� ���� üũ
        {
            if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused ||
                _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
            {
                //���� ������� ����־���.
                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
            }
            else
            {
                //�Ѽ����� ����־���.
                if (tempIsRightHandWeapon == true)
                {
                    willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                }
                else
                {
                    willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                }
            }
        }

        if ((_currentBusyAnimatorLayer_BitShift & willUsingAnimatorLayer) != 0)
        {
            return;
        }

        //���� Lock�� ��´�
        UpdateBusyAnimatorLayers(willUsingAnimatorLayer, true);

        AnimationClip drawNextWeaponAnimationClip = null;
        AnimationClip putawayCurrentWeaponAnimationClip = null;

        AnimatorLayerTypes layerType = (tempIsRightHandWeapon == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        //PutAway�ִϸ��̼� ����
        {
            GameObject currentWeapon = (tempIsRightHandWeapon == true)
                ? _tempCurrRightWeapon
                : _tempCurrLeftWeapon;

            if (currentWeapon == null) //���� ���⸦ �����ϰ� ���� �ʾҴ�
            {
                putawayCurrentWeaponAnimationClip = null;
            }
            else
            {
                WeaponScript weaponScript = currentWeapon.GetComponent<WeaponScript>();
                putawayCurrentWeaponAnimationClip = (tempIsRightHandWeapon == true)
                    ? weaponScript._putawayAnimation
                    : weaponScript._putawayAnimation_Mirrored;

                Debug.Assert(putawayCurrentWeaponAnimationClip != null, "����ִ� �ִϸ��̼��� ���������� �ʽ��ϴ�");
            }
        }

        //Draw�ִϸ��̼� ����
        {
            if (nextWeaponPrefab == null) //���� ���⸦ �����ϰ� ���� �ʾҴ�
            {
                drawNextWeaponAnimationClip = null;
            }
            else
            {
                WeaponScript weaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();
                drawNextWeaponAnimationClip = (tempIsRightHandWeapon == true)
                    ? weaponScript._drawAnimation
                    : weaponScript._drawAnimation_Mirrored;

                Debug.Assert(drawNextWeaponAnimationClip != null, "������ �ִϸ��̼��� ���������� �ʽ��ϴ�");
            }
        }

        //�׼��� ���ϱ� ���� �ڷ�ƾ�� ȣ���Ѵ�.
        StartCoroutine(WeaponChangingCoroutine(putawayCurrentWeaponAnimationClip, drawNextWeaponAnimationClip, layerType, nextWeaponPrefab));
    }


    private IEnumerator WeaponChangingCoroutine(AnimationClip putAwayAnimationClip, AnimationClip drawAnimationClip, AnimatorLayerTypes targetBody, GameObject nextWeaponPrefab)
    {


        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];
        Debug.Assert(targetPart != null, "�ش� ��Ʈ�� ������� �ʴ´ٰ� �������ֽ��ϴ�");
        bool tempIsRightWeapon = (targetBody == AnimatorLayerTypes.RightHand);

        //0. ������� ����־��ٸ� ��ո�带 �����Ұ�.
        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused ||
            _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
        {
            AnimatorLayerTypes oppositePart = (tempIsRightWeapon == true)
                ? AnimatorLayerTypes.LeftHand
                : AnimatorLayerTypes.RightHand;

            if (_tempGrabFocusType == WeaponGrabFocus.LeftHandFocused ||
                _tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
            {
                GameObject currentFocusedWeapon = (_tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
                    ? _tempCurrLeftWeapon
                    : _tempCurrRightWeapon;

                WeaponScript currentFocusedWeaponScript = currentFocusedWeapon.GetComponent<WeaponScript>();

                AnimationClip onehandHandlingAnimation = (_tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
                    ? currentFocusedWeaponScript._handlingIdleAnimation_OneHand_Mirrored
                    : currentFocusedWeaponScript._handlingIdleAnimation_OneHand;

                AnimatorBlendingDesc.BlendingCoroutineType coroutineWorkType = AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer;
                if (onehandHandlingAnimation != null)
                {
                    WeaponChange_Animation(onehandHandlingAnimation, oppositePart, false); //�ִϸ��̼��� �ٲ��
                }
                else
                {
                    coroutineWorkType = AnimatorBlendingDesc.BlendingCoroutineType.DeactiveAllLayer;
                }
                StartCoroutine_WithCallBack
                (
                    _partBlendingDesc[(int)oppositePart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, coroutineWorkType, null),
                    (args) =>
                    {
                        _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)args[0]));
                    },
                    new object[] { (int)oppositePart },
                    null
                );

                //�ڷ�ƾ�� ������ �ݴ���� �����ο�����.
            }
        }

        //4. ���� ������ ���
        {
            _tempGrabFocusType = WeaponGrabFocus.Normal;
        }

        //1. ���� ����ֱ�
        if (putAwayAnimationClip != null)
        {
            {
                ////����ؾ��� ��尡 �ٲ��. ��, 0������ �ִϸ��̼����� ���õ��ִ�.
                //{
                //    WeaponChange_Animation(putAwayAnimationClip, targetBody, true);
                //}

                ////����ؾ��� ����� Layer Weight�� �ٲ��
                //{
                //    CoroutineLock targetHandLock = new CoroutineLock();

                //    StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                //    while (targetHandLock._isEnd == false)
                //    {
                //        yield return null;
                //    }
                //}

                ////Layer Weight ���� �Ϸ�

                ////�ִϸ��̼� �ٽ� ��� ����
                //{
                //    int layerIndex = (targetPart._isUsingFirstLayer == true)
                //        ? (int)targetBody * 2
                //        : (int)targetBody * 2 + 1;

                //    string nextNodeName = (targetPart._isUsingFirstLayer == true)
                //        ? "State1"
                //        : "State2";

                //    _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = putAwayAnimationClip;

                //    _animator.Play(nextNodeName, layerIndex, 0.0f);
                //}

                ////�ִϸ��̼��� �� ����ɶ����� Ȧ��
                //while (true)
                //{
                //    int layerIndex = (targetPart._isUsingFirstLayer == true)
                //        ? (int)targetBody * 2
                //        : (int)targetBody * 2 + 1;

                //    float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                //    if (normalizedTime >= 1.0f)
                //    {
                //        break;
                //    }

                //    yield return null;
                //}
            }

            //����ؾ��� ��尡 �ٲ��. ��, 0������ �ִϸ��̼����� ���õ��ִ�.
            {
                WeaponChange_Animation(putAwayAnimationClip, targetBody, false);
            }

            //����ؾ��� ����� Layer Weight�� �ٲ��
            {
                CoroutineLock targetHandLock = new CoroutineLock();

                StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight ���� �Ϸ�

            //�ִϸ��̼��� �� ����ɶ����� Ȧ��
            while (true)
            {
                int layerIndex = (targetPart._isUsingFirstLayer == true)
                    ? (int)targetBody * 2
                    : (int)targetBody * 2 + 1;

                float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                if (normalizedTime >= 1.0f)
                {
                    break;
                }

                yield return null;
            }
        }

        //2. ����ֱ� �ִϸ��̼� �Ϸ� / Ȥ�� ���⸦ ��� ���� �ʾƼ� ��ŵ�ߴ� =>//���� ���⸦ �����Ѵ�
        {
            if (tempIsRightWeapon == true && _tempCurrRightWeapon != null)
            {
                Destroy(_tempCurrRightWeapon);
                _tempCurrRightWeapon = null;
            }
            else if (tempIsRightWeapon == false && _tempCurrLeftWeapon != null)
            {
                Destroy(_tempCurrLeftWeapon);
                _tempCurrLeftWeapon = null;
            }
        }

        //3. ���� ������
        if (drawAnimationClip != null)
        {
            //����ؾ��� ��尡 �ٲ��. ��, 0������ �ִϸ��̼����� ���õ��ִ�.
            {
                WeaponChange_Animation(drawAnimationClip, targetBody, true);
            }

            //����ؾ��� ����� Layer Weight�� �ٲ��
            {
                CoroutineLock targetHandLock = new CoroutineLock();
                StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight ���� �Ϸ�

            //���� ���� ����
            {
                CreateWeaponModelAndEquip(tempIsRightWeapon, nextWeaponPrefab);
            }

            //�ִϸ��̼� �ٽ� ��� ����
            {
                int layerIndex = (targetPart._isUsingFirstLayer == true)
                    ? (int)targetBody * 2
                    : (int)targetBody * 2 + 1;

                string nextNodeName = (targetPart._isUsingFirstLayer == true)
                    ? "State1"
                    : "State2";

                _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = drawAnimationClip;

                _animator.Play(nextNodeName, layerIndex, 0.0f);
            }

            //�ִϸ��̼��� �� ����ɶ����� Ȧ��
            while (true)
            {
                int layerIndex = (targetPart._isUsingFirstLayer == true)
                    ? (int)targetBody * 2
                    : (int)targetBody * 2 + 1;

                float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                if (normalizedTime >= 1.0f)
                {
                    break;
                }

                yield return null;
            }
        }
        else
        {
            CoroutineLock targetHandLock = new CoroutineLock();

            StartCoroutine_WithCallBack
            (
                _partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.DeactiveAllLayer, targetHandLock),
                (args) =>
                {
                    _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)args[0]));
                },
                new object[] { (int)targetBody },
                null
            );
           
            while (targetHandLock._isEnd == false)
            {
                yield return null;
            }
        }

        //�ڷ�ƾ �������� (�߰��� �ǰݴ��ϰų� �̺�Ʈ�� ������)
        UpdateBusyAnimatorLayers(1 << (int)targetBody, false);
    }







































    private void UseItemCheck()
    {
        ItemInfo newTestingItem = null;

        if (Input.GetKeyDown(_useItemKeyCode1) == true)
        {
            newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
        }
        else if (Input.GetKeyDown(_useItemKeyCode2) == true)
        {
            newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
        }
        else if (Input.GetKeyDown(_useItemKeyCode3) == true)
        {
            newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
        }
        else if (Input.GetKeyDown(_useItemKeyCode4) == true)
        {
            newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
        }

        if (newTestingItem == null)
        {
            return;
        }

        if (_stateContoller.GetCurrState().GetStateDesc()._canUseItem == false)
        {
            return;
        }


        //��� ���� üũ
        int willBusyLayer = 0;
        if (newTestingItem._usingItemMustNotBusyLayers != null || newTestingItem._usingItemMustNotBusyLayers.Count > 0)
        {
            if (newTestingItem._usingItemMustNotBusyLayer < 0)
            {
                newTestingItem._usingItemMustNotBusyLayer = 0;

                foreach (var item in newTestingItem._usingItemMustNotBusyLayers)
                {
                    newTestingItem._usingItemMustNotBusyLayer = (newTestingItem._usingItemMustNotBusyLayer | 1 << (int)item);
                }
            }

            willBusyLayer = newTestingItem._usingItemMustNotBusyLayer;
        }


        if ((_currentBusyAnimatorLayer_BitShift & willBusyLayer) != 0)
        {
            return; //�ش� �������� ���� ������ �ִ�
        }
        
        UpdateBusyAnimatorLayers(willBusyLayer, true); //���� ���� ��´�.

        StartCoroutine(UseItemCoroutine(newTestingItem));
    }


























    private IEnumerator UseItemCoroutine(ItemInfo usingItemInfo)
    {
        //_corutineStarted_Weapon = true;
        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
        Transform tempRightHandTransformParent = null;

        //���⸦ ������� ����ֽ��ϱ�?
        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
        {
            //�޼��̸� �������� �������� ��� �ִٸ� ��� �޼տ� ����ش�.
            //transform�� �θ� ��� �ٲٴ� �۾�
            WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();

            tempRightHandTransformParent = weaponScript._socketTranform;

            WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;

            Debug.Assert(weaponScript != null, "����� WeaponScript�� �־�� �Ѵ�");

            //�ݴ�� ���� ã�Ƽ� �ӽ÷� �ٿ��ֱ�
            {
                Debug.Assert(_characterModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

                WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

                Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");


                ItemInfo.WeaponType targetType = weaponScript._weaponType;

                foreach (var socketComponent in weaponSockets)
                {
                    if (socketComponent._sideType != targetSide)
                    {
                        continue;
                    }

                    foreach (var type in socketComponent._equippableWeaponTypes)
                    {
                        if (type == targetType)
                        {
                            //_tempCurrRightWeapon.transform.parent = socketComponent.gameObject.transform;
                            weaponScript.Equip_OnSocket(socketComponent.gameObject.transform);
                            break;
                        }
                    }
                }
            }
        }
        else if (_tempCurrRightWeapon != null)
        {
            //�����տ� ���Ⱑ �ֳ�?
            //���⸦ ����ִ´�
            AnimationClip rightHandWeaponPutAwayAnimation = _tempCurrRightWeapon.GetComponent<WeaponScript>()._putawayAnimation;
            WeaponChange_Animation(rightHandWeaponPutAwayAnimation, AnimatorLayerTypes.RightHand, false);
            yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            //���⸦ ����ִ� �ִϸ��̼��� �� ����ɶ����� Ȧ��
            while (true)
            {
                int layerIndex = (rightHandBlendingDesc._isUsingFirstLayer == true)
                    ? (int)AnimatorLayerTypes.RightHand * 2
                    : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                if (normalizedTime >= 1.0f)
                {
                    break;
                }

                yield return null;
            }

            Destroy(_tempCurrRightWeapon);
            _tempCurrRightWeapon = null;
        }


        //�������� ����ϴ� �ִϸ��̼����� �ٲ۴�.
        WeaponChange_Animation(usingItemInfo._usingItemAnimation, AnimatorLayerTypes.RightHand, false);
        yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));


        //�������� ����ϴ� �ִϸ��̼��� �� ����ɶ����� Ȧ��
        while (true)
        {
            int layerIndex = (rightHandBlendingDesc._isUsingFirstLayer == true)
                ? (int)AnimatorLayerTypes.RightHand * 2
                : (int)AnimatorLayerTypes.RightHand * 2 + 1;

            float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

            if (normalizedTime >= 1.0f)
            {
                break;
            }

            yield return null;
        }


        //���� ���⸦ ��� ������ ���󺹱� �Ѵ�.


        //�Ʊ� ������� ����־���?
        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
        {
            WeaponScript currRightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
            WeaponChange_Animation(currRightWeaponScript._handlingIdleAnimation_TwoHand, AnimatorLayerTypes.RightHand, false);
            yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            //_tempCurrRightWeapon.transform.parent = tempRightHandTransformParent;
            _tempCurrRightWeapon.GetComponent<WeaponScript>().Equip_OnSocket(tempRightHandTransformParent);
        }
        else
        {
            GameObject rightWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

            if (rightWeaponPrefab == null)
            {
                UpdateBusyAnimatorLayers(1 << (int)AnimatorLayerTypes.RightHand, false);
                yield break;
            }


            WeaponScript rightWeaponScript = rightWeaponPrefab.GetComponent<WeaponScript>();

            WeaponChange_Animation(rightWeaponScript._drawAnimation, AnimatorLayerTypes.RightHand, true);
            yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            CreateWeaponModelAndEquip(true, rightWeaponPrefab);

            //�ִϸ��̼� �ٽ� ��� ����
            {
                int layerIndex = (rightHandBlendingDesc._isUsingFirstLayer == true)
                    ? (int)AnimatorLayerTypes.RightHand * 2
                    : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                string nextNodeName = (rightHandBlendingDesc._isUsingFirstLayer == true)
                    ? "State1"
                    : "State2";

                _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = rightWeaponScript._drawAnimation;

                _animator.Play(nextNodeName, layerIndex, 0.0f);
            }

            //�ִϸ��̼��� �� ����ɶ����� Ȧ��
            while (true)
            {
                int layerIndex = (rightHandBlendingDesc._isUsingFirstLayer == true)
                    ? (int)AnimatorLayerTypes.RightHand * 2
                    : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                if (normalizedTime >= 1.0f)
                {
                    break;
                }

                yield return null;
            }
        }

        UpdateBusyAnimatorLayers(usingItemInfo._usingItemMustNotBusyLayer, false);
    }
























    private void ChangeWeaponHandling()
    {
        bool isChangeWeaponHandlingTry = false;
        bool isRightHandWeapon = false;

        if (Input.GetKeyDown(_changeRightHandWeaponHandlingKey) == true)
        {
            isChangeWeaponHandlingTry = true;
            isRightHandWeapon = true;
        }
        else if (Input.GetKeyDown(_changeLeftHandWeaponHandlingKey) == true)
        {
            isChangeWeaponHandlingTry = true;
        }

        if (isChangeWeaponHandlingTry == false)
        {
            return; //������ �õ��� �̷������ �ʾҴ�. �ƹ��ϵ� �Ͼ�� �ʴ´�
        }

        GameObject targetWeapon = (isRightHandWeapon == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        if (targetWeapon == null)
        {
            return; //�����⸦ �õ������� ���Ⱑ ����.
        }

        bool isRelease = false;

        if (isRightHandWeapon == true)
        {
            if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
            {
                isRelease = true;
            }
        }
        else
        {
            if (_tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
            {
                isRelease = true;
            }
        }

        int willBusyLayer = 0;
        //����� ���̾� ���
        {
            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
        }

        UpdateBusyAnimatorLayers(willBusyLayer, true);

        //�ڷ�ƾ ȣ��
        if (isRelease == true)
        {
            StartCoroutine(ChangeWeaponHandling_ReleaseMode(isRightHandWeapon));
            return;
        }

        GameObject oppositeWeapon = (isRightHandWeapon == true)
            ? _tempCurrLeftWeapon
            : _tempCurrRightWeapon;

        AnimatorLayerTypes targetHandType = (isRightHandWeapon == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        AnimatorLayerTypes oppositeHandType = (isRightHandWeapon == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

        StartCoroutine(ChangeWeaponHandlingCoroutine(targetWeapon, oppositeWeapon, isRightHandWeapon, targetHandType, oppositeHandType));
    }

























    private IEnumerator ChangeWeaponHandling_ReleaseMode(bool isRightHandWeapon)
    {
        //�������� ����Ѵ�
        {
            _tempGrabFocusType = WeaponGrabFocus.Normal;
        }

        GameObject targetWeapon = (isRightHandWeapon == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        AnimatorLayerTypes targetPart = (isRightHandWeapon == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        CoroutineLock targetHandLock = new CoroutineLock();
        CoroutineLock oppositeHandLock = new CoroutineLock();


        //Ÿ�� �� �Լ� ����
        {
            WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();
            AnimationClip targetWeaponsOneHandHandlingAnimation = targetWeaponScript._handlingIdleAnimation_OneHand;

            if (targetWeaponsOneHandHandlingAnimation != null)
            {
                //���� �ִϸ��̼��� �ֽ��ϴ�.
                WeaponChange_Animation(targetWeaponsOneHandHandlingAnimation, targetPart, false);
                StartCoroutine_WithCallBack
                    (
                    _partBlendingDesc[(int)targetPart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock),
                    (args) =>
                    {
                        _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)args[0]));
                    }, 
                    new object[] { (int)targetPart},
                    null
                    );
                //StartCoroutine(_partBlendingDesc[(int)targetPart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
            }
            else
            {
                //���� �ִϸ��̼��� �����ϴ�.
                StartCoroutine_WithCallBack
                    (
                    _partBlendingDesc[(int)targetPart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.DeactiveAllLayer, targetHandLock),
                    (args) =>
                    {
                        _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)args[0]));
                    },
                    new object[] { (int)targetPart },
                    null
                    );
                //StartCoroutine(_partBlendingDesc[(int)targetPart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.DeactiveAllLayer, targetHandLock));
            }
        }


        //�ݴ�� �Լ� ����
        GameObject oppositeWeaponPrefab = (isRightHandWeapon == true)
            ? _tempLeftWeaponPrefabs[_currLeftWeaponIndex]
            : _tempRightWeaponPrefabs[_currRightWeaponIndex];

        AnimatorLayerTypes oppositePart = (isRightHandWeapon == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

        if (oppositeWeaponPrefab == null) //������� �����鼭 ������� ���Ⱑ ����.
        {
            StartCoroutine_WithCallBack
                (
                _partBlendingDesc[(int)oppositePart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.DeactiveAllLayer, oppositeHandLock),
                (args) =>
                {
                    _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)args[0]));
                },
                new object[] { (int)oppositePart },
                null
                );

            yield break;            
        }


        WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();

        AnimationClip oppositeWeaponDrawAnimation = (isRightHandWeapon == true)
            ? oppositeWeaponScript._drawAnimation_Mirrored
            : oppositeWeaponScript._drawAnimation;

        //�ݴ���� ���� ������ Zero Frame �ִϸ��̼����� �ٲ۴�.
        WeaponChange_Animation(oppositeWeaponDrawAnimation, oppositePart, true);

        //�ݴ���� ���� ������ Zero Frame �ִϸ��̼� Layer Weight�� ���� �ø���.
        yield return StartCoroutine(_partBlendingDesc[(int)oppositePart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, oppositeHandLock));

        //�ݴ�տ� ���⸦ ����ش�.
        {
            CreateWeaponModelAndEquip(!isRightHandWeapon, oppositeWeaponPrefab);
        }

        AnimatorBlendingDesc oppositePartBlendingDesc = _partBlendingDesc[(int)oppositePart];

        //�ݴ���� ���� ������ �ִϸ��̼����� �ٲ۴�.
        {
            int layerIndex = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                ? (int)oppositePart * 2
                : (int)oppositePart * 2 + 1;

            string nextNodeName = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                ? "State1"
                : "State2";

            _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = oppositeWeaponDrawAnimation;

            _animator.Play(nextNodeName, layerIndex, 0.0f);
        }

        //�ݴ���� ���� ������ �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
        while (true)
        {
            int layerIndex = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                ? (int)oppositePart * 2
                : (int)oppositePart * 2 + 1;

            float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

            if (normalizedTime >= 1.0f)
            {
                break;
            }

            yield return null;
        }

        _currentBusyAnimatorLayer_BitShift = (_currentBusyAnimatorLayer_BitShift & ~(1 << (int)oppositePart));
    }


    private IEnumerator ChangeWeaponHandlingCoroutine(GameObject targetWeapon, GameObject oppositeWeapon, bool isRightHandWeapon, AnimatorLayerTypes targetHand, AnimatorLayerTypes oppositeHand)
    {
        //�������� ����Ѵ�
        {
            if (isRightHandWeapon == true)
            {
                _tempGrabFocusType = WeaponGrabFocus.RightHandFocused;
            }
            else
            {
                _tempGrabFocusType = WeaponGrabFocus.LeftHandFocused;
            }
        }

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        //�ݴ� �տ� ���⸦ ����־���.
        if (oppositeWeapon != null)
        {
            WeaponScript oppositeWeaponScript = oppositeWeapon.GetComponent<WeaponScript>();

            AnimationClip oppsiteWeaponPutAwayAnimation = (isRightHandWeapon == true)
                ? oppositeWeaponScript._putawayAnimation_Mirrored
                : oppositeWeaponScript._putawayAnimation;

            //�ݴ���� ���� ����ֱ� Zero Frame �ִϸ��̼����� �ٲ۴�.
            WeaponChange_Animation(oppsiteWeaponPutAwayAnimation, oppositeHand, true);

            //�ݴ���� ���� ����ֱ� Zero Frame �ִϸ��̼� Layer Weight�� ���� �ø���.
            yield return StartCoroutine(_partBlendingDesc[(int)oppositeHand].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            AnimatorBlendingDesc oppositePartBlendingDesc = _partBlendingDesc[(int)oppositeHand];

            //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
            {
                int layerIndex = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                    ? (int)oppositeHand * 2
                    : (int)oppositeHand * 2 + 1;

                string nextNodeName = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                    ? "State1"
                    : "State2";

                _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = oppsiteWeaponPutAwayAnimation;

                _animator.Play(nextNodeName, layerIndex, 0.0f);
            }

            //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            while (true)
            {
                int layerIndex = (oppositePartBlendingDesc._isUsingFirstLayer == true)
                    ? (int)oppositeHand * 2
                    : (int)oppositeHand * 2 + 1;

                float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                if (normalizedTime >= 1.0f)
                {
                    break;
                }

                yield return null;
            }

            //���� ����ֱⰡ ������. �ݴ�տ� ���Ⱑ �־��ٸ� ���⸦ �����Ѵ�.
            {
                if (isRightHandWeapon == true)
                {
                    if (_tempCurrLeftWeapon != null)
                    {
                        Destroy(_tempCurrLeftWeapon);
                        _tempCurrLeftWeapon = null;
                    }
                }
                else
                {
                    if (_tempCurrRightWeapon != null)
                    {
                        Destroy(_tempCurrRightWeapon);
                        _tempCurrRightWeapon = null;
                    }
                }
            }
        }

        AnimationClip focusGrabAnimation = (isRightHandWeapon == true)
            ? targetWeaponScript._handlingIdleAnimation_TwoHand
            : targetWeaponScript._handlingIdleAnimation_TwoHand_Mirrored;

        //�ش� ���⸦ ������� ��� �ִϸ��̼��� �����Ѵ�. �ش� ���� �ִϸ��̼��� �ٲ۴�
        {
            WeaponChange_Animation(focusGrabAnimation, targetHand, false);
            WeaponChange_Animation(focusGrabAnimation, oppositeHand, false);

            CoroutineLock targetHandLock = new CoroutineLock();
            CoroutineLock oppositeHandLock = new CoroutineLock();

            StartCoroutine(_partBlendingDesc[(int)targetHand].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
            StartCoroutine(_partBlendingDesc[(int)oppositeHand].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, oppositeHandLock));

            while (targetHandLock._isEnd == false || oppositeHandLock._isEnd == false)
            {
                yield return null;
            }
        }

        UpdateBusyAnimatorLayers(1 << (int)AnimatorLayerTypes.LeftHand, false);
        UpdateBusyAnimatorLayers(1 << (int)AnimatorLayerTypes.RightHand, false);
    }










    private void CreateWeaponModelAndEquip(bool tempIsRightWeapon, GameObject nextWeaponPrefab)
    {
        WeaponSocketScript.SideType targetSide = (tempIsRightWeapon == true)
            ? WeaponSocketScript.SideType.Right
            : WeaponSocketScript.SideType.Left;

        WeaponScript nextWeaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();

        Debug.Assert(nextWeaponScript != null, "����� WeaponScript�� �־�� �Ѵ�");

        //���� ã��
        Transform correctSocket = null;
        {
            Debug.Assert(_characterModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");


            ItemInfo.WeaponType targetType = nextWeaponScript._weaponType;

            foreach (var socketComponent in weaponSockets)
            {
                if (socketComponent._sideType != targetSide)
                {
                    continue;
                }

                foreach (var type in socketComponent._equippableWeaponTypes)
                {
                    if (type == targetType)
                    {
                        correctSocket = socketComponent.gameObject.transform;
                        break;
                    }
                }
            }

            Debug.Assert(correctSocket != null, "���⸦ ���� �� �ִ� ������ �����ϴ�");
        }

        //������ ������ ����, ����
        GameObject newObject = Instantiate(nextWeaponPrefab);
        {
            nextWeaponScript = newObject.GetComponent<WeaponScript>();
            nextWeaponScript._weaponType = ItemInfo.WeaponType.MediumGun;
            nextWeaponScript.Equip(this, correctSocket);
            newObject.transform.SetParent(transform);

            if (tempIsRightWeapon == true)
            {
                _tempCurrRightWeapon = newObject;
            }
            else
            {
                _tempCurrLeftWeapon = newObject;
            }
        }
    }
}
