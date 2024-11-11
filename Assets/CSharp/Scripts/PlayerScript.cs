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

    private void LateUpdate()
    {
        //�ִϸ��̼� ������Ʈ
    }

    public void StateChanged()
    {
        /*-----------------------------------------------------------------------------------------
        |TODO| ���� �� �ڵ带 ���� ���ߵǳ�? Root ����� Late Tick �� ������ ���Ǽ� �ʿ��ϱ� �ѵ�
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



    public void ChangeAnimation(State nextState)
    {
        /*----------------------------------------------------------------------------------------------------------
        |TODO| Jump -> Sprint ���� ��ȯ��
        �����Ƿ� ���� Jump -> Move -> Idle�� ��ȯ�� �̷������.
        �̶� �ʹ����� ��ȯ���� ���� ��� �ڷ�ƾ�� ����Ǹ鼭 ����� �ڷ���Ʈ�Ѵ� �����ض�
        ----------------------------------------------------------------------------------------------------------*/

        /*----------------------------------------------------
        |NOTI| ��� �ִϸ��̼��� RightHand �������� ��ȭ�ƽ��ϴ�.
        ------------------------------------------------------*/

        AnimationClip targetClip = nextState.GetStateDesc()._stateAnimationClip;
        //if (targetClip == _currAnimClip)
        //{
        //    return;
        //}

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

        //���� ���̾� ����, Mirror ���� (���� �ִϸ��̼��̶��)
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
                            //������ ���⸦ ����Ϸ��մϴ�.
                            _animator.SetLayerWeight(3, 0.0f); //�������� �ݵ�� ���󰡾��ؼ� 0.0f

                            if (_tempCurrLeftWeapon == null) //�޼չ��⸦ ������� �ʴٸ�
                            {
                                _animator.SetLayerWeight(2, 0.0f);
                            }
                            else
                            {
                                _animator.SetLayerWeight(2, 1.0f);
                            }

                            //�̷��� �۾�
                            _animator.SetBool("IsMirroring", false);
                        }
                        else
                        {
                            //�޼� ���⸦ ����Ϸ��մϴ�.
                            _animator.SetLayerWeight(2, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                            if (_tempCurrRightWeapon == null) //������ ���⸦ ������� �ʴٸ�
                            {
                                _animator.SetLayerWeight(3, 0.0f);
                            }
                            else
                            {
                                _animator.SetLayerWeight(3, 1.0f);
                            }

                            //�̷��� �۾�
                            _animator.SetBool("IsMirroring", true);
                        }
                    }
                    break;

                case WeaponGrabFocus.RightHandFocused:
                    {
                        Debug.Assert(false, "������ �������� �߰��ƽ��ϱ�?");
                    }
                    break;

                case WeaponGrabFocus.LeftHandFocused:
                    {
                        Debug.Assert(false, "������ �������� �߰��ƽ��ϱ�?");
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "������ �������� �߰��ƽ��ϱ�?");
                    }
                    break;
            }
        }
        else
        {
            //���� ���°� ���ݻ��´� �ƴմϴ�.
            //����, ������, IDLE �� ���� �÷��̾����� �����ϼ����ְ�
            //�ǰ�, ���Ͽ� ���� �ܺο��� �����ϼ��� �ֽ��ϴ�.

            //_animator.SetBool("IsMirroring", false);

            if (false/*�ڵ鸵 �ִϸ��̼� ����*/)
            {

            }
            else
            {
                if (_tempCurrLeftWeapon == null) //�޼� ���⸦ ������� �ʴ�
                {
                    _animator.SetLayerWeight(2, 0.0f);
                }
                else
                {
                    _animator.SetLayerWeight(2, 1.0f);
                }


                if (_tempCurrRightWeapon == null) //������ ���⸦ ������� �ʴ�
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


        //���� ���� ����
        {
            if (tempIsRightHandWeapon == true)
            {
                if (_tempCurrRightWeapon != null) //������ �� ������ ����־���.
                {
                    Destroy(_tempCurrRightWeapon);
                }
                _tempCurrRightWeapon = null;
            }
            else
            {
                if (_tempCurrLeftWeapon != null) //�޼տ� ������ ����־���.
                {
                    Destroy(_tempCurrLeftWeapon);
                }
                _tempCurrLeftWeapon = null;
            }
        }

        //���ο� ���� �����ڵ�
        if (nextWeaponPrefab != null)
        {
            WeaponSocketScript.SideType targetSide = (tempIsRightHandWeapon == true)
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

                if (tempIsRightHandWeapon == true)
                {
                    _tempCurrRightWeapon = newObject;
                }
                else
                {
                    _tempCurrLeftWeapon = newObject;
                }
            }

            //������θ� �� �� �ִ� ���⿡ ���� ��ó��
            if (nextWeaponScript._onlyTwoHand == true)
            {
                //Debug.Assert(false, "�۾� ���� ������ ���⿡ ���ͼ� �ȵȴ�");

                //if (tempIsRightHandWeapon == true)
                //{
                //    //�������� ������� ����ִ�.
                //    _tempCurrLeftWeaponPrefab = null; //�������� ������� ���� �ؼ� �޼��� ����ִ´�
                //}
                //else
                //{
                //    //�޼��� ������� ����ִ�.
                //    _tempCurrRightWeaponPrefab = null; //�޼��� ������� ���� �ؼ� �������� ����ִ´�
                //}
            }
        }

        //���� ����� ���̾� �����ڵ�
        //_WeaponAnimationLayerChange();
        WeaponLayerChange(_stateContoller.GetCurrState());
    }

    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon == null)
        {
            //���⸦ �ƹ��͵� ������� �ʴ�
            Debug.Log("�޼�, ������ ���Ⱑ '��'Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight(2, 0.0f);
            _animator.SetLayerWeight(3, 0.0f);
        }

        else if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon != null)
        {
            //�޼�, ������ ���̾ Ȱ��ȭ �ؼ� Handling�� �����Ѵ�.
            Debug.Log("�޼�, ������ ���Ⱑ Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight(2, 1.0f);
            _animator.SetLayerWeight(3, 1.0f);
        }

        else
        {
            //������ ���⸸ �ִ�.
            if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon != null)
            {
                if (_tempCurrRightWeapon.GetComponent<WeaponScript>()._onlyTwoHand == true)
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
            if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon == null)
            {
                if (_tempCurrLeftWeapon.GetComponent<WeaponScript>()._onlyTwoHand == true)
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
}
