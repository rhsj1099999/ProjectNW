using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAimDragger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_camera == null ||
           _aimTarget == null)
        {
            //Crash;
            return;
        }


        //_aimTarget.transform.position = _camera.transform.position + _camera.transform.forward * _distance;
        _aimTarget.transform.position = Camera.main.transform.position + Camera.main.transform.forward * _distance;
    }

    [SerializeField] private CinemachineFreeLook _camera = null;
    [SerializeField] private GameObject _aimTarget = null;
    [SerializeField] private float _distance = 1000.0f;
}
