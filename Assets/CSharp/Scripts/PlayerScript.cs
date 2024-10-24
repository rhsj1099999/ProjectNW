using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    //대표 컴포넌트

    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;



    private Animator _animator = null;
    private AnimatorOverrideController _overrideController = null;
    private bool _currAnimNode = true; //true = State1;
    [SerializeField] private AnimContoller _AnimController = null;
    [SerializeField] private List<string> _enemyTags = new List<string>();
    private float _crossFadeTime = 0.1f;
    private string _targetName1 = "Zombie Idle";
    private string _targetName2 = "UseThisToChange";
    private AnimationClip _currAnimClip = null;
    private AnimationClip _prevAnimClip = null;

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
    }

    private void Start()
    {
    }

    private void Update()
    {
        {//기본적으로 중력은 계속 업데이트 한다

        }

        _stateContoller.DoWork();
        State currState = _stateContoller.GetCurrState();

        AnimationClip currAnimationClip = currState.GetStateDesc().Value._stateAnimationClip;
        if (_currAnimClip != currAnimationClip) 
        {
            ChangeAnimation(currAnimationClip);
        }




    }




    private void ChangeAnimation(AnimationClip targetClip)
    {
        AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);

        AnimatorClipInfo currentClipInfo = currentClipInfos[0];

        AnimationClip currentClip = currentClipInfo.clip;

        if (currentClip == targetClip)
        {
            return;
        }

        string nextNode = (_currAnimNode == true) ? "State2" : "State1";
        _currAnimNode = !_currAnimNode;
        if (_currAnimNode == true)
        {
            _overrideController[_targetName1] = targetClip;
        }
        else
        {
            _overrideController[_targetName2] = targetClip;
        }

        _animator.CrossFade(nextNode, _crossFadeTime);

        _prevAnimClip = currentClip;
        _currAnimClip = targetClip;
    }

}
