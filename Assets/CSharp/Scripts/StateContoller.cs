using System;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public struct StateInitial
{
    public string _stateName;
    public StateDesc _stateDesc;
}

public class StateContoller : MonoBehaviour
{
    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //그냥 여기에 들어있는거만큼 어디선가 복사해오면 좋겠다
    private List<State> _states = new List<State>();


    private State _currState;
    private InputController _inputController = null;

    public State GetCurrState()
    {
        return _currState;
    }
    

    private void Awake()
    {
        //Debug.Assert(_stateInitial.Count < 0, "계획된 상태들이 없습니다");

        PlayerScript playerScript = GetComponent<PlayerScript>();

        for (int i = 0; i < _stateInitial.Count; ++i)
        {
            State newState = new State(_stateInitial[i]);
            
            newState.Initialize(playerScript);

            _states.Add(newState);
        }

        foreach (var state in _states) 
        {
            state.LinkingStates(ref _states, playerScript);
        }

        _currState = _states[0]; //가장 앞에껄 Idle로 설정하세요
    }


    public void DoWork()
    {
        Debug.Assert(_currState != null, "스테이트 null입니다");

        {
            State nextState = _currState.CheckChangeState();

            if (nextState != null && nextState != _currState) 
            {
                Debug.Log(nextState.GetStateDesc().Value._stataName);

                //상태가 달라졌다.
                _currState.DoActions(_currState.GetStateDesc().Value._ExitStateActionTypes);
                _currState = nextState;
                _currState.DoActions(_currState.GetStateDesc().Value._EnterStateActionTypes);
            }

            _currState.DoActions(_currState.GetStateDesc().Value._inStateActionTypes);
        }
    }
}
