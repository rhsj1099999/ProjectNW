using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

using static CharacterScript;
using static BodyPartBlendingWork_LayerModifier;
using static BodyPartBlendingWork_CoroutineLock;





/*----------------------------------------------------
|TODO| 이 컴포넌트가 각 부위에 대해 뭔가를 쭉 진행하다가
상태 변경 간섭을 받는다면? 예정된 일을 한꺼번에 처리하는 
구조가 필요하다.
----------------------------------------------------*/

public abstract class BodyPartBlendingWorkBase
{
    protected BodyPartBlendingWorkBase(AnimatorLayerTypes layerType)
    {
        _layerType = layerType;
    }

    protected AnimatorLayerTypes _layerType = AnimatorLayerTypes.End;
    /*------------------------------------------------------------
    |NOTI| 상속된 클래스는 자신의 리소스만으로 일을 할 수 있어야 한다.
    ------------------------------------------------------------*/
    public abstract IEnumerator DoWork();
}

/*----------------------------------------------------
|NOTI| Character Script가 필요한 Work들
----------------------------------------------------*/
public abstract class BodyPartBlendingWork_OwnerNeed : BodyPartBlendingWorkBase
{
    protected CharacterScript _owner = null;
    protected BodyPartBlendingWork_OwnerNeed(AnimatorLayerTypes layerType, CharacterScript owner) : base(layerType)
    {
        _owner = owner;
    }
}

public class BodyPartBlendingWork_DestroyHandObject : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_DestroyHandObject(AnimatorLayerTypes layerType, CharacterScript owner) : base(layerType, owner) {}

    public override IEnumerator DoWork()
    {
        _owner.DestroyHandObject(_layerType);
        return null;
    }
}

public class BodyPartBlendingWork_IncreaseWeaponIndex : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_IncreaseWeaponIndex(AnimatorLayerTypes layerType, CharacterScript owner) : base(layerType, owner)
    {}

    public override IEnumerator DoWork()
    {
        _owner.IncreaseWeaponIndex(_layerType);
        return null;
    }
}

public class BodyPartBlendingWork_AttachObject : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_AttachObject(AnimatorLayerTypes layerType, CharacterScript owner, ItemStoreDescBase itemStoreDescBase) : base(layerType, owner)
    {
        _itemStoreDescBase = itemStoreDescBase;
    }

    private ItemStoreDescBase _itemStoreDescBase = null;

    public override IEnumerator DoWork()
    {
        _owner.CreateWeaponModelAndEquip(_layerType, _itemStoreDescBase);
        return null;
    }
}

public class BodyPartBlendingWork_ApplyConsumeable : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_ApplyConsumeable(AnimatorLayerTypes layerType, CharacterScript owner, List<BuffAssetBase> buffs) : base(layerType, owner)
    {
        _buffs = buffs;
    }

    private List<BuffAssetBase> _buffs = null; //ApplyComsumeableItemSkill

    public override IEnumerator DoWork()
    {
        _owner.ApplyPotionBuff(_buffs);
        return null;
    }
}

public class BodyPartBlendingWork_ChangeGrabFocus : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_ChangeGrabFocus(AnimatorLayerTypes layerType, CharacterScript owner, WeaponGrabFocus targetGrabFocusType) : base(layerType, owner)
    {
        _targetGrabFocusType = targetGrabFocusType;
    }

    private WeaponGrabFocus _targetGrabFocusType = WeaponGrabFocus.Normal; //OwnerChangeGrabFocusType

    public override IEnumerator DoWork()
    {
        _owner.ChangeGrabFocusType(_targetGrabFocusType);
        return null;
    }
}

public class BodyPartBlendingWork_SwitchHand : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_SwitchHand(AnimatorLayerTypes layerType, Transform oppositeTransform, CharacterScript owner) : base(layerType, owner)
    {
        _oppositeTransform = oppositeTransform;
    }

    private Transform _oppositeTransform = null; //OwnerChangeGrabFocusType

    public override IEnumerator DoWork()
    {
        _owner.WeaponSwitchHand(_layerType, _oppositeTransform);
        return null;
    }
}

public class BodyPartBlendingWork_DestroyWeapon : BodyPartBlendingWork_OwnerNeed
{
    public BodyPartBlendingWork_DestroyWeapon(AnimatorLayerTypes layerType, CharacterScript owner) : base(layerType, owner) {}

    public override IEnumerator DoWork()
    {
        _owner.DestroyWeapon(_layerType);
        return null;
    }
}


/*----------------------------------------------------
|NOTI| CharacterAnimatorScript가 필요한 Work들
----------------------------------------------------*/
public abstract class BodyPartBlendingWork_CharacterAnimatorScriptNeed : BodyPartBlendingWorkBase
{
    public BodyPartBlendingWork_CharacterAnimatorScriptNeed(AnimatorLayerTypes layerType, CharacterAnimatorScript characterAnimatorScript) : base (layerType) 
    {
        _characterAnimatorScript = characterAnimatorScript;    
    }

    protected CharacterAnimatorScript _characterAnimatorScript = null;
}

public class BodyPartBlendingWork_ChangeAnimation : BodyPartBlendingWork_CharacterAnimatorScriptNeed
{
    public enum AnimationType
    {
        ChangeAnimation,
        ChangeAnimation_ZeroFrame,
        End,
    }

