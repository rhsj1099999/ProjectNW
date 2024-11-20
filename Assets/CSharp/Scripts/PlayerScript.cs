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


    private void ChangeWeaponHandling()
    {
        if (_corutineStarted_Weapon == true) //�̹� ���� �ٲٱ� �ڷ�ƾ�� �������̴�
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
            return; //������ �õ��� �̷������ �ʾҴ�. �ƹ��ϵ� �Ͼ�� �ʴ´�
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

        //�ڷ�ƾ ȣ��
        if (isRelease == true)
        {
            StartCoroutine(ChangeWeaponHandling_ReleaseMode(isRightHandWeapon));
            return;
        }


        GameObject targetWeapon = null;
        GameObject oppositeWeapon = null;
        AnimatorLayerTypes targetHandType = AnimatorLayerTypes.End;
        AnimatorLayerTypes oppositeHandType = AnimatorLayerTypes.End;

        //�ڷ�ƾ ��������
        {
            targetWeapon = (isRightHandWeapon == true)
                ? _tempCurrRightWeapon
                : _tempCurrLeftWeapon;

            if (targetWeapon == null)
            {
                return; //�����⸦ �õ������� ���Ⱑ ����.
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





        //�ڷ�ƾ ȣ��
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

        if (targetWeaponsOneHandHandlingAnimation == null) //���� �ִϸ��̼��� �ֽ��ϴ�.
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
            //|TODO| �����Ҵٰ� �Ѽ������� �ݴ�չ��� �ڵ����� ������ �ڵ�¥���ִµ�,
            //���߿� �׳� ���� ����ֱ� ����鼭 �����Ұ�
            //--------------------------------------------------------------*/
            //if (currOppositeWeaponPrefabs != null) //�Ʊ� ������� �����鼭 ������� ���Ⱑ �ֽ��ϴ�.
            //{
            //    WeaponChange_Animation(currOppositeWeaponPrefabs.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand, oppositePart, false);
            //    isOppositedLayerWeightModify = true;
            //}//�ִϸ��̼��� �ٲ�� Ÿ�� ���̾ �ٲ����.
        }

        //���� LayerWeight�� �����Ѵ�.


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




        //�������� ����Ѵ�
        {
            _tempGrabFocusType = WeaponGrabFocus.Normal;
        }

        _corutineStarted_Weapon = false;
    }


    private IEnumerator ChangeWeaponHandlingCoroutine(GameObject targetWeapon, GameObject oppositeWeapon, bool isRightHandWeapon, AnimatorLayerTypes targetHand, AnimatorLayerTypes oppositeHand)
    {
        _corutineStarted_Weapon = true;

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();
        Debug.Assert(targetWeaponScript != null, "����� �ݵ�� ��ũ��Ʈ�� �־���Ѵ�");

        //�ݴ� �տ� ���⸦ ����־���.
        if (oppositeWeapon != null)
        {
            //���⸦ ����ִ� ������ �����Ѵ�
        }

        //���� ����ֱⰡ ������. �ݴ�տ� ���Ⱑ �־��ٸ� ���⸦ �����Ѵ�.
        {

        }

        AnimationClip focusGrabAnimation = targetWeaponScript._handlingIdleAnimation_TwoHand;

        //�ش� ���⸦ ������� ��� �ִϸ��̼��� �����Ѵ�. �ش� ���� �ִϸ��̼��� �ٲ۴�
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

        WeaponLayerChange(nextState);
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
                            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f); //�������� �ݵ�� ���󰡾��ؼ� 0.0f

                            if (_tempCurrLeftWeapon == null) //�޼չ��⸦ ������� �ʴٸ�
                            {
                                _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
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
                            _animator.SetLayerWeight(2, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                            if (_tempCurrRightWeapon == null) //������ ���⸦ ������� �ʴٸ�
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


                        //�����հ� �޼��� �ݵ�� ���󰡾��Ѵ�.
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


                        //�����հ� �޼��� �ݵ�� ���󰡾��Ѵ�.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 0.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 0.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
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


            switch (_tempGrabFocusType)
            {
                case WeaponGrabFocus.Normal:
                    {
                        if (_tempCurrLeftWeapon == null || //�޼� ���⸦ ������� �ʰų�
                            _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //�޼� ������ ���� �ִϸ��̼��� ����
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                        }
                        else
                        {
                            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                        }

                        if (_tempCurrRightWeapon == null || //������ ���⸦ ������� �ʰų�
                            _tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //������ ������ ���� �ִϸ��̼��� ����
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


                        //�����հ� �޼��� �ݵ�� ���󰡾��Ѵ�.
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


                        //�����հ� �޼��� �ݵ�� ���󰡾��Ѵ�.
                        _animator.SetLayerWeight(rightHandCurrentLayer, 1.0f);
                        _animator.SetLayerWeight(leftHandCurrentLayer, 1.0f);

                        _animator.SetBool("IsMirroring", false);
                    }
                    break;

                case WeaponGrabFocus.DualGrab:
                    {
                        Debug.Assert(false, "�ּ� �������� �߰��ƽ��ϱ�?");
                    }
                    break;
            }



            //if (_tempCurrLeftWeapon == null || //�޼� ���⸦ ������� �ʰų�
            //    _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //�޼� ������ ���� �ִϸ��̼��� ����
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
            //}
            //else
            //{
            //    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
            //}

            //if (_tempCurrRightWeapon == null || //������ ���⸦ ������� �ʰų�
            //    _tempCurrRightWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //������ ������ ���� �ִϸ��̼��� ����
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
        if (_corutineStarted_Weapon == true) //�̹� ���� �ٲٱ� �ڷ�ƾ�� �������̴�
        {
            return;
        }

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


        AnimationClip drawNextWeaponAnimationClip = null;
        AnimationClip putawayCurrentWeaponAnimationClip = null;
        AnimatorLayerTypes layerType = AnimatorLayerTypes.End;

        //�ڷ�ƾ ���� ����
        {
            //���� ����
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

            //PutAway�ִϸ��̼� ����
            {
                GameObject currentWeapon = (tempIsRightHandWeapon == true)
                    ?_tempCurrRightWeapon
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
        }

        //�ڷ�ƾ ȣ��
        {
            StartCoroutine(WeaponChangingCoroutine(putawayCurrentWeaponAnimationClip, drawNextWeaponAnimationClip, layerType, nextWeaponPrefab));
        }
    }

    private IEnumerator WeaponChangingCoroutine(AnimationClip putAwayAnimationClip, AnimationClip drawAnimationClip, AnimatorLayerTypes targetBody, GameObject nextWeaponPrefab)
    {
        //������ȯ�� �� �� �ִٸ� �� �ڷ�ƾ�� ���۵ɰ��̴�.
        _corutineStarted_Weapon = true;

        AnimatorBlendingDesc targetPart = _partBlendingDesc[(int)targetBody];
        Debug.Assert(targetPart != null, "�ش� ��Ʈ�� ������� �ʴ´ٰ� �������ֽ��ϴ�");
        bool tempIsRightWeapon = (targetBody == AnimatorLayerTypes.RightHand);


        //1. ���� ����ֱ�
        if (putAwayAnimationClip != null)
        {
            //����ؾ��� ��尡 �ٲ��. ��, 0������ �ִϸ��̼����� ���õ��ִ�.
            {
                WeaponChange_Animation(putAwayAnimationClip, targetBody, true);
            }

            //����ؾ��� ����� Layer Weight�� �ٲ��
            {
                CoroutineLock targetHandLock = new CoroutineLock();
                StartCoroutine(ChangeNextLayerWeightSubCoroutine(targetBody, false, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight ���� �Ϸ�

            //�ִϸ��̼� �ٽ� ��� ����
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
                StartCoroutine(ChangeNextLayerWeightSubCoroutine(targetBody, false, targetHandLock));
                while (targetHandLock._isEnd == false)
                {
                    yield return null;
                }
            }

            //Layer Weight ���� �Ϸ�

            //���� ���� ����
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

                //���� ������ ���
                {
                    if (nextWeaponScript._onlyTwoHand == true)
                    {
                        Debug.Assert(false, "�۾� ���� ������ ���⿡ ���ͼ� �ȵȴ�");

                        if (tempIsRightWeapon == true)
                        {
                            //�������� ������� ����ִ�.
                            if (_tempCurrLeftWeapon != null)
                            {
                                Destroy(_tempCurrLeftWeapon);
                                _tempCurrLeftWeapon = null;
                            }
                        }
                        else
                        {
                            //�޼��� ������� ����ִ�.
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
            //���Ⱑ ���ٸ� �׳� ���带 ��������
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

        //�ڷ�ƾ �������� (�߰��� �ǰݴ��ϰų� �̺�Ʈ�� ������)
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
                //�޼�
                if (_tempCurrLeftWeapon != null)
                {
                    WeaponScript currLeftWeaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currLeftWeaponScript != null, "�޼� ���⿡ Script�� �����ϴ�");
                    AnimationClip leftWeaponHandlingAnimationClip = currLeftWeaponScript._handlingIdleAnimation_OneHand;

                    if (leftWeaponHandlingAnimationClip != null)
                    {
                        //����־�� �ϴ� �ִϸ��̼��� �ִ�.
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

                //������
                if (_tempCurrRightWeapon != null)
                {
                    WeaponScript currRightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currRightWeaponScript != null, "������ ���⿡ Script�� �����ϴ�");
                    AnimationClip rightWeaponHandlingAnimationClip = currRightWeaponScript._handlingIdleAnimation_OneHand;

                    if (rightWeaponHandlingAnimationClip != null)
                    {
                        //����־�� �ϴ� �ִϸ��̼��� �ִ�.
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
                //�޼�
                _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);

                if (_tempCurrLeftWeapon != null)
                {
                    WeaponScript currLeftWeaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currLeftWeaponScript != null, "�޼� ���⿡ Script�� �����ϴ�");

                    AnimationClip leftWeaponHandlingAnimationClip = currLeftWeaponScript._handlingIdleAnimation_TwoHand;
                    Debug.Assert(leftWeaponHandlingAnimationClip != null, "������� �ִϸ��̼��� �ݵ�� �־���Ѵ�");

                    _overrideController[MyUtil._motionChangingAnimationNames[4]] = leftWeaponHandlingAnimationClip;
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                }
                break;

            case WeaponGrabFocus.RightHandFocused:
                //������
                _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);

                if (_tempCurrRightWeapon != null)
                {
                    WeaponScript currRightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
                    Debug.Assert(currRightWeaponScript != null, "������ ���⿡ Script�� �����ϴ�");

                    AnimationClip rightWeaponHandlingAnimationClip = currRightWeaponScript._handlingIdleAnimation_TwoHand;
                    Debug.Assert(rightWeaponHandlingAnimationClip != null, "������� �ִϸ��̼��� �ݵ�� �־���Ѵ�");


                    _overrideController[MyUtil._motionChangingAnimationNames[4]] = rightWeaponHandlingAnimationClip;
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.FullBody, 1.0f);
                }
                break;

            case WeaponGrabFocus.DualGrab:
                Debug.Assert(false, "���� ��չ���� ����");
                break;
        }
    }












    private void _WeaponAnimationLayerChange()
    {
        if (_tempCurrLeftWeapon == null && _tempCurrRightWeapon == null)
        {
            //���⸦ �ƹ��͵� ������� �ʴ�
            Debug.Log("�޼�, ������ ���Ⱑ '��'Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
        }

        else if (_tempCurrLeftWeapon != null && _tempCurrRightWeapon != null)
        {
            //�޼�, ������ ���̾ Ȱ��ȭ �ؼ� Handling�� �����Ѵ�.
            Debug.Log("�޼�, ������ ���Ⱑ Ȱ��ȭ�� �ƴ�");
            _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
            _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
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
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 0.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 1.0f);
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
                    _animator.SetLayerWeight((int)AnimatorLayer.LeftHand, 1.0f);
                    _animator.SetLayerWeight((int)AnimatorLayer.RightHand, 0.0f);
                }
            }
        }
    }
}
