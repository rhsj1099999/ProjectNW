using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class InAirCheckColliderScript : MonoBehaviour
{
}

//public class InAirCheckColliderScript : GameCharacterSubScript
//{
//    private bool _inAir = false;
//    public bool GetInAir() { return _inAir; }

//    private CapsuleCollider _capsuleCollider = null;
//    private Vector3 _localEndPosition = Vector3.zero;

//    public override void Init(CharacterScript owner)
//    {
//        _owner = owner;
//        _myType = typeof(InAirCheckColliderScript);
//        _capsuleCollider = GetComponent<CapsuleCollider>();
//    }

//    public override void SubScriptStart() 
//    {
//        CharacterController ownerCharacterController = _owner.GCST<CharacterController>();

//        _capsuleCollider.height = ownerCharacterController.height;
//        _capsuleCollider.radius = ownerCharacterController.radius;

//        //Vector3 ownerCharacterColliderCenter = ownerCharacterController.center;
//        //ownerCharacterColliderCenter.y -= ownerCharacterController.skinWidth * 5.0f;
//        //_capsuleCollider.center = ownerCharacterColliderCenter;

//        _capsuleCollider.includeLayers = ownerCharacterController.includeLayers;
//        _capsuleCollider.excludeLayers = ownerCharacterController.excludeLayers;

//        float bottomY = _capsuleCollider.center.y - (_capsuleCollider.height / 2.0f);
//        _localEndPosition = new Vector3(_capsuleCollider.center.x, bottomY, _capsuleCollider.center.z);
//    }

//    public Vector3 GetFootEndPosition()
//    {
//        return _localEndPosition;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        _inAir = false;
//    }

//    private void OnTriggerStay(Collider other)
//    {
//        _inAir = false;    
//    }

//    private void Update()
//    {
//        //if (_inAir == true)
//        //{
//        //    Debug.Log("InAir");
//        //}
//        //else
//        //{
//        //    Debug.Log("OffAir");
//        //}
//    }


//    private void OnTriggerExit(Collider other)
//    {
//        _inAir = true;
//    }
//}