    public BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes layerType, CharacterAnimatorScript characterAnimatorScript, bool isZeroFrame, AnimationClip animationClip) : base(layerType, characterAnimatorScript)
    {
        _isZeroFrame = isZeroFrame;
        _animationClip = animationClip;
    }

    private bool _isZeroFrame = false;
    private AnimationClip _animationClip = null;

    public override IEnumerator DoWork()
    {
        AnimatorBlendingDesc targetPart = _characterAnimatorScript.GetBlendingDesc(_layerType);

        targetPart._isUsingFirstLayer = !(targetPart._isUsingFirstLayer);

        int layerStartIndex = (int)_layerType * 2;

        int layerIndex = (targetPart._isUsingFirstLayer == true)
            ? layerStartIndex
            : layerStartIndex + 1;

        string nextNodeName = (targetPart._isUsingFirstLayer == true)
            ? "State1"
            : "State2";

        AnimationClip targetclip = (_isZeroFrame == true)
            ? ResourceDataManager.Instance.GetZeroFrameAnimation(_animationClip.name)
            : _animationClip;

        _characterAnimatorScript._OverrideController[MyUtil._motionChangingAnimationNames[layerIndex]] = targetclip;

        _characterAnimatorScript._Animator.Play(nextNodeName, layerIndex, 0.0f);

        return null;
    }
}

public class BodyPartBlendingWork_LayerModifier : BodyPartBlendingWork_CharacterAnimatorScriptNeed
{
    public enum LayerModifyType
    {
        ActiveNextLayer,
        ActiveAllLayer,
        DeActiveAllLayer,
        TurnOffAllLayer,
        ActiveNextLayerDirectly,
        AnimationPlayHold,
        End,
    }

