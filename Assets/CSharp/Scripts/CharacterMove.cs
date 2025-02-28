using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMoveScript2 : CharacterContollerable
{
    private CharacterController _characterController = null;

    public override void CharacterTeleport(Vector3 position)
    {
        _characterController.Move(position);
    }

    public override void CharacterRootMove_Speed(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        Vector3 desiredMove = delta * similarities * ratio;

        _characterController.Move(desiredMove);

        Debug.Log("Moved" + desiredMove);

        _latestPlaneVelocityDontUseY = _characterController.velocity;
    }

    public override void SubScriptStart()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public override void MoverUpdate()
    {
        //throw new System.NotImplementedException();
    }

    public override void LookAt_Plane(Vector3 dir)
    {
        dir.y = 0;
        dir = dir.normalized;
        gameObject.transform.rotation = Quaternion.LookRotation(dir);
    }

    public override void ClearLatestVelocity()
    {
        if (_moveTriggerd == false)
        {
            _latestPlaneVelocityDontUseY = Vector3.zero;
        }
        _moveTriggerd = false;
    }

    public override bool GetIsInAir()
    {
        return !_characterController.isGrounded;
    }

    public override void CharacterRotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public override void CharacterRevive()
    {
        
    }

    public override void CharacterDie()
    {
        _characterController.includeLayers = (LayerMask.GetMask("StaticNavMeshLayer"));
    }

    public override void GravityUpdate() //매 프레임마다 호출될 함수니까
    {
        _gravitySpeed += new Vector3(0.0f, Time.deltaTime * Physics.gravity.y * _mass, 0.0f);

        _characterController.Move(_gravitySpeed);

        if (_characterController.isGrounded == true)
        {
            _gravitySpeed = Vector3.zero;
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

    public override void StateChanged() {}

    public override void CharacterRootMove(Vector3 delta, float similarities, float ratio)
    {
        _moveTriggerd = true;

        Vector3 desiredMove = delta * similarities * ratio;

        _characterController.Move(desiredMove);

        Debug.Log("Moved" + desiredMove);

        _latestPlaneVelocityDontUseY = _characterController.velocity;
    }

    public override void DoJump()
    {
        if (_characterController.isGrounded == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

        _gravitySpeed = new Vector3(0.0f, _jumpForce, 0.0f);

        //_characterController.Move(new Vector3(0.0f, 0.1f, 0.0f));
    }



    public override void DoKnuckBack()
    {
        if (_characterController.isGrounded == false)
        {
            return; //더블 점프 컨텐츠, 스킬 생기면 어떻게할꺼야
        }

        Vector3 myForward = transform.forward;
        Vector3 myBackward = Quaternion.AngleAxis(180, transform.right) * myForward;
        _gravitySpeed = new Vector3(0.0f, _jumpForce/2.0f, 0.0f) + myBackward;

        //_characterController.Move(new Vector3(0.0f, 0.1f, 0.0f));
    }


    public override void CharacterInertiaMove(float ratio)
    {
        Vector3 planeVelocity = _latestPlaneVelocityDontUseY;
        planeVelocity.y = 0.0f;

        _characterController.Move(planeVelocity * Time.deltaTime * ratio);
        _moveTriggerd = true;
    }

    public override void CharacterMove(Vector3 inputDirection, float similarities, float ratio)
    {
        _moveTriggerd = true;

        Vector3 desiredMove = inputDirection * _owner.GCST<StatScript>().GetPassiveStat(LevelStatAsset.PassiveStat.MoveSpeed) * Time.deltaTime * similarities * ratio;

        _characterController.Move(desiredMove);

        _latestPlaneVelocityDontUseY = _characterController.velocity;
    }


    public override void CharacterRotate(Vector3 inputDirection, float ratio)
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

    public override void CharacterRotateDirectly(Quaternion rotation)
    {
        CharacterRotate(rotation);
    }
}