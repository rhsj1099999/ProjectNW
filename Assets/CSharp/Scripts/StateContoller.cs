using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class StateInitial
{
    public string _stateName;
    public StateDesc _stateDesc;
}

public class StateContoller : MonoBehaviour
{
    [SerializeField] private List<StateAsset> _stateInitial = new List<StateAsset>(); //�׳� ���⿡ ����ִ°Ÿ�ŭ ��𼱰� �����ؿ��� ���ڴ�
    private List<State> _states = new List<State>();


    private State _currState;
    private PlayerScript _owner = null;
    public State GetCurrState(){return _currState;}

    //private HashSet<Condition> _createdCondition = new HashSet<Condition>(); //�̰� ������ ����մϴ�




    private void Awake()
    {
        //Debug.Assert(_stateInitial.Count < 0, "��ȹ�� ���µ��� �����ϴ�");

        PlayerScript playerScript = GetComponent<PlayerScript>();
        _owner = playerScript;

        for (int i = 0; i < _stateInitial.Count; ++i)
        {
            State newState = new State(_stateInitial[i]);
            
            newState.Initialize(playerScript);

            _states.Add(newState);
        }

        foreach (var state in _states) 
        {
            state.LinkingStates(ref _states, playerScript);

            /*----------------------------------------------------------------------------------------------------------
            |TODO| �Ʒ��ٰ� ���� ������ �ǰ� �ٲ���Ѵ�. ���� ������� ���� ĳ���� ������ ������ �� �����ϱ�
            ----------------------------------------------------------------------------------------------------------*/
            //state.LinkingStates(ref _states, playerScript, _createdCondition);
        }

        ChangeState(_states[0]);
    }

    private void ChangeState(State nextState)
    {
        if (nextState == null)
        {
            return;
        }

        //Weapon�� ���� �۾� �����ֱ�
        {
            //Weapon --> ???
        }
            
        if (nextState != _currState)  //���°� �޶�����.
        {
            Debug.Log(nextState.GetStateDesc()._stataName);

            _owner.StateChanged();

            if (_currState != null) 
            {
                _currState.DoActions(_currState.GetStateDesc()._ExitStateActionTypes);
            }

            _currState = nextState;

            _currState.DoActions(_currState.GetStateDesc()._EnterStateActionTypes);
        }
    }


    public void DoWork()
    {
        Debug.Assert(_currState != null, "������Ʈ null�Դϴ�");

        {
            State nextState = _currState.CheckChangeState();

            ChangeState(nextState);

            _currState.DoActions(_currState.GetStateDesc()._inStateActionTypes);
        }
    }
}