    public BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes layerType, CharacterAnimatorScript characterAnimatorScript, LayerModifyType workType) : base(layerType, characterAnimatorScript)
    {
        _workType = workType;
    }

    private LayerModifyType _workType = LayerModifyType.End;

    public override IEnumerator DoWork()
    {
        AnimatorBlendingDesc blendingDesc = _characterAnimatorScript.GetBlendingDesc(_layerType);

        if (blendingDesc == null)
        {
            Debug.Assert(false, "사용하지 않는 파트에 대해 함수를 호출했습니다");
            Debug.Break();
            yield break;
        }

        switch (_workType)
        {
            case LayerModifyType.ActiveNextLayer:
                {
                    if (blendingDesc == null)
                    {
                        Debug.Assert(false, "사용하지 않는 파트에 대해 함수를 호출했습니다");
                        Debug.Break();
                        yield break;
                    }

                    int targetHandMainLayerIndex = (int)blendingDesc._myPart * 2;
                    int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

                    while (true)
                    {
                        float mainLayerBlendDelta = 0.0f;
                        float subLayerBlendDelta = 0.0f;

                        mainLayerBlendDelta = (blendingDesc._isUsingFirstLayer == true)
                            ? blendingDesc._transitionSpeed * Time.deltaTime
                            : blendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

                        subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

                        blendingDesc._blendTarget += mainLayerBlendDelta;
                        blendingDesc._blendTarget_Sub += subLayerBlendDelta;

                        blendingDesc._blendTarget = Mathf.Clamp(blendingDesc._blendTarget, 0.0f, 1.0f);
                        blendingDesc._blendTarget_Sub = Mathf.Clamp(blendingDesc._blendTarget_Sub, 0.0f, 1.0f);

                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, blendingDesc._blendTarget);
                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, blendingDesc._blendTarget_Sub);

                        float target = (blendingDesc._isUsingFirstLayer == true)
                            ? blendingDesc._blendTarget
                            : blendingDesc._blendTarget_Sub;

                        if (target >= 1.0f || target <= 0.0f)
                        {
                            break;
                        }

                        yield return null;
                    }
                }
                break;

            case LayerModifyType.ActiveAllLayer:
                {
                    int targetHandMainLayerIndex = (int)blendingDesc._myPart * 2;
                    int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

                    while (true)
                    {
                        float mainLayerBlendDelta = 0.0f;
                        float subLayerBlendDelta = 0.0f;

                        mainLayerBlendDelta = (blendingDesc._isUsingFirstLayer == true)
                            ? blendingDesc._transitionSpeed * Time.deltaTime
                            : blendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

                        subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

                        blendingDesc._blendTarget += mainLayerBlendDelta;
                        blendingDesc._blendTarget_Sub += subLayerBlendDelta;

                        blendingDesc._blendTarget = Mathf.Clamp(blendingDesc._blendTarget, 0.0f, 1.0f);
                        blendingDesc._blendTarget_Sub = Mathf.Clamp(blendingDesc._blendTarget_Sub, 0.0f, 1.0f);

                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, blendingDesc._blendTarget);
                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, blendingDesc._blendTarget_Sub);

                        float target = (blendingDesc._isUsingFirstLayer == true)
                            ? blendingDesc._blendTarget
                            : blendingDesc._blendTarget_Sub;

                        if (target >= 1.0f)
                        {
                            break;
                        }

                        yield return null;
                    }
                }
                break;

            case LayerModifyType.DeActiveAllLayer:
                {
                    int targetHandMainLayerIndex = (int)blendingDesc._myPart * 2;
                    int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

                    while (true)
                    {
                        float mainLayerBlendDelta = blendingDesc._transitionSpeed * Time.deltaTime * -1.0f;

                        blendingDesc._blendTarget += mainLayerBlendDelta;
                        blendingDesc._blendTarget_Sub += mainLayerBlendDelta;

                        blendingDesc._blendTarget = Mathf.Clamp(blendingDesc._blendTarget, 0.0f, 1.0f);
                        blendingDesc._blendTarget_Sub = Mathf.Clamp(blendingDesc._blendTarget_Sub, 0.0f, 1.0f);

                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, blendingDesc._blendTarget);
                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, blendingDesc._blendTarget_Sub);

                        float maxTarget = Mathf.Max(blendingDesc._blendTarget, blendingDesc._blendTarget_Sub);

                        if (maxTarget <= 0.0f)
                        {
                            break;
                        }

                        yield return null;
                    }
                }
                break;

            case LayerModifyType.TurnOffAllLayer:
                {
                    int targetHandMainLayerIndex = (int)blendingDesc._myPart * 2;
                    int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

                    blendingDesc._blendTarget = 0.0f;
                    blendingDesc._blendTarget_Sub = 0.0f;

                    blendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, blendingDesc._blendTarget);
                    blendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, blendingDesc._blendTarget_Sub);
                }
                break;

            case LayerModifyType.ActiveNextLayerDirectly:
                {
                    int targetHandMainLayerIndex = (int)blendingDesc._myPart * 2;
                    int targetHandSubLayerIndex = targetHandMainLayerIndex + 1;

                    while (true)
                    {
                        float mainLayerBlendDelta = 0.0f;
                        float subLayerBlendDelta = 0.0f;

                        mainLayerBlendDelta = (blendingDesc._isUsingFirstLayer == true)
                            ? 1.0f
                            : -1.0f;

                        subLayerBlendDelta = -1.0f * mainLayerBlendDelta;

                        blendingDesc._blendTarget += mainLayerBlendDelta;
                        blendingDesc._blendTarget_Sub += subLayerBlendDelta;

                        blendingDesc._blendTarget = Mathf.Clamp(blendingDesc._blendTarget, 0.0f, 1.0f);
                        blendingDesc._blendTarget_Sub = Mathf.Clamp(blendingDesc._blendTarget_Sub, 0.0f, 1.0f);

                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandMainLayerIndex, blendingDesc._blendTarget);
                        blendingDesc._ownerAnimator.SetLayerWeight(targetHandSubLayerIndex, blendingDesc._blendTarget_Sub);

                        float target = (blendingDesc._isUsingFirstLayer == true)
                            ? blendingDesc._blendTarget
                            : blendingDesc._blendTarget_Sub;

                        if (target >= 1.0f || target <= 0.0f)
                        {
                            break;
                        }

                        yield return null;
                    }
                }
                break;
            case LayerModifyType.AnimationPlayHold:
                {
                    while (true)
                    {
                        int layerIndex = (blendingDesc._isUsingFirstLayer == true)
                            ? (int)_layerType * 2
                            : (int)_layerType * 2 + 1;

                        float normalizedTime = _characterAnimatorScript._Animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;

                        if (normalizedTime >= 1.0f)
                        {
                            break;
                        }

                        yield return null;
                    }
                }
                break;

            default:
                {
                    Debug.Assert(false, "대응이 되지 ㅇㄶ습니다");
                    Debug.Break();
                }
                break;
        }
        yield break;
    }
}



public class BodyPartBlendingWork_CoroutineLock : BodyPartBlendingWorkBase
{
    public enum CoroutineLockType
    {
        WaitFor,
        Unlock,
        End,
    }

    public BodyPartBlendingWork_CoroutineLock(AnimatorLayerTypes layerType, CoroutineLockType lockType, CoroutineLock targetCoroutineLock) : base(layerType)
    {
        _lockType = lockType;
        _coroutineLock = targetCoroutineLock;
    }

    private CoroutineLockType _lockType = CoroutineLockType.End;
    private CoroutineLock _coroutineLock = null;

    public override IEnumerator DoWork()
    {
        switch (_lockType)
        {
            case CoroutineLockType.WaitFor:
                while (_coroutineLock._isEnd == false)
                {
                    yield return null;
                }
                break;

            case CoroutineLockType.Unlock:
            _coroutineLock._isEnd = true;
            break;

            case CoroutineLockType.End:
                break;

            default:
                {
                    Debug.Assert(false, "대응이 되지 않는다");
                    Debug.Break();
                }
                break;
        }
    }
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
    public Animator _Animator => _animator;

    protected GameObject _characterModelObject = null;

    protected AnimatorOverrideController _overrideController = null;
    public AnimatorOverrideController _OverrideController => _overrideController;

    protected AnimationClip _currAnimClip = null;

    private StateAsset _currStateAsset = null;
    private int _currAnimIndex = -1;

    private CharacterModelDataInitializer _currModelDataInitializer = null;
    public CharacterModelDataInitializer _CurrModelDataInitializer => _currModelDataInitializer;

