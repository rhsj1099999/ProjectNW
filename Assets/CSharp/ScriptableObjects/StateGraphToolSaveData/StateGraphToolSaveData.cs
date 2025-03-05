using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static StateGraphToolSaveData.StateGraphToolProjectLoadDesc;
using static StateGraphAsset;

public class StateGraphToolSaveData : ScriptableObject
{
    [Serializable]
    public class StateGraphToolProjectSaveDesc
    {
        /*---------------------------------------------------------
        |NOTI| 그래프 편집기에서 가장 핵심 요소인 2개가 있다.
        ---------------------------------------------------------*/
        public HashSet<StateNode> _nodeDatas = new HashSet<StateNode>();
        public HashSet<Arrow_Ready> _arrowDatas = new HashSet<Arrow_Ready>();
    }



    [Serializable]
    public class StateGraphToolProjectLoadDesc
    {
        [Serializable]
        public class LoadData_StateNode
        {
            public Vector3 _position = Vector3.zero;
            public StateAsset _asset = null;
            public bool _isEntry = false;
            public List<ConditionAssetWrapper> _entryConditions = new List<ConditionAssetWrapper>();
        }

        [Serializable]
        public class LoadData_ArrowNode
        {
            public StateAsset _fromAsset = null;
            public StateAsset _toAsset = null;
            public List<ConditionAssetWrapper> _conditions = new List<ConditionAssetWrapper>();
        }

        public List<LoadData_StateNode> _nodeDatas = new List<LoadData_StateNode>();
        public List<LoadData_ArrowNode> _arrowDatas = new List<LoadData_ArrowNode>();
    }







    [Serializable]
    public class StateGraphToolProjectLoadDescWrapper
    {
        public StateGraphToolProjectLoadDesc _loadData = new StateGraphToolProjectLoadDesc();
        public StateGraphAsset _graphAssetTag = null;
    }



    private void DeleteCheck()
    {
        for (int i = 0; i < _projects.Count; i++)
        {
            if (_projects[i]._graphAssetTag == null)
            {
                _projects.RemoveAt(i);
                --i;
                continue;
            }
        }
    }


    public void InitData(StateGraphAsset savedGraphAsset, StateGraphToolProjectSaveDesc projectData)
    {
        DeleteCheck();

        StateGraphToolProjectLoadDescWrapper existDataWrapper = GetLoadDescWrapper(savedGraphAsset);

        if (existDataWrapper == null) 
        {
            existDataWrapper = new StateGraphToolProjectLoadDescWrapper();
            existDataWrapper._graphAssetTag = savedGraphAsset;
            _projects.Add(existDataWrapper);
        }

        existDataWrapper._loadData = new StateGraphToolProjectLoadDesc();

        StateGraphToolProjectLoadDesc existData = existDataWrapper._loadData;

        /*----------------------------------------------------------------------------------
        |NOTI| '복사'합니다. 복사 해야될까요? 몰라요 안전하게 복사합니다. 창 끄면 참조라서 사라져요
        ----------------------------------------------------------------------------------*/
        {
            foreach (StateNode stateNode in projectData._nodeDatas)
            {
                LoadData_StateNode newStateNodeLoadDesc = new LoadData_StateNode();

                {
                    newStateNodeLoadDesc._asset = stateNode._StateAsset;
                    newStateNodeLoadDesc._position = stateNode.GetPosition_C();
                    newStateNodeLoadDesc._isEntry = stateNode._IsEntryState;

                    newStateNodeLoadDesc._entryConditions.Clear();
                    newStateNodeLoadDesc._entryConditions.AddRange(stateNode._entryConditionDatas);
                }


                existData._nodeDatas.Add(newStateNodeLoadDesc);
            }

            foreach (Arrow_Ready arrowNode in projectData._arrowDatas)
            {
                LoadData_ArrowNode newArrowNodeLoadDesc = new LoadData_ArrowNode();

                {
                    newArrowNodeLoadDesc._toAsset = arrowNode._ToNode._StateAsset;
                    newArrowNodeLoadDesc._fromAsset = arrowNode._FromNode._StateAsset;

                    newArrowNodeLoadDesc._conditions.Clear();
                    newArrowNodeLoadDesc._conditions.AddRange(arrowNode._ConditionDatas);
                }

                existData._arrowDatas.Add(newArrowNodeLoadDesc);
            }
        }
    }

    public StateGraphToolProjectLoadDesc GetLoadDesc(StateGraphAsset stateGraph)
    {
        if (_projects.Count <=0)
        {
            return null;
        }

        foreach (StateGraphToolProjectLoadDescWrapper dataWrapper in _projects)
        {
            if (dataWrapper._graphAssetTag == stateGraph)
            {
                return dataWrapper._loadData;
            }
        }

        return null;
    }

    
    public StateGraphToolProjectLoadDescWrapper GetLoadDescWrapper(StateGraphAsset stateGraph)
    {
        if (_projects.Count <= 0)
        {
            return null;
        }

        foreach (StateGraphToolProjectLoadDescWrapper dataWrapper in _projects)
        {
            if (dataWrapper._graphAssetTag == stateGraph)
            {
                return dataWrapper;
            }
        }

        return null;
    }

    [SerializeField] private List<StateGraphToolProjectLoadDescWrapper> _projects = new List<StateGraphToolProjectLoadDescWrapper>();
    //[SerializeField] private Dictionary<StateGraphAsset, StateGraphToolProjectLoadDesc> _projects = new Dictionary<StateGraphAsset, StateGraphToolProjectLoadDesc>();
    //public IReadOnlyDictionary<StateGraphAsset, StateGraphToolProjectLoadDesc> _Projects => _projects;
}