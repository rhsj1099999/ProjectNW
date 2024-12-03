using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimatorBlendingDesc;
using static BodyPartBlendingWork;
using static StateGraphAsset;

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


public enum AdditionalBehaveType
{
    ChangeWeapon,
    ChangeFocus,
    UseItem_Drink,
    UseItem_Break,
    UseItem_Throw,
}

public enum WeaponGrabFocus
{
    Normal,
    RightHandFocused,
    LeftHandFocused,
    DualGrab,
}

public class CoroutineLock
{
    public bool _isEnd = false;
}

public class DamageDesc
{
    public enum DamageType
    {
        Damage_Lvl_0 = 0,
        Damage_Lvl_1,
        Damage_Lvl_2,
        Damage_Lvl_3,
    }
    public int _damage = 0;
    public int _damagePower = 1;
    public int _dagingStamina = 0;
    public DamageType _damageType = DamageType.Damage_Lvl_0;
}



public class BodyPartBlendingWork
{
    public enum BodyPartWorkType
    {
        ActiveNextLayer,
        ActiveAllLayer,
        DeActiveAllLayer,
        TurnOffAllLayer,
        ChangeAnimation,
        ChangeAnimation_ZeroFrame,
        AnimationPlayHold,
        AttatchObjet,
        DestroyWeapon,
        WaitCoroutineUnlock,
        UnLockCoroutine,
        SwitchHand,
        WorkComplete,
        End,
    }

    public BodyPartBlendingWork(BodyPartWorkType type)
    {
        _workType = type;
    }

    public BodyPartWorkType _workType = BodyPartWorkType.End;
    public AnimationClip _animationClip = null;
    public GameObject _attachObjectPrefab = null;
    public GameObject _deleteObjectTarget = null;
    public CoroutineLock _coroutineLock = null;
    public Transform _weaponEquipTransform = null;
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
        TurnOffAllLayer,
    }

    public IEnumerator ChangeNextLayerWeightSubCoroutine(float transitionSpeed, BlendingCoroutineType workType)
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

                case BlendingCoroutineType.TurnOffAllLayer:
                    {
                        _blendTarget = 0.0f;
                        _blendTarget_Sub = 0.0f;

                        _ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, _blendTarget);
                        _ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, _blendTarget_Sub);

                        signal = true;
                    }
                    break;
            }

            if (signal == true)
            {
                break;
            }

            yield return null;
        }
    }
}



