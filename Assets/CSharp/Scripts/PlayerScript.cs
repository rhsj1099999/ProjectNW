using Microsoft.SqlServer.Server;
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
    End,
}


public enum AnimatorLayer
{
    FullBody = 0    ,
    FullBody_Sub    ,
    LeftHand        ,
    LeftHand_Sub    ,
    RightHand       ,
    RightHand_Sub   ,
}

public enum WeaponHandlingType
{
    Off,
    RightOnLeftOff,
    LeftOnRightOff,
    RightOnLeftOn,
    RightFocused,
    LeftFocused,
    DualHand,
}

public class AnimatorBlendingDesc
{
    public bool _isUsingFirstLayer = false;
    public float _blendTarget = 0.0f;
    public float _blendTarget_Sub = 0.0f;
}

public enum WeaponGrabFocus
{
    Normal,
    RightHandFocused,
    LeftHandFocused,
    DualGrab,
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
        private bool _corutineStarted_Weapon = false;
        private float _blendTarget_Weapon = 0.0f;
        [SerializeField] private float _transitionSpeed_Weapon = 20.0f;



    private AnimationClip _currAnimClip = null;

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
        }

        foreach (var type in _usingBodyPart)
        {
            if (_partBlendingDesc[(int)type] != null)
            {
                continue;
            }

            _partBlendingDesc[(int)type] = new AnimatorBlendingDesc();
        }
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


    private void ChangeWeaponHandling()
    {
        if (_corutineStarted_Weapon == true) //이미 무기 바꾸기 코루틴이 진행중이다
        {
            return;
        }

        bool isChangeWeaponHandlingTry = false;
        bool isRightHandWeapon = false;

        if(Input.GetKeyDown(_changeRightHandWeaponHandlingKey) == true)
        {
            isChangeWeaponHandlingTry = true;
            isRightHandWeapon = true;
        }
        else if(Input.GetKeyDown(_changeLeftHandWeaponHandlingKey) == true)
        {
            isChangeWeaponHandlingTry = true;
        }


        if(isChangeWeaponHandlingTry == false) 
        {
            return; //양손잡기 시도가 이루어지지 않았다. 아무일도 일어나지 않는다
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

        //코루틴 호출
        if (isRelease == true)
        {
            StartCoroutine(ChangeWeaponHandling_ReleaseMode(isRightHandWeapon));
            return;
        }


        GameObject targetWeapon = null;
        GameObject oppositeWeapon = null;
        AnimatorLayerTypes targetHandType = AnimatorLayerTypes.End;
        AnimatorLayerTypes oppositeHandType = AnimatorLayerTypes.End;

        //코루틴 변수결정
        {
            targetWeapon = (isRightHandWeapon == true)
                ? _tempCurrRightWeapon
                : _tempCurrLeftWeapon;

            if (targetWeapon == null)
            {
                return; //양손잡기를 시도했지만 무기가 없다.
            }

            oppositeWeapon = (isRightHandWeapon == true)
                ? _tempCurrLeftWeapon
                : _tempCurrRightWeapon;

            targetHandType = (isRightHandWeapon == true)
                ? AnimatorLayerTypes.RightHand
                : AnimatorLayerTypes.LeftHand;

            oppositeHandType = (isRightHandWeapon == true)
                ? AnimatorLayerTypes.LeftHand
                : AnimatorLayerTypes.RightHand;


        }





        //코루틴 호출
        {
            StartCoroutine(ChangeWeaponHandlingCoroutine(targetWeapon, oppositeWeapon, isRightHandWeapon, targetHandType, oppositeHandType));
        }
    }



    private IEnumerator ChangeNextLayerWeightSubCoroutine(AnimatorLayerTypes targetPart, bool isReverse = false, CoroutineLock coroutineLock = null)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)targetPart];
        int targetHandMainLayerIndex = (int)targetPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? Time.deltaTime * _transitionSpeed_Weapon
                : Time.deltaTime * -_transitionSpeed_Weapon;

            if (isReverse == true)
            {
                mainLayerBlendDelta *= -1.0f;
            }

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            _animator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            _animator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            if (targetBlendingDesc._blendTarget >= 1.0f || targetBlendingDesc._blendTarget <= 0.0f)
            {
                break;
            }

            yield return null;
        }

        if (coroutineLock != null)
        {
            coroutineLock._isEnd = true;
        }
    }


    private IEnumerator ChangeWeaponHandling_ReleaseMode(bool isRightHandWeapon)
    {
        _corutineStarted_Weapon = true;

        AnimatorLayerTypes targetPart = (isRightHandWeapon)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        AnimatorLayerTypes oppositePart = (isRightHandWeapon)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

        GameObject targetWeapon = (isRightHandWeapon)
            ?_tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        AnimationClip targetWeaponsOneHandHandlingAnimation = targetWeaponScript._handlingIdleAnimation_OneHand;

        if (targetWeaponsOneHandHandlingAnimation == null) //파지 애니메이션이 있습니다.
        {
            targetWeaponsOneHandHandlingAnimation = _tempWeaponHandling_NoPlay;
        }

        WeaponChange_Animation(targetWeaponsOneHandHandlingAnimation, targetPart, false);
        WeaponChange_Animation(_tempWeaponHandling_NoPlay, oppositePart, false);

        {
            //GameObject currOppositeWeaponPrefabs = (isRightHandWeapon == true)
            //    ? _tempRightWeaponPrefabs[_currRightWeaponIndex]
            //    : _tempLeftWeaponPrefabs[_currLeftWeaponIndex];

            //bool isOppositedLayerWeightModify = false;

            ///*--------------------------------------------------------------
            //|TODO| 양손잡았다가 한손잡으면 반대손무기 자동으로 꺼내게 코드짜고있는데,
            //나중에 그냥 무기 집어넣기 만들면서 수정할것
            //--------------------------------------------------------------*/
            //if (currOppositeWeaponPrefabs != null) //아까 양손으로 잡으면서 집어넣은 무기가 있습니다.
            //{
            //    WeaponChange_Animation(currOppositeWeaponPrefabs.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand, oppositePart, false);
            //    isOppositedLayerWeightModify = true;
            //}//애니메이션이 바뀌고 타겟 레이어가 바뀌었다.
        }

        //이제 LayerWeight를 수정한다.


        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)targetPart];
        int targetHandMainLayerIndex = (int)targetPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        AnimatorBlendingDesc oppositeBlendingDesc = _partBlendingDesc[(int)oppositePart];
        int oppositeHandMainLayerIndex = (int)oppositePart * 2;
        int oppositeHandSubLayerIndex = oppositeHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? Time.deltaTime * _transitionSpeed_Weapon
                : Time.deltaTime * -_transitionSpeed_Weapon;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            _animator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            _animator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);



            float oppositeLayerBlendDelta = (oppositeBlendingDesc._isUsingFirstLayer == true)
                ? Time.deltaTime * _transitionSpeed_Weapon
                : Time.deltaTime * -_transitionSpeed_Weapon;

            oppositeBlendingDesc._blendTarget += oppositeLayerBlendDelta;
            oppositeBlendingDesc._blendTarget_Sub += -1.0f * oppositeLayerBlendDelta;

            oppositeBlendingDesc._blendTarget = Mathf.Clamp(oppositeBlendingDesc._blendTarget, 0.0f, 1.0f);
            oppositeBlendingDesc._blendTarget_Sub = Mathf.Clamp(oppositeBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            _animator.SetLayerWeight(oppositeHandMainLayerIndex, oppositeBlendingDesc._blendTarget);
            _animator.SetLayerWeight(oppositeHandSubLayerIndex, oppositeBlendingDesc._blendTarget_Sub);


            if (targetBlendingDesc._blendTarget >= 1.0f || targetBlendingDesc._blendTarget <= 0.0f)
            {
                break;
            }

            yield return null;
        }




        //파지법을 계산한다
        {
            _tempGrabFocusType = WeaponGrabFocus.Normal;
        }

        _corutineStarted_Weapon = false;
    }


    private IEnumerator ChangeWeaponHandlingCoroutine(GameObject targetWeapon, GameObject oppositeWeapon, bool isRightHandWeapon, AnimatorLayerTypes targetHand, AnimatorLayerTypes oppositeHand)
    {
        _corutineStarted_Weapon = true;

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();
        Debug.Assert(targetWeaponScript != null, "무기는 반드시 스크립트가 있어야한다");

        //반대 손에 무기를 들고있었다.
        if (oppositeWeapon != null)
        {
            //무기를 집어넣는 과정을 실행한다
        }

        //무기 집어넣기가 끝났다. 반대손에 무기가 있었다면 무기를 삭제한다.
        {

        }

        AnimationClip focusGrabAnimation = targetWeaponScript._handlingIdleAnimation_TwoHand;

        //해당 무기를 양손으로 잡는 애니메이션을 실행한다. 해당 손의 애니메이션을 바꾼다
        {
            WeaponChange_Animation(focusGrabAnimation, targetHand, false);
            WeaponChange_Animation(focusGrabAnimation, oppositeHand, false);

            CoroutineLock targetHandLock = new CoroutineLock();
            CoroutineLock oppositeHandLock = new CoroutineLock();
            StartCoroutine(ChangeNextLayerWeightSubCoroutine(targetHand, false, targetHandLock));
            StartCoroutine(ChangeNextLayerWeightSubCoroutine(oppositeHand, false, oppositeHandLock));
            while (targetHandLock._isEnd == false || oppositeHandLock._isEnd == false)
            {
                yield return null;
            }
        }

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

        _corutineStarted_Weapon = false;
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

        WeaponLayerChange(nextState);
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

            _animator.SetLayerWeight((int)AnimatorLayer.FullBody_Sub, _blendTarget);

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
        if (currState.GetStateDesc()._isAttackState == true)
        {
            switch (_tempGrabFocusType)
            {
                case WeaponGrabFocus.Normal:
                    {
                        if (_tempUsingRightHandWeapon)
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f); //오른손은 반드시 따라가야해서 0.0f

                            if (_tempCurrLeftWeapon == null) //왼손무기를 쥐고있지 않다면
                            {
                                _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
                            }
                            else
                            {
                                if (_tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
                                {
                                    _overrideController[MyUtil._motionChangingAnimationNames[2]] = _tempWeaponHandling_NoPlay;
                                }
                                _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                            }

                            _animator.SetBool("IsMirroring", false);
                        }
                        else
                        {
                            _animator.SetLayerWeight(2, 0.0f); //왼손은 반드시 따라가야해서 0.0f

                            if (_tempCurrRightWeapon == null) //오른손 무기를 쥐고있지 않다면
                            {
                                _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                            }
                            else 
                            {
                                if (_tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
                                {
                                    _overrideController[MyUtil._motionChangingAnimationNames[4]] = _tempWeaponHandling_NoPlay;
                                }
                                _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                            }

                            _animator.SetBool("IsMirroring", true);
                        }
                    }
                    break;

                case WeaponGrabFocus.RightHandFocused:
                    {
                        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
                        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];

                        int rightHandCurrentLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.RightHand * 2
                            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                        int leftHandCurrentLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.LeftHand * 2
                            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;


                        //오른손과 왼손이 반드시 따라가야한다.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 0.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 0.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.LeftHandFocused:
                    {
                        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
                        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];

                        int rightHandCurrentLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.RightHand * 2
                            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                        int leftHandCurrentLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.LeftHand * 2
                            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;


                        //오른손과 왼손이 반드시 따라가야한다.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 0.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 0.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "쌍수 컨텐츠가 추가됐습니까?");
                    }
                    break;
            }
        }
        else
        {
            //다음 상태가 공격상태는 아닙니다.
            //점프, 움직임, IDLE 과 같은 플레이어의지 상태일수고있고
            //피격, 낙하와 같은 외부요인 상태일수도 있습니다.

            //_animator.SetBool("IsMirroring", false);


            switch (_tempGrabFocusType)
            {
                case WeaponGrabFocus.Normal:
                    {
                        if (_tempCurrLeftWeapon == null || //왼손 무기를 쥐고있지 않거나
                            _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //왼손 무기의 파지 애니메이션이 없다
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                        }
                        else
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                        }

                        if (_tempCurrRightWeapon == null || //오른손 무기를 쥐고있지 않거나
                            _tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //오른손 무기의 파지 애니메이션이 없다
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                        }
                        else
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                        }
                    }
                    break;

                case WeaponGrabFocus.RightHandFocused:
                    {
                        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
                        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];

                        int rightHandCurrentLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.RightHand * 2
                            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                        int leftHandCurrentLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.LeftHand * 2
                            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;


                        //오른손과 왼손이 반드시 따라가야한다.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 1.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 1.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.LeftHandFocused:
                    {
                        AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
                        AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];

                        int rightHandCurrentLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.RightHand * 2
                            : (int)AnimatorLayerTypes.RightHand * 2 + 1;

                        int leftHandCurrentLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
                            ? (int)AnimatorLayerTypes.LeftHand * 2
                            : (int)AnimatorLayerTypes.LeftHand * 2 + 1;


                        //오른손과 왼손이 반드시 따라가야한다.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 1.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 1.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "쌍수 컨텐츠가 추가됐습니까?");
                    }
                    break;
            }



            //if (_tempCurrLeftWeapon == null || //왼손 무기를 쥐고있지 않거나
            //    _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //왼손 무기의 파지 애니메이션이 없다
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
            //}
            //else
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
            //}

            //if (_tempCurrRightWeapon == null || //오른손 무기를 쥐고있지 않거나
            //    _tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //오른손 무기의 파지 애니메이션이 없다
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
            //}
            //else
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
            //}
        }
    }





    private void WeaponChangeCheck2()
    {
        if (_corutineStarted_Weapon == true) //이미 무기 바꾸기 코루틴이 진행중이다
        {
            return;
        }

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


        AnimationClip drawNextWeaponAnimationClip = null;
        AnimationClip putawayCurrentWeaponAnimationClip = null;
        AnimatorLayerTypes layerType = AnimatorLayerTypes.End;

        //코루틴 변수 결정
        {
            //부위 결정
            {
                if (tempIsRightHandWeapon == true)
                {
                    layerType = AnimatorLayerTypes.RightHand;
                }
                else
                {
                    layerType = AnimatorLayerTypes.LeftHand;
                }
            }

            //PutAway애니메이션 결정
            {
                GameObject currentWeapon = (tempIsRightHandWeapon == true)
                    ?_tempCurrRightWeapon
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
        }

        //코루틴 호출
        {
            StartCoroutine(WeaponChangingCoroutine(putawayCurrentWeaponAnimationClip, drawNextWeaponAnimationClip, layerType, nextWeaponPrefab));
        }
    }

    private IEnumerator WeaponChangingCoroutine(AnimationClip putAwayAnimationClip, AnimationClip drawAnimationClip, AnimatorLayerTypes targetBody, GameObject nextWeaponPrefab)
    {
        //무기전환을 할 수 있다면 이 코루틴이 시작될것이다.
        _corutineStarted_Weapon = true;

        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];
        Debug.Assert(targetPart != null, "해당 파트는 사용하지 않는다고 설정돼있습니다");
        bool tempIsRightWeapon = (targetBody == AnimatorLayerTypes.RightHand);


        //1. 무기 집어넣기
        if (putAwayAnimationClip != null)
        {
            //재생해야할 노드가 바뀐다. 단, 0프레임 애니메이션으로 세팅돼있다.
            {
                WeaponChange_Animation(putAwayAnimationClip, targetBody, true);
            }

            //재생해야할 노드의 Layer Weight가 바뀐다
            {
                CoroutineLock targetHandLock = new CoroutineLock();
                StartCoroutine(ChangeNextLayerWeightSubCoroutine(targetBody, false, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight 세팅 완료

            //애니메이션 다시 재생 시작
            {
                int layerIndex = (targetPart._isUsingFirstLayer == true)
                    ? (int)targetBody * 2
                    : (int)targetBody * 2 + 1;

                string nextNodeName = (targetPart._isUsingFirstLayer == true)
                    ? "State1"
                    : "State2";

                _overrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = putAwayAnimationClip;

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
                StartCoroutine(ChangeNextLayerWeightSubCoroutine(targetBody, false, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight 세팅 완료

            //다음 무기 생성
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

                //현재 파지법 계산
                {
                    if (nextWeaponScript._onlyTwoHand == true)
                    {
                        Debug.Assert(false, "작업 시작 전까지 여기에 들어와선 안된다");

                        if (tempIsRightWeapon == true)
                        {
                            //오른손을 양손으로 쥐고있다.
                            if (_tempCurrLeftWeapon != null)
                            {
                                Destroy(_tempCurrLeftWeapon);
                                _tempCurrLeftWeapon = null;
                            }
                        }
                        else
                        {
                            //왼손을 양손으로 쥐고있다.
                            if (_tempCurrRightWeapon != null)
                            {
                                Destroy(_tempCurrRightWeapon);
                                _tempCurrRightWeapon = null;
                            }
                        }
                    }
                    else
                    {
                        _tempGrabFocusType = WeaponGrabFocus.Normal;
                    }
                }
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
            //무기가 없다면 그냥 블렌드를 꺼버릴것
            {
                int mainLayerIndex = (int)targetBody * 2;
                int subLayerIndex = mainLayerIndex + 1;

                while (true)
                {
                    float mainLayerBlendDelta = Time.deltaTime * -_transitionSpeed_Weapon;

                    targetPart._blendTarget += mainLayerBlendDelta;
                    targetPart._blendTarget_Sub += mainLayerBlendDelta;

                    targetPart._blendTarget = Mathf.Clamp(targetPart._blendTarget, 0.0f, 1.0f);
                    targetPart._blendTarget_Sub = Mathf.Clamp(targetPart._blendTarget_Sub, 0.0f, 1.0f);

                    _animator.SetLayerWeight(mainLayerIndex, targetPart._blendTarget);
                    _animator.SetLayerWeight(subLayerIndex, targetPart._blendTarget_Sub);

                    float largeBlend = Mathf.Max(targetPart._blendTarget, targetPart._blendTarget_Sub);

                    if (largeBlend <= 0.0f)
                    {
                        break;
                    }

                    yield return null;
                }
            }
        }

        //코루틴 정상종료 (중간에 피격당하거나 이벤트가 없었다)
        _corutineStarted_Weapon = false;
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










































    private void WeaponChangeLayerModiry()
    {
        switch (_tempGrabFocusType)
        {
            case WeaponGrabFocus.Normal:
                //왼손
                if (_tempCurrLeftWeapon != null)
                {
                    WeaponScript currLeftWeaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currLeftWeaponScript != null, "왼손 무기에 Script가 없습니다");
                    AnimationClip leftWeaponHandlingAnimationClip = currLeftWeaponScript._handlingIdleAnimation_OneHand;

                    if (leftWeaponHandlingAnimationClip != null)
                    {
                        //쥐고있어야 하는 애니메이션이 있다.
                        _overrideController[MyUtil._motionChangingAnimationNames[2]] = leftWeaponHandlingAnimationClip;
                        _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                    }
                    else
                    {
                        _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                    }
                }
                else
                {
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                }

                //오른손
                if (_tempCurrRightWeapon != null)
                {
                    WeaponScript currRightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currRightWeaponScript != null, "오른손 무기에 Script가 없습니다");
                    AnimationClip rightWeaponHandlingAnimationClip = currRightWeaponScript._handlingIdleAnimation_OneHand;

                    if (rightWeaponHandlingAnimationClip != null)
                    {
                        //쥐고있어야 하는 애니메이션이 있다.
                        _overrideController[MyUtil._motionChangingAnimationNames[4]] = rightWeaponHandlingAnimationClip;
                        _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                    }
                    else
                    {
                        _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                    }
                }
                else
                {
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                }

                break;

            case WeaponGrabFocus.LeftHandFocused:
                //왼손
                _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);

                if (_tempCurrLeftWeapon != null)
                {
                    WeaponScript currLeftWeaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currLeftWeaponScript != null, "왼손 무기에 Script가 없습니다");

                    AnimationClip leftWeaponHandlingAnimationClip = currLeftWeaponScript._handlingIdleAnimation_TwoHand;
                    Debug.Assert(leftWeaponHandlingAnimationClip != null, "양손잡기는 애니메이션이 반드시 있어야한다");

                    _overrideController[MyUtil._motionChangingAnimationNames[4]] = leftWeaponHandlingAnimationClip;
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                }
                break;

            case WeaponGrabFocus.RightHandFocused:
                //오른손
                _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);

                if (_tempCurrRightWeapon != null)
                {
                    WeaponScript currRightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currRightWeaponScript != null, "오른손 무기에 Script가 없습니다");

                    AnimationClip rightWeaponHandlingAnimationClip = currRightWeaponScript._handlingIdleAnimation_TwoHand;
                    Debug.Assert(rightWeaponHandlingAnimationClip != null, "양손잡기는 애니메이션이 반드시 있어야한다");


                    _overrideController[MyUtil._motionChangingAnimationNames[4]] = rightWeaponHandlingAnimationClip;
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.FullBody, 1.0f);
                }
                break;

            case WeaponGrabFocus.DualGrab:
                Debug.Assert(false, "아직 양손무기는 없다");
                break;
        }
    }












    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon == null)
        {
            //무기를 아무것도 쥐고있지 않다
            Debug.Log("왼손, 오른손 무기가 '비'활성화가 됐다");
            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
        }

        else if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon != null)
        {
            //왼손, 오른손 레이어를 활성화 해서 Handling을 연출한다.
            Debug.Log("왼손, 오른손 무기가 활성화가 됐다");
            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
        }

        else
        {
            //오른손 무기만 있다.
            if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon != null)
            {
                if (_tempCurrRightWeapon.GetComponent<WeaponScript>()._onlyTwoHand == true)
                {
                    //오른손 무기를 양손으로 쥐고있다.
                    Debug.Assert(false, "작업 시작 전까지 여기에 들어와선 안된다");
                }
                else
                {
                    Debug.Log("오른손 무기가 활성화가 됐다");
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                }
            }
            //왼손무기만 있다.
            if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon == null)
            {
                if (_tempCurrLeftWeapon.GetComponent<WeaponScript>()._onlyTwoHand == true)
                {
                    //왼손 무기를 양손으로 쥐고있다.
                    Debug.Assert(false, "작업 시작 전까지 여기에 들어와선 안된다");
                }
                else
                {
                    Debug.Log("왼손 무기가 활성화가 됐다");
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                }
            }
        }
    }
}
