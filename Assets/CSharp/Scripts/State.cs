using System.Collections.Generic;


public class State
{
    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;
    }

    private StateDesc _stateDesc;
    public StateDesc GetStateDesc() { return _stateDesc; }

    private StateAsset _stateAssetCreateFrom = null;
    public StateAsset GetStateAssetFrom() { return _stateAssetCreateFrom; }

    private Dictionary<State, List<ConditionDesc>> _linkedState = new Dictionary<State , List<ConditionDesc>>();
    public Dictionary<State, List<ConditionDesc>> GetLinkedState() { return _linkedState; }
}
