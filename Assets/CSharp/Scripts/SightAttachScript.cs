using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightAttachScript : MonoBehaviour
{
    [SerializeField] private DampingFollower _cameraTarget = null;
    private Vector3 _prevPosition = Vector3.zero;
    private void Awake()
    {
        //Debug.Assert( _cameraTarget != null, "카메라가 null이여서는 안된다");
    }

    private void FixedUpdate()
    {
        
    }

    public void SightAttachUpdate()
    {
        //Vector3 deltaPosition = transform.position - _prevPosition;
        //_cameraTarget.HardLimitDrag(deltaPosition);
        //_cameraTarget.WhereThisFuncToLocateFuck2();
        //_prevPosition = transform.position;
    }
}
