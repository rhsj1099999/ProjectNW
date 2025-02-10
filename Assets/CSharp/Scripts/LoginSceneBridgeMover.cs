using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoginSceneBridgeMover : MonoBehaviour
{
    public enum LoginSceneState
    {
        Moving,
        StoppedAndSteady,
        SceneChangeReady,
        End,
    }



    [SerializeField] private float _eachModelOffset = 64.0f;
    [SerializeField] private float _deAccelTime = 2.0f;

    [SerializeField] private List<GameObject> _bridgePrefabs = new List<GameObject>();
    [SerializeField] private List<Vector3> _bridgeLocalPositions = new List<Vector3>();
    [SerializeField] private Vector3 _anchoredPositionOffset = Vector3.zero;
    [SerializeField] private float _liftOffAnchoredStartHeight = -15.0f;
    [SerializeField] private float _liftOffAnchoredHightLimit = 0.0f;
    [SerializeField] private float _liftCutLine = 10.0f;

    private Vector3 _myAnchoredPosition = Vector3.zero;
    private float _myAnchoredHeightLimit = 0.0f;
    private float _deAccel = 0.0f;

    private LoginSceneState _state = LoginSceneState.Moving;
    private bool _isBusy = false;

    [SerializeField] private GameObject _doorSpawnPosition = null;
    [SerializeField] private GameObject _doorPrefab = null;
    [SerializeField] private LoginSceneConstructMover _construct = null;




    [SerializeField] private float _liftSpeed = 1.0f;
    [SerializeField] private float _moveSpeed = 1.0f;

    private bool _isMoving = true;

    private List<float> _eachMoveDistanceAcc = new List<float>();

    private List<GameObject> _createdObjects = new List<GameObject>();

    private GameObject _createdDoor = null;


    private void Awake()
    {
        foreach (GameObject prefab in _bridgePrefabs)
        {
            GameObject newGameObject = Instantiate(prefab,transform);
            _bridgeLocalPositions.Add(newGameObject.transform.localPosition);
            _createdObjects.Add(newGameObject);
            _eachMoveDistanceAcc.Add(0.0f);
        }

        _myAnchoredPosition = transform.position + _anchoredPositionOffset;
        _myAnchoredHeightLimit = transform.position.y + _liftOffAnchoredHightLimit;
        
    }


    private IEnumerator StopMovingCoroutine()
    {
        _isBusy = true;

        _deAccel = _moveSpeed / _deAccelTime;

        while (true)
        {
            _moveSpeed -= _deAccel * Time.deltaTime;
            if (_moveSpeed <= 0)
            {
                _moveSpeed = 0.0f;

                _createdDoor = Instantiate(_doorPrefab, _doorSpawnPosition.transform);
                SceneOpenDoorScript doorScript = _createdDoor.GetComponent<SceneOpenDoorScript>();
                doorScript.DoorCall();

                _isBusy = false;
                _state = LoginSceneState.StoppedAndSteady;


                _isMoving = false;
                break;
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) == true && _isBusy == false)
        {
            switch (_state)
            {
                case LoginSceneState.Moving:
                    {
                        StartCoroutine(StopMovingCoroutine());
                        _construct.StopMoving();
                    }
                    break;

                case LoginSceneState.StoppedAndSteady:
                    {
                        SceneOpenDoorScript doorScript = _createdDoor.GetComponent<SceneOpenDoorScript>();
                        doorScript.SetTargetStage("StageScene_Vil");
                        doorScript.DoorCall();
                        _state = LoginSceneState.SceneChangeReady;
                    }
                    break;

                case LoginSceneState.SceneChangeReady:
                    {
                        SceneOpenDoorScript doorScript = _createdDoor.GetComponent<SceneOpenDoorScript>();
                        doorScript.SceneChange(true);
                        _state = LoginSceneState.End;
                    }
                    break;


                default:
                    break;
            }

        }

        {
            /*-------------------------------------------------------
            |NOTI| 인스펙터에서 조정하면서 볼려면 이거 주석해제하세요
            -------------------------------------------------------*/
            //_myAnchoredPosition = transform.position + _anchoredPositionOffset;
            //_myAnchoredHeightLimit = transform.position.y + _liftOffAnchoredHightLimit;
        }

        float moveDelta = Time.deltaTime * _moveSpeed;
        float liftDelta = Time.deltaTime * _liftSpeed;

        int index = 0;
        foreach (var createdObject in _createdObjects)
        {
            BridgeLift(index, liftDelta);
            BridgeMove(index, moveDelta);
            index++;
        }
    }

    private void BridgeLift(int index, float liftDelta)
    {
        GameObject createdObject = _createdObjects[index];

        if (createdObject.transform.position.y > _myAnchoredHeightLimit)
        {
            return;
        }

        if (_isMoving == false)
        {
            float distance_Y = Mathf.Abs(createdObject.transform.position.y - _myAnchoredHeightLimit);
            if (distance_Y >= _liftCutLine)
            {
                return;
            }
        }

        Vector3 newHeightPosition = createdObject.transform.position + new Vector3(0.0f, liftDelta, 0.0f);

        if (newHeightPosition.y > _myAnchoredHeightLimit)
        {
            newHeightPosition.y = _myAnchoredHeightLimit;
        }

        createdObject.transform.position = newHeightPosition;
    }

    private void BridgeMove(int index, float moveDelta)
    {
        if (_isMoving == false)
        {
            return;
        }

        GameObject createdObject = _createdObjects[index];

        Vector3 moveDeltaVector = new Vector3(0.0f, 0.0f, moveDelta);

        createdObject.transform.position -= moveDeltaVector;

        _eachMoveDistanceAcc[index] += Mathf.Abs(moveDelta);

        Vector3 toMyVector = (_myAnchoredPosition - createdObject.transform.position);
        Vector3 toMyDir = toMyVector.normalized;

        if (Vector3.Dot(transform.forward, toMyDir) <= 0.0f)
        {
            return;
        }

        float overDistanceSecond = Mathf.Abs(toMyVector.z) / Mathf.Abs(_moveSpeed);

        float compensateHeight = (overDistanceSecond * _liftSpeed);

        Vector3 newPosition = createdObject.transform.position + new Vector3(0.0f, 0.0f, _eachModelOffset);
        newPosition.y = transform.position.y + _liftOffAnchoredStartHeight + compensateHeight;
        if (newPosition.y > _myAnchoredHeightLimit)
        {
            newPosition.y = _myAnchoredHeightLimit;
        }

        createdObject.transform.position = newPosition;
    }
}
