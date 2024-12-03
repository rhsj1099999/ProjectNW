using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AnimatorBlendingDesc;
using static BodyPartBlendingWork;
using static CharacterScript;







/*----------------------------------------------------
|TODO| �� ������Ʈ�� �� ������ ���� ������ �� �����ϴٰ�
���� ���� ������ �޴´ٸ�? ������ ���� �Ѳ����� ó���ϴ� 
������ �ʿ��ϴ�.
----------------------------------------------------*/

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
        OwnerChangeGrabFocusType,
        End,
    }

    public BodyPartBlendingWork(BodyPartWorkType type)
    {
        _workType = type;
    }

    public WeaponGrabFocus _targetGrabFocusType = WeaponGrabFocus.Normal;
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
        _myPart = type;
        _ownerAnimator = ownerAnimator;
    }

    public AnimatorLayerTypes _myPart = AnimatorLayerTypes.FullBody;

    public Animator _ownerAnimator = null;

    public float _blendTarget = 0.0f;
    public float _blendTarget_Sub = 0.0f;
    public float _transitionSpeed = 7.0f;

    public bool _isUsingFirstLayer = false;
}




public class CharacterAnimatorScript : MonoBehaviour
{
    [SerializeField] protected AnimationClip _tempWeaponHandling_NoPlay = null;
    //���� ����ִ� �ִϸ��̼� ���°� ����ؼ� ��µ� �׳� �� ä���༭ �ʿ���� �����Դϴ�.



    [SerializeField] CharacterScript _owner = null;

    protected Animator _animator = null;

    protected AnimatorOverrideController _overrideController = null;

    protected AnimationClip _currAnimClip = null;

    

    [SerializeField] protected int _currentBusyAnimatorLayer_BitShift = 0;
    public int GetBusyLayer() { return _currentBusyAnimatorLayer_BitShift; }

    protected List<bool> _currentBusyAnimatorLayer = new List<bool>();


    protected List<bool> _bodyCoroutineStarted = new List<bool>();
    protected List<Coroutine> _currentBodyCoroutine = new List<Coroutine>();
    protected List<Action_LayerType> _bodyPartDelegates = new List<Action_LayerType>();


