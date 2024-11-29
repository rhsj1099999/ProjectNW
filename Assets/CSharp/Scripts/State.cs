using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static StateContoller;
using static StateGraphAsset;


public class State
{
    private bool _isTimerHandleAnimation = false;

    //public class StateAnimActionInfo
    //{
    //    public AnimationHipCurve _myAnimationCurve = null;
    //    public FrameData _myFrameData = null;

    //    public float _currStateSecond = 0.0f;
    //    public float _prevStateSecond = 0.0f;
    //    public float _prevReadedSecond = 0.0f;
    //}

    public State(StateAsset stateAsset)
    {
        _stateDesc = stateAsset._myState; //복사 완료
        _stateAssetCreateFrom = stateAsset;

        //HipCurve Data
        {
            //ResourceDataManager.Instance.AddHipCurve(_stateDesc._stateAnimationClip);
            //_stateAnimActionInfo._myAnimationCurve = ResourceDataManager.Instance.GetHipCurve(_stateDesc._stateAnimationClip);
        }
    }

    private PlayerScript _owner = null;

    private StateDesc _stateDesc;
    public StateDesc GetStateDesc() { return _stateDesc; }

    private StateAsset _stateAssetCreateFrom = null;
    public StateAsset GetStateAssetFrom() { return _stateAssetCreateFrom; }



    private Dictionary<State, List<ConditionDesc>> _linkedState = new Dictionary<State , List<ConditionDesc>>();
    public Dictionary<State, List<ConditionDesc>> GetLinkedState() { return _linkedState; }

    //private SortedDictionary<int, List<ConditionDesc>> _


    //public void SettingOwnerComponent(PlayerScript owner, StateContollerComponentDesc ownerComponent)
    //{
    //    _owner = owner;
    //    InitPartial(ownerComponent, _stateDesc._EnterStateActionTypes, owner);
    //    InitPartial(ownerComponent, _stateDesc._inStateActionTypes, owner);
    //    InitPartial(ownerComponent, _stateDesc._ExitStateActionTypes, owner);
    //}


    
}
