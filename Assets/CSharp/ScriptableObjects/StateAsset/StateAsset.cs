using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "StateAsset", menuName = "Scriptable Object/CreateStateAsset", order = int.MinValue)]
public class StateAsset : ScriptableObject
{
    [SerializeField] public StateDesc _myState = null;

    private void OnValidate()
    {
        {
            ////루프 상태입니까?
            //{
            //    if (_myState._isLoopState == true)
            //    {
            //        if (_myState._breakLoopStateCondition == null)
            //        {
            //            _myState._breakLoopStateCondition = new List<ConditionDesc>();
            //        }
            //    }
            //    else
            //    {
            //        if (_myState._breakLoopStateCondition != null)
            //        {
            //            _myState._breakLoopStateCondition = null;
            //        }
            //    }
            //}

            ////AI공격 상태입니까?
            //{
            //    if (_myState._isAIAttackState == true)
            //    {
            //        if (_myState._aiAttackStateDesc == null)
            //        {
            //            _myState._aiAttackStateDesc = new AIAttackStateDesc();
            //        }
            //    }
            //    else
            //    {
            //        if (_myState._aiAttackStateDesc != null)
            //        {
            //            _myState._aiAttackStateDesc = null;
            //        }
            //    }
            //}
        }

        //AI 상태입니까?
        {
            if (_myState._isAttackState == true)
            {
                if (_myState._attackDamageMultiply == null)
                {
                    _myState._attackDamageMultiply = new DamageDesc();
                }
            }
            else
            {
                if (_myState._attackDamageMultiply != null)
                {
                    _myState._attackDamageMultiply = null;
                }
            }
        }



        //AI공격 상태입니까?
        {
            if (_myState._isAIState == true)
            {
                if (_myState._aiStateDesc == null)
                {
                    _myState._aiStateDesc = new AIStateDesc();
                }
            }
            else
            {
                if (_myState._aiStateDesc != null)
                {
                    _myState._aiStateDesc = null;
                }
            }
        }

        //블렌드 트리가 있습니까
        {
            if (_myState._isSubBlendTreeExist == false)
            {
                if (_myState._subBlendTree != null)
                {
                    _myState._subBlendTree = null;
                }
            }
            else
            {
                if (_myState._subBlendTree == null)
                {
                    _myState._subBlendTree = new SubBlendTreeAsset_2D();
                }
            }
        }

        //애니메이션 서브스테이트 머신이 있습니까?
        {
            if (_myState._isSubAnimationStateMachineExist == false)
            {
                if (_myState._subAnimationStateMachine != null)
                {
                    _myState._subAnimationStateMachine = null;
                }
            }
            else
            {
                if (_myState._subAnimationStateMachine == null)
                {
                    _myState._subAnimationStateMachine = new SubAnimationStateMachine();
                }
            }
        }



        //스텟제한이 있나요?
        {
            if (_myState._isNeedStat == false)
            {
                if (_myState._needStat != null)
                {
                    _myState._needStat = null;
                }
            }
            else
            {
                if (_myState._needStat == null)
                {
                    _myState._needStat = new NeedStatDesc();
                }
            }
        }
    }
}