    [SerializeField] protected List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();
    protected List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    protected List<LinkedList<BodyPartBlendingWork>> _bodyPartWorks = new List<LinkedList<BodyPartBlendingWork>>();




    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "Animator�� ����");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

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


        if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody] == null)
        {
            Debug.Assert(false, "FullBody�� �ݵ�� ����ؾ� �Ѵ�");
            Debug.Break();
            _partBlendingDesc[(int)AnimatorLayerTypes.FullBody] = new AnimatorBlendingDesc(AnimatorLayerTypes.FullBody, _animator);
        }
    }


    #region Coroutine StartFunc

    protected void StartNextCoroutine(AnimatorLayerTypes layerType)
    {
        LinkedList<BodyPartBlendingWork> target = _bodyPartWorks[(int)layerType];

        target.RemoveFirst();

        if (target.Count <= 0)
        {
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

    #endregion Start CoroutineFunc



    public void StateChanged(AnimationClip nextAnimation)
    {
        /*-------------------------------------------------------------------
        |NOTI| FullBody�� Ư���մϴ�. ���� ���ϰ��ִٸ� ����ϰ� �����ؾ� �մϴ�.
        -------------------------------------------------------------------*/

        AnimatorLayerTypes fullBodyLayer = AnimatorLayerTypes.FullBody;
        int fullBodyLayerIndex = (int)fullBodyLayer;

        if (_currentBodyCoroutine[(int)AnimatorLayerTypes.FullBody] != null)
        {
            StopCoroutine(_currentBodyCoroutine[(int)AnimatorLayerTypes.FullBody]);
            _bodyPartWorks[fullBodyLayerIndex].Clear();
        }

        _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        {
            _bodyPartWorks[fullBodyLayerIndex].Last.Value._animationClip = nextAnimation;
        }
        _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        StartProceduralWork(fullBodyLayer, _bodyPartWorks[fullBodyLayerIndex].First.Value);
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












    public void WeaponLayerChange_EnterAttack(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon)
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

        bool isMirrored = (isUsingRightWeapon == false);

        switch (grapFocusType)
        {
            case WeaponGrabFocus.Normal:
                {
                    //���� �ִϸ��̼��̴�
                    if (isAttackState == true)
                    {
                        int usingHandLayer = (isUsingRightWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (isUsingRightWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                        WeaponScript oppositeWeaponScript = _owner.GetWeaponScript(!isUsingRightWeapon);

                        if (oppositeWeaponScript == null) //�޼չ��⸦ ������� �ʴٸ�
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
                        }
                        else
                        {
                            if (oppositeWeaponScript._handlingIdleAnimation_OneHand == null)
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

                        WeaponScript leftHandWeaponScript = _owner.GetWeaponScript(false);
                        WeaponScript rightHandWeaponScript = _owner.GetWeaponScript(true);

                        //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
                        float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
                        float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
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








    public void WeaponLayerChange_ExitAttack(StateAsset currState, WeaponGrabFocus ownerWeaponGrabFocusType, bool isUsingRightWeapon)
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

        bool isMirrored = (isUsingRightWeapon == false);

        switch (ownerWeaponGrabFocusType)
        {
            case WeaponGrabFocus.Normal:
                {
                    //���� �ִϸ��̼��̴�
                    if (isAttackState == true)
                    {
                        int usingHandLayer = (isUsingRightWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (isUsingRightWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //�޼��� �ݵ�� ���󰡾��ؼ� 0.0f

                        WeaponScript oppositeWeaponScript = _owner.GetWeaponScript(!isUsingRightWeapon);

                        if (oppositeWeaponScript == null) //�޼չ��⸦ ������� �ʴٸ�
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //�޼� ���⸦ ������� �ʴٸ� ����� ���󰡾��ؼ� ���̾ ��������.
                        }
                        else
                        {
                            if (oppositeWeaponScript.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
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
                        WeaponScript leftHandWeaponScript = _owner.GetWeaponScript(false);
                        WeaponScript rightHandWeaponScript = _owner.GetWeaponScript(true);

                        //�޼� ���⸦ ������� �ʰų� �޼� ������ ���� �ִϸ��̼��� ����
                        float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //������ ���⸦ ������� �ʰų� ������ ������ ���� �ִϸ��̼��� ����
                        float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
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
    #region Coroutines







    public void CalculateBodyWorkType_ChangeWeapon(WeaponGrabFocus ownerWeaponGrabFocusType, bool isRightWeaponTrigger, int layerLockResult)
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
            if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused || ownerWeaponGrabFocusType == WeaponGrabFocus.LeftHandFocused) //���⸦ ������� ����־���.
            {
                WeaponScript currentOppositeWeaponScript = _owner.GetWeaponScript(!isRightWeaponTrigger);

                if (currentOppositeWeaponScript != null) //�ݴ�տ� ���Ⱑ �ִ�.
                {
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                    {
                        _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = currentOppositeWeaponScript.GetOneHandHandlingAnimation(oppositePartType);
                    }

                    //���̾� ����Ʈ �ٲٱ�
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                }
                else
                {
                    if (currentOppositeWeaponScript != null)
                    {
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
                            _bodyPartWorks[(int)oppositePartType].Last.Value._attachObjectPrefab = _owner.GetCurrentWeaponPrefab(oppositePartType);
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
            WeaponScript targetWeaponScript = _owner.GetWeaponScript(targetPartType);

            if (targetWeaponScript != null)
            {
                //���� ����ֱ� �ִϸ��̼����� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = targetWeaponScript.GetPutawayAnimation(targetPartType);
                }

                //LayerWeight �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //�� ����Ҷ����� ����ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
                //��������ϱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
            }


            GameObject nextWeaponPrefab = _owner.GetNextWeaponPrefab(targetPartType);

            if (nextWeaponPrefab != null)
            {
                //�ٲٷ��� ���� ���Ⱑ �ִ�

                //���Ⲩ���� ZeroFrame ���� �ٲٱ�
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                {
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
        ownerWeaponGrabFocusType = WeaponGrabFocus.Normal;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }

    }

    public void CalculateBodyWorkType_ChangeFocus(bool isRightHandTrigger, int layerLockResult)
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

        GameObject targetWeapon = _owner.GetCurrentWeaponPrefab(targetPartType);

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        GameObject oppositeWeapon = _owner.GetCurrentWeaponPrefab(oppositePartType);

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


        //_tempGrabFocusType = (isRightHandTrigger == true)
        //    ? WeaponGrabFocus.RightHandFocused
        //    : WeaponGrabFocus.LeftHandFocused;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_ChangeFocus_ReleaseMode(bool isRightHandTrigger, int layerLockResult)
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


        GameObject oppositeWeaponPrefab = _owner.GetCurrentWeaponPrefab(oppositePartType);

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

        GameObject targetWeapon = _owner.GetCurrentWeaponPrefab(targetPartType);

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
        //_tempGrabFocusType = WeaponGrabFocus.Normal;

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_UseItem_Drink(WeaponGrabFocus ownerWeaponGrabFocusType, ItemInfo usingItemInfo, int layerLockResult)
    {
        //HashSet<int> lockCompares = new HashSet<int>();

        //int rightHandIndex = (int)AnimatorLayerTypes.RightHand;

        //Transform rightHandWeaponOriginalTransform = null;

        ////���⸦ �����տ� ������� ����ֽ��ϱ�?
        //if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //{
        //    WeaponScript weaponScript = _owner.GetWeaponScript(AnimatorLayerTypes.RightHand);

        //    //�����տ� ������� ��� �־��ٸ� ��� �޼տ� ����ش�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //    {
        //        WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;
        //        //���� ����ִ� Ʈ������ ĳ��
        //        rightHandWeaponOriginalTransform = weaponScript._socketTranform;

        //        //�ݴ�� ���� ã��
        //        {
        //            Debug.Assert(_characterModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

        //            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

        //            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");

        //            ItemInfo.WeaponType targetType = weaponScript._weaponType;

        //            foreach (var socketComponent in weaponSockets)
        //            {
        //                if (socketComponent._sideType != targetSide)
        //                {
        //                    continue;
        //                }

        //                foreach (var type in socketComponent._equippableWeaponTypes)
        //                {
        //                    if (type == targetType)
        //                    {
        //                        _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = socketComponent.gameObject.transform;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //else if (_tempCurrRightWeapon != null) //������� ������� �ʾҽ��ϴ�. �ٵ� �����տ� ���⸦ ��� �߽��ϴ�.
        //{
        //    WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();

        //    //�ݴ���� ���� ����ֱ� �ִϸ��̼����� �ٲ۴�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //    {
        //        _bodyPartWorks[rightHandIndex].Last.Value._animationClip = weaponScript.GetPutawayAnimation(true);
        //    }

        //    //Layer Weight�� ���� �ø���.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    //�ݴ���� ���� ����ֱ� �ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        //    //�ݴ���� ���⸦ �����Ѵ�.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
        //}

        //CoroutineLock waitForRightHandReady = new CoroutineLock();
        //_bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        //{
        //    _bodyPartWorks[rightHandIndex].Last.Value._coroutineLock = waitForRightHandReady;
        //}
        //lockCompares.Add(rightHandIndex);

        ////�������� ����ϴ� �����鿡�� �ϰ��� �����Ѵ�.
        //{
        //    List<AnimatorLayerTypes> mustUsingLayers = usingItemInfo._usingItemMustNotBusyLayers;

        //    foreach (AnimatorLayerTypes type in mustUsingLayers)
        //    {
        //        int typeIndex = (int)type;
        //        if (lockCompares.Contains(typeIndex) == false)
        //        {
        //            _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
        //            {
        //                _bodyPartWorks[typeIndex].Last.Value._coroutineLock = waitForRightHandReady;
        //            }
        //        }


        //        //�������� ����ϴ� �ִϸ��̼����� �ٲ۴�.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[typeIndex].Last.Value._animationClip = usingItemInfo._usingItemAnimation;
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //�ִϸ��̼��� �� ����ɶ����� Ȧ���Ѵ�.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));

        //        //Layer Weight�� ��� ��������.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.TurnOffAllLayer));

        //        lockCompares.Add(typeIndex);
        //    }

        //}

        //GameObject rightWeaponPrefab = _tempRightWeaponPrefabs[_currRightWeaponIndex];

        //if (rightWeaponPrefab != null) //�����տ� ���� �վ����ϴ�.
        //{
        //    WeaponScript rightWeaponScript = rightWeaponPrefab.GetComponent<WeaponScript>();

        //    if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //    {
        //        //������� ��� �ִϸ��̼����� �ٲ۴�.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetTwoHandHandlingAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //�Ʊ� �ݴ�տ� ������ ���⸦ �ٽ� �ݴ�� ���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = rightHandWeaponOriginalTransform;
        //        }
        //    }
        //    else
        //    {
        //        //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //���⸦ ����ش�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._attachObjectPrefab = rightWeaponPrefab;
        //        }

        //        //���⸦ ������ Zero Frame Animation ���� �ٲ۴�
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight�� ���� �ø���.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    }

        //    lockCompares.Add(rightHandIndex);
        //}
        //else
        //{
        //    //�׳� ��������
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));

        //    lockCompares.Add(rightHandIndex);
        //}

        //int usingLockResult = 0;
        //foreach (int index in lockCompares)
        //{
        //    usingLockResult = usingLockResult | (1 << index);
        //}

        //if (usingLockResult != layerLockResult)
        //{
        //    Debug.Assert(false, "����Ϸ��� ������ Lock�� ��ġ���� �ʽ��ϴ�. �ִϸ��̼� ������ �����ϼ���");
        //    Debug.Break();
        //    return;
        //}

        //UpdateBusyAnimatorLayers(usingLockResult, true);

        //foreach (int index in lockCompares)
        //{
        //    StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        //}
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
        _owner.WeaponSwitchHand(layerType, work);
        return null;
    }

    protected IEnumerator DestroyWeaponCoroutine(AnimatorLayerTypes layerType)
    {
        _owner.DestroyWeapon(layerType);
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

    protected IEnumerator CreateWeaponModelAndEquipCoroutine(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        CreateWeaponModelAndEquip(layerType, nextWeaponPrefab);
        return null;
    }



    protected IEnumerator OwnerChangeWeaponGrabFocusType(WeaponGrabFocus targetType)
    {
        _owner.ChangeGrabFocusType(targetType);
        return null;
    }


    #endregion Coroutines





    protected void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        _owner.CreateWeaponModelAndEquip(layerType, nextWeaponPrefab);
    }

    #endregion Coroutines





    public IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveNextLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        if (targetBlendingDesc == null)
        {
            Debug.Assert(false, "������� �ʴ� ��Ʈ�� ���� �Լ��� ȣ���߽��ϴ�");
            Debug.Break();
            yield break;
        }

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._transitionSpeed * Time.deltaTime
                : targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

            subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float target = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._blendTarget
                : targetBlendingDesc._blendTarget_Sub;

            if (target >= 1.0f || target <= 0.0f)
            {
                break;
            }

            yield return null;
        }
    }

    public IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._transitionSpeed * Time.deltaTime
                : targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

            subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex,targetBlendingDesc. _blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float target = (targetBlendingDesc._isUsingFirstLayer == true)
                ? targetBlendingDesc._blendTarget
                : targetBlendingDesc._blendTarget_Sub;

            if (target >= 1.0f)
            {
                break;
            }

            yield return null;
        }
    }

    public IEnumerator ChangeNextLayerWeightSubCoroutine_DeActiveAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;
        
        while (true)
        {
            float mainLayerBlendDelta = 0.0f;
            float subLayerBlendDelta = 0.0f;

            mainLayerBlendDelta = targetBlendingDesc._transitionSpeed * Time.deltaTime * -1.0f;
            subLayerBlendDelta = mainLayerBlendDelta;

            targetBlendingDesc._blendTarget += mainLayerBlendDelta;
            targetBlendingDesc._blendTarget_Sub += subLayerBlendDelta;

            targetBlendingDesc._blendTarget = Mathf.Clamp(targetBlendingDesc._blendTarget, 0.0f, 1.0f);
            targetBlendingDesc._blendTarget_Sub = Mathf.Clamp(targetBlendingDesc._blendTarget_Sub, 0.0f, 1.0f);

            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
            targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

            float maxTarget = Mathf.Max(targetBlendingDesc._blendTarget, targetBlendingDesc._blendTarget_Sub);

            if (maxTarget <= 0.0f)
            {
                break;
            }

            yield return null;
        }
    }


    public IEnumerator ChangeNextLayerWeightSubCoroutine_TurnOffAllLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        int targetHandMainLayerIndex = (int)targetBlendingDesc._myPart * 2;
        int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

        targetBlendingDesc._blendTarget = 0.0f;
        targetBlendingDesc._blendTarget_Sub = 0.0f;

        targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, targetBlendingDesc._blendTarget);
        targetBlendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, targetBlendingDesc._blendTarget_Sub);

        return null;
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
                        ChangeNextLayerWeightSubCoroutine_ActiveNextLayer(layerType),
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
                        ChangeNextLayerWeightSubCoroutine_ActiveAllLayer(layerType),
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
                        ChangeNextLayerWeightSubCoroutine_DeActiveAllLayer(layerType),
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
                        ChangeNextLayerWeightSubCoroutine_TurnOffAllLayer(layerType),
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
                        CreateWeaponModelAndEquipCoroutine(layerType, work._attachObjectPrefab),
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

            case BodyPartWorkType.OwnerChangeGrabFocusType:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        OwnerChangeWeaponGrabFocusType(work._targetGrabFocusType),
                        StartNextCoroutine,
                        layerType
                    );
                }
                break;

            case BodyPartWorkType.End:
                {
                    startedCoroutine = null;
                }
                break;

            default:
                Debug.Assert(false, "���̽��� �߰��ƽ��ϱ�?");
                Debug.Break();
                break;

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
}
