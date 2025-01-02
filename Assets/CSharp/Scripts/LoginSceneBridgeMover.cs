using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoginSceneBridgeMover : MonoBehaviour
{
    [SerializeField] private float _eachModelOffset = 64.0f;

    [SerializeField] private List<GameObject> _bridgePrefabs = new List<GameObject>();
    [SerializeField] private List<Vector3> _bridgeLocalPositions = new List<Vector3>();
    [SerializeField] private Vector3 _anchoredPositionOffset = Vector3.zero;
    [SerializeField] private float _liftOffAnchoredStartHeight = -15.0f;
    [SerializeField] private float _liftOffAnchoredHightLimit = 0.0f;


    [SerializeField] private float _liftSpeed = 1.0f;
    [SerializeField] private float _moveSpeed = 1.0f;

    private List<float> _eachMoveDistanceAcc = new List<float>();

    private List<GameObject> _createdObjects = new List<GameObject>();

    private void Awake()
    {
        foreach (GameObject prefab in _bridgePrefabs)
        {
            GameObject newGameObject = Instantiate(prefab,transform);
            _bridgeLocalPositions.Add(newGameObject.transform.localPosition);
            _createdObjects.Add(newGameObject);
            _eachMoveDistanceAcc.Add(0.0f);
        }
    }

    private void Update()
    {
        float moveDelta = Time.deltaTime * _moveSpeed;

        int index = 0;

        Vector3 myAnchoredPosition = transform.position + _anchoredPositionOffset;
        float anchoredHeightLimit = transform.position.y + _liftOffAnchoredHightLimit;

        foreach (var createdObject in _createdObjects)
        {
            if (createdObject.transform.position.y <= anchoredHeightLimit)
            {
                float liftDelta = Time.deltaTime * _liftSpeed;
                Vector3 newHeightPosition = createdObject.transform.position + new Vector3(0.0f, liftDelta, 0.0f);

                if (newHeightPosition.y > anchoredHeightLimit)
                {
                    newHeightPosition.y = anchoredHeightLimit;
                }

                createdObject.transform.position = newHeightPosition;
            }



            Vector3 moveDeltaVector = new Vector3(0.0f, 0.0f, moveDelta);

            createdObject.transform.position -= moveDeltaVector;

            _eachMoveDistanceAcc[index] += Mathf.Abs(moveDelta);

            Vector3 toMyVector = (myAnchoredPosition - createdObject.transform.position);
            Vector3 toMyDir = toMyVector.normalized;

            if (Vector3.Dot(transform.forward, toMyDir) > 0.0f)
            {
                //첫 시작 프레임에 넘어간 오브젝트가 여러개가 있다
                // = 얼마나 넘어갔습니까의 float이 필요하다.

                float overDistanceSecond = Mathf.Abs(toMyVector.z) / Mathf.Abs(_moveSpeed);

                float compensateHeight = (overDistanceSecond * _liftSpeed);




                Vector3 newPosition = createdObject.transform.position + new Vector3(0.0f, 0.0f, _eachModelOffset);
                newPosition.y = transform.position.y + _liftOffAnchoredStartHeight + compensateHeight;
                if (newPosition.y > anchoredHeightLimit)
                {
                    newPosition.y = anchoredHeightLimit;
                }

                createdObject.transform.position = newPosition;
            }

            index++;
        }
    }
}
