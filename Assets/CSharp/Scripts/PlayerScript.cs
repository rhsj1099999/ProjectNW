using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    //대표 컴포넌트
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;

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

        {//상태 업데이트
            _stateContoller.DoWork();
            State currState = _stateContoller.GetCurrState();
            //Debug.Log("CurrState : " + currState.GetStateDesc()._stataName);
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
        }
    }

    private IEnumerator SwitchingBlendCoroutine()
    {
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0번 레이어를 재생해야하나
                ? Time.deltaTime * -_transitionSpeed   //0번 레이어를 재생해야한다면 목표값은 0.0
                : Time.deltaTime * _transitionSpeed;  //1번 레이어를 재생해야한다면 목표값은 1.0

            _blendTarget += blendDelta;

            _animator.SetFloat("Blend", _blendTarget);

            if (_blendTarget >= 1.0f)
            {
                _blendTarget = 1.0f;
                break;
            }
            else if (_blendTarget <= 0.0f)
            {
                _blendTarget = 0.0f;
                break;
            }

            yield return null;
        }

        _corutineStarted = false;
    }


    private void ChangeAnimation(AnimationClip targetClip)
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

        if (_currentLayerIndex == 0)
        {
            _overrideController[_targetName1] = targetClip;
        }
        else
        {
            _overrideController[_targetName2] = targetClip;
        }

        //Start Coroutine
        if (_corutineStarted == false)
        {
            _corutineStarted = true;
            StartCoroutine("SwitchingBlendCoroutine");
        }
    }
}
