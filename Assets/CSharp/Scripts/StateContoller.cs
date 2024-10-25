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
    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //�׳� ���⿡ ����ִ°Ÿ�ŭ ��𼱰� �����ؿ��� ���ڴ�
    private List<State> _states = new List<State>();


    private State _currState;
    private InputController _inputController = null;

    public State GetCurrState()
    {
        return _currState;
    }
    

    private void Awake()
    {
        //Debug.Assert(_stateInitial.Count < 0, "��ȹ�� ���µ��� �����ϴ�");

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

        _currState = _states[0]; //���� �տ��� Idle�� �����ϼ���
    }


    public void DoWork()
    {
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

        {
            State nextState = _currState.CheckChangeState();

            if (nextState != null && nextState != _currState) 
            {
                Debug.Log(nextState.GetStateDesc().Value._stataName);

                //���°� �޶�����.
                _currState.DoActions(_currState.GetStateDesc().Value._ExitStateActionTypes);
                _currState = nextState;
                _currState.DoActions(_currState.GetStateDesc().Value._EnterStateActionTypes);
            }

            _currState.DoActions(_currState.GetStateDesc().Value._inStateActionTypes);
        }
    }
}
