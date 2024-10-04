using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunscript : MonoBehaviour
{
    void Update()
    {
        if (_followingTransformStartPoint != null)
        {
            transform.position = _followingTransformStartPoint.position;
            transform.rotation = _followingTransformStartPoint.rotation;
        }
    }


    [SerializeField] private Transform _followingTransformStartPoint = null;

    public bool _isAim { get; set; }

}
