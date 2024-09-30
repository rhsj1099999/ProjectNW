using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class AnimContoller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (_animator == null)
        {
            //Crash
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //_updator();

    }


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


    [SerializeField] private Animator _animator = null;
}
