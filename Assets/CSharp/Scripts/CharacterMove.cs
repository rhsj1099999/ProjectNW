using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMoveScript2 : GameCharacterSubScript
{
    [SerializeField] private bool _logMe = false;

    //[SerializeField] private float _mass = 30.0f;
    //[SerializeField] private float _inAirThreshould = 0.05f;
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotatingSpeed_DEG = 90.0f;
    [SerializeField] private float _jumpForce = 3.0f;
    [SerializeField] private GameObject _debuggingMesh = null;
    [SerializeField] private GameObject _debuggingMesh2 = null;
    [SerializeField] private GameObject _debuggingMesh3 = null;
    private List<GameObject> _created = new List<GameObject>();
    private Vector3 _afterMove = Vector3.zero;
    private Vector3 _beforeMove = Vector3.zero;
    private bool _collisionTrigger = false;
    private int _onTriggeerCount = 0;
    private bool _slopeLimitFreeze = false;







    private Vector3 _moveDir;
    [SerializeField] bool _checkAll = false;
    private float _stepOffsetOriginal = 0.0f;
    

    private Vector3 _latestPlaneVelocityDontUseY = Vector3.zero;

    private bool _isJumping = false;
    private bool _isInAir = false;
    private float _verticalSpeedAcc = 0.0f;
    private bool _moveTriggerd = false;

    private float _inAirSlopeLimit = 0.0f;
    private float _inGroundSlopeLimit = 45.0f;
    private float _inAirStepOffset = 0.0f;
    private float _inGroundStepOffset = 0.3f;




    public bool GetIsJumping() { return _isJumping; }
    public bool GetIsInAir() { return _isInAir; }
    public Vector3 GetLatestVelocity() { return _latestPlaneVelocityDontUseY; }
    public void ResetLatestVelocity() { _latestPlaneVelocityDontUseY = Vector3.zero; }

    public override void Init(CharacterScript owner)
    {
        _owner = owner;
        _myType = typeof(CharacterMoveScript2);
    }

    public override void SubScriptStart()
    {
        CharacterController ownerCharacterController = _owner.GCST<CharacterController>();
        _stepOffsetOriginal = ownerCharacterController.stepOffset;
    }

    public void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            _latestPlaneVelocityDontUseY = Vector3.zero;
        }
        _moveTriggerd = false;
    }

    private void Update()
    {
        if (_collisionTrigger == true)
        {
            _collisionTrigger = false;
            Vector3 afterCollisionTriggerPosition = transform.position;
        }

        if (Input.GetKey(KeyCode.U) == true)
        {
            foreach (var item in _created)
            {
                Destroy(item);
            }
            _created.Clear();


            //transform.position = new Vector3(11.0f, 0.3f, 16.0f);

            //_owner.GCST<CharacterController>().Move(new Vector3(0.0f, -1.0f, 0.0f));
        }

        if (Input.GetKeyDown(KeyCode.P) == true)
        {
            Vector3 characterInputDir = Vector3.forward;
            characterInputDir = GetDirectionConvertedByCamera(characterInputDir);
            CharacterMove(characterInputDir, 1.0f);
        }
    }

    
    private void CustomSlopeLimit(ControllerColliderHit hit)
    {
        //올라가지 않았다면 종료
        //if (_beforeMove.y <= transform.position.y)
        //{
        //    return;
        //}
        
        //지면이 아니라면 종료
        if ((1 << hit.collider.gameObject.layer) != LayerMask.GetMask("StaticNavMeshLayer"))
        {
            return;
        }

        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(Vector3.up, hit.normal), -1.0f, 1.0f)) * Mathf.Rad2Deg;

        //올라갈 수 있는 각도라면 종료
        if (angle < 30.0f)
        {
            return;
        }
        RaycastHit rayHit;

        bool rayRet = Physics.Raycast(transform.position, transform.forward, out rayHit);

        if (rayRet == false)
        {
            return;
        }

        if (rayHit.collider != hit.collider)
        {
            return;
        }

        _slopeLimitFreeze = true;
        //transform.position = _beforeMove;
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _onTriggeerCount++;

        CustomSlopeLimit(hit);


        //float yDelta = transform.position.y - _beforeMove.y;

        //if (_moveTriggerd == true &&
        //    yDelta > 0.1f)
        //{
        //    Debug.Break();
        //}








        //{
        //    //구체 생성
        //    GameObject hitObject = Instantiate(_debuggingMesh);
        //    hitObject.transform.position = hit.point;
        //    _created.Add(hitObject);
        //    Debug.Log(hit.normal);

        //    //이쑤시개 생성
        //    GameObject hitObject_Thin = Instantiate(_debuggingMesh);
        //    hitObject_Thin.transform.position = hit.point;
        //    hitObject_Thin.transform.LookAt(hit.point + hit.normal);
        //    _created.Add(hitObject_Thin);
        //    hitObject_Thin.transform.localScale = new Vector3(0.3f, 0.3f, 5.0f);
        //}
    }



    public void GravityUpdate() //매 프레임마다 호출될 함수니까
    {
        
        //if (_owner.GCST<InAirCheckColliderScript>().GetInAir() == false)
        //{
        //    _verticalSpeedAcc = 0.0f;
        //    return;
        //}

        //{
        //    if (_owner.GCST<CharacterController>().isGrounded == true)
        //    {
        //        _verticalSpeedAcc = 0.0f;
        //        return;
        //    }

        //    _verticalSpeedAcc += Time.deltaTime * Physics.gravity.y;

        //    Vector3 gravityMove = Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        //    Debug.Assert(Mathf.Abs(gravityMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

        //    _owner.GCST<CharacterController>().Move(gravityMove);
        //}

        if (_owner.GCST<CharacterController>().isGrounded == true)
        {
            _verticalSpeedAcc = 0.0f;
            return;
        }

        _verticalSpeedAcc += Time.deltaTime * Physics.gravity.y;

        Vector3 gravityMove = Vector3.up * _verticalSpeedAcc * Time.deltaTime;

        Debug.Assert(Mathf.Abs(gravityMove.y) >= float.Epsilon, "부유하는 움직임입니다.");

        _owner.GCST<CharacterController>().Move(gravityMove);


        if (_owner.GCST<CharacterController>().isGrounded == true)
        {
            //_owner.GCST<CharacterController>().stepOffset = _inGroundStepOffset;
            //_owner.GCST<CharacterController>().slopeLimit = _inGroundSlopeLimit;

            _verticalSpeedAcc = 0.0f;

            _isJumping = false;
            _isInAir = false;
        }
        else
        {
            //_owner.GCST<CharacterController>().stepOffset = _inAirStepOffset;
            //_owner.GCST<CharacterController>().slopeLimit = _inAirSlopeLimit;

            _isInAir = true;
        }

        //if (_logMe == true)
        //{
        //    if (_isInAir == true)
        //    {
        //        Debug.Log("InAir");
        //    }
        //    else
        //    {
        //        Debug.Log("InGround");
        //    }
        //}
    }

    public void DoJump()
    {
        if (_owner.GCST<CharacterController>().isGrounded == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

        //_owner.GCST<CharacterController>().stepOffset = _inAirStepOffset;
        //_owner.GCST<CharacterController>().slopeLimit = _inAirSlopeLimit;

        _verticalSpeedAcc = _jumpForce;

        //_notGroundedCount = 0;
        _isJumping = true;
        _isInAir = true;
    }

    public void CharacterMove_NoSimilarity(Vector3 inputDirection, float ratio = 1.0f)
    {
        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * ratio;

        _owner.GCST<CharacterController>().Move(desiredMove);
        _moveTriggerd = true;

        _latestPlaneVelocityDontUseY = _owner.GCST<CharacterController>().velocity;
    }



    public void HandleStepOffset()
    {
        CharacterController ownerController = _owner.GCST<CharacterController>();

        Vector3 moveDirXZ = new(_moveDir.x, 0, _moveDir.z);
        Vector3 normalizedMoveDirXZ = moveDirXZ.normalized;

        //Distance for raycast, to detect objects closest to moveDirection
        float distance = ownerController.radius + ownerController.skinWidth;
        //Position of player's ground level
        Vector3 bottom = transform.position - new Vector3(0f, ownerController.height / 2f - ownerController.center.y, 0f);
        //Position of player's ground level + StepOffset
        Vector3 bottomWithStepOffset = new(bottom.x, bottom.y + _stepOffsetOriginal, bottom.z);
        //Raycast at player's ground level in direction of movement
        bool bottomRaycast = Physics.Raycast(bottom, normalizedMoveDirXZ, out _, distance);
        //Raycast at player's ground level + StepOffset in direction of movement
        bool bottomWithStepOffsetRaycast = Physics.Raycast(bottomWithStepOffset, normalizedMoveDirXZ, out _, distance);
        if (bottomRaycast && bottomWithStepOffsetRaycast)
        {
            //Wall in move direction
            //Block stepping over object
            ownerController.stepOffset = 0;
        }
        else if (bottomRaycast && !bottomWithStepOffsetRaycast)
        {
            //Step in move direction
            //Allow stepping over object
            ownerController.stepOffset = _stepOffsetOriginal;
        }
        else
        {
            //Nothing in move direction
            //Block stepping over object
            ownerController.stepOffset = 0;
        }
    }


    public void CharacterMove(Vector3 inputDirection, float ratio = 1.0f)
    {
        _moveTriggerd = true;

        float similarities = Mathf.Clamp(Vector3.Dot(transform.forward, inputDirection), 0.0f, 1.0f);

        Vector3 desiredMove = inputDirection * _speed * Time.deltaTime * similarities * ratio;

        

        {
            //Simple Move
            _beforeMove = transform.position;

            //_onTriggeerCount = 0;
            _owner.GCST<CharacterController>().Move(desiredMove);

            //if (_slopeLimitFreeze == true)
            //{
            //    transform.position = _beforeMove;
            //    _slopeLimitFreeze = false;
            //}
            //Debug.Log("OnTriggerCount = " + _onTriggeerCount);

            Vector3 movedDir = _owner.GCST<CharacterController>().velocity;
            _moveDir = movedDir.normalized;
            _latestPlaneVelocityDontUseY = movedDir;

            /*----------------------------------------
            움직인 직후의 포지션
            ----------------------------------------*/
            _afterMove = transform.position;

            //if (_moveDir.y >= 0.01f) 
            //{
            //    Debug.Log("경사면을 올라갔다" + _moveDir.x + "//" + _moveDir.y + "//" + _moveDir.z + "//");
            //}
        }
    }

    public Vector3 GetDirectionConvertedByCamera(Vector3 inputDirection)
    {
        Vector3 cameraLook = Camera.main.transform.forward;
        cameraLook.y = 0.0f;
        cameraLook = cameraLook.normalized;
        return (Quaternion.LookRotation(cameraLook) * inputDirection);
    }

    public void CharacterForcedMove(Vector3 latestDelta, float ratio = 1.0f)
    {
        _owner.GCST<CharacterController>().Move(latestDelta * ratio * Time.deltaTime);
        _moveTriggerd = true;
    }


    public void CharacterRotate(Vector3 inputDirection, float ratio = 1.0f)
    {
        float deltaDEG = Vector3.Angle(transform.forward.normalized, inputDirection);

        if (deltaDEG > 180.0f)
        {
            deltaDEG -= 180.0f;
        }

        float nextDeltaDEG = _rotatingSpeed_DEG * Time.deltaTime * ratio;

        if (nextDeltaDEG >= deltaDEG)
        {
            transform.LookAt(transform.position + inputDirection);
            return;
        }
        else
        {
            float isLeftRotate = Vector3.Cross(transform.forward.normalized, inputDirection).y;
            if (isLeftRotate <= 0.0f)
            {
                nextDeltaDEG *= -1.0f;
            }
            transform.Rotate(transform.up, nextDeltaDEG);
            return;
        }
    }
}