/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class CustomCharacterController
{
}

//public class CustomCharacterController : GameCharacterSubScript
//{
//    [SerializeField] private float _speed = 5.0f;
//    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
//    [SerializeField] private float _jumpForce = 3.0f;
//    [SerializeField] private float _mass = 2.0f;
//    //[SerializeField] CapsuleCollider _myCollider = null;
//    private Rigidbody _rigidBody = null;
//    private RaycastHit _raycastHit = new RaycastHit();
//    //private const float _moveMinDistance = 0.001f;

//    public override void Init(CharacterScript owner)
//    {
//        _owner = owner;
//        _myType = typeof(CustomCharacterController);
//        _rigidBody = GetComponent<Rigidbody>();
//    }

//    public override void SubScriptStart() { }



//    private void GravityUpdate()
//    {

//    }


//    private void FixedUpdate()
//    {

//    }


//    private void Update()
//    {
//        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
//        Vector3 cameraLook = Camera.main.transform.forward;
//        cameraLook.y = 0.0f;
//        cameraLook = cameraLook.normalized;
//        Vector3 converted = (Quaternion.LookRotation(cameraLook) * characterInputDir);

//        if (converted.magnitude > 0.0f)
//        {
//            Rotate(converted);
//            Move(converted);
//        }
//    }




//    private void Rotate(Vector3 inputDirection, float ratio = 1.0f)
//    {
//        float deltaDEG = Vector3.Angle(transform.forward.normalized, inputDirection);

//        if (deltaDEG > 180.0f)
//        {
//            deltaDEG -= 180.0f;
//        }

//        float nextDeltaDEG = _rotatingSpeed_DEG * Time.deltaTime * ratio;

//        if (nextDeltaDEG >= deltaDEG)
//        {
//            transform.LookAt(transform.position + inputDirection);
//            return;
//        }
//        else
//        {
//            float isLeftRotate = Vector3.Cross(transform.forward.normalized, inputDirection).y;
//            if (isLeftRotate <= 0.0f)
//            {
//                nextDeltaDEG *= -1.0f;
//            }
//            transform.Rotate(transform.up, nextDeltaDEG);
//            return;
//        }
//    }



//    public void Move(Vector3 inputDirection, float ratio = 1.0f)
//    {
//        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);
//        float desiredDistance = _speed * Time.deltaTime * similarities * ratio;

//        _rigidBody.MovePosition(transform.position + inputDirection * desiredDistance);
//    }




//    private Vector3 Reflect_Original(ref Vector3 inDir, ref Vector3 inNormal)
//    {
//        return Vector3.Reflect(inDir, inNormal);
//    }

//    private Vector3 Reflect_GamePlane(Vector3 inDir, Vector3 inNormal)
//    {
//        Vector3 ret = Vector3.Reflect(inDir, inNormal);
//        ret.y = 0.0f;
//        return ret;
//    }


//}




////public class CustomCharacterController : GameCharacterSubScript
////{
////    [SerializeField] private float _speed = 5.0f;
////    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
////    [SerializeField] private float _jumpForce = 3.0f;
////    [SerializeField] private float _mass = 2.0f;
////    [SerializeField] CapsuleCollider _myCollider = null;
////    private RaycastHit _raycastHit = new RaycastHit();
////    private const float _moveMinDistance = 0.001f;

////    public override void Init(CharacterScript owner)
////    {
////        _owner = owner;
////        _myType = typeof(CustomCharacterController);

////        if (_myCollider == null)
////        {
////            Debug.Assert(false, "캡슐콜라이더를 지정하세요");
////            Debug.Break();
////        }
////    }

////    public override void SubScriptStart() {}



////    private void GravityUpdate()
////    {

////    }


////    private void FixedUpdate()
////    {

////    }


////    private void Update()
////    {
////        Vector3 characterInputDir = _owner.GCST<InputController>()._pr_directionByInput;
////        Vector3 cameraLook = Camera.main.transform.forward;
////        cameraLook.y = 0.0f;
////        cameraLook = cameraLook.normalized;
////        Vector3 converted = (Quaternion.LookRotation(cameraLook) * characterInputDir);

////        if (converted.magnitude > 0.0f)
////        {
////            //Rotate(converted);
////            Move(converted);
////        }

////        if (Input.GetKeyDown(KeyCode.B) == true)
////        {
////            Vector3 zeroPosition = transform.position;
////            zeroPosition.y = 0.0f;
////            transform.position = zeroPosition;
////        }

////        if (Input.GetKeyDown(KeyCode.N) == true)
////        {
////            Move(Vector3.down);
////        }

////    }


////    private float Move_Internal(ref Vector3 originalDesired, Vector3 moveDesiredDir, float distance, ref bool rayInterrupted)
////    {
////        Vector3 beforePosition = transform.position;

////        Vector3 point1 = beforePosition + _myCollider.center + Vector3.up * (_myCollider.height / 2 - _myCollider.radius);
////        Vector3 point2 = beforePosition + _myCollider.center - Vector3.up * (_myCollider.height / 2 - _myCollider.radius);

////        rayInterrupted = Physics.CapsuleCast(point1, point2, _myCollider.radius, moveDesiredDir, out _raycastHit, distance);

////        if (rayInterrupted)
////        {
////            distance = _raycastHit.distance - _myCollider.radius;

////            if (distance < 0f)
////            {
////                distance = 0f;
////            }
////        }


////        transform.position = transform.position + moveDesiredDir * distance * Vector3.Dot(originalDesired, moveDesiredDir);

////        float moved = Vector3.Distance(beforePosition, transform.position);

////        //transform.position = transform.position + moveDesiredDir * distance;
////        Debug.Log("Vel = " + (moved / Time.deltaTime));
////        return distance;
////    }




