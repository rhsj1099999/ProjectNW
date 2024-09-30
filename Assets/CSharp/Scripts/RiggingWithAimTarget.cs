using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggingWithAimTarget : MonoBehaviour
{

    private void Awake()
    {
        _initialLocalTransform = transform.localPosition;

    }
    // Start is called before the first frame update
    void Start()
    {
    }

    private void FixedUpdate()
    {
        if (_isAim == true && _targetTransform != null)
        {
            transform.position = _targetTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {


        //if (_isAim == true)
        //{
        //    transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1000.0f;
        //}
    }

    public void AimCall()
    {
        _isAim = !_isAim;

        if (_isAim == false)
        {
            transform.localPosition = _initialLocalTransform;
        }

        if (_isAim == true)
        {
            transform.localPosition = Vector3.zero;
            //transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1000.0f;
        }
    }

    private Vector3 _initialLocalTransform = Vector3.zero;
    public bool _isAim = false;

    [SerializeField] Transform _targetTransform = null;
}
