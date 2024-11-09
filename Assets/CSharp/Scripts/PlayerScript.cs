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
    public enum HoldingType //������������(������ �߰�) �̳Ѱ� �ø����
    {
        None, //�տ� �ƹ��͵� ����.
        SmallMelee,             //SmallMelee : ��Ŭ��, �ܰ� ���
        TwoSmallMelee,          //SmallMelee : ��Ŭ��, �ܰ� ���
        MediumMeelee,           //MediumMelee : �Ѽհ�, �Ϲ� ���� ���
        TwoMediumMeelee,         //MediumMelee : �Ѽհ�, �Ϲ� ���� ���
        LargeMelee,
        TwoLargeMelee,
    }


    void CalculateCurrHoldingType(WeaponScript leftHand, WeaponScript rightHand)
    {
        if (leftHand == null && rightHand == null) //�Ѵ� �����ϰ����� �ʴ�.
        {
            _currHoldingType = HoldingType.None;
            return;
        }


        if (leftHand != null && rightHand != null) //�Ѵ� ������ �����ϰ� �ִ�.
        {
            _currHoldingType = HoldingType.None;
            return;
        }
    }

    public HoldingType _currHoldingType = HoldingType.None;
    public bool _isRightSided = true;  //�����տ��� �����ϰ� �ִ�
    public bool _isSideFocused = true; //�Ѽչ��⸦ ������� ��ҽ��ϱ�
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
    //��ǥ ������Ʈ
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> �̰� �ٸ� ������Ʈ�� ������(���� ��������ִ°� EquipmentBoard Ȥ�� Inventory)
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

        //�ӽ� ���⺯�� �ڵ�
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

        //�⺻������ �߷��� ��� ������Ʈ �Ѵ�
        {
            _characterMoveScript2.GravityUpdate();
            _characterMoveScript2.ClearLatestVelocity();

        }
    }

    private void LateUpdate()
    {
        //�ִϸ��̼� ������Ʈ
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
        |TODO| ���� �� �ڵ带 ���� ���ߵǳ�? Root ����� Late Tick �� ������ ���Ǽ� �ʿ��ϱ� �ѵ�
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
        //Layer �ε��� ����
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0�� ���̾ ����ؾ��ϳ�
                ? Time.deltaTime * -_transitionSpeed   //0�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 0.0
                : Time.deltaTime * _transitionSpeed;  //1�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 1.0

            _blendTarget += blendDelta;

            /*-------------------
            |TODO| �� �ڵ�� ����
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
        //Layer �ε��� ����
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0�� ���̾ ����ؾ��ϳ�
                ? Time.deltaTime * -_transitionSpeed   //0�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 0.0
                : Time.deltaTime * _transitionSpeed;  //1�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 1.0

            _blendTarget += blendDelta;

            /*-------------------
            |TODO| �� �ڵ�� ����
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
        |TODO| Jump -> Sprint ���� ��ȯ��
        �����Ƿ� ���� Jump -> Move -> Idle�� ��ȯ�� �̷������.
        �̶� �ʹ����� ��ȯ���� ���� ��� �ڷ�ƾ�� ����Ǹ鼭 ����� �ڷ���Ʈ�Ѵ� �����ض�
        ----------------------------------------------------------------------------------------------------------*/
        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "������� �ִϸ��̼��� �Ҿ���Ƚ��ϴ�");
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
            //�޼� ���� �������� ��ȯ
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
            //������ ���� �������� ��ȯ
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

            //������ ������ ����
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
                Debug.Assert(false, "���� Middle Type�� ����� ����");
            }

            //������θ� �� �� �ִ� ���⿡ ���� ��ó��
            if (nextWeaponScript._onlyTwoHand == true)
            {
                Debug.Assert(false, "�۾� ���� ������ ���⿡ ���ͼ� �ȵȴ�");

                if (tempIsRightHandWeapon == true)
                {
                    //�������� ������� ����ִ�.
                    _tempCurrLeftWeaponPrefab = null; //�������� ������� ���� �ؼ� �޼��� ����ִ´�
                }
                else
                {
                    //�޼��� ������� ����ִ�.
                    _tempCurrRightWeaponPrefab = null; //�޼��� ������� ���� �ؼ� �������� ����ִ´�
                }
            }


            _tempCurrLeftWeaponPrefab = _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
            _tempCurrRightWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];
        }

        /*----------------------------------------------------------------------------------------------
        |NOTI| ���̾ Ȱ��ȭ �ϴ� ����, �������� ���̾�� Ư���� ���ð� ������
        Ȱ��ȭ �Ƿ��� ������ ������. (�ǰ�, ��� ��� �������̵��� ������� �ʴ� �ִϸ��̼Ǹ� �������̵� �Ұ�)
        ----------------------------------------------------------------------------------------------*/

        //���� ����� ���̾� �����ڵ�
        _WeaponAnimationLayerChange();
    }

    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeaponPrefab == null && _tempCurrRightWeaponPrefab == null)
        {
            //���⸦ �ƹ��͵� ������� �ʴ�
            Debug.Log("�޼�, ������ ���Ⱑ '��'Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight(2, 0.0f);
            _animator.SetLayerWeight(3, 0.0f);
        }

        else if (_tempCurrLeftWeaponPrefab != null && _tempCurrRightWeaponPrefab != null)
        {
            //�޼�, ������ ���̾ Ȱ��ȭ �ؼ� Handling�� �����Ѵ�.
            Debug.Log("�޼�, ������ ���Ⱑ Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight(2, 1.0f);
            _animator.SetLayerWeight(3, 1.0f);
        }

        else
        {
            //������ ���⸸ �ִ�.
            if (_tempCurrLeftWeaponPrefab == null && _tempCurrRightWeaponPrefab != null)
            {
                if (_tempCurrRightWeaponPrefab.GetComponent<WeaponScript>()._onlyTwoHand == true)
                {
                    //������ ���⸦ ������� ����ִ�.
                    Debug.Assert(false, "�۾� ���� ������ ���⿡ ���ͼ� �ȵȴ�");
                }
                else
                {
                    Debug.Log("������ ���Ⱑ Ȱ��ȭ�� �ƴ�");
                    _animator.SetLayerWeight(2, 0.0f);
                    _animator.SetLayerWeight(3, 1.0f);
                }
            }
            //�޼չ��⸸ �ִ�.
            if (_tempCurrLeftWeaponPrefab != null && _tempCurrRightWeaponPrefab == null)
            {
                if (_tempCurrLeftWeaponPrefab.GetComponent<WeaponScript>()._onlyTwoHand == true)
                {
                    //�޼� ���⸦ ������� ����ִ�.
                    Debug.Assert(false, "�۾� ���� ������ ���⿡ ���ͼ� �ȵȴ�");
                }
                else
                {
                    Debug.Log("�޼� ���Ⱑ Ȱ��ȭ�� �ƴ�");
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
    //        Debug.Assert((currentClipInfos.Length > 0), "������� �ִϸ��̼��� �Ҿ���Ƚ��ϴ�");
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
