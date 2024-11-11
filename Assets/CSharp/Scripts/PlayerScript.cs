using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

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

public class WeaponHoldingType
{
    public enum HoldingType //생각날때마다(컨텐츠 추가) 이넘값 늘리기로
    {
        None, //손에 아무것도 없다.
        SmallMelee,             //SmallMelee : 버클러, 단검 등등
        TwoSmallMelee,          //SmallMelee : 버클러, 단검 등등
        MediumMeelee,           //MediumMelee : 한손검, 일반 방패 등등
        TwoMediumMeelee,         //MediumMelee : 한손검, 일반 방패 등등
        LargeMelee,
        TwoLargeMelee,
    }


    void CalculateCurrHoldingType(WeaponScript leftHand, WeaponScript rightHand)
    {
        if (leftHand == null && rightHand == null) //둘다 장착하고있지 않다.
        {
            _currHoldingType = HoldingType.None;
            return;
        }


        if (leftHand != null && rightHand != null) //둘다 뭔가를 장착하고 있다.
        {
            _currHoldingType = HoldingType.None;
            return;
        }
    }

    public HoldingType _currHoldingType = HoldingType.None;
    public bool _isRightSided = true;  //오른손에만 장착하고 있다
    public bool _isSideFocused = true; //한손무기를 양손으로 잡았습니까
}

public enum WeaponGrabFocus
{
    Normal,
    RightHandFocused,
    LeftHandFocused,
    DualGrab,
}

public class PlayerScript : MonoBehaviour
{
    //대표 컴포넌트
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> 이거 다른 컴포넌트로 빼세요(현재 만들어져있는건 EquipmentBoard 혹은 Inventory)
    [SerializeField] private List<GameObject> _tempLeftWeaponPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> _tempRightWeaponPrefabs = new List<GameObject>();

    private int _currLeftWeaponIndex = 0;
    private int _currRightWeaponIndex = 0;

    private GameObject _tempCurrLeftWeapon = null;
    private GameObject _tempCurrRightWeapon = null;

    public GameObject GetLeftWeapon() { return _tempCurrLeftWeapon; }
    public GameObject GetRightWeapon() { return _tempCurrRightWeapon; }

