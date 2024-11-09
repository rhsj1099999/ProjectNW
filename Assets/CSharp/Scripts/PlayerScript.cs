using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Linq;
using Unity.VisualScripting;
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
    private int _currLeftWeaponIndex = 0;
    private int _currRightWeaponIndex = 0;

    private WeaponScript _tempCurrLeftWeapon = null;
    private WeaponScript _tempCurrRightWeapon = null;

    private WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }
    private GameObject _tempCurrLeftWeaponPrefab = null;
    public GameObject GetLeftWeaponPrefab() { return _tempCurrLeftWeaponPrefab; }
    private GameObject _tempCurrRightWeaponPrefab = null;
    public GameObject GetRightWeaponPrefab() { return _tempCurrRightWeaponPrefab; }
    private WeaponHandlingType _weaponHandlingType = WeaponHandlingType.Off;
    public WeaponHandlingType GetWeaponHandlingType() { return _weaponHandlingType; }

    [SerializeField] private List<WeaponScript> _tempLeftWeapons = new List<WeaponScript>();
    [SerializeField] private List<WeaponScript> _tempRightWeapons = new List<WeaponScript>();

    [SerializeField] private List<GameObject> _tempLeftWeaponPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> _tempRightWeaponPrefabs = new List<GameObject>();
    private WeaponHoldingType _weaponHoldingType = new WeaponHoldingType();


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
    private string _leftHandNodeName = "LeftHand";
    private string _rightHandNodeName = "RightHand";
    private int _currentLayerIndex = 0;
    private int _maxLayer = 2;
    private AnimationClip _currAnimClip = null;
    private float _currAnimationSeconds = 0.0f;
    private int _currAnimationLoopCount = 0;

    public float GetCurrAnimationClipFrame() { return _currAnimClip.frameRate * _currAnimationSeconds; }
    public float GetCurrAnimationClipSecond() { return _currAnimationSeconds; }
    public int GetCurrAnimationLoopCount() { return _currAnimationLoopCount; }

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
            bool isAimed = Input.GetButton("Fire2");

            if (isAimed != _isAim)
            {
                _isAim = isAimed;

                if (isAimed == true)
                {
                    _aimScript.OnAimState();

                    if (_currRightWeaponIndex == 1)
                    {
                        _tempRightWeapons[1].TurnOnAim();
                    }
                }
                else
                {
                    _aimScript.OffAimState();

                    if (_tempRightWeapons[_currRightWeaponIndex] != null)
                    {
                        _tempRightWeapons[_currRightWeaponIndex].TurnOffAim();
                    }
                }
            }
        }

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
        AnimationClip currAnimationClip = _stateContoller.GetCurrState().GetStateDesc()._stateAnimationClip;

        if (_currAnimClip != currAnimationClip)
        {
            _currAnimClip = currAnimationClip;
            ChangeAnimation(currAnimationClip);
        }

        _currAnimationSeconds += Time.deltaTime * _animator.speed;

        if (_currAnimationSeconds > currAnimationClip.length)
        {
            _currAnimationSeconds -= currAnimationClip.length;
            _currAnimationLoopCount++;
        }
    }

    public void StateChanged()
    {
        /*-----------------------------------------------------------------------------------------
        |TODO| 굳이 이 코드를 따로 빼야되나? Root 모션은 Late Tick 이 지나야 계산되서 필요하긴 한데
        -----------------------------------------------------------------------------------------*/
        _currAnimationSeconds = 0.0f;
        _currAnimationLoopCount = 0;
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
            ? _tempCurrRightWeaponPrefab
            : _tempCurrLeftWeaponPrefab;


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




    private IEnumerator SwitchingBlendCoroutine_Weapon()
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

    public void ChangeAnimation(AnimationClip targetClip)
    {
        /*----------------------------------------------------------------------------------------------------------
        |TODO| Jump -> Sprint 상태 전환시
        부주의로 인해 Jump -> Move -> Idle의 전환이 이루어졌다.
        이때 너무빠른 전환으로 인해 즉시 코루틴이 종료되면서 모션이 텔레포트한다 수정해라
        ----------------------------------------------------------------------------------------------------------*/
        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "재생중인 애니메이션을 잃어버렸습니다");
        }
        
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

        //Weapon Layer Change
        if (_stateContoller.GetCurrState().GetStateDesc()._rightWeaponOverride == true) 
        {
            _WeaponAnimationLayerChange();
        }
        else
        {
            _animator.SetLayerWeight(2, 0.0f);
            _animator.SetLayerWeight(3, 0.0f);
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
            if (_currLeftWeaponIndex >= 3)
            {
                _currLeftWeaponIndex = _currLeftWeaponIndex % 3;
            }
            nextWeaponPrefab = _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            //오른손 무기 다음으로 전환
            weaponChangeTry = true;

            _currRightWeaponIndex++;
            if (_currRightWeaponIndex >= 3)
            {
                _currRightWeaponIndex = _currRightWeaponIndex % 3;
            }
            nextWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

            tempIsRightHandWeapon = true;
        }



        if (weaponChangeTry == false)
        { return; }

        WeaponScript nextWeaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();

        WeaponSocketScript.SideType targetSide = (tempIsRightHandWeapon == true)
            ? WeaponSocketScript.SideType.Right
            : WeaponSocketScript.SideType.Left;

        if (nextWeaponPrefab != null)
        {
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

            //아이템 프리팹 생성
            GameObject newObject = null;
            {
                newObject = Instantiate(nextWeaponPrefab);
                nextWeaponScript = newObject.GetComponent<WeaponScript>();
                nextWeaponScript._weaponType = ItemInfo.WeaponType.MediumGun;
                nextWeaponScript.Equip(this, correctSocket);
                newObject.transform.SetParent(transform);
            }

            if (targetSide == WeaponSocketScript.SideType.Left)
            {
                _tempLeftWeaponPrefabs[_currLeftWeaponIndex] = newObject;
            }
            else if (targetSide == WeaponSocketScript.SideType.Right)
            {
                _tempRightWeaponPrefabs[_currRightWeaponIndex] = newObject;
            }
            else
            {
                Debug.Assert(false, "아직 Middle Type의 무기는 없다");
            }

            //양손으로만 쥘 수 있는 무기에 따른 후처리
            if (nextWeaponScript._onlyTwoHand == true)
            {
                Debug.Assert(false, "작업 시작 전까지 여기에 들어와선 안된다");

                if (tempIsRightHandWeapon == true)
                {
                    //오른손을 양손으로 쥐고있다.
                    _tempCurrLeftWeaponPrefab = null; //오른손을 양손으로 쥐어야 해서 왼손을 집어넣는다
                }
                else
                {
                    //왼손을 양손으로 쥐고있다.
                    _tempCurrRightWeaponPrefab = null; //왼손을 양손으로 쥐어야 해서 오른손을 집어넣는다
                }
            }


            _tempCurrLeftWeaponPrefab = _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
            _tempCurrRightWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];
        }

        /*----------------------------------------------------------------------------------------------
        |NOTI| 레이어를 활성화 하는 순간, 이제부터 레이어는 특별한 지시가 없으면
        활성화 되려는 관성을 가진다. (피격, 잠수 등등 오버라이딩을 허용하지 않는 애니메이션만 오버라이드 불가)
        ----------------------------------------------------------------------------------------------*/

        //무기 변경시 레이어 변경코드
        _WeaponAnimationLayerChange();
    }

    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeaponPrefab == null && _tempCurrRightWeaponPrefab == null)
        {
            //무기를 아무것도 쥐고있지 않다
            Debug.Log("왼손, 오른손 무기가 '비'활성화가 됐다");
            _animator.SetLayerWeight(2, 0.0f);
            _animator.SetLayerWeight(3, 0.0f);
        }

        else if (_tempCurrLeftWeaponPrefab != null && _tempCurrRightWeaponPrefab != null)
        {
            //왼손, 오른손 레이어를 활성화 해서 Handling을 연출한다.
            Debug.Log("왼손, 오른손 무기가 활성화가 됐다");
            _animator.SetLayerWeight(2, 1.0f);
            _animator.SetLayerWeight(3, 1.0f);
        }

        else
        {
            //오른손 무기만 있다.
            if (_tempCurrLeftWeaponPrefab == null && _tempCurrRightWeaponPrefab != null)
            {
                if (_tempCurrRightWeaponPrefab.GetComponent<WeaponScript>()._onlyTwoHand == true)
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
            if (_tempCurrLeftWeaponPrefab != null && _tempCurrRightWeaponPrefab == null)
            {
                if (_tempCurrLeftWeaponPrefab.GetComponent<WeaponScript>()._onlyTwoHand == true)
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






    //public void AnimationOverride(AnimationClip targetClip)
    //{
    //    {
    //        AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
    //        Debug.Assert((currentClipInfos.Length > 0), "재생중인 애니메이션을 잃어버렸습니다");
    //    }

    //    if (_currentLayerIndex == 0)
    //    {
    //        _overrideController[_targetName1] = targetClip;
    //    }
    //    else
    //    {
    //        _overrideController[_targetName2] = targetClip;
    //    }
    //}
}