    protected List<bool> _currentBusyAnimatorLayer = new List<bool>();
    protected List<Action_LayerType> _bodyPartDelegates = new List<Action_LayerType>();

    protected List<bool> _bodyCoroutineStarted = new List<bool>();
    protected List<Coroutine> _currentBodyCoroutine = new List<Coroutine>();


    [SerializeField] protected int _currentBusyAnimatorLayer_BitShift = 0;
    [SerializeField] protected List<AnimatorLayerTypes> _usingBodyPart = new List<AnimatorLayerTypes>();
    protected List<AnimatorBlendingDesc> _partBlendingDesc = new List<AnimatorBlendingDesc>();
    public AnimatorBlendingDesc GetBlendingDesc(AnimatorLayerTypes layerType) {return _partBlendingDesc[(int)layerType];}
    protected List<LinkedList<BodyPartBlendingWorkBase>> _bodyPartWorks = new List<LinkedList<BodyPartBlendingWorkBase>>();

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

    private Action<float> _animationSpeedChanged = null;

    private void OnDestroy()
    {
        TimeScaler.Instance?.RemoveTimeChangeDelegate(TimeChanged);

        if (_playableGraph.IsValid() == true)
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
        }
    }


    public void SetAnimationProgress(int layerIndex, float normalizedTime)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);

        _animator.Play(stateInfo.fullPathHash, layerIndex, normalizedTime);
    }

    public float GetAnimationProgress(int layerIndex)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);

        //if (stateInfo.IsName(stateInfo.fullPathHash.ToString()))
        //{
        //    _animator.Play(stateInfo.fullPathHash, layerIndex, normalizedTime);
        //}

        return stateInfo.normalizedTime;
    }

    public int GetCurrFullBodyLayer()
    {
        int layer = (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
            ? (int)AnimatorLayerTypes.FullBody
            : (int)AnimatorLayerTypes.FullBody + 1;

        return layer;
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

        for (int i = 0; i < _usingBodyPart.Count; i++)
        {
            _partBlendingDesc.Add(null);
            _currentBusyAnimatorLayer.Add(false);
            _bodyPartWorks.Add(new LinkedList<BodyPartBlendingWorkBase>());
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

        _currModelDataInitializer = _characterModelObject.GetComponent<CharacterModelDataInitializer>();

        if (_currModelDataInitializer == null)
        {
            Debug.Assert(false, "모델로 쓸 오브젝트는 반드시 이 컴포넌트를 필요로 한다");
            Debug.Break();
        }

        _currModelDataInitializer.Init(_owner);
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
        newModel.GetComponentInChildren<CharacterModelDataInitializer>().Init(_owner);
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
        _owner.MoveWeapons();

        _currModelDataInitializer = _characterModelObject.GetComponent<CharacterModelDataInitializer>();

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

    public void AnimationOverride(AnimationClip overrideAnimation)
    {
        FullBodyAnimationChange(overrideAnimation);
    }

    private void FullBodyAnimationChange(AnimationClip nextAnimation)
    {
        _currAnimClip = nextAnimation;

        float animationSpeed = (_currStateAsset._myState._isAttackState_effectedBySpeed == true)
            ? _owner.GCST<StatScript>().GetPassiveStat(LevelStatAsset.PassiveStat.AttackSpeedPercentage) / 100.0f
            : 1.0f;

        _animator.SetFloat("Speed", animationSpeed);

        /*-------------------------------------------------------------------
        |NOTI| AnimationSpeed 변경됨. 액션호출
        -------------------------------------------------------------------*/
        {
            _animationSpeedChanged?.Invoke(animationSpeed);
        }

        /*-------------------------------------------------------------------
        |NOTI| FullBody Animation 변경
        -------------------------------------------------------------------*/
        {
            AnimatorLayerTypes fullBodyLayer = AnimatorLayerTypes.FullBody;
            int fullBodyLayerIndex = (int)fullBodyLayer;

            if (_currentBodyCoroutine[(int)fullBodyLayer] != null)
            {
                StopCoroutine(_currentBodyCoroutine[(int)fullBodyLayer]);
                _bodyPartWorks[fullBodyLayerIndex].Clear();
            }

            _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(fullBodyLayer, this, false, nextAnimation));
            _bodyPartWorks[fullBodyLayerIndex].AddLast(new BodyPartBlendingWork_LayerModifier(fullBodyLayer, this, LayerModifyType.ActiveNextLayer));

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

    public List<bool> GetCurrentBusyAnimatorLayer()
    {
        return _currentBusyAnimatorLayer;
    }

    public int GetCurrentBusyAnimatorLayer_BitShift()
    {
        return _currentBusyAnimatorLayer_BitShift;
    }



    public void WeaponLayerChange(WeaponGrabFocus grapFocusType, StateAsset currState, bool isUsingRightWeapon, bool isEnter)
    {
        bool isAttackState = isEnter;

        AnimatorLayerTypes targetType = (isUsingRightWeapon == true)
            ? AnimatorLayerTypes.RightHand
            : AnimatorLayerTypes.LeftHand;

        AnimatorLayerTypes oppositeType = (isUsingRightWeapon == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

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

                        GameObject currOppositeWeapon = _owner.GetCurrentWeapon(oppositeType);
                        

                        if (currOppositeWeapon == null) //왼손무기를 쥐고있지 않다면
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 0.0f); //왼손 무기를 쥐고있지 않다면 모션을 따라가야해서 레이어를 꺼버린다.
                        }
                        else
                        {
                            _animator.SetLayerWeight(oppositeHandLayer, 1.0f);
                        }

                        _animator.SetBool("IsMirroring", isMirrored);
                    }
                    //공격 애니메이션이 아니다
                    else
                    {
                        GameObject currLeftWeapon = _owner.GetCurrentWeapon(AnimatorLayerTypes.LeftHand);
                        GameObject currRightWeapon = _owner.GetCurrentWeapon(AnimatorLayerTypes.RightHand);

                        ItemAsset_Weapon leftHandWeaponScript = _owner.GetCurrentWeaponInfo(AnimatorLayerTypes.LeftHand);
                        ItemAsset_Weapon rightHandWeaponScript = _owner.GetCurrentWeaponInfo(AnimatorLayerTypes.RightHand);

                        //왼손 무기를 쥐고있지 않거나 왼손 무기의 파지 애니메이션이 없다
                        float leftHandLayerWeight = (currLeftWeapon == null || ResourceDataManager.Instance.GetHandlingAnimationInfo(leftHandWeaponScript._WeaponType)._HandlingIdleAnimation_OneHand == null) //
                            ? 0.0f
                            : 1.0f;
                        _animator.SetLayerWeight(leftHandMainLayer, leftHandLayerWeight);

                        //오른손 무기를 쥐고있지 않거나 오른손 무기의 파지 애니메이션이 없다
                        float rightHandLayerWeight = (currRightWeapon == null || ResourceDataManager.Instance.GetHandlingAnimationInfo(rightHandWeaponScript._WeaponType)._HandlingIdleAnimation_OneHand == null)
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

    public CoroutineLock CalculateBodyWorkType_WeaponReload(bool isRightWeaponTrigger)
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

        GameObject currOppositeWeapon = _owner.GetCurrentWeapon(oppositePartType);

        CoroutineLock ret = null;

        if (currOppositeWeapon != null) //반대손에 무언가를 들고있다.
        {
            ItemAsset_Weapon currentOppositeWeaponInfo = _owner.GetCurrentWeaponInfo(oppositePartType);

            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currentOppositeWeaponInfo._WeaponType).GetPutawayAnimation(oppositePartType)));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));
            //다 재생할때까지 대기하기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.AnimationPlayHold));
            //무기삭제하기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_DestroyWeapon(oppositePartType, _owner));
            //레이어 웨이트 내리기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.DeActiveAllLayer));
            //코루틴 락 해제
            ret = new CoroutineLock();
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_CoroutineLock(oppositePartType, BodyPartBlendingWork_CoroutineLock.CoroutineLockType.Unlock, ret));




            lockCompares.Add(oppositePartIndex);

            foreach (int index in lockCompares)
            {
                StartProceduralWork((AnimatorLayerTypes)index, _bodyPartWorks[index].First.Value);
            }
        }

        return ret;
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
            ItemStoreDesc_Weapon currentOppositeWeapon = _owner.GetCurrentWeaponStoreDesc(oppositePartType);
            ItemAsset_Weapon currentOppositeWeaponScript = _owner.GetCurrentWeaponInfo(oppositePartType);

            if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused ||
                ownerWeaponGrabFocusType == WeaponGrabFocus.LeftHandFocused) //무기를 양손으로 잡고있었다.
            {
                if (currentOppositeWeaponScript != null) //반대손에 무기가 있다.
                {
                    //무기 집어넣기 애니메이션으로 바꾸기
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currentOppositeWeaponScript._WeaponType).GetDrawAnimation(oppositePartType)));
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_AttachObject(oppositePartType, _owner, currentOppositeWeapon));
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.AnimationPlayHold));
                }
                else
                {
                    //반대손에 양손으로 잡으면서 집어넣은 무기가 없다.
                    _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.DeActiveAllLayer));
                }

                lockCompares.Add(oppositePartIndex);
            }
        }

        //Target Work 예약
        {
            GameObject currWeapon = _owner.GetCurrentWeapon(targetPartType);

            if (currWeapon != null)
            {
                WeaponScript currWeaponScript = currWeapon.GetComponent<WeaponScript>();

                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(targetPartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currWeaponScript.GetItemAsset()._WeaponType).GetPutawayAnimation(targetPartType)));
                //LayerWeight 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.ActiveNextLayer));
                //다 재생할때까지 대기하기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.AnimationPlayHold));
                //무기삭제하기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_DestroyWeapon(targetPartType, _owner));
            }

            ItemAsset_Weapon nextWeaponScript = _owner.GetNextWeaponInfo(targetPartType);
            ItemStoreDesc_Weapon nextWeaponStoreDesc = _owner.GetNextWeaponStoreDesc(targetPartType);

            if (nextWeaponScript != null)
            {
                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(targetPartType, this, true, ResourceDataManager.Instance.GetHandlingAnimationInfo(nextWeaponScript._WeaponType).GetDrawAnimation(targetPartType)));
                //LayerWeight 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.ActiveNextLayer));
                //무기쥐어주기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_AttachObject(targetPartType, _owner, nextWeaponStoreDesc));
                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(targetPartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(nextWeaponScript._WeaponType).GetDrawAnimation(targetPartType)));
                //LayerWeight 바꾸기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.ActiveNextLayer));
                //다 재생할때까지 대기하기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.AnimationPlayHold));
            }
            else
            {
                //바꾸려는 다음 무기가 없다
                //레이어 꺼버리기
                _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.DeActiveAllLayer));
            }

            lockCompares.Add(targetPartIndex);
        }

        _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_IncreaseWeaponIndex(targetPartType, _owner));


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

        //무기 집어넣기 애니메이션으로 바꾸기
        _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeGrabFocus(targetPartType, _owner, WeaponGrabFocus.Normal));

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

        ItemAsset_Weapon targetWeaponInfo = _owner.GetCurrentWeaponInfo(targetPartType);
        ItemAsset_Weapon oppositeWeaponInfo = _owner.GetCurrentWeaponInfo(oppositePartType);

        //반대 손에 무기를 들고있다.
        if (oppositeWeaponInfo != null)
        {
            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(oppositeWeaponInfo._WeaponType).GetPutawayAnimation(oppositePartType)));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));
            //다 재생할때까지 대기하기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.AnimationPlayHold));
            //반대손의 무기를 삭제한다.
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_DestroyWeapon(oppositePartType, _owner));

            lockCompares.Add(oppositeBodyIndex);
        }

        //이 Behave에서 단계를 넘어섬을 코루틴 락을 해제하여 알려준다.
        CoroutineLock waitOppsiteWeaponPutAwayLock = new CoroutineLock();
        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_CoroutineLock(oppositePartType, CoroutineLockType.Unlock, waitOppsiteWeaponPutAwayLock));
        //무기 집어넣기 애니메이션으로 바꾸기
        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(targetWeaponInfo._WeaponType).GetTwoHandHandlingAnimation(targetPartType)));
        //LayerWeight 바꾸기
        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));


        lockCompares.Add(oppositeBodyIndex);


        //해당 무기를 양손으로 잡는 애니메이션을 실행한다. 해당 손의 애니메이션을 바꾼다
        {
            //반대손의 이벤트가 끝날때까지 무한 대기한다
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_CoroutineLock(targetPartType, CoroutineLockType.WaitFor, waitOppsiteWeaponPutAwayLock));
            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(targetPartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(targetWeaponInfo._WeaponType).GetTwoHandHandlingAnimation(targetPartType)));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.ActiveNextLayer));

            lockCompares.Add(targetBodyIndex);
        }

        WeaponGrabFocus nextGrabFocus = (isRightHandTrigger == true)
                ? WeaponGrabFocus.RightHandFocused
                : WeaponGrabFocus.LeftHandFocused;
        _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeGrabFocus(oppositePartType, _owner, nextGrabFocus));

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


        ItemAsset_Weapon oppositeWeaponInfo = _owner.GetCurrentWeaponInfo(oppositePartType);
        ItemStoreDesc_Weapon oppositeWeaponStoreDesc = _owner.GetCurrentWeaponStoreDesc(oppositePartType);

        //반대 손에 양손으로 잡기 전, 무기를 들고있었다.
        if (oppositeWeaponInfo != null)
        {
            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, true, ResourceDataManager.Instance.GetHandlingAnimationInfo(oppositeWeaponInfo._WeaponType).GetDrawAnimation(oppositePartType)));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));
            //무기 쥐어주기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_AttachObject(oppositePartType, _owner, oppositeWeaponStoreDesc));


            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(oppositePartType, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(oppositeWeaponInfo._WeaponType).GetDrawAnimation(oppositePartType)));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.ActiveNextLayer));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.AnimationPlayHold));
        }
        else
        {
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)oppositePartType].AddLast(new BodyPartBlendingWork_LayerModifier(oppositePartType, this, LayerModifyType.DeActiveAllLayer));
        }

        lockCompares.Add(oppositeBodyIndex);


        ItemAsset_Weapon targetWeaponInfo = _owner.GetCurrentWeaponInfo(targetPartType);

        AnimationClip oneHandHadlingAnimation = ResourceDataManager.Instance.GetHandlingAnimationInfo(targetWeaponInfo._WeaponType).GetOneHandHandlingAnimation(targetPartType);

        if (oneHandHadlingAnimation != null)
        {
            //자세 애니메이션이 있습니다.

            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeAnimation(targetPartType, this, false, oneHandHadlingAnimation));
            //LayerWeight 바꾸기
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.ActiveNextLayer));
        }
        else
        {
            //자세 애니메이션이 없습니다.
            _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_LayerModifier(targetPartType, this, LayerModifyType.DeActiveAllLayer));
        }

        _bodyPartWorks[(int)targetPartType].AddLast(new BodyPartBlendingWork_ChangeGrabFocus(targetPartType, _owner, WeaponGrabFocus.Normal));

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

    public void CalculateBodyWorkType_UseItem_Drink(WeaponGrabFocus ownerWeaponGrabFocusType, ItemStoreDescBase usingItemInfo, int layerLockResult)
    {
        HashSet<int> lockCompares = new HashSet<int>();

        int rightHandIndex = (int)AnimatorLayerTypes.RightHand;

        Transform rightHandWeaponOriginalTransform = null;

        GameObject currRightHandWeapon = _owner.GetCurrentWeapon(AnimatorLayerTypes.RightHand);

        //무기를 오른손에 양손으로 잡고있습니까?
        if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused &&
            currRightHandWeapon != null)
        {
            WeaponScript weaponScript = currRightHandWeapon.GetComponent<WeaponScript>();
            ItemAsset_Weapon weaponItemAsset = weaponScript.GetItemAsset();

            //오른손에 양손으로 쥐고 있었다면 잠시 왼손에 쥐어준다.
            {
                WeaponSocketScript.SideType targetSide = WeaponSocketScript.SideType.Left;

                //원래 쥐고있는 트랜스폼 캐싱
                rightHandWeaponOriginalTransform = weaponScript._socketTranform;

                GameObject ownerModelObject = _characterModelObject;

                //반대손 소켓 찾기
                Transform oppositeTransform = null;
                {
                    Debug.Assert(ownerModelObject != null, "무기를 붙이려는데 모델이 없어서는 안된다");

                    WeaponSocketScript[] weaponSockets = ownerModelObject.GetComponentsInChildren<WeaponSocketScript>();

                    Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");

                    foreach (var socketComponent in weaponSockets)
                    {
                        if (socketComponent._sideType != targetSide)
                        {
                            continue;
                        }

                        foreach (var type in socketComponent._equippableWeaponTypes)
                        {
                            if (type == weaponItemAsset._WeaponType)
                            {
                                oppositeTransform = socketComponent.gameObject.transform;
                                break;
                            }
                        }
                    }
                }

                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_SwitchHand(AnimatorLayerTypes.RightHand, oppositeTransform, _owner));
            }
        }
        else if (currRightHandWeapon != null) //양손으로 잡고있진 않았습니다. 근데 오른손에 무기를 들긴 했습니다.
        {
            WeaponScript weaponScript = currRightHandWeapon.GetComponent<WeaponScript>();
            ItemAsset_Weapon weaponItemAsset = weaponScript.GetItemAsset();

            //무기 집어넣기 애니메이션으로 바꾸기
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(weaponItemAsset._WeaponType).GetPutawayAnimation(true)));
            //LayerWeight 바꾸기
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
            //LayerWeight 바꾸기
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.AnimationPlayHold));
            //반대손의 무기를 삭제한다.
            _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_DestroyWeapon(AnimatorLayerTypes.RightHand, _owner));
        }

        CoroutineLock waitForRightHandReady = new CoroutineLock();
        _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_CoroutineLock(AnimatorLayerTypes.RightHand,CoroutineLockType.Unlock, waitForRightHandReady));


        lockCompares.Add(rightHandIndex);

        //아이템을 사용하는 부위들에게 일감을 배정한다.
        {
            ItemAsset_Consume itemAsset_Consume = (ItemAsset_Consume)usingItemInfo._itemAsset;
            List<AnimatorLayerTypes> mustUsingLayers = itemAsset_Consume._UsingItemMustNotBusyLayers;

            foreach (AnimatorLayerTypes type in mustUsingLayers)
            {
                int typeIndex = (int)type;

                if (lockCompares.Contains(typeIndex) == false)
                {
                    _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_CoroutineLock(type, CoroutineLockType.WaitFor, waitForRightHandReady));
                }


                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(type, this, false, itemAsset_Consume._UsingItemAnimation_Phase1));
                //LayerWeight 바꾸기
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_LayerModifier(type, this, LayerModifyType.ActiveNextLayer));


                if (type == AnimatorLayerTypes.RightHand)
                {
                    //아이템을 손에 붙입니다--------------------------------------------------------------------------
                    _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_AttachObject(type, _owner, usingItemInfo));
                    //-----------------------------------------------------------------------------------------------
                }


                //LayerWeight 바꾸기
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_LayerModifier(type, this, LayerModifyType.AnimationPlayHold));

                //Phase1이 끝났습니다
                {
                    if (type == AnimatorLayerTypes.RightHand)
                    {
                        //LayerWeight 바꾸기
                        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_ApplyConsumeable(type, _owner, itemAsset_Consume._Buffs));

                        if (itemAsset_Consume._UsingItemAnimation_Phase2 != null)
                        {
                            //충전횟수를 1회 감소시킨다.
                        }
                        else
                        {
                            int nextCount = usingItemInfo._Count - 1;
                            usingItemInfo.SetItemCount(nextCount);
                        }
                    }

                    if (itemAsset_Consume._UsingItemAnimation_Phase2 != null)
                    {
                        //Phase2가 존재하나요? -> 다시 주머니에 집어넣는 충전형인가봐요

                        //무기 집어넣기 애니메이션으로 바꾸기
                        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(type, this, false, itemAsset_Consume._UsingItemAnimation_Phase2));
                        //LayerWeight 바꾸기
                        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_LayerModifier(type, this, LayerModifyType.ActiveNextLayerDirectly));
                        //LayerWeight 바꾸기
                        _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_LayerModifier(type, this, LayerModifyType.AnimationPlayHold));
                    }
                }

                if (type == AnimatorLayerTypes.RightHand)
                {
                    //아이템을 손에서 제거합니다---------------------------------------------------------------------- -
                    _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_DestroyHandObject(type, _owner));
                    //-----------------------------------------------------------------------------------------------
                }
                
                //LayerWeight 바꾸기
                _bodyPartWorks[typeIndex].AddLast(new BodyPartBlendingWork_LayerModifier(type, this, LayerModifyType.DeActiveAllLayer));

                lockCompares.Add(typeIndex);
            }
        }


        ItemAsset_Weapon currRightHandWeaponInfo = _owner.GetCurrentWeaponInfo(AnimatorLayerTypes.RightHand);
        ItemStoreDesc_Weapon currRightHandWeaponStoreDesc = _owner.GetCurrentWeaponStoreDesc(AnimatorLayerTypes.RightHand); 

        if (currRightHandWeaponInfo != null) //오른손에 뭔가 잇었습니다.
        {
            if (ownerWeaponGrabFocusType == WeaponGrabFocus.RightHandFocused)
            {
                //무기 집어넣기 애니메이션으로 바꾸기
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currRightHandWeaponInfo._WeaponType).GetTwoHandHandlingAnimation(AnimatorLayerTypes.RightHand)));
                //LayerWeight 바꾸기
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
                //LayerWeight 바꾸기
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_SwitchHand(AnimatorLayerTypes.RightHand, rightHandWeaponOriginalTransform, _owner));
            }
            else
            {
                if (ownerWeaponGrabFocusType != WeaponGrabFocus.LeftHandFocused)
                {
                    //무기 집어넣기 애니메이션으로 바꾸기
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, true, ResourceDataManager.Instance.GetHandlingAnimationInfo(currRightHandWeaponInfo._WeaponType).GetDrawAnimation(AnimatorLayerTypes.RightHand)));
                    //LayerWeight 바꾸기
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
                    //LayerWeight 바꾸기
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_AttachObject(AnimatorLayerTypes.RightHand, _owner, currRightHandWeaponStoreDesc));


                    //무기 집어넣기 애니메이션으로 바꾸기
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currRightHandWeaponInfo._WeaponType).GetDrawAnimation(AnimatorLayerTypes.RightHand)));
                    //LayerWeight 바꾸기
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
                }
                else
                {
                    ItemAsset_Weapon currLeftHandWeaponInfo = _owner.GetCurrentWeaponInfo(AnimatorLayerTypes.LeftHand);

                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currLeftHandWeaponInfo._WeaponType).GetTwoHandHandlingAnimation(AnimatorLayerTypes.LeftHand)));
                    _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
                }

            }

            lockCompares.Add(rightHandIndex);
        }
        else
        {
            if (ownerWeaponGrabFocusType == WeaponGrabFocus.LeftHandFocused)
            {
                ItemAsset_Weapon currLeftHandWeaponInfo = _owner.GetCurrentWeaponInfo(AnimatorLayerTypes.LeftHand);

                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_ChangeAnimation(AnimatorLayerTypes.RightHand, this, false, ResourceDataManager.Instance.GetHandlingAnimationInfo(currLeftHandWeaponInfo._WeaponType).GetTwoHandHandlingAnimation(AnimatorLayerTypes.LeftHand)));
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.ActiveNextLayer));
            }
            else
            {
                //그냥 꺼버린다
                _bodyPartWorks[rightHandIndex].AddLast(new BodyPartBlendingWork_LayerModifier(AnimatorLayerTypes.RightHand, this, LayerModifyType.DeActiveAllLayer));
            }

            lockCompares.Add(rightHandIndex);
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




    #region Coroutines

    //Corous

    #endregion Coroutines


    #region Coroutine StartFunc

    protected void StartNextCoroutine(AnimatorLayerTypes layerType)
    {
        LinkedList<BodyPartBlendingWorkBase> target = _bodyPartWorks[(int)layerType];
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

    protected void StartProceduralWork(AnimatorLayerTypes layerType, BodyPartBlendingWorkBase work)
    {
        Coroutine startedCoroutine = StartCoroutine_CallBackAction(work.DoWork(), StartNextCoroutine, layerType);

        if (startedCoroutine == null)
        {
            Debug.Assert(false, "코루틴 시작에 실패했습니다.");
            Debug.Break();
            return;
        }

        int layerTypeIndex = (int)layerType;

        _bodyCoroutineStarted[layerTypeIndex] = true;
        _currentBodyCoroutine[layerTypeIndex] = startedCoroutine;
    }


    public float GetStateChangingPercentage()
    {
        float fullBodySubLayerWeight = _partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._blendTarget_Sub;

        if (_partBlendingDesc[(int)AnimatorLayerTypes.FullBody]._isUsingFirstLayer == true)
        {
            fullBodySubLayerWeight = 1.0f - fullBodySubLayerWeight;
        }

        fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.0f, 1.0f);

        if (_bodyPartWorks[(int)AnimatorLayerTypes.FullBody].Count > 0)
        {
            BodyPartBlendingWork_ChangeAnimation cast = _bodyPartWorks[(int)AnimatorLayerTypes.FullBody].First.Value as BodyPartBlendingWork_ChangeAnimation;

            if (cast != null)
            {
                fullBodySubLayerWeight = Mathf.Clamp(fullBodySubLayerWeight, 0.01f, 0.99f);
            }
        }

        return fullBodySubLayerWeight;
    }
}