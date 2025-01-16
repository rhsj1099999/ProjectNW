using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;
using static BodyPartBlendingWork;
using static CharacterScript;



/*----------------------------------------------------
|TODO| 이 컴포넌트가 각 부위에 대해 뭔가를 쭉 진행하다가
상태 변경 간섭을 받는다면? 예정된 일을 한꺼번에 처리하는 
구조가 필요하다.
----------------------------------------------------*/



/*----------------------------------------------------
|TODO| 

몬스터가 플레이어를 죽이면 nullref 오류 = 몬스터는 AimScript가 없어서

Aim을 했다가 풀고 죽이면 nullref 오류 = 죽였을때 null의 trnasform을 받아와서

Lock On 햇다가 바로 Aim하면 이동속도 더 빠름
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
        OwnerIncreaeWeaponIndex,
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




public class CharacterAnimatorScript : GameCharacterSubScript
{
    private Avatar _gameBasicAvatar = null;
    private GameObject _gameBasicCharacter = null;

    protected Animator _animator = null;
    protected GameObject _characterModelObject = null;

    protected AnimatorOverrideController _overrideController = null;
    protected AnimationClip _currAnimClip = null;

    private StateAsset _currStateAsset = null;
    private int _currAnimIndex = -1;


    protected List<bool> _bodyCoroutineStarted = new List<bool>();
    protected List<bool> _currentBusyAnimatorLayer = new List<bool>();
    protected List<Coroutine> _currentBodyCoroutine = new List<Coroutine>();
    protected List<Action_LayerType> _bodyPartDelegates = new List<Action_LayerType>();


    [SerializeField] protected int _currentBusyAnimatorLayer_BitShift = 0;
    [SerializeField] protected List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();
    protected List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    protected List<LinkedList<BodyPartBlendingWork>> _bodyPartWorks = new List<LinkedList<BodyPartBlendingWork>>();

    [SerializeField] protected AnimationClip _ifWeaponIsLight = null;










    public int GetBusyLayer() { return _currentBusyAnimatorLayer_BitShift; }
    public Animator GetCurrActivatedAnimator() { return _animator; }
    public GameObject GetCurrActivatedModelObject() { return _characterModelObject; }
    public AnimationClip GetCurrAnimationClip() { return _currAnimClip; }
    public Rig GetCharacterRig() { return _characterRig; }
    public RigBuilder GetCharacterRigBuilder() { return _characterRigBuilder; }
    public float GetCharacterHeight() { return 2.0f; }

    public AnimatorStateInfo? GetCurrentStateAnimationInfo()
    {
        int targetLayer = (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
        ? (int)AnimatorLayerTypes.FullBody
        : (int)AnimatorLayerTypes.FullBody + 1;

        return _animator.GetCurrentAnimatorStateInfo(targetLayer);
    }

    public bool GetIsInTransition(AnimatorLayerTypes type)
    {
        int targetLayer = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
            ? (int)type
            : (int)type + 1;

        return _animator.IsInTransition(targetLayer);
    }


    protected RigBuilder _characterRigBuilder = null;
    protected Rig _characterRig = null;

    private void OnDestroy()
    {
        TimeScaler.Instance?.RemoveTimeChangeDelegate(TimeChanged);

        if (_playableGraph.IsValid() == true)
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
        }
    }

    private void TimeChanged(float timeScale)
    {
        _layerMixer.SetSpeed(timeScale);
    }


    private void LateUpdate()
    {
        if (_currStateAsset._myState._isSubAnimationStateMachineExist == false)
        {
            return;
        }

        int animationIndex = _owner.GCST<StateContoller>().SubAnimationStateIndex(_currStateAsset);

        if (animationIndex == _currAnimIndex)
        {
            return;
        }

        AnimationClip nextAnimation = _currStateAsset._myState._subAnimationStateMachine._animations[animationIndex];
        FullBodyAnimationChange(nextAnimation);
        _currAnimIndex = animationIndex;
    }

    private PlayableGraph _playableGraph;
    private AnimatorControllerPlayable _controllerPlayable;
    private AnimationPlayableOutput _playableOutput;
    private AnimationLayerMixerPlayable _layerMixer;
    private AvatarMask _currentAdditiveAvatarMask = null;

    private Coroutine _clipPlayableDetachCoroutine = null;

    private IEnumerator ClipPlayableDetachCoroutine(float time)
    {
        float timeACC = 0.0f;

        while (true)
        {
            timeACC += Time.deltaTime;

            if (timeACC >= time)
            {
                _clipPlayableDetachCoroutine = null;
                AnimationClipPlayable? oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);

                if (oldPlayable == null) 
                {
                    Debug.Assert(false, "ClipPlayable을 잃어버렸습니다");
                    Debug.Break();
                }

                _layerMixer.DisconnectInput(1);
                oldPlayable.Value.Destroy();

                break;
            }

            yield return null;
        }
    }

    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterAnimatorScript);

        _animator = GetComponentInChildren<Animator>();
        _gameBasicAvatar = _animator.avatar;

        Debug.Assert(_animator != null, "Animator가 없다");
        _characterModelObject = _animator.gameObject;
        _gameBasicCharacter = Instantiate(_characterModelObject, transform);
        _gameBasicCharacter.SetActive(false);

        _characterRig = GetComponentInChildren<Rig>();
        _characterRigBuilder = GetComponentInChildren<RigBuilder>();


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
            Debug.Assert(false, "FullBody는 반드시 사용해야 한다");
            Debug.Break();
            _partBlendingDesc[(int)AnimatorLayerTypes.FullBody] = new AnimatorBlendingDesc(AnimatorLayerTypes.FullBody, _animator);
        }

        if (_gameBasicAvatar == null)
        {
            Debug.Assert(false, "캐릭터 초기값 아바타입니다. 반드시 설정돼있어야합니다");
            Debug.Break();
        }

        _playableGraph = PlayableGraph.Create("AvatarMaskChanger");
        _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);


        _layerMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
        _layerMixer.ConnectInput(0, _controllerPlayable, 0);

        _layerMixer.SetLayerAdditive(1, true);
        _layerMixer.SetInputWeight(0, 1.0f);
        _playableOutput.SetSourcePlayable(_layerMixer);
        _playableGraph.Play();

        TimeScaler.Instance.AddTimeChangeDelegate(TimeChanged);
    }



    public void RunAdditivaAnimationClip(AvatarMask additiveAvatarMask, AnimationClip targetAnimationClip, bool isLoop, float weight)
    {
        _currentAdditiveAvatarMask = additiveAvatarMask;

        _layerMixer.SetInputWeight(1, weight);

        if (_currentAdditiveAvatarMask != additiveAvatarMask)
        {
            _layerMixer.SetLayerMaskFromAvatarMask(1, additiveAvatarMask);
            if (_layerMixer.IsLayerAdditive(1) == false)
            {
                Debug.Assert(false);
            }
            
        }

        if (_clipPlayableDetachCoroutine != null)
        {
            StopCoroutine(_clipPlayableDetachCoroutine);
            _clipPlayableDetachCoroutine = null;
        }

        if (isLoop == false)
        {
            _clipPlayableDetachCoroutine = StartCoroutine(ClipPlayableDetachCoroutine(targetAnimationClip.length));
        }

        AnimationClipPlayable oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);

        if (oldPlayable.IsValid() == true)
        {
            if (oldPlayable.GetAnimationClip() == targetAnimationClip)
            {
                oldPlayable.SetTime(0.0f);
                return;
            }

            _layerMixer.DisconnectInput(1);
            oldPlayable.Destroy();
        }

        AnimationClipPlayable newPlayable = AnimationClipPlayable.Create(_playableGraph, targetAnimationClip);


        bool temp = newPlayable.GetApplyFootIK();

        _layerMixer.ConnectInput(1, newPlayable, 0);

        _layerMixer.SetInputWeight(1, 1.0f);

        _layerMixer.SetLayerMaskFromAvatarMask(1, additiveAvatarMask);
    }

    public bool IsLayerConnected(int layerIndex)
    {
        if (_layerMixer.GetInputCount() <= layerIndex)
        {
            return false;
        }

        return _layerMixer.GetInput(layerIndex).IsValid();
    }

    public void StopAdditivaAnimationClip()
    {
        if (IsLayerConnected(1) == false)
        {
            return;
        }

        AnimationClipPlayable? oldPlayable = (AnimationClipPlayable)_layerMixer.GetInput(1);
        _layerMixer.DisconnectInput(1);
        oldPlayable.Value.Destroy();
    }


    public override void SubScriptStart()
    {
        _owner.GCST<CharacterColliderScript>().InitModelCollider(_characterModelObject);
    }


    public void ResetCharacterModel()
    {
        ModelChange(_gameBasicCharacter);
    }



    public bool IsSameSkeleton(Avatar newModelAvatar)
    {
        return (newModelAvatar == _gameBasicAvatar);
    }

    public GameObject ModelChange(GameObject modelObject)
    {
        GameObject newModel = Instantiate(modelObject);
        newModel.SetActive(true);
        SkinnedMeshRenderer[] meshRenderers = newModel.GetComponents<SkinnedMeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
        }

        newModel.transform.SetParent(transform);
        newModel.transform.localPosition = Vector3.zero;
        newModel.transform.rotation = _characterModelObject.transform.rotation;

        Animator newAnimator = newModel.GetComponentInChildren<Animator>();
        newAnimator.runtimeAnimatorController = new AnimatorOverrideController(newAnimator.runtimeAnimatorController);

        SyncAnimatorState(_animator, newAnimator);

        StartCoroutine(WaitNextFrameCoroutine(newModel, meshRenderers));

        return newModel;
    }


    void SyncAnimatorState(Animator sourceAnimator, Animator targetAnimator)
    {
        //0. Playable 동기화
        {
            float firstLayerWeight = _layerMixer.GetInputWeight(0);
            float secondLayerWeight = _layerMixer.GetInputWeight(1);

            _layerMixer.DisconnectInput(0);

            AnimatorControllerPlayable oldPlayable = _controllerPlayable;
            _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, targetAnimator.runtimeAnimatorController);

            _layerMixer.ConnectInput(0, _controllerPlayable, 0);

            _layerMixer.SetInputWeight(0, firstLayerWeight);
            _layerMixer.SetInputWeight(1, secondLayerWeight);

            _playableOutput.SetTarget(targetAnimator);

            oldPlayable.Destroy();
        }

        //1. 변수 동기화
        {
            foreach (AnimatorControllerParameter parameter in sourceAnimator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        bool boolValue = sourceAnimator.GetBool(parameter.name);
                        targetAnimator.SetBool(parameter.name, boolValue);
                        break;

                    case AnimatorControllerParameterType.Float:
                        float floatValue = sourceAnimator.GetFloat(parameter.name);
                        targetAnimator.SetFloat(parameter.name, floatValue);
                        break;

                    case AnimatorControllerParameterType.Int:
                        int intValue = sourceAnimator.GetInteger(parameter.name);
                        targetAnimator.SetInteger(parameter.name, intValue);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        if (sourceAnimator.GetBool(parameter.name))
                        {
                            targetAnimator.SetTrigger(parameter.name);
                        }
                        break;
                }
            }
        }

        //2. 런타임 클립 동기화
        {
            AnimatorOverrideController sourceOverrideController = sourceAnimator.runtimeAnimatorController as AnimatorOverrideController;
            AnimatorOverrideController targeteOverrideController = targetAnimator.runtimeAnimatorController as AnimatorOverrideController;


            List<KeyValuePair<AnimationClip, AnimationClip>> paris = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            sourceOverrideController.GetOverrides(paris);

            foreach (var clipPair in paris)
            {
                if (clipPair.Value == null)
                {
                    continue;
                }
                targeteOverrideController[clipPair.Key] = clipPair.Value;
            }
        }

        //3. Weight 동기화
        {
            int index = 0;

            foreach (AnimatorBlendingDesc desc in _partBlendingDesc)
            {
                desc._ownerAnimator = targetAnimator;

                for (int i = 0; i < 2; i++)
                {
                    AnimatorStateInfo stateInfo = sourceAnimator.GetCurrentAnimatorStateInfo(index);
                    targetAnimator.Play(stateInfo.shortNameHash, index, stateInfo.normalizedTime);
                    targetAnimator.SetLayerWeight(index, _animator.GetLayerWeight(index));

                    index++;
                }
            }
        }




    }

    private IEnumerator WaitNextFrameCoroutine(GameObject newModel, SkinnedMeshRenderer[] meshRenderers)
    {
        yield return new WaitForEndOfFrame();

        GameObject destroyThis = _characterModelObject;

        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = true;
        }

        _characterModelObject = newModel;
        _characterRig = _characterModelObject.GetComponentInChildren<Rig>();
        _characterRigBuilder = _characterModelObject.GetComponentInChildren<RigBuilder>();

        _animator = newModel.GetComponentInChildren<Animator>();
        _overrideController = _animator.runtimeAnimatorController as AnimatorOverrideController;

        _owner.GetComponentInChildren<AimScript2>().SetRigging(_characterRigBuilder, _characterRig);
        _owner.MoveWeapons(newModel);

        CharacterColliderScript ownerCharacterColliderScript = GetComponentInParent<CharacterColliderScript>();
        ownerCharacterColliderScript.InitModelCollider(newModel);



        Destroy(destroyThis);


    }







    public void StateChanged(StateAsset nextState)
    {
        _currStateAsset = nextState;

        AnimationClip nextAnimation = nextState._myState._stateAnimationClip;

        if (nextState._myState._isSubAnimationStateMachineExist == true)
        {
            //애니메이션을 다시 결정해야한다.

            int animationIndex = _owner.GCST<StateContoller>().SubAnimationStateIndex(nextState);

            _currAnimIndex = animationIndex;

            nextAnimation = nextState._myState._subAnimationStateMachine._animations[animationIndex];
        }

        FullBodyAnimationChange(nextAnimation);
    }

    private void FullBodyAnimationChange(AnimationClip nextAnimation)
    {
        _currAnimClip = nextAnimation;
        //FullBody Animation 변경
        {
            /*-------------------------------------------------------------------
            |NOTI| FullBody는 특별합니다. 뭔가 일하고있다면 취소하고 진행해야 합니다.
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

    public void WeaponLayerChange(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon, bool isEnter)
    {
        bool isAttackState = isEnter;

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
                    //공격 애니메이션이다
                    if (isAttackState == true)
                    {
                        int usingHandLayer = (isUsingRightWeapon == true)
                            ? rightHandMainLayer
                            : leftHandMainLayer;

                        int oppositeHandLayer = (isUsingRightWeapon == true)
                            ? leftHandMainLayer
                            : rightHandMainLayer;


                        _animator.SetLayerWeight(usingHandLayer, 0.0f); //왼손은 반드시 따라가야해서 0.0f

                        WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

                        if (oppositeWeaponScript == null) //왼손무기를 쥐고있지 않다면
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
                        }
                        else
                        {
                            if (oppositeWeaponScript._handlingIdleAnimation_OneHand == null)
                            {
                                //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
                            }
                            _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
                        }

                        _animator.SetBool("IsMirroring", isMirrored);
                    }
                    //공격 애니메이션이 아니다
                    else
                    {

                        WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
                        WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

                        //왼손 무기를 쥐고있지 않거나 왼손 무기의 파지 애니메이션이 없다
                        float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //오른손 무기를 쥐고있지 않거나 오른손 무기의 파지 애니메이션이 없다
                        float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

                        _animator.SetBool("IsMirroring", false);
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

                    _animator.SetBool("IsMirroring", isMirrored);

                    if (isAttackState == true)
                    {
                        _animator.SetBool("IsMirroring", isMirrored);
                    }

                    if (currState._myState._isAttackState == true)
                    {
                        if (isAttackState == false)
                        {
                            _animator.SetBool("IsMirroring", isMirrored);
                        }
                    }
                    else
                    {
                        _animator.SetBool("IsMirroring", false);
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



    public void CalculateBodyWorkType_ChangeWeapon(WeaponGrabFocus ownerWeaponGrabFocusType, bool isRightWeaponTrigger, int layerLockResult, bool forcedGo = false)
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

        //반대손 Work 예약
        {
            WeaponScript currentOppositeWeaponScript = _owner.GetCurrentWeaponScript(oppositePartType);

            if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused || ownerWeaponGrabFocusType == WeaponGrabFocus.LeftHandFocused) //무기를 양손으로 잡고있었다.
            {
                if (currentOppositeWeaponScript != null) //반대손에 무기가 있다.
                {
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                    {
                        _bodyPartWorks[(int)oppositePartType].Last.Value._animationClip = currentOppositeWeaponScript.GetOneHandHandlingAnimation(oppositePartType);
                    }

                    //레이어 웨이트 바꾸기
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                }
                else
                {
                    //반대손에 양손으로 잡으면서 집어넣은 무기가 없다.
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
                }

                lockCompares.Add(oppositePartIndex);
            }
        }

        //Target Work 예약
        {
            WeaponScript currTargetWeaponScript = _owner.GetCurrentWeaponScript(targetPartType);

            if (currTargetWeaponScript != null)
            {
                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = currTargetWeaponScript.GetPutawayAnimation(targetPartType);
                }

                //LayerWeight 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //다 재생할때까지 대기하기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
                //무기삭제하기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
            }

            GameObject nextWeaponPrefab = _owner.GetNextWeaponPrefab(targetPartType);
            WeaponScript nextWeaponScript = _owner.GetNextWeaponScript(targetPartType);

            if (nextWeaponScript != null)
            {
                //바꾸려는 다음 무기가 있다

                //무기꺼내기 ZeroFrame 으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = nextWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

                //무기쥐어주기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._attachObjectPrefab = nextWeaponPrefab;
                }

                //무기꺼내기 애니메이션으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
                {
                    _bodyPartWorks[(int)targetPartType].Last.Value._animationClip = nextWeaponScript.GetDrawAnimation(targetPartType);
                }

                //Layer Wright 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
                //다 재생할때까지 기다리기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            }
            else
            {
                //바꾸려는 다음 무기가 없다

                //레이어 꺼버리기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
            }

            lockCompares.Add(targetPartIndex);
        }

        _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerIncreaeWeaponIndex));

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (forcedGo == false && 
            (usingLockResult != layerLockResult))
        {
            Debug.Assert(false, "사용하려는 부위가 Lock과 일치하지 않습니다. 애니메이션 로직을 점검하세요");
            Debug.Break();
            return;
        }

        _bodyPartWorks[targetPartIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[targetPartIndex].Last.Value._targetGrabFocusType = WeaponGrabFocus.Normal;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

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

        //반대 손에 무기를 들고있다.
        if (oppositeWeapon != null)
        {
            //반대손을 무기 집어넣기 애니메이션으로 바꾼다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeapon.GetComponent<WeaponScript>();
                _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetPutawayAnimation(oppositePartType);
            }

            //Layer Weight로 점점 올린다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //반대손의 무기 집어넣기 애니메이션이 다 재생될때까지 홀딩한다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
            //반대손의 무기를 삭제한다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));

            lockCompares.Add(oppositeBodyIndex);
        }

        //이 Behave에서 단계를 넘어섬을 코루틴 락을 해제하여 알려준다.
        CoroutineLock waitOppsiteWeaponPutAwayLock = new CoroutineLock();
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        {
            _bodyPartWorks[oppositeBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
        }

        //반대손의 애니메이션을 바꾼다
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        {
            _bodyPartWorks[(int)oppositeBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
        }

        //반대손의 LayerWeight를 올린다.
        _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        lockCompares.Add(oppositeBodyIndex);


        //해당 무기를 양손으로 잡는 애니메이션을 실행한다. 해당 손의 애니메이션을 바꾼다
        {
            //반대손의 이벤트가 끝날때까지 무한 대기한다
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.WaitCoroutineUnlock));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._coroutineLock = waitOppsiteWeaponPutAwayLock;
            }
            //해당 손의 애니메이션을 바꾼다
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[(int)targetBodyIndex].Last.Value._animationClip = targetWeaponScript.GetTwoHandHandlingAnimation(targetPartType);
            }
            //해당 손의 Layer Weight를 바꾼다.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            lockCompares.Add(targetBodyIndex);
        }

        _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[(int)targetBodyIndex].Last.Value._targetGrabFocusType = (isRightHandTrigger == true)
                ? WeaponGrabFocus.RightHandFocused
                : WeaponGrabFocus.LeftHandFocused;
        }

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "사용하려는 부위가 Lock과 일치하지 않습니다. 애니메이션 로직을 점검하세요");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

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

        //반대 손에 양손으로 잡기 전, 무기를 들고있었다.
        if (oppositeWeaponPrefab != null)
        {
            //반대손을 무기 꺼내기 Zero Frame 애니메이션으로 바꾼다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //반대손 Layer Weight로 점점 올린다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
            //무기를 쥐어준다
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
            {
                _bodyPartWorks[oppositeBodyIndex].Last.Value._attachObjectPrefab = oppositeWeaponPrefab;
            }

            //반대손을 무기 꺼내기 애니메이션으로 바꾼다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                WeaponScript oppositeWeaponScript = oppositeWeaponPrefab.GetComponent<WeaponScript>();
                _bodyPartWorks[oppositeBodyIndex].Last.Value._animationClip = oppositeWeaponScript.GetDrawAnimation(oppositePartType);
            }

            //반대손 Layer Weight로 점점 올린다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

            //반대손의 무기 꺼내기 애니메이션이 다 재생될때까지 홀딩한다.
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        }
        else
        {
            //그냥 꺼버린다
            _bodyPartWorks[oppositeBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        lockCompares.Add(oppositeBodyIndex);

        GameObject targetWeapon = _owner.GetCurrentWeaponPrefab(targetPartType);

        WeaponScript targetWeaponScript = targetWeapon.GetComponent<WeaponScript>();

        AnimationClip oneHandHadlingAnimation = targetWeaponScript.GetOneHandHandlingAnimation(targetPartType);

        if (oneHandHadlingAnimation != null)
        {
            //자세 애니메이션이 있습니다.

            //해당 손의 애니메이션을 바꾼다
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
            {
                _bodyPartWorks[targetBodyIndex].Last.Value._animationClip = oneHandHadlingAnimation;
            }
            //해당 손의 LayerWeight을 바꾼다
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        }
        else
        {
            //자세 애니메이션이 없습니다.
            _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DeActiveAllLayer));
        }

        _bodyPartWorks[targetBodyIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.OwnerChangeGrabFocusType));
        {
            _bodyPartWorks[(int)targetBodyIndex].Last.Value._targetGrabFocusType = WeaponGrabFocus.Normal;
        }

        lockCompares.Add(targetBodyIndex);

        int usingLockResult = 0;
        foreach (int index in lockCompares)
        {
            usingLockResult = usingLockResult | (1 << index);
        }

        if (usingLockResult != layerLockResult)
        {
            Debug.Assert(false, "사용하려는 부위가 Lock과 일치하지 않습니다. 애니메이션 로직을 점검하세요");
            Debug.Break();
            return;
        }

        UpdateBusyAnimatorLayers(usingLockResult, true);

        foreach (int index in lockCompares)
        {
            StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
        }
    }

    public void CalculateBodyWorkType_UseItem_Drink(WeaponGrabFocus ownerWeaponGrabFocusType, ItemAsset usingItemInfo, int layerLockResult)
    {
        Debug.Assert(false, "이곳은 수정해야합니다");
        Debug.Break();

        //HashSet<int> lockCompares = new HashSet<int>();

        //int rightHandIndex = (int)AnimatorLayerTypes.RightHand;

        //Transform rightHandWeaponOriginalTransform = null;

        //WeaponScript weaponScript = _owner.GetCurrentWeaponScript(AnimatorLayerTypes.RightHand);


        ////무기를 오른손에 양손으로 잡고있습니까?
        //if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //{
        //    //오른손에 양손으로 쥐고 있었다면 잠시 왼손에 쥐어준다.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //    {
        //        WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;

        //        //원래 쥐고있는 트랜스폼 캐싱
        //        rightHandWeaponOriginalTransform = weaponScript._socketTranform;

        //        GameObject ownerModelObject = _characterModelObject;

        //        //반대손 소켓 찾기
        //        {
        //            Debug.Assert(ownerModelObject != null, "무기를 붙이려는데 모델이 없어서는 안된다");

        //            WeaponSocketScript[] weaponSockets = ownerModelObject.GetComponentsInChildren<WeaponSocketScript>();

        //            Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");

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
        //else if (weaponScript != null) //양손으로 잡고있진 않았습니다. 근데 오른손에 무기를 들긴 했습니다.
        //{
        //    //반대손을 무기 집어넣기 애니메이션으로 바꾼다.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //    {
        //        _bodyPartWorks[rightHandIndex].Last.Value._animationClip = weaponScript.GetPutawayAnimation(true);
        //    }

        //    //Layer Weight로 점점 올린다.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    //반대손의 무기 집어넣기 애니메이션이 다 재생될때까지 홀딩한다.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));
        //    //반대손의 무기를 삭제한다.
        //    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.DestroyWeapon));
        //}

        //CoroutineLock waitForRightHandReady = new CoroutineLock();
        //_bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.UnLockCoroutine));
        //{
        //    _bodyPartWorks[rightHandIndex].Last.Value._coroutineLock = waitForRightHandReady;
        //}
        //lockCompares.Add(rightHandIndex);

        ////아이템을 사용하는 부위들에게 일감을 배정한다.
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


        //        //아이템을 사용하는 애니메이션으로 바꾼다.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[typeIndex].Last.Value._animationClip = usingItemInfo._usingItemAnimation;
        //        }

        //        //Layer Weight로 점점 올린다.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //애니메이션이 다 재생될때까지 홀딩한다.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AnimationPlayHold));

        //        //Layer Weight를 즉시 꺼버린다.
        //        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.TurnOffAllLayer));

        //        lockCompares.Add(typeIndex);
        //    }

        //}

        //GameObject rightWeaponPrefab = _owner.GetCurrentWeaponPrefab(AnimatorLayerTypes.RightHand);

        //if (rightWeaponPrefab != null) //오른손에 뭔가 잇었습니다.
        //{
        //    WeaponScript rightWeaponScript = rightWeaponPrefab.GetComponent<WeaponScript>();

        //    if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
        //    {
        //        //양손으로 쥐는 애니메이션으로 바꾼다.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetTwoHandHandlingAnimation(true);
        //        }

        //        //Layer Weight로 점점 올린다.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //아까 반대손에 쥐어줬던 무기를 다시 반대로 쥔다.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.SwitchHand));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._weaponEquipTransform = rightHandWeaponOriginalTransform;
        //        }
        //    }
        //    else
        //    {
        //        //무기를 꺼내는 Zero Frame Animation 으로 바꾼다
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation_ZeroFrame));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight로 점점 올린다.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));

        //        //무기를 쥐어준다
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.AttatchObjet));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._attachObjectPrefab = rightWeaponPrefab;
        //        }

        //        //무기를 꺼내는 Zero Frame Animation 으로 바꾼다
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ChangeAnimation));
        //        {
        //            _bodyPartWorks[rightHandIndex].Last.Value._animationClip = rightWeaponScript.GetDrawAnimation(true);
        //        }

        //        //Layer Weight로 점점 올린다.
        //        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork(BodyPartWorkType.ActiveNextLayer));
        //    }

        //    lockCompares.Add(rightHandIndex);
        //}
        //else
        //{
        //    //그냥 꺼버린다
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
        //    Debug.Assert(false, "사용하려는 부위가 Lock과 일치하지 않습니다. 애니메이션 로직을 점검하세요");
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

    protected IEnumerator OwnerIncreaseWeaponIndex(AnimatorLayerTypes targetBody)
    {
        _owner.IncreaseWeaponIndex(targetBody);
        return null;
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

    protected void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        _owner.CreateWeaponModelAndEquip(layerType, nextWeaponPrefab);
    }

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveNextLayer(AnimatorLayerTypes layerType)
    {
        AnimatorBlendingDesc targetBlendingDesc = _partBlendingDesc[(int)layerType];

        if (targetBlendingDesc == null)
        {
            Debug.Assert(false, "사용하지 않는 파트에 대해 함수를 호출했습니다");
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

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_ActiveAllLayer(AnimatorLayerTypes layerType)
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

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_DeActiveAllLayer(AnimatorLayerTypes layerType)
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

    protected IEnumerator ChangeNextLayerWeightSubCoroutine_TurnOffAllLayer(AnimatorLayerTypes layerType)
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

    #endregion Coroutines


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

            case BodyPartWorkType.OwnerIncreaeWeaponIndex:
                {
                    startedCoroutine = StartCoroutine_CallBackAction
                    (
                        OwnerIncreaseWeaponIndex(layerType),
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
                Debug.Assert(false, "케이스가 추가됐습니까?");
                Debug.Break();
                break;

        }

        if (startedCoroutine == null)
        {
            Debug.Assert(false, "코루틴 시작에 실패했습니다.");
            Debug.Break();
            return null;
        }

        int layerTypeIndex = (int)layerType;

        _bodyCoroutineStarted[layerTypeIndex] = true;
        _currentBodyCoroutine[layerTypeIndex] = startedCoroutine;

        return startedCoroutine;
    }


    public float GetStateChangingPercentage()
    {
        float fullBodySubLayerWeight = _partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._blendTarget_Sub;

        if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
        {
            fullBodySubLayerWeight = 1.0f - fullBodySubLayerWeight;
        }

        fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.0f, 1.0f);

        if (_bodyPartWorks[(int)AnimatorLayerTypes.FullBody].Count > 0 && _bodyPartWorks[(int)AnimatorLayerTypes.FullBody].First.Value._workType == BodyPartWorkType.ChangeAnimation)
        {
            fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.01f, 0.99f);
        }

        return fullBodySubLayerWeight;
    }

    #region BlendTree...언젠간 해볼것
    //private PlayableGraph _playableGraph;
    //private AnimationLayerMixerPlayable _layerMixer;
    //private Vector2 _blendTreeWeight = Vector2.zero;
    //private AnimationPlayableOutput _playableOutput;
    //private AnimatorControllerPlayable _controllerPlayable;

    //private AnimationMixerPlayable _mixerPlayable_Main;
    //private SubBlendTreeAsset_2D _currBlendTreeAsset_Main = null;

    //private AnimationMixerPlayable _mixerPlayable_Sub;
    //private SubBlendTreeAsset_2D _currBlendTreeAsset_Sub = null;


    //private void Update()
    //{

    //}


    //private void OnDestroy()
    //{
    //    //if (_playableGraph.IsValid() == true)
    //    //{
    //    //    _playableGraph.Stop();
    //    //    _playableGraph.Destroy();
    //    //}
    //}


    ////private void InitFullbodyBlendTree()
    ////{
    ////    _playableGraph = PlayableGraph.Create("BlendTreeGraph");
    ////    _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Output", _animator);
    ////    _layerMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 3/*2개의 레이어를 사용합니다. 상수 뺄것*/);
    ////    _controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);

    ////    _mixerPlayable_Main = AnimationMixerPlayable.Create(_playableGraph, 4/*4개의 애니메이션 사용합니다. 상수 뺄것*/);
    ////    _mixerPlayable_Sub = AnimationMixerPlayable.Create(_playableGraph, 4/*4개의 애니메이션 사용합니다. 상수 뺄것*/);

    ////    _playableGraph.Connect(_controllerPlayable, 0, _layerMixer, 0); // Layer 0
    ////    _playableGraph.Connect(_mixerPlayable_Main, 0, _layerMixer, 1); // Layer 0
    ////    _playableGraph.Connect(_mixerPlayable_Sub, 0, _layerMixer, 2); // Layer 1

    ////    //_layerMixer.SetInputWeight((int)AnimatorLayerTypes.FullBody * 2, 0.0f);
    ////    //_layerMixer.SetInputWeight((int)AnimatorLayerTypes.FullBody * 2 + 1, 0.0f);
    ////    _layerMixer.SetInputWeight(0, 1.0f);
    ////    _layerMixer.SetInputWeight(1, 0.0f);
    ////    _layerMixer.SetInputWeight(2, 0.0f);


    ////    _playableOutput.SetSourcePlayable(_layerMixer);
    ////    _playableGraph.Stop();
    ////}


    ////private float CalculateBodyLayerWeight(AnimatorLayerTypes type)
    ////{
    ////    if (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////    {
    ////        return _partBlendingDesc[(int)type]._blendTarget;
    ////    }
    ////    return _partBlendingDesc[(int)type]._blendTarget_Sub;
    ////}

    ////private int CalculateBodyLayerInder(AnimatorLayerTypes type)
    ////{
    ////    int currBodyTargetget = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////        ? (int)type
    ////        : (int)type + 1;

    ////    return currBodyTargetget;
    ////}


    ////private AnimationMixerPlayable? CalculateBodyLayerMixer(AnimatorLayerTypes type)
    ////{
    ////    AnimationMixerPlayable? currMixer = (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////        ? _mixerPlayable_Main
    ////        : _mixerPlayable_Sub;

    ////    return currMixer;
    ////}


    ////private SubBlendTreeAsset_2D CalculateCurrBlendTree(AnimatorLayerTypes type)
    ////{
    ////    if (_partBlendingDesc[(int)type]._isUsingFirstLayer == true)
    ////    {
    ////        return _currBlendTreeAsset_Main;
    ////    }

    ////    return _currBlendTreeAsset_Sub;
    ////}


    ////public void ReadyBlendTree(SubBlendTreeAsset_2D blendTreeAsset)
    ////{
    ////    int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    ////    AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);

    ////    if (currFullbodyTarget == 0)
    ////    {
    ////        _currBlendTreeAsset_Main = blendTreeAsset;
    ////    }
    ////    else
    ////    {
    ////        _currBlendTreeAsset_Sub = blendTreeAsset;
    ////    }

    ////    var playable_YUP = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_YUP);
    ////    var playable_XUP = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_XUP);
    ////    var playable_YDOWN = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_YDOWN);
    ////    var playable_XDOWN = AnimationClipPlayable.Create(_playableGraph, blendTreeAsset._animation_XDOWN);

    ////    _playableGraph.Connect(playable_YUP, 0, currMixer.Value, 0);
    ////    _playableGraph.Connect(playable_XUP, 0, currMixer.Value, 1);
    ////    _playableGraph.Connect(playable_YDOWN, 0, currMixer.Value, 2);
    ////    _playableGraph.Connect(playable_XDOWN, 0, currMixer.Value, 3);

    ////    _playableGraph.Play();
    ////}


    ////public void ClearBlendTree()
    ////{
    ////    int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    ////    AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);

    ////    if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
    ////    {
    ////        _currBlendTreeAsset_Main = null;
    ////    }
    ////    else
    ////    {
    ////        _currBlendTreeAsset_Sub = null;
    ////    }

    ////    if (currMixer.Value.IsValid() == false)
    ////    {
    ////        return;
    ////    }

    ////    for (int i = 0; i < 4; i++)
    ////    {
    ////        var oldPlayable = (AnimationClipPlayable)currMixer.Value.GetInput(i);
    ////        if (oldPlayable.IsValid() == false)
    ////        {
    ////            continue;
    ////        }
    ////        _playableGraph.Disconnect(currMixer.Value, i);
    ////        oldPlayable.Destroy();
    ////    }
    ////}


    //private void LateUpdate()
    //{
    //    //int currFullbodyTarget = CalculateBodyLayerInder(AnimatorLayerTypes.FullBody);
    //    //AnimationMixerPlayable? currMixer = CalculateBodyLayerMixer(AnimatorLayerTypes.FullBody);
    //    //SubBlendTreeAsset_2D currAsset = CalculateCurrBlendTree(AnimatorLayerTypes.FullBody);

    //    //if (currAsset != null)
    //    //{
    //    //    float total = Mathf.Abs(_blendTreeWeight.x) + Mathf.Abs(_blendTreeWeight.y);
    //    //    float xRatio = Mathf.Abs(_blendTreeWeight.x) / total;
    //    //    float yRatio = Mathf.Abs(_blendTreeWeight.y) / total;

    //    //    if (_blendTreeWeight.y >= 0.0f)
    //    //    {
    //    //        currMixer.Value.SetInputWeight(0, yRatio); // Idle
    //    //        currMixer.Value.SetInputWeight(2, 0.0f);  // Run
    //    //    }
    //    //    else
    //    //    {
    //    //        currMixer.Value.SetInputWeight(0, 0.0f); // Idle
    //    //        currMixer.Value.SetInputWeight(2, yRatio);  // Run
    //    //    }

    //    //    if (_blendTreeWeight.x >= 0.0f)
    //    //    {
    //    //        currMixer.Value.SetInputWeight(1, xRatio); // Walk
    //    //        currMixer.Value.SetInputWeight(3, 0.0f);  // Run
    //    //    }
    //    //    else
    //    //    {
    //    //        currMixer.Value.SetInputWeight(1, 0.0f); // Walk
    //    //        currMixer.Value.SetInputWeight(3, xRatio);  // Run
    //    //    }


    //    //    float currWeight = CalculateBodyLayerWeight(AnimatorLayerTypes.FullBody);
    //    //    _layerMixer.SetInputWeight(currFullbodyTarget + 1, currWeight);
    //    //}
    //}


    ////public void SetBlendTreeWeight(Vector2 weight)
    ////{
    ////    _blendTreeWeight = weight;
    ////}
    #endregion BlendTree...언젠간 해볼것
}


