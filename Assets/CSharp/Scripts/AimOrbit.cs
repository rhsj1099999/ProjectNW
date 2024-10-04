using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimOrbit : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (_controlOn == true && _riggingTarget != null)
        {
            transform.position = _riggingTarget.transform.position;
        }
    }

    public void CallControlOn()
    {
        _controlOn = !(_controlOn);
    }

    [SerializeField] private bool _controlOn = false;
    [SerializeField] private GameObject _riggingTarget = null;
}
