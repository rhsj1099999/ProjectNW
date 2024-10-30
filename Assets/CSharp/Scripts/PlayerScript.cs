using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    //대표 컴포넌트
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> 이거 다른 컴포넌트로 빼세요(현재 만들어져있는건 EquipmentBoard 혹은 Inventory)
    private WeaponScript _currEquipWeapon = null;
    private ItemInfo _currEquipWeaponItem = null;
    public WeaponScript GetWeapon() { return _currEquipWeapon; }
    public ItemInfo GetWeaponItem() { return _currEquipWeaponItem; }
    private List<WeaponScript> _tempLeftWeapons = new List<WeaponScript>();
    private int _currLeftWeaponIndex = 0;
    private WeaponScript _tempCurrLeftWeapon = null;
    private List<WeaponScript> _tempRightWeapons = new List<WeaponScript>();
    private int _currRightWeaponIndex = 0;
    private WeaponScript _tempCurrTightWeapon = null;
    //현재는 캐릭터메쉬가 애니메이터를 갖고있기 때문에 애니메이터를 갖고있는 게임오브젝트가 캐릭터 메쉬다
    private GameObject _characterModelObject = null; //애니메이터는 얘가 갖고있다
    public Animator GetAnimator() { return _animator; }


    //Aim System
    private AimScript2 _aimScript = null;
    private bool _isAim = false;



    //Animator Secton -> 이거 다른 컴포넌트로 빼세요
    private Animator _animator = null;
    private AnimatorOverrideController _overrideController = null;
    private bool _corutineStarted = false;
    private float _blendTarget = 0.0f;
    private string _targetName1 = "Human@Idle01";
    private string _targetName2 = "UseThisToChange1";
    private int _currentLayerIndex = 0;
    private int _maxLayer = 2;
    private AnimationClip _currAnimClip = null;
    [SerializeField] private float _transitionSpeed = 10.0f;
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

        /*------------------------
        |TODO| 임시코드이다. 지워라
        --------------------------*/
        {
            _tempLeftWeapons.Add(gameObject.AddComponent<WeaponScript>());
            _tempLeftWeapons.Add(gameObject.AddComponent<WeaponScript>());
            _tempLeftWeapons.Add(gameObject.AddComponent<WeaponScript>());

            _tempRightWeapons.Add(gameObject.AddComponent<WeaponScript>());
            _tempRightWeapons[0]._weaponType = ItemInfo.WeaponType.LargeSword;

            _tempRightWeapons.Add(gameObject.AddComponent<Gunscript2>());
            //_tempRightWeapons[1]._weaponType = ItemInfo.WeaponType.MediumGun;
            _tempRightWeapons[1]._weaponType = ItemInfo.WeaponType.LargeSword;
            //어깨에 붙인다는 이론은 더이상 쓰지 않는다
            //어깨는 IK를 통해 따라간다.
            _tempRightWeapons[1]._itemPrefabRoute = "RuntimePrefab/Weapon/GunPivot";

            _tempRightWeapons.Add(gameObject.AddComponent<WeaponScript>());

            for (int i = 0; i < 3; i++)
            {
                _tempLeftWeapons[i].enabled = false;
                _tempRightWeapons[i].enabled = false;
            }
        }
    }

    private void Start()
    {
        State currState = _stateContoller.GetCurrState();
        AnimationClip currAnimationClip = currState.GetStateDesc()._stateAnimationClip;
        _currAnimClip = currAnimationClip;
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

        /*-----------------------------------------------------------------------------------------------------------------
        |TODO| 이 섹션이 있어야 하나? 공통적으로 쓰이는 스테이트에 대해서, 만들어져있는 스테이트들에게 이어붙이는게 귀찮아서 빼긴했음
        -----------------------------------------------------------------------------------------------------------------*/
        //Weapon Change Check
        {
            WeaponScript nextWeapon = null;

            bool weaponChangeTry = false;

            if (Input.GetKeyDown(KeyCode.R))
            {
                //왼손 무기 다음으로 전환
                weaponChangeTry = true;

                _currLeftWeaponIndex++;
                if (_currLeftWeaponIndex >= 3)
                {
                    _currLeftWeaponIndex = _currLeftWeaponIndex % 3;
                }
                nextWeapon = _tempLeftWeapons[_currLeftWeaponIndex];
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
                nextWeapon = _tempRightWeapons[_currRightWeaponIndex];
            }

            //무기 전환을 시도했다
            if (weaponChangeTry == true) 
            {
                //다음 무기가 없다.
                if (nextWeapon == null)
                {
                    if (_aimScript != null)
                    {
                        _aimScript.enabled = false;
                    }
                }
                else
                {
                    //다음 무기가 있다.
                    //nextWeapon.enabled = true;
                    //1. 조준 시스템 활성/비활성화
                    {
                        //if (nextWeapon._weaponType >= ItemInfo.WeaponType.NotWeapon && nextWeapon._weaponType <= ItemInfo.WeaponType.LargeSword)
                        //{
                        //    if (_aimScript != null)
                        //    {
                        //        _aimScript.enabled = false;
                        //    }
                        //}

                        //if (nextWeapon._weaponType >= ItemInfo.WeaponType.SmallGun && nextWeapon._weaponType <= ItemInfo.WeaponType.LargeGun)
                        //{
                        //    ReadyAimSystem();
                        //}

                        ReadyAimSystem();
                    }

                    //2. 아이템 소켓 장착(메쉬 붙이기) -> 필요하다면 IK 까지
                    GameObject prefab = Resources.Load<GameObject>(nextWeapon._itemPrefabRoute);
                    if (prefab != null)
                    {


                        Transform correctSocket = null;
                        //소켓 찾기
                        {
                            Debug.Assert(_characterModelObject != null, "무기를 붙이려는데 모델이 없어서는 안된다");

                            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();
                            Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");

                            ItemInfo.WeaponType targetType = nextWeapon._weaponType;

                            foreach (var socketComponent in weaponSockets)
                            {
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
                            newObject = Instantiate(prefab);
                            WeaponScript component = newObject.GetComponent<WeaponScript>();
                            component.Equip(this, correctSocket);
                            _tempRightWeapons[1] = component;

                            //조종자의 자식으로 부모를 설정해야한다
                            //움직임은 WeaponScript의 Follow를 호출해 직접 움직인다
                            newObject.transform.SetParent(transform);
                            newObject.transform.localPosition = Vector3.zero;
                        }

                        /*-----------------------------------------------------------------------------------------------------------------
                        |NOTI| 아이템 프리팹은 기본 PIVOT을 들고있다.
                        무기의 위치는 자식 Transform으로 결정돠면 안된다 : (IK를 이용할 가능성 때문에)
                        -----------------------------------------------------------------------------------------------------------------*/

                        //Vector3 weaponPivot = newObject.transform.localPosition;
                        //newObject.transform.localPosition = Vector3.zero;
                        //newObject.transform.position = correctSocket.position;
                        //newObject.transform.rotation = correctSocket.rotation;
                        //newObject.transform.localPosition = weaponPivot;
                    }
                }
            }

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

        /*-----------------------------------------------------------------------------------------------------------------
        |TODO| 현재는 이 컴포넌트가 사용할 상태를 전부 들고있다. 공격상태의 경우에는 무기가 상태를 들고있어야 하지 않을까?
        -----------------------------------------------------------------------------------------------------------------*/
        //Weapon Change Check
        {
            //여기서 상태전환을 시도했으면 -----|
        }                                   //|
        {//상태 업데이트                     //|
            _stateContoller.DoWork(); // <----| 이걸 씹어야한다
        }


        {//기본적으로 중력은 계속 업데이트 한다
            _characterMoveScript2.GravityUpdate();
        }

        _characterMoveScript2.ClearLatestVelocity();
    }

    private void LateUpdate()
    {
        {//애니메이션 업데이트
            State currState = _stateContoller.GetCurrState();
            AnimationClip currAnimationClip = currState.GetStateDesc()._stateAnimationClip;

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

            /*-----------------------------------------------------------------
            |TODO| 이 코드는 뭐야
            -----------------------------------------------------------------*/

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

    public void AnimationOverride(AnimationClip targetClip)
    {
        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "재생중인 애니메이션을 잃어버렸습니다");
        }
        if (_currentLayerIndex == 0)
        {
            _overrideController[_targetName1] = targetClip;
        }
        else
        {
            _overrideController[_targetName2] = targetClip;
        }
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
    }
}
