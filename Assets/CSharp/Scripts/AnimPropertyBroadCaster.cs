using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimPropertyBroadCaster : MonoBehaviour
{
    [SerializeField] List<Animator> _animators = new List<Animator>();
    [SerializeField] Dictionary<GameObject, Animator> _animatorsDic = new Dictionary<GameObject, Animator>();
    [SerializeField] Animator _ownerAnimator;
    [SerializeField] GameObject _ownerSkeleton;

    public void AddAnimator(GameObject caller)
    {
        Animator callerAnimator = caller.GetComponent<Animator>();

        if (callerAnimator == null ) { return; }

        Debug.Assert(!_animatorsDic.ContainsKey(caller), "이미 있다");

        _animatorsDic.Add(caller, callerAnimator);
    }

    public void RemoveAnimator(GameObject caller)
    {
        Animator callerAnimator = caller.GetComponent<Animator>();

        if (callerAnimator == null) { return; }

        Debug.Assert(_animatorsDic.ContainsKey(caller), "없는데 지우려 하고있다");

        _animatorsDic.Remove(caller);
    }

    private void LateUpdate()
    {
        if (_ownerAnimator == null) { return; }

        BroadCastAnimProperty();
    }

    private void BroadCastAnimProperty()
    {
        AnimatorControllerParameter[] ownerParameters = _ownerAnimator.parameters;

        foreach (KeyValuePair<GameObject, Animator> eachAnimator in _animatorsDic) 
        {

            //if (_ownerAnimator && eachAnimator.Value)
            //{
            //    AnimatorStateInfo stateInfo = _ownerAnimator.GetCurrentAnimatorStateInfo(0);
            //    eachAnimator.Value.Play(stateInfo.fullPathHash, 0, stateInfo.normalizedTime);
            //}

            //if (_ownerAnimator && eachAnimator.Value)
            //{
            //    AnimatorStateInfo stateInfo = _ownerAnimator.GetCurrentAnimatorStateInfo(1);
            //    eachAnimator.Value.Play(stateInfo.fullPathHash, 1, stateInfo.normalizedTime);
            //}


            foreach (var ownerParameter in ownerParameters)
            {
                switch (ownerParameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        eachAnimator.Value.SetFloat(ownerParameter.name, _ownerAnimator.GetFloat(ownerParameter.name));
                        break;
                    case AnimatorControllerParameterType.Int:
                        eachAnimator.Value.SetInteger(ownerParameter.name, _ownerAnimator.GetInteger(ownerParameter.name));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        eachAnimator.Value.SetBool(ownerParameter.name, _ownerAnimator.GetBool(ownerParameter.name));
                        break;
                    //case AnimatorControllerParameterType.Trigger:
                    //    eachAnimator.SetTrigger(ownerParameter.name, ownerParameter.def);
                    //    break;
                    default:
                        break;
                }
            }

            if (_ownerSkeleton != null)
            {
                /*----------------------------------------------------------
                 |TODO| 애니메이션을 돌리면 돌릴수록 위치, 회전이 어긋남 ,이 if문을 주석처리하면 해당 현상 발생
                   왜그런지 알아내야 한다.
                 ----------------------------------------------------------*/
                eachAnimator.Value.gameObject.transform.position = _ownerSkeleton.transform.position;
                eachAnimator.Value.gameObject.transform.rotation = _ownerSkeleton.transform.rotation;
            }
        }
    }
}
