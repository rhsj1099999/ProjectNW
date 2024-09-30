using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimOrbit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (_controlOn == true && _riggingTarget != null)
        {
            transform.position = _riggingTarget.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (_inputController == null)
        //{
        //    return;
        //}

        //Vector2 mouseInput = _inputController._pr_mouseMove;
        //Vector2 mouseInput = _inputController._pr_mouseMove;

    }

    public void CallControlOn()
    {
        _controlOn = !(_controlOn);
    }

    [SerializeField] private InputController _inputController = null;
    [SerializeField] private float _orbitRadius = 30.0f;
    [SerializeField] private bool _controlOn = false;
    [SerializeField] private GameObject _riggingTarget = null;
}
