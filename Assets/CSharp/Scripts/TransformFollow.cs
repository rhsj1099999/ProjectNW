using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_target == null && _isTick != false)
        {
            if (_isPositionFollow == true)
            {
                transform.position = _target.position;
            }
            if (_isRotationFollow == true)
            {
                transform.rotation = _target.rotation;
            }
        }
    }

    private void LateUpdate()
    {
        if (_target == null && _isLateTick != false)
        {
            if (_isPositionFollow == true)
            {
                transform.position = _target.position;
            }
            if (_isRotationFollow == true)
            {
                transform.rotation = _target.rotation;
            }
        }
    }

    [SerializeField] private bool _isLateTick = false;
    [SerializeField] private bool _isTick = false;
    [SerializeField] private bool _isPositionFollow = false;
    [SerializeField] private bool _isRotationFollow = false;
    [SerializeField] private Transform _target = null;


}