//public void WeaponLayerChange_EnterAttack(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon)
//{
//    bool isAttackState = true;

//    AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
//    int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.LeftHand * 2
//        : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
//    AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
//    int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.RightHand * 2
//        : (int)AnimatorLayerTypes.RightHand * 2 + 1;

//    bool isMirrored = (isUsingRightWeapon == false);

//    switch (grapFocusType)
//    {
//        case WeaponGrabFocus.Normal:
//            {
//                //공격 애니메이션이다
//                if (isAttackState == true)
//                {
//                    int usingHandLayer = (isUsingRightWeapon == true)
//                        ? rightHandMainLayer
//                        : leftHandMainLayer;

//                    int oppositeHandLayer = (isUsingRightWeapon == true)
//                        ? leftHandMainLayer
//                        : rightHandMainLayer;


//                    _animator.SetLayerWeight(usingHandLayer, 0.0f); //왼손은 반드시 따라가야해서 0.0f

//                    WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

//                    if (oppositeWeaponScript == null) //왼손무기를 쥐고있지 않다면
//                    {
//                        _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
//                    }
//                    else
//                    {
//                        if (oppositeWeaponScript._handlingIdleAnimation_OneHand == null)
//                        {
//                            //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
//                        }
//                        _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
//                    }

//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                //공격 애니메이션이 아니다
//                else
//                {

