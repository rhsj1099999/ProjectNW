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
        Debug.Assert(_stateContoller != null, "StateController가 null이여서는 안된다");

        RepresentStateType representType = RepresentStateType.Hit_Fall;

        //데미지를 가지고 얼마나 아프게 맞았는지 계산
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

        //StateController에게 상태 변경 지시
        _stateContoller.TryChangeState(representType);
    }




    //대표 컴포넌트
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> 이거 다른 컴포넌트로 빼세요(현재 만들어져있는건 EquipmentBoard 혹은 Inventory)
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


    private bool _tempUsingRightHandWeapon = false; //최근에 사용한 무기가 오른손입니까?
    public bool GetLatestWeaponUse() { return _tempUsingRightHandWeapon; }
    public void SetLatestWeaponUse(bool isRightHandWeapon)
    {
        _tempUsingRightHandWeapon = isRightHandWeapon;
    }

    //현재는 캐릭터메쉬가 애니메이터를 갖고있기 때문에 애니메이터를 갖고있는 게임오브젝트가 캐릭터 메쉬다
    private GameObject _characterModelObject = null; //애니메이터는 얘가 갖고있다
    public Animator GetAnimator() { return _animator; }


    //Aim System
    private AimScript2 _aimScript = null;
    private bool _isAim = false;
    private bool _isTargeting = false;
    public bool GetIsTargeting() { return _isTargeting; }


    //Animator Secton -> 이거 다른 컴포넌트로 빼세요
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
        Debug.Assert( _inputController != null, "인풋 컨트롤러가 없다");

        _characterMoveScript2 = GetComponent<CharacterMoveScript2>();
        Debug.Assert(_characterMoveScript2 != null, "CharacterMove 컴포넌트 없다");

        _stateContoller = GetComponent<StateContoller>();
        Debug.Assert(_stateContoller != null, "StateController가 없다");

        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "StateController가 없다");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
        _characterModelObject = _animator.gameObject;

        /*-----------------------------------------------------------------
        |NOTI| 총 무기 뿐만 아니라 그냥 무기투적을 생각해서라도 TPS 시스템을 준비한다
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
        //타임디버깅
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.L))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.O))  // R키를 누르면 게임 속도를 정상으로 복원
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        //임시 Hit 디버깅 코드
        {
            if (Input.GetKeyDown(KeyCode.H) == true)
            {
                DealMe(1, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.J) == true)
            {
                DealMe(4, this.gameObject);
                // Hit_L로 변경
            }

            if (Input.GetKeyDown(KeyCode.K) == true)
            {
                DealMe(100, this.gameObject);
                // Hit_Fall로 변경
            }
        }

        //임시 양손잡기 코드
        {
            ChangeWeaponHandling();
        }

        //임시 무기변경 코드
        {
            WeaponChangeCheck2();
        }

        //임시 아이템 사용 코드
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

        //현재 상태 업데이트
        {
            _stateContoller.DoWork();
        }

        //기본적으로 중력은 계속 업데이트 한다
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
        |NOTI| 모든 애니메이션은 RightHand 기준으로 녹화됐습니다.
        ------------------------------------------------------*/

        AnimationClip targetClip = nextState.GetStateDesc()._stateAnimationClip;

        //디버그
        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "재생중인 애니메이션을 잃어버렸습니다");
        }

        //노드전환 작업
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

        //무기 레이어 변경, Mirror 적용 (무기 애니메이션이라면)

        //WeaponLayerChange(nextState);
    }


    private IEnumerator SwitchingBlendCoroutine()
    {
        _corutineStarted = true;

        //Layer 인덱스 변경
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0번 레이어를 재생해야하나
                ? Time.deltaTime * -_transitionSpeed   //0번 레이어를 재생해야한다면 목표값은 0.0
                : Time.deltaTime * _transitionSpeed;  //1번 레이어를 재생해야한다면 목표값은 1.0

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
        //뭘 하던 애니메이션이 바뀌면 여기에 들어옵니다
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
                    //공격 애니메이션이다
                    if (currState.GetStateDesc()._isAttackState == true)
                    {
                        int usingHandLayer = (_tempUsingRightHandWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (_tempUsingRightHandWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //왼손은 반드시 따라가야해서 0.0f

                        GameObject oppositeWeapon = (_tempUsingRightHandWeapon == true)
                            ? _tempCurrLeftWeapon
                            : _tempCurrRightWeapon;

                        if (oppositeWeapon == null) //왼손무기를 쥐고있지 않다면
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
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
                    //공격 애니메이션이 아니다
                    else
                    {
                        //왼손 무기를 쥐고있지 않거나 왼손 무기의 파지 애니메이션이 없다
                        float leftHandLayerWeight = (_tempCurrLeftWeapon == null ||_tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //오른손 무기를 쥐고있지 않거나 오른손 무기의 파지 애니메이션이 없다
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
                    Debug.Assert(false, "쌍수 컨텐츠가 추가됐습니까?");
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














    // 코루틴 실행 함수
    public void StartCoroutineWithCallback(IEnumerator coroutine, Action<object[]> onComplete, params object[] parameters)
    {
        StartCoroutine(RunCoroutineWithCallback(coroutine, onComplete, parameters));
    }

    // 내부적으로 래핑된 코루틴
    private IEnumerator RunCoroutineWithCallback(IEnumerator coroutine, Action<object[]> onComplete, object[] parameters)
    {
        yield return StartCoroutine(coroutine); // 코루틴이 완료될 때까지 대기
        onComplete?.Invoke(parameters); // 코루틴 완료 후 람다 함수 실행
    }



    //private void testFunc()
    //{
    //    // 가변 매개변수 콜백 바인딩
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
    






















    // 예제 코루틴
    private IEnumerator ExampleCoroutine()
    {
        Debug.Log("Coroutine started.");
        yield return new WaitForSeconds(2); // 2초 대기
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
            //왼손 무기 다음으로 전환
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
            //오른손 무기 다음으로 전환
            weaponChangeTry = true;

            _currRightWeaponIndex++;
            if (_currRightWeaponIndex >= _tempMaxWeaponSlot)
            {
                _currRightWeaponIndex = _currRightWeaponIndex % _tempMaxWeaponSlot;
            }
            nextWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

            tempIsRightHandWeapon = true;
        }

        if (weaponChangeTry == false) //무기 전환을 시도하지 않았다. 아무일도 일어나지 않을것이다.
        {
            return;
        }

        int willUsingAnimatorLayer = 0;

        //사용할 애니메이션 부위 체크
        {
            if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused ||
                _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
            {
                //현재 양손으로 잡고있었다.
                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
            }
            else
            {
                //한손으로 잡고있었다.
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

        //부위 Lock을 잡는다
        UpdateBusyAnimatorLayers(willUsingAnimatorLayer, true);

        AnimationClip drawNextWeaponAnimationClip = null;
        AnimationClip putawayCurrentWeaponAnimationClip = null;

        AnimatorLayerTypes layerType = (tempIsRightHandWeapon == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        //PutAway애니메이션 결정
        {
            GameObject currentWeapon = (tempIsRightHandWeapon == true)
                ? _tempCurrRightWeapon
                : _tempCurrLeftWeapon;

            if (currentWeapon == null) //현재 무기를 장착하고 있지 않았다
            {
                putawayCurrentWeaponAnimationClip = null;
            }
            else
            {
                WeaponScript weaponScript = currentWeapon.GetComponent<WeaponScript>();
                putawayCurrentWeaponAnimationClip = (tempIsRightHandWeapon == true)
                    ? weaponScript._putawayAnimation
                    : weaponScript._putawayAnimation_Mirrored;

                Debug.Assert(putawayCurrentWeaponAnimationClip != null, "집어넣는 애니메이션이 설정돼있지 않습니다");
            }
        }

        //Draw애니메이션 결정
        {
            if (nextWeaponPrefab == null) //현재 무기를 장착하고 있지 않았다
            {
                drawNextWeaponAnimationClip = null;
            }
            else
            {
                WeaponScript weaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();
                drawNextWeaponAnimationClip = (tempIsRightHandWeapon == true)
                    ? weaponScript._drawAnimation
                    : weaponScript._drawAnimation_Mirrored;

                Debug.Assert(drawNextWeaponAnimationClip != null, "꺼내는 애니메이션이 설정돼있지 않습니다");
            }
        }

        //액션을 취하기 위해 코루틴을 호출한다.
        StartCoroutine(WeaponChangingCoroutine(putawayCurrentWeaponAnimationClip, drawNextWeaponAnimationClip, layerType, nextWeaponPrefab));
    }


    private IEnumerator WeaponChangingCoroutine(AnimationClip putAwayAnimationClip, AnimationClip drawAnimationClip, AnimatorLayerTypes targetBody, GameObject nextWeaponPrefab)
    {


        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];
        Debug.Assert(targetPart != null, "해당 파트는 사용하지 않는다고 설정돼있습니다");
        bool tempIsRightWeapon = (targetBody == AnimatorLayerTypes.RightHand);

        //0. 양손으로 잡고있었다면 양손모드를 해제할것.
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
                    WeaponChange_Animation(onehandHandlingAnimation, oppositePart, false); //애니메이션이 바뀐다
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

                //코루틴이 끝나면 반대손은 자유로워진다.
            }
        }

        //4. 무기 파지법 계산
        {
            _tempGrabFocusType = WeaponGrabFocus.Normal;
        }

        //1. 무기 집어넣기
        if (putAwayAnimationClip != null)
        {
            {
                ////재생해야할 노드가 바뀐다. 단, 0프레임 애니메이션으로 세팅돼있다.
                //{
                //    WeaponChange_Animation(putAwayAnimationClip, targetBody, true);
                //}

                ////재생해야할 노드의 Layer Weight가 바뀐다
                //{
                //    CoroutineLock targetHandLock = new CoroutineLock();

                //    StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                //    while (targetHandLock._isEnd == false)
                //    {
                //        yield return null;
                //    }
                //}

                ////Layer Weight 세팅 완료

                ////애니메이션 다시 재생 시작
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

                ////애니메이션이 다 재생될때까지 홀딩
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

            //재생해야할 노드가 바뀐다. 단, 0프레임 애니메이션으로 세팅돼있다.
            {
                WeaponChange_Animation(putAwayAnimationClip, targetBody, false);
            }

            //재생해야할 노드의 Layer Weight가 바뀐다
            {
                CoroutineLock targetHandLock = new CoroutineLock();

                StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight 세팅 완료

            //애니메이션이 다 재생될때까지 홀딩
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

        //2. 집어넣기 애니메이션 완료 / 혹은 무기를 들고 있지 않아서 스킵했다 =>//기존 무기를 삭제한다
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

        //3. 무기 꺼내기
        if (drawAnimationClip != null)
        {
            //재생해야할 노드가 바뀐다. 단, 0프레임 애니메이션으로 세팅돼있다.
            {
                WeaponChange_Animation(drawAnimationClip, targetBody, true);
            }

            //재생해야할 노드의 Layer Weight가 바뀐다
            {
                CoroutineLock targetHandLock = new CoroutineLock();
                StartCoroutine(_partBlendingDesc[(int)targetBody].ChangeNextLayerWeightSubCoroutine(_transitionSpeed, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight 세팅 완료

            //다음 무기 생성
            {
                CreateWeaponModelAndEquip(tempIsRightWeapon, nextWeaponPrefab);
            }

            //애니메이션 다시 재생 시작
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

            //애니메이션이 다 재생될때까지 홀딩
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

        //코루틴 정상종료 (중간에 피격당하거나 이벤트가 없었다)
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


        //사용 부위 체크
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
            return; //해당 부위들은 지금 할일이 있다
        }
        
        UpdateBusyAnimatorLayers(willBusyLayer, true); //부위 락을 잡는다.

        StartCoroutine(UseItemCoroutine(newTestingItem));
    }


























    private IEnumerator UseItemCoroutine(ItemInfo usingItemInfo)
    {
        //_corutineStarted_Weapon = true;
        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
        Transform tempRightHandTransformParent = null;

        //무기를 양손으로 잡고있습니까?
        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
        {
            //왼손이면 괜찮은데 오른손이 쥐고 있다면 잠시 왼손에 쥐어준다.
            //transform의 부모를 잠시 바꾸는 작업
            WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();

            tempRightHandTransformParent = weaponScript._socketTranform;

            WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;

            Debug.Assert(weaponScript != null, "무기는 WeaponScript가 있어야 한다");

            //반대손 소켓 찾아서 임시로 붙여주기
            {
                Debug.Assert(_characterModelObject != null, "무기를 붙이려는데 모델이 없어서는 안된다");

                WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

                Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");


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
            //오른손에 무기가 있나?
            //무기를 집어넣는다
            AnimationClip rightHandWeaponPutAwayAnimation = _tempCurrRightWeapon.GetComponent<WeaponScript>()._putawayAnimation;
            WeaponChange_Animation(rightHandWeaponPutAwayAnimation, AnimatorLayerTypes.RightHand, false);
            yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            //무기를 집어넣는 애니메이션이 다 재생될때까지 홀딩
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


        //아이템을 사용하는 애니메이션으로 바꾼다.
        WeaponChange_Animation(usingItemInfo._usingItemAnimation, AnimatorLayerTypes.RightHand, false);
        yield return StartCoroutine(rightHandBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));


        //아이템을 사용하는 애니메이션이 다 재생될때까지 홀딩
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


        //이제 무기를 잡는 손으로 원상복구 한다.


        //아까 양손으로 잡고있었나?
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

            //애니메이션 다시 재생 시작
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

            //애니메이션이 다 재생될때까지 홀딩
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
            return; //양손잡기 시도가 이루어지지 않았다. 아무일도 일어나지 않는다
        }

        GameObject targetWeapon = (isRightHandWeapon == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        if (targetWeapon == null)
        {
            return; //양손잡기를 시도했지만 무기가 없다.
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
        //사용할 레이어 계산
        {
            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
        }

        UpdateBusyAnimatorLayers(willBusyLayer, true);

        //코루틴 호출
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
        //파지법을 계산한다
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


        //타겟 손 함수 실행
        {
            WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();
            AnimationClip targetWeaponsOneHandHandlingAnimation = targetWeaponScript._handlingIdleAnimation_OneHand;

            if (targetWeaponsOneHandHandlingAnimation != null)
            {
                //파지 애니메이션이 있습니다.
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
                //파지 애니메이션이 없습니다.
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


        //반대손 함수 실행
        GameObject oppositeWeaponPrefab = (isRightHandWeapon == true)
            ? _tempLeftWeaponPrefabs[_currLeftWeaponIndex]
            : _tempRightWeaponPrefabs[_currRightWeaponIndex];

        AnimatorLayerTypes oppositePart = (isRightHandWeapon == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

        if (oppositeWeaponPrefab == null) //양손으로 잡으면서 집어넣은 무기가 없다.
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

        //반대손을 무기 꺼내기 Zero Frame 애니메이션으로 바꾼다.
        WeaponChange_Animation(oppositeWeaponDrawAnimation, oppositePart, true);

        //반대손을 무기 꺼내기 Zero Frame 애니메이션 Layer Weight로 점점 올린다.
        yield return StartCoroutine(_partBlendingDesc[(int)oppositePart].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, oppositeHandLock));

        //반대손에 무기를 쥐어준다.
        {
            CreateWeaponModelAndEquip(!isRightHandWeapon, oppositeWeaponPrefab);
        }

        AnimatorBlendingDesc oppositePartBlendingDesc = _partBlendingDesc[(int)oppositePart];

        //반대손을 무기 꺼내기 애니메이션으로 바꾼다.
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

        //반대손의 무기 꺼내기 애니메이션이 다 재생될때까지 홀딩한다.
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
        //파지법을 계산한다
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

        //반대 손에 무기를 들고있었다.
        if (oppositeWeapon != null)
        {
            WeaponScript oppositeWeaponScript = oppositeWeapon.GetComponent<WeaponScript>();

            AnimationClip oppsiteWeaponPutAwayAnimation = (isRightHandWeapon == true)
                ? oppositeWeaponScript._putawayAnimation_Mirrored
                : oppositeWeaponScript._putawayAnimation;

            //반대손을 무기 집어넣기 Zero Frame 애니메이션으로 바꾼다.
            WeaponChange_Animation(oppsiteWeaponPutAwayAnimation, oppositeHand, true);

            //반대손을 무기 집어넣기 Zero Frame 애니메이션 Layer Weight로 점점 올린다.
            yield return StartCoroutine(_partBlendingDesc[(int)oppositeHand].ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, AnimatorBlendingDesc.BlendingCoroutineType.ActiveNextLayer, null));

            AnimatorBlendingDesc oppositePartBlendingDesc = _partBlendingDesc[(int)oppositeHand];

            //반대손을 무기 집어넣기 애니메이션으로 바꾼다.
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

            //반대손의 무기 집어넣기 애니메이션이 다 재생될때까지 홀딩한다.
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

            //무기 집어넣기가 끝났다. 반대손에 무기가 있었다면 무기를 삭제한다.
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

        //해당 무기를 양손으로 잡는 애니메이션을 실행한다. 해당 손의 애니메이션을 바꾼다
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

        Debug.Assert(nextWeaponScript != null, "무기는 WeaponScript가 있어야 한다");

        //소켓 찾기
        Transform correctSocket = null;
        {
            Debug.Assert(_characterModelObject != null, "무기를 붙이려는데 모델이 없어서는 안된다");

            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");


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

            Debug.Assert(correctSocket != null, "무기를 붙일 수 있는 소켓이 없습니다");
        }

        //아이템 프리팹 생성, 장착
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
