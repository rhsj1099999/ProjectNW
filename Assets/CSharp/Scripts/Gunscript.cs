using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //_isAim = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_followingTransformStartPoint != null)
        {
            transform.position = _followingTransformStartPoint.position;
            transform.rotation = _followingTransformStartPoint.rotation;
        }
    }

    void LateUpdate()
    {
        //if (_followingTransformStartPoint != null && _followingTransformEndPoint != null)
        //{
        //    transform.position = _followingTransformStartPoint.position;

        //    Quaternion lookQuaternion = Quaternion.LookRotation(_followingTransformEndPoint.position - _followingTransformStartPoint.position);
        //    transform.rotation = lookQuaternion;
        //}


        //if (_followingTransformStartPoint != null)
        //{
        //    transform.position = _followingTransformStartPoint.position;
        //    transform.rotation = _followingTransformStartPoint.rotation;
        //}

        if (_lookAtObject != null && _isAim == true)
        {
            //transform.LookAt(_lookAtObject.position);
        }
    }

    [SerializeField] private Transform _followingTransformStartPoint = null;
    [SerializeField] private Transform _followingTransformEndPoint = null;
    [SerializeField] private Transform _lookAtObject = null;

    public bool _isAim { get; set; }

}