//                    WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
//                    WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

//                    //왼손 무기를 쥐고있지 않거나 왼손 무기의 파지 애니메이션이 없다
//                    float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

//                    //오른손 무기를 쥐고있지 않거나 오른손 무기의 파지 애니메이션이 없다
//                    float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.RightHandFocused:
//        case WeaponGrabFocus.LeftHandFocused:
//            {
//                float layerWeight = (isAttackState == true)
//                    ? 0.0f
//                    : 1.0f;

//                _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
//                _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

//                _animator.SetBool("IsMirroring", isMirrored);


//                if (currState._myState._isAttackState == true)
//                {
//                }
//                else
//                {
//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.DualGrab:
//            {
//                Debug.Assert(false, "쌍수 컨텐츠가 추가됐습니까?");
//            }
//            break;
//    }
//}

//public void WeaponLayerChange_ExitAttack(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon)
//{
//    bool isAttackState = false;

//    AnimatorBlendingDesc leftHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.LeftHand];
//    int leftHandMainLayer = (leftHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.LeftHand * 2
//        : (int)AnimatorLayerTypes.LeftHand * 2 + 1;
//    AnimatorBlendingDesc rightHandBlendingDesc = _partBlendingDesc[(int)AnimatorLayerTypes.RightHand];
//    int rightHandMainLayer = (rightHandBlendingDesc._isUsingFirstLayer == true)
//        ? (int)AnimatorLayerTypes.RightHand * 2
//        : (int)AnimatorLayerTypes.RightHand * 2 + 1;