public class CharacterScript : MonoBehaviour, IHitable
{
    //��������Ʈ Ÿ�Ե�
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);


    //Buff ���� ������Ʈ��
    protected Dictionary<BuffTypes, BuffScript> _currBuffs = new Dictionary<BuffTypes, BuffScript>();
    protected StatScript _myStat = new StatScript();


    //��ǥ ������Ʈ
    [SerializeField] protected CharacterMoveScript2 _characterMoveScript2 = null;
    [SerializeField] protected StateContoller _stateContoller = null;
    //����� ĳ���͸޽��� �ִϸ����͸� �����ֱ� ������ �ִϸ����͸� �����ִ� ���ӿ�����Ʈ�� ĳ���� �޽���
    protected GameObject _characterModelObject = null; //�ִϸ����ʹ� �갡 �����ִ�
    public Animator GetAnimator() { return _animator; }








    //Weapon Section -> �̰� �ٸ� ������Ʈ�� ������(���� ��������ִ°� EquipmentBoard Ȥ�� Inventory)
    [SerializeField] protected List<GameObject> _tempLeftWeaponPrefabs = new List<GameObject>();
    [SerializeField] protected List<GameObject> _tempRightWeaponPrefabs = new List<GameObject>();

    protected KeyCode _changeRightHandWeaponHandlingKey = KeyCode.B;
    protected KeyCode _changeLeftHandWeaponHandlingKey = KeyCode.V;
    protected KeyCode _useItemKeyCode1 = KeyCode.N;
    protected KeyCode _useItemKeyCode2 = KeyCode.M;
    protected KeyCode _useItemKeyCode3 = KeyCode.Comma;
    protected KeyCode _useItemKeyCode4 = KeyCode.Period;
    protected int _currLeftWeaponIndex = 0;
    protected int _currRightWeaponIndex = 0;
    protected int _tempMaxWeaponSlot = 3;
    protected GameObject _tempCurrLeftWeapon = null;
    protected GameObject _tempCurrRightWeapon = null;
    public GameObject GetLeftWeapon() { return _tempCurrLeftWeapon; }
    public GameObject GetRightWeapon() { return _tempCurrRightWeapon; }
    public GameObject GetCurrentWeapon(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _tempCurrRightWeapon;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _tempCurrLeftWeapon;
        }
        else
        {
            return null;
        }
    }
    public GameObject GetLeftWeaponPrefab() { return _tempLeftWeaponPrefabs[_currLeftWeaponIndex]; }
    public GameObject GetRightWeaponPrefab() { return _tempRightWeaponPrefabs[_currRightWeaponIndex]; }
    public GameObject GetCurrentWeaponPrefab(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _tempRightWeaponPrefabs[_currRightWeaponIndex];
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
        }
        else
        {
            return null;
        }
    }
    protected WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }
    protected bool _tempUsingRightHandWeapon = false; //�ֱٿ� ����� ���Ⱑ �������Դϱ�?
    public bool GetLatestWeaponUse() { return _tempUsingRightHandWeapon; }
    public void SetLatestWeaponUse(bool isRightHandWeapon)
    {
        _tempUsingRightHandWeapon = isRightHandWeapon;
    }



    //Aim System
    protected AimScript2 _aimScript = null;
    protected bool _isAim = false;
    protected bool _isTargeting = false;
    public bool GetIsTargeting() { return _isTargeting; }


    //Animator Secton -> �̰� �ٸ� ������Ʈ�� ������
    protected Animator _animator = null;
    protected AnimatorOverrideController _overrideController = null;



    //Animation_TotalBlendSection
    protected bool _corutineStarted = false;
    protected float _blendTarget = 0.0f;
    [SerializeField] protected float _transitionSpeed = 5.0f;

    //Animation_WeaponBlendSection
    protected float _blendTarget_Weapon = 0.0f;
    [SerializeField] protected float _transitionSpeed_Weapon = 20.0f;



    protected AnimationClip _currAnimClip = null;
    protected List<bool> _currentBusyAnimatorLayer = new List<bool>();
    [SerializeField] protected int _currentBusyAnimatorLayer_BitShift = 0;
    protected int _currentLayerIndex = 0;
    protected int _maxLayer = 2;

    protected List<bool> _bodyCoroutineStarted = new List<bool>();
    protected List<Coroutine> _currentBodyCoroutine = new List<Coroutine>();
    protected List<Action_LayerType> _bodyPartDelegates = new List<Action_LayerType>();
    protected List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    [SerializeField] protected List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();
    protected List<LinkedList<BodyPartBlendingWork>> _bodyPartWorks = new List<LinkedList<BodyPartBlendingWork>>();
    [SerializeField] protected AnimationClip _tempWeaponHandling_NoPlay = null;

    protected virtual void Awake()
    {
        _characterMoveScript2 = GetComponent<CharacterMoveScript2>();
        Debug.Assert(_characterMoveScript2 != null, "CharacterMove ������Ʈ ����");

        _stateContoller = GetComponent<StateContoller>();
        Debug.Assert(_stateContoller != null, "StateController�� ����");


        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "Animator�� ����");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
        _characterModelObject = _animator.gameObject;


        for (int i = 0; i < (int)AnimatorLayerTypes.End; i++)
        {
            _partBlendingDesc.Add(null);
            _currentBusyAnimatorLayer.Add(false);
            _bodyPartWorks.Add(new LinkedList<BodyPartBlendingWork>());
            _bodyCoroutineStarted.Add(false);
            _currentBodyCoroutine.Add(null);
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

    protected virtual void Update()
    {
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


    protected void StartNextCoroutine(AnimatorLayerTypes layerType)
    {
        LinkedList<BodyPartBlendingWork> target = _bodyPartWorks[(int)layerType];

        target.RemoveFirst();

        if (target.Count <= 0)
        {
            //���� �� ������.
            UpdateBusyAnimatorLayers(1 << (int)layerType, false);
            return;
        }

        StartProceduralWork(layerType, target.First.Value);
    }

    protected Coroutine StartCoroutine_CallBackAction(IEnumerator coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        return StartCoroutine(CoroutineWrapper(coroutine, callBack, layerType));
    }

    protected Coroutine StartCoroutine_CallBackAction(System.Func<IEnumerator> coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        return StartCoroutine(CoroutineWrapper(coroutine, callBack, layerType));
    }

    protected IEnumerator CoroutineWrapper(IEnumerator coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        yield return coroutine;

        callBack.Invoke(layerType);
    }

    protected IEnumerator CoroutineWrapper(System.Func<IEnumerator> coroutine, Action_LayerType callBack, AnimatorLayerTypes layerType)
    {
        yield return coroutine;

        callBack.Invoke(layerType);
    }




    protected Coroutine StartProceduralWork(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        Coroutine startedCoroutine = null;

        switch (work._workType)
        {
            case BodyPartWorkType.ActiveNextLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        targetBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, BlendingCoroutineType.ActiveNextLayer),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ActiveAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        targetBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, BlendingCoroutineType.ActiveAllLayer),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.DeActiveAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        targetBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, BlendingCoroutineType.DeactiveAllLayer),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.TurnOffAllLayer:
                {
                    AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        targetBlendingDesc.ChangeNextLayerWeightSubCoroutine(_transitionSpeed_Weapon, BlendingCoroutineType.TurnOffAllLayer),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ChangeAnimation:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WeaponChange_AnimationCoroutine(work._animationClip, layerType, false),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.ChangeAnimation_ZeroFrame:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WeaponChange_AnimationCoroutine(work._animationClip, layerType, true),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.AnimationPlayHold:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        AnimationPlayHoldCoroutine(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.AttatchObjet:
                {
                    bool isRightHand = (layerType == AnimatorLayerTypes.RightHand);
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        CreateWeaponModelAndEquipCoroutine(isRightHand, work._attachObjectPrefab),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.DestroyWeapon:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        DestroyWeaponCoroutine(layerType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.WaitCoroutineUnlock:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        WaitCoroutineLock(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.UnLockCoroutine:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        UnlockCoroutineLock(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.SwitchHand:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        SwitchHandCoroutine(layerType, work),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.End:
                {
                    startedCoroutine = null;
                }
                break; ;
        }

        if (startedCoroutine == null)
        {
            Debug.Assert(false, "�ڷ�ƾ ���ۿ� �����߽��ϴ�.");
            Debug.Break();
            return null;
        }

        int layerTypeIndex = (int)layerType;

        _bodyCoroutineStarted[layerTypeIndex] = true;
        _currentBodyCoroutine[layerTypeIndex] = startedCoroutine;

        return startedCoroutine;
    }


    //public void StateChanged()
    //{
    //    ChangeAnimation(_stateContoller.GetCurrState());
    //}


    protected void ReadyAimSystem()
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


    public void ChangeAnimation(StateAsset nextState)
    {
        /*----------------------------------------------------
        |NOTI| ��� �ִϸ��̼��� RightHand �������� ��ȭ�ƽ��ϴ�.
        ------------------------------------------------------*/

        AnimationClip targetClip = nextState._myState._stateAnimationClip;

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
    }



    protected void UpdateBusyAnimatorLayers(int willBusyLayers_BitShift, bool isOn)
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





    public void WeaponLayerChange_EnterAttack(StateAsset currState)
    {
        bool isAttackState = true;

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
                    if (isAttackState == true)
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
                        float leftHandLayerWeight = (_tempCurrLeftWeapon == null || _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //
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
                    float layerWeight = (isAttackState == true)
                        ? 0.0f
                        : 1.0f;

                    _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
                    _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

                    if (currState._myState._isAttackState == true)
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





    public void WeaponLayerChange_ExitAttack(StateAsset currState)
    {
        bool isAttackState = false;

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
                    if (isAttackState == true)
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
                        float leftHandLayerWeight = (_tempCurrLeftWeapon == null || _tempCurrLeftWeapon.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null) //
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
                    float layerWeight = (isAttackState == true)
                        ? 0.0f
                        : 1.0f;

                    _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
                    _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

                    if (currState._myState._isAttackState == true)
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





    protected IEnumerator SwitchingBlendCoroutine()
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







    public void CheckBehave(AdditionalBehaveType additionalBehaveType)
    {
        switch (additionalBehaveType)
        {
            case AdditionalBehaveType.ChangeWeapon:
                {
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
                        //���� �ش��ϴ� �������� �ʹ� �ٻڴ�
                        return;
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    CalculateBodyWorkType_ChangeWeapon(tempIsRightHandWeapon, willUsingAnimatorLayer);
                }
                break;

            case AdditionalBehaveType.ChangeFocus:
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

                    int willUsingAnimatorLayer = 0;
                    //����� ���̾� ���
                    {
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                    }

                    if ((_currentBusyAnimatorLayer_BitShift & willUsingAnimatorLayer) != 0)
                    {
                        //���� �ش��ϴ� �������� �ʹ� �ٻڴ�
                        return;
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    if (isRelease == true) //����� �����Ϸ��� ����Դϴ�
                    {
                        CalculateBodyWorkType_ChangeFocus_ReleaseMode(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                    else
                    {
                        CalculateBodyWorkType_ChangeFocus(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                }
                break;

            case AdditionalBehaveType.UseItem_Drink:
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

                    if (_stateContoller.GetCurrState()._myState._canUseItem == false)
                    {
                        return;
                    }


                    //��� ���� üũ
                    int willBusyLayer = 0;

                    {
                        //���� �����۸����� �ʿ��� ���̾� üũ
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

                        //���� ���� �������� ���� �ʿ��� ���̾� üũ
                        if (_tempCurrRightWeapon != null)
                        {
                            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        }
                    }



                    if ((_currentBusyAnimatorLayer_BitShift & willBusyLayer) != 0)
                    {
                        return; //�ش� �������� ���� ������ �ִ�
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    CalculateBodyWorkType_UseItem_Drink(newTestingItem, willBusyLayer);
                }
                break;

            case AdditionalBehaveType.UseItem_Break:
                {
                    Debug.Assert(false, "�̱����Դϴ�");
                    Debug.Break();
                    //Work�� ��´� ������ Lock ����� ������.
                    //CalculateBodyWorkType_UseItem_Break();
                }
                break;

            default:
                break;
        }
    }

    protected void CalculateBodyWorkType_ChangeWeapon(bool isRightWeaponTrigger, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightWeaponTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositePartIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightWeaponTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetPartIndex = (int)targetPartType;

        //�ݴ�� Work ����
        {
            if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused || _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused) //���⸦ ������� ����־���.
            {
                GameObject currentOppositeWeapon = (isRightWeaponTrigger == true)
                    ? _tempCurrLeftWeapon
                    : _tempCurrRightWeapon;

                if (currentOppositeWeapon != null) //�ݴ�տ� ���Ⱑ �ִ�.
                {
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                    {
                        WeaponScript oppositeWeaponScript = currentOppositeWeapon.GetComponent<WeaponScript>();
                        _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = oppositeWeaponScript.GetOneHandHandlingAnimation(currentOppositeWeapon == _tempCurrRightWeapon);
                    }

                    //���̾� ����Ʈ �ٲٱ�
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                }
                else
                {
                    GameObject currentOppositeWeaponPrefab = GetCurrentWeaponPrefab(oppositePartType);

                    if (currentOppositeWeaponPrefab != null)
                    {
                        WeaponScript currentOppositeWeaponScript = currentOppositeWeaponPrefab.GetComponent<WeaponScript>();

                        //���⸦ ������ Zero Frame�ִϸ��̼����� �ٲ۴�
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                        {
                            _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = currentOppositeWeaponScript.GetDrawAnimation(oppositePartType);
                        }

                        //LayerWeight�� �����Ѵ�
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                        //���⸦ ����ش�
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
                        {
                            _bodyPartWorks[(int)oppositePartType].Last.Value._attachObjectPrefab = currentOppositeWeaponPrefab;
                        }

                        //���⸦ ������ �ִϸ��̼����� �ٲ۴�
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                        {
                            _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = currentOppositeWeaponScript.GetDrawAnimation(oppositePartType);
                        }
                        //LayerWeight�� �����Ѵ�
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                        //�� ����Ҷ����� ����Ѵ�.
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
                    }
                    else
                    {
                        //�ݴ�տ� ������� �����鼭 ������� ���Ⱑ ����.
                        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
                    }
                }

                lockCompares.Add(oppositePartIndex);
            }
        }

        //Target Work ����
        {
            GameObject currentWeapon = (isRightWeaponTrigger == true)
                ? _tempCurrRightWeapon
                : _tempCurrLeftWeapon;

            if (currentWeapon != null)
            {
                //���� ����ֱ� �ִϸ��̼����� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    WeaponScript targetWeaponScript = currentWeapon.GetComponent<WeaponScript>();
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = targetWeaponScript.GetPutawayAnimation(targetPartType);
                }

                //LayerWeight �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //�� ����Ҷ����� ����ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
                //��������ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
            }


            GameObject nextWeaponPrefab = (isRightWeaponTrigger == true)
                ? _tempRightWeaponPrefabs[_currRightWeaponIndex]
                : _tempLeftWeaponPrefabs[_currLeftWeaponIndex];

            if (nextWeaponPrefab != null)
            {
                //�ٲٷ��� ���� ���Ⱑ �ִ�

                //���Ⲩ���� ZeroFrame ���� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                {
                    WeaponScript targetWeaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = targetWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //��������ֱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._attachObjectPrefab = nextWeaponPrefab;
                }

                //���Ⲩ���� �ִϸ��̼����� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    WeaponScript targetWeaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = targetWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //�� ����Ҷ����� ��ٸ���
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            }
            else
            {
                //�ٲٷ��� ���� ���Ⱑ ����

                //���̾� ��������
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
            }

            lockCompares.Add(targetPartIndex);
        }

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);
        _tempGrabFocusType = WeaponGrabFocus.Normal;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }

    }

    protected void CalculateBodyWorkType_ChangeFocus(bool isRightHandTrigger, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositeBodyIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetBodyIndex = (int)targetPartType;

        GameObject targetWeapon = (isRightHandTrigger == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        GameObject oppositeWeapon = (isRightHandTrigger == true)
            ? _tempCurrLeftWeapon
            : _tempCurrRightWeapon;

        //�ݴ� �տ� ���⸦ ����ִ�.
        if (oppositeWeapon != null)
        {
            //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeapon.GetComponent<WeaponScript>();
                _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetPutawayAnimation(oppositePartType);
            }

            //Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            //�ݴ���� ���⸦ �����Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));

            lockCompares.Add(oppositeBodyIndex);
        }

        //�� Behave���� �ܰ踦 �Ѿ�� �ڷ�ƾ ���� �����Ͽ� �˷��ش�.
        CoroutineLock waitOppsiteWeaponPutAwayLock = new CoroutineLock();
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        {
            _bodyPartWorks[oppositeBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
        }

        //�ݴ���� �ִϸ��̼��� �ٲ۴�
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        {
            _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
        }

        //�ݴ���� LayerWeight�� �ø���.
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        lockCompares.Add(oppositeBodyIndex);


        //�ش� ���⸦ ������� ��� �ִϸ��̼��� �����Ѵ�. �ش� ���� �ִϸ��̼��� �ٲ۴�
        {
            //�ݴ���� �̺�Ʈ�� ���������� ���� ����Ѵ�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
            }
            //�ش� ���� �ִϸ��̼��� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[(int)targetBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
            }
            //�ش� ���� Layer Weight�� �ٲ۴�.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            lockCompares.Add(targetBodyIndex);
        }


        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);
        _tempGrabFocusType = (isRightHandTrigger == true)
            ? WeaponGrabFocus.RightHandFocused
            : WeaponGrabFocus.LeftHandFocused;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    protected void CalculateBodyWorkType_ChangeFocus_ReleaseMode(bool isRightHandTrigger, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        AnimatorLayerTypes oppositePartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;
        int oppositeBodyIndex = (int)oppositePartType;

        AnimatorLayerTypes targetPartType = (isRightHandTrigger == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;
        int targetBodyIndex = (int)targetPartType;


        GameObject oppositeWeaponPrefab = (isRightHandTrigger == true)
            ? _tempLeftWeaponPrefabs[_currLeftWeaponIndex]
            : _tempRightWeaponPrefabs[_currRightWeaponIndex];

        //�ݴ� �տ� ������� ��� ��, ���⸦ ����־���.
        if (oppositeWeaponPrefab != null)
        {
            //�ݴ���� ���� ������ Zero Frame �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //�ݴ�� Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //���⸦ ����ش�
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
            {
                _bodyPartWorks[oppositeBodyIndex].Last.Value._attachObjectPrefab = oppositeWeaponPrefab;
            }

            //�ݴ���� ���� ������ �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //�ݴ�� Layer Weight�� ���� �ø���.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            //�ݴ���� ���� ������ �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        }
        else
        {
            //�׳� ��������
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        lockCompares.Add(oppositeBodyIndex);

        GameObject targetWeapon = (isRightHandTrigger == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        AnimationClip oneHandHadlingAnimation = targetWeaponScript.GetOneHandHandlingAnimation(targetPartType);

        if (oneHandHadlingAnimation != null)
        {
            //�ڼ� �ִϸ��̼��� �ֽ��ϴ�.

            //�ش� ���� �ִϸ��̼��� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._animationClip = oneHandHadlingAnimation;
            }
            //�ش� ���� LayerWeight�� �ٲ۴�
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        }
        else
        {
            //�ڼ� �ִϸ��̼��� �����ϴ�.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        lockCompares.Add(targetBodyIndex);

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);
        _tempGrabFocusType = WeaponGrabFocus.Normal;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    protected void CalculateBodyWorkType_UseItem_Drink(ItemInfo usingItemInfo, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        int rightHandIndex = (int)AnimatorLayerTypes.RightHand;

        Transform rightHandWeaponOriginalTransform = null;

        //���⸦ �����տ� ������� ����ֽ��ϱ�?
        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
        {
            WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();

            //�����տ� ������� ��� �־��ٸ� ��� �޼տ� ����ش�.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
            {
                WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;
                //���� ����ִ� Ʈ������ ĳ��
                rightHandWeaponOriginalTransform = weaponScript._socketTranform;

                //�ݴ�� ���� ã��
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
                                _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = socketComponent.gameObject.transform;
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (_tempCurrRightWeapon != null) //������� ������� �ʾҽ��ϴ�. �ٵ� �����տ� ���⸦ ��� �߽��ϴ�.
        {
            WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();

            //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[rightHandIndex].Last.Value._animationClip = weaponScript.GetPutawayAnimation(true);
            }

            //Layer Weight�� ���� �ø���.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            //�ݴ���� ���⸦ �����Ѵ�.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
        }

        CoroutineLock waitForRightHandReady = new CoroutineLock();
        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        {
            _bodyPartWorks[rightHandIndex].Last.Value._coroutineLock = waitForRightHandReady;
        }
        lockCompares.Add(rightHandIndex);

        //�������� ����ϴ� �����鿡�� �ϰ��� �����Ѵ�.
        {
            List<AnimatorLayerTypes> mustUsingLayers = usingItemInfo._usingItemMustNotBusyLayers;

            foreach (AnimatorLayerTypes type in mustUsingLayers)
            {
                int typeIndex = (int)type;
                if (lockCompares.Contains(typeIndex) == false)
                {
                    _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
                    {
                        _bodyPartWorks[typeIndex].Last.Value._coroutineLock = waitForRightHandReady;
                    }
                }


                //�������� ����ϴ� �ִϸ��̼����� �ٲ۴�.
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[typeIndex].Last.Value._animationClip = usingItemInfo._usingItemAnimation;
                }

                //Layer Weight�� ���� �ø���.
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //�ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));

                //Layer Weight�� ��� ��������.
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.TurnOffAllLayer));

                lockCompares.Add(typeIndex);
            }

        }

        GameObject rightWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

        if (rightWeaponPrefab != null) //�����տ� ���� �վ����ϴ�.
        {
            WeaponScript rightWeaponScript = rightWeaponPrefab.GetComponent<WeaponScript>();

            if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
            {
                //������� ��� �ִϸ��̼����� �ٲ۴�.
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetTwoHandHandlingAnimation(true);
                }

                //Layer Weight�� ���� �ø���.
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //�Ʊ� �ݴ�տ� ������ ���⸦ �ٽ� �ݴ�� ���.
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
                {
                    _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = rightHandWeaponOriginalTransform;
                }
            }
            else
            {
                //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                {
                    _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
                }

                //Layer Weight�� ���� �ø���.
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //���⸦ ����ش�
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
                {
                    _bodyPartWorks[rightHandIndex].Last.Value._attachObjectPrefab = rightWeaponPrefab;
                }

                //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
                }

                //Layer Weight�� ���� �ø���.
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            }

            lockCompares.Add(rightHandIndex);
        }
        else
        {
            //�׳� ��������
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));

            lockCompares.Add(rightHandIndex);
        }

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    protected void CalculateBodyWorkType_UseItem_Break()
    {

    }

    public List<bool> GetCurrentBusyAnimatorLayer()
    {
        return _currentBusyAnimatorLayer;
    }

    public int GetCurrentBusyAnimatorLayer_BitShift()
    {
        return _currentBusyAnimatorLayer_BitShift;
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



    #region Coroutines

    protected IEnumerator SwitchHandCoroutine(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        WeaponScript targetWeaponScript = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempCurrRightWeapon.GetComponent<WeaponScript>()
            : _tempCurrLeftWeapon.GetComponent<WeaponScript>();

        targetWeaponScript.Equip_OnSocket(work._weaponEquipTransform);

        return null;
    }

    protected IEnumerator DestroyWeaponCoroutine(AnimatorLayerTypes layerType)
    {
        bool tempIsRightWeapon = (layerType == AnimatorLayerTypes.RightHand);

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

        return null;
    }

    protected IEnumerator WaitCoroutineLock(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        while (work._coroutineLock._isEnd == false)
        {
            yield return null;
        }
    }

    protected IEnumerator UnlockCoroutineLock(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        work._coroutineLock._isEnd = true;
        return null;
    }

    protected IEnumerator AnimationPlayHoldCoroutine(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        AnimatorBlendingDesc blendingDesc = _partBlendingDesc[(int)layerType];

        while (true)
        {
            int layerIndex = (blendingDesc._isUsingFirstLayer == true)
                ? (int)layerType * 2
                : (int)layerType * 2 + 1;

            float normalizedTime = _animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

            if (normalizedTime >= 1.0f)
            {
                break;
            }

            yield return null;
        }
    }

    protected IEnumerator WeaponChange_AnimationCoroutine(AnimationClip nextAnimation, AnimatorLayerTypes targetBody, bool isZeroFrame)
    {
        WeaponChange_Animation(nextAnimation, targetBody, isZeroFrame);

        return null;
    }

    protected IEnumerator CreateWeaponModelAndEquipCoroutine(bool tempIsRightWeapon, GameObject nextWeaponPrefab)
    {
        CreateWeaponModelAndEquip(tempIsRightWeapon, nextWeaponPrefab);
        return null;
    }
    #endregion Coroutines





    protected void CreateWeaponModelAndEquip(bool tempIsRightWeapon, GameObject nextWeaponPrefab)
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

            StateGraphAsset stateGraphAsset = nextWeaponScript._weaponStateGraph;

            StateGraphAsset.StateGraphType stateGraphType = (tempIsRightWeapon == true)
                ? StateGraphAsset.StateGraphType.WeaponState_RightGraph
                : StateGraphAsset.StateGraphType.WeaponState_LeftGraph;

            //������ ��, ���±׷����� ��ü�Ѵ�.
            _stateContoller.EquipStateGraph(stateGraphAsset, stateGraphType);
        }
    }









    public virtual void DealMe(DamageDesc damage, GameObject caller)
    {
    }
}