    private WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }

    //private WeaponHandlingType _weaponHandlingType = WeaponHandlingType.Off;
    //public WeaponHandlingType GetWeaponHandlingType() { return _weaponHandlingType; }

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

        //Animation_TotalBlendSection
        private bool _corutineStarted = false;
        private float _blendTarget = 0.0f;
        [SerializeField] private float _transitionSpeed = 10.0f;

        //Animation_WeaponBlendSection
        private bool _corutineStarted_Weapon = false;
        private float _blendTarget_Weapon = 0.0f;
        [SerializeField] private float _transitionSpeed_Weapon = 10.0f;

    private string _targetName1 = "Human@Idle01";
    private string _targetName2 = "UseThisToChange1";
    private int _tempMaxWeaponSlot = 3;

    private int _currentLayerIndex = 0;
    private int _maxLayer = 2;
    private AnimationClip _currAnimClip = null;


    //public float GetCurrAnimationClipFrame() { return _currAnimClip.frameRate * _currAnimationSeconds; }
    //public float GetCurrAnimationClipSecond() { return _currAnimationSeconds; }
    //public int GetCurrAnimationLoopCount() { return _currAnimationLoopCount; }
    //private float _currAnimationSeconds = 0.0f;
    //private int _currAnimationLoopCount = 0;
    //private string _leftHandNodeName = "LeftHand";
    //private string _rightHandNodeName = "RightHand";

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

        //임시 무기변경 코드
        {
            WeaponChangeCheck();
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

    private void LateUpdate()
    {
        //애니메이션 업데이트
    }

    public void StateChanged()
    {
        /*-----------------------------------------------------------------------------------------
        |TODO| 굳이 이 코드를 따로 빼야되나? Root 모션은 Late Tick 이 지나야 계산되서 필요하긴 한데
        -----------------------------------------------------------------------------------------*/
        //_currAnimationSeconds = 0.0f;
        //_currAnimationLoopCount = 0;

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


    private IEnumerator SwitchingBlendCoroutine()
    {
        //Layer 인덱스 변경
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0번 레이어를 재생해야하나
                ? Time.deltaTime * -_transitionSpeed   //0번 레이어를 재생해야한다면 목표값은 0.0
                : Time.deltaTime * _transitionSpeed;  //1번 레이어를 재생해야한다면 목표값은 1.0

            _blendTarget += blendDelta;

            /*-------------------
            |TODO| 이 코드는 뭐야
            ---------------------*/
            if (_blendTarget > 1.0f)
            {
                _blendTarget = 1.0f - float.Epsilon;
                _animator.SetLayerWeight(1, _blendTarget);
                break;
            }
            else if (_blendTarget < 0.0f)
            {
                _blendTarget = 0.0f;
                _animator.SetLayerWeight(1, _blendTarget);
                break;
            }

            _animator.SetLayerWeight(1, _blendTarget);

            yield return null;
        }

        _corutineStarted = false;
    }



    public void ChangeAnimation(State nextState)
    {
        /*----------------------------------------------------------------------------------------------------------
        |TODO| Jump -> Sprint 상태 전환시
        부주의로 인해 Jump -> Move -> Idle의 전환이 이루어졌다.
        이때 너무빠른 전환으로 인해 즉시 코루틴이 종료되면서 모션이 텔레포트한다 수정해라
        ----------------------------------------------------------------------------------------------------------*/

        /*----------------------------------------------------
        |NOTI| 모든 애니메이션은 RightHand 기준으로 녹화됐습니다.
        ------------------------------------------------------*/

        AnimationClip targetClip = nextState.GetStateDesc()._stateAnimationClip;
        //if (targetClip == _currAnimClip)
        //{
        //    return;
        //}

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
                _overrideController[_targetName1] = targetClip;
                nextNodeName = "State1";
            }
            else
            {
                _overrideController[_targetName2] = targetClip;
                nextNodeName = "State2";
            }

            _animator.Play(nextNodeName, _currentLayerIndex, 0.0f);

            //Start Coroutine
            if (_corutineStarted == false)
            {
                _corutineStarted = true;
                StartCoroutine("SwitchingBlendCoroutine");
            }

            _currAnimClip = targetClip;
        }

        //무기 레이어 변경, Mirror 적용 (무기 애니메이션이라면)
        WeaponLayerChange(nextState);
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
                            //오른손 무기를 사용하려합니다.
                            _animator.SetLayerWeight(3, 0.0f); //오른손은 반드시 따라가야해서 0.0f

                            if (_tempCurrLeftWeapon == null) //왼손무기를 쥐고있지 않다면
                            {
                                _animator.SetLayerWeight(2, 0.0f);
                            }
                            else
                            {
                                _animator.SetLayerWeight(2, 1.0f);
                            }

                            //미러링 작업
                            _animator.SetBool("IsMirroring", false);
                        }
                        else
                        {
                            //왼손 무기를 사용하려합니다.
                            _animator.SetLayerWeight(2, 0.0f); //왼손은 반드시 따라가야해서 0.0f

                            if (_tempCurrRightWeapon == null) //오른손 무기를 쥐고있지 않다면
                            {
                                _animator.SetLayerWeight(3, 0.0f);
                            }
                            else
                            {
                                _animator.SetLayerWeight(3, 1.0f);
                            }

                            //미러링 작업
                            _animator.SetBool("IsMirroring", true);
                        }
                    }
                    break;

                case WeaponGrabFocus.RightHandFocused:
                    {
                        Debug.Assert(false, "양손잡기 컨텐츠가 추가됐습니까?");
                    }
                    break;

                case WeaponGrabFocus.LeftHandFocused:
                    {
                        Debug.Assert(false, "양손잡기 컨텐츠가 추가됐습니까?");
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "양손잡기 컨텐츠가 추가됐습니까?");
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

            if (false/*핸들링 애니메이션 관련*/)
            {

            }
            else
            {
                if (_tempCurrLeftWeapon == null) //왼손 무기를 쥐고있지 않다
                {
                    _animator.SetLayerWeight(2, 0.0f);
                }
                else
                {
                    _animator.SetLayerWeight(2, 1.0f);
                }


                if (_tempCurrRightWeapon == null) //오른손 무기를 쥐고있지 않다
                {
                    _animator.SetLayerWeight(3, 0.0f);
                }
                else
                {
                    _animator.SetLayerWeight(3, 1.0f);
                }

            }
        }
    }







    private void WeaponChangeCheck()
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


        //기존 무기 삭제
        {
            if (tempIsRightHandWeapon == true)
            {
                if (_tempCurrRightWeapon != null) //오른손 에 뭔가를 쥐고있었다.
                {
                    Destroy(_tempCurrRightWeapon);
                }
                _tempCurrRightWeapon = null;
            }
            else
            {
                if (_tempCurrLeftWeapon != null) //왼손에 뭔가를 쥐고있었다.
                {
                    Destroy(_tempCurrLeftWeapon);
                }
                _tempCurrLeftWeapon = null;
            }
        }

        //새로운 무기 장착코드
        if (nextWeaponPrefab != null)
        {
            WeaponSocketScript.SideType targetSide = (tempIsRightHandWeapon == true)
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

                if (tempIsRightHandWeapon == true)
                {
                    _tempCurrRightWeapon = newObject;
                }
                else
                {
                    _tempCurrLeftWeapon = newObject;
                }
            }

            //양손으로만 쥘 수 있는 무기에 따른 후처리
            if (nextWeaponScript._onlyTwoHand == true)
            {
                //Debug.Assert(false, "작업 시작 전까지 여기에 들어와선 안된다");

                //if (tempIsRightHandWeapon == true)
                //{
                //    //오른손을 양손으로 쥐고있다.
                //    _tempCurrLeftWeaponPrefab = null; //오른손을 양손으로 쥐어야 해서 왼손을 집어넣는다
                //}
                //else
                //{
                //    //왼손을 양손으로 쥐고있다.
                //    _tempCurrRightWeaponPrefab = null; //왼손을 양손으로 쥐어야 해서 오른손을 집어넣는다
                //}
            }
        }

        //무기 변경시 레이어 변경코드
        //_WeaponAnimationLayerChange();
        WeaponLayerChange(_stateContoller.GetCurrState());
    }

    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon == null)
        {
            //무기를 아무것도 쥐고있지 않다
            Debug.Log("왼손, 오른손 무기가 '비'활성화가 됐다");
            _animator.SetLayerWeight(2, 0.0f);
            _animator.SetLayerWeight(3, 0.0f);
        }

        else if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon != null)
        {
            //왼손, 오른손 레이어를 활성화 해서 Handling을 연출한다.
            Debug.Log("왼손, 오른손 무기가 활성화가 됐다");
            _animator.SetLayerWeight(2, 1.0f);
            _animator.SetLayerWeight(3, 1.0f);
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
                    _animator.SetLayerWeight(2, 0.0f);
                    _animator.SetLayerWeight(3, 1.0f);
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
                    _animator.SetLayerWeight(2, 1.0f);
                    _animator.SetLayerWeight(3, 0.0f);
                }
            }
        }
    }
}