//    bool isMirrored = (isUsingRightWeapon == false);

//    switch (grapFocusType)
//    {
//        case WeaponGrabFocus.Normal:
//            {
//                //공격 애니메이션이다
//                if (isAttackState == true)
//                {
//                    int usingHandLayer = (isUsingRightWeapon == true)
//                        ? rightHandMainLayer
//                        : leftHandMainLayer;

//                    int oppositeHandLayer = (isUsingRightWeapon == true)
//                        ? leftHandMainLayer
//                        : rightHandMainLayer;


//                    _animator.SetLayerWeight(usingHandLayer, 0.0f); //왼손은 반드시 따라가야해서 0.0f

//                    WeaponScript oppositeWeaponScript = _owner.GetCurrentWeaponScript(!isUsingRightWeapon);

//                    if (oppositeWeaponScript == null) //왼손무기를 쥐고있지 않다면
//                    {
//                        _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
//                    }
//                    else
//                    {
//                        if (oppositeWeaponScript.GetComponent<WeaponScript>()._handlingIdleAnimation_OneHand == null)
//                        {
//                            //_overrideController[MyUtil._motionChangingAnimationNames[oppositeHandLayer]] = _tempWeaponHandling_NoPlay;
//                        }
//                        _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
//                    }

//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                //공격 애니메이션이 아니다
//                else
//                {
//                    WeaponScript leftHandWeaponScript = _owner.GetCurrentWeaponScript(false);
//                    WeaponScript rightHandWeaponScript = _owner.GetCurrentWeaponScript(true);