////    public void Move(Vector3 inputDirection, float ratio = 1.0f)
////    {
////        int moveCount = 0;

////        Vector3 desiredDir = inputDirection.normalized;

////        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, desiredDir), 0.0f, 1.0f);
////        float desiredDistance = _speed * Time.deltaTime * similarities * ratio;

////        bool rayInteruppted = false;

////        while (true) 
////        {
////            //강제 움직임 종료 코드----------------------

////            if (moveCount > 100)
////            {
////                Debug.Break();
////                Debug.Assert(false, "움직임 로직이 이상합니다_Step1");
////            }


////            if (moveCount > 200)
////            {
////                Debug.Break();
////                Debug.Assert(false, "움직임 로직이 이상합니다_Step2");
////                return;
////            }

////            float moved = Move_Internal(ref inputDirection, desiredDir, desiredDistance, ref rayInteruppted);

////            desiredDistance -= moved;

////            if (desiredDistance <= 0.0f)
////            {
////                return;
////            }

////            //---------------------------------------------------------------------------------
////            if (rayInteruppted == false) { return; } //방해가 없었다면 무조건 잘 갔다고 취급합니다.
////            //---------------------------------------------------------------------------------

////            desiredDir += Vector3.Reflect(desiredDir, _raycastHit.normal);
////            desiredDir = desiredDir.normalized;

////            moveCount++;
////        }
////    }




////    private Vector3 Reflect_Original(ref Vector3 inDir, ref Vector3 inNormal)
////    {
////        return Vector3.Reflect(inDir, inNormal);
////    }

////    private Vector3 Reflect_GamePlane(Vector3 inDir, Vector3 inNormal)
////    {
////        Vector3 ret = Vector3.Reflect(inDir, inNormal);
////        ret.y = 0.0f;
////        return ret;
////    }
////}



/////*
//// * 
//// * 
//// * 
//// * 

////        //if (castRet == true)
////        //{
////        //    transform.position = _raycastHit.point - inputDirection.normalized * _myCollider.radius;

////        //    Vector3 penDir = Vector3.zero;
////        //    float penDistance = 0.0f;

////        //    Physics.ComputePenetration
////        //    (
////        //        _myCollider,
////        //        _myCollider.transform.position,
////        //        _myCollider.transform.rotation,
////        //        _raycastHit.collider,
////        //        _raycastHit.collider.transform.position,
////        //        _raycastHit.collider.transform.rotation,
////        //        out penDir,
////        //        out penDistance
////        //    );

////        //    transform.position += penDir * penDistance;
////        //}






//// using System.Collections;
////using System.Collections.Generic;
////using Unity.Burst.CompilerServices;
////using UnityEngine;

////public class MoveScript : MonoBehaviour
////{
////    [SerializeField] private float _velocity = 3.0f;
////    [SerializeField] CapsuleCollider _myCollider = null;
////    [SerializeField] Collider _temp = null;

////    private void Awake()
////    {
////        _myCollider = GetComponent<CapsuleCollider>();
////    }

////    void Update()
////    {
////        bool moveTriggerd = false;

////        Vector3 beforePosition = transform.position;

////        if (Input.GetKey(KeyCode.W) == true)
////        {
////            transform.position += new Vector3(0.0f, 0.0f, 1.0f) * Time.deltaTime * _velocity;
////            moveTriggerd = true;
////        }
////        else if (Input.GetKey(KeyCode.S) == true)
////        {
////            transform.position += new Vector3(0.0f, 0.0f, -1.0f) * Time.deltaTime * _velocity;
////            moveTriggerd = true;
////        }

////        if (Input.GetKey(KeyCode.A) == true)
////        {
////            transform.position += new Vector3(-1.0f, 0.0f, 0.0f) * Time.deltaTime * _velocity;
////            moveTriggerd = true;
////        }
////        else if (Input.GetKey(KeyCode.D) == true)
////        {
////            transform.position += new Vector3(1.0f, 0.0f, 0.0f) * Time.deltaTime * _velocity;
////            moveTriggerd = true;
////        }

////        Vector3 afterPosition = transform.position;

////        if (moveTriggerd == true)
////        {
////            CapsuleCast(Vector3.Distance(beforePosition, afterPosition));
////        }
////    }

////    //private void OnCollisionEnter(Collision collision)
////    //{
////    //    Debug.Break();
////    //    Debug.Log("Collision");
////    //}

////    //private void OnTriggerEnter(Collider other)
////    //{
////    //    Debug.Break();
////    //    Debug.Log("Trigger");
////    //}


////    private void CapsuleCast(float distance)
////    {
////        Vector3 point1 = transform.position + _myCollider.center + Vector3.up * (_myCollider.height / 2 - _myCollider.radius);
////        Vector3 point2 = transform.position + _myCollider.center - Vector3.up * (_myCollider.height / 2 - _myCollider.radius);

////        // Sweep 방향
////        Vector3 direction = transform.forward;

////        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, _myCollider.radius, direction, distance, ~0);

////        //if (hits.Length <= 0)
////        //{
////        //    return;
////        //}

////        //foreach (RaycastHit hit in hits)
////        //{
////        //    if (hit.collider == this)
////        //    {
////        //        continue;
////        //    }
////        //}

////        Vector3 penDir;
////        float penDistance;
////        bool ret = Physics.ComputePenetration(_myCollider, _myCollider.transform.position, _myCollider.transform.rotation, _temp, _temp.transform.position, _temp.transform.rotation, out penDir, out penDistance);

////        if (ret == false) 
////        {
////            return;
////        }

////        _myCollider.transform.position += penDir * penDistance;


////    }




////}


//// */