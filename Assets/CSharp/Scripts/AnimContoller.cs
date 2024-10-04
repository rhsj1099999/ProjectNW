using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class AnimContoller : MonoBehaviour
{
    public void UpdateParameter(string valName, int val)
    {
        _animator.SetInteger(valName, val);
    }
    public void UpdateParameter(string valName, float val)
    {
        _animator.SetFloat(valName, val);
    }
    public void UpdateParameter(string valName, bool val)
    {
        _animator.SetBool(valName, val);
    }

    public Animator GetAnimator() { return _animator; }

    [SerializeField] private Animator _animator = null;
}