//                    //왼손 무기를 쥐고있지 않거나 왼손 무기의 파지 애니메이션이 없다
//                    float leftHandLayerWeight = (leftHandWeaponScript == null || leftHandWeaponScript._handlingIdleAnimation_OneHand == null) //
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

//                    //오른손 무기를 쥐고있지 않거나 오른손 무기의 파지 애니메이션이 없다
//                    float rightHandLayerWeight = (rightHandWeaponScript == null || rightHandWeaponScript._handlingIdleAnimation_OneHand == null)
//                        ? 0.0f
//                        : 1.0f;
//                    _animator.SetLayerWeight(rightHandMainLayer, rightHandLayerWeight);

//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.RightHandFocused:
//        case WeaponGrabFocus.LeftHandFocused:
//            {
//                float layerWeight = (isAttackState == true)
//                    ? 0.0f
//                    : 1.0f;

//                _animator.SetLayerWeight(rightHandMainLayer, layerWeight);
//                _animator.SetLayerWeight(leftHandMainLayer, layerWeight);

//                if (currState._myState._isAttackState == true)
//                {
//                    _animator.SetBool("IsMirroring", isMirrored);
//                }
//                else
//                {
//                    _animator.SetBool("IsMirroring", false);
//                }
//            }
//            break;

//        case WeaponGrabFocus.DualGrab:
//            {
//                Debug.Assert(false, "쌍수 컨텐츠가 추가됐습니까?");
//            }
//            break;
//    }
//}
