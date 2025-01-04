using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSceneConstructMover : MonoBehaviour
{
    [SerializeField] private Vector3 _speed = Vector3.zero;
    [SerializeField] private float _deAccelTime = 2.0f;
    private float _deAccel = 0.0f;

    [SerializeField] int _construtcPrefabCount = 5;

    [SerializeField] GameObject _construtcPrefab = null;
    [SerializeField] Vector3 _construtcPrefabOffset = Vector3.zero;
    [SerializeField] Vector3 _firstPositionOffset = Vector3.zero;
    [SerializeField] private bool _isMoving = true;

    private List<GameObject> _prefabCreated = new List<GameObject>();
    private Vector3 _lastPosition = Vector3.zero;

    private IEnumerator StopMovingCoroutine()
    {
        _deAccel = _speed.z / _deAccelTime;
        
        while (true) 
        {
            _speed.z -= _deAccel * Time.deltaTime;
            if (_speed.z >= 0)
            {
                _speed.z = 0.0f;
                _isMoving = false;
                break;
            }
            yield return null;
        }
    }


    public void StopMoving()
    {
        StartCoroutine(StopMovingCoroutine());
    }

    private void Awake()
    {
        if (_construtcPrefab == null)
        {
            Debug.Assert(false, "프리팹을 설정해주세요");
            Debug.Break();
        }

        for (int i = 0; i < _construtcPrefabCount; i++)
        {
            GameObject newObject = Instantiate(_construtcPrefab);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.position = (transform.position + _firstPositionOffset + _construtcPrefabOffset * i);
            newObject.transform.SetParent(transform);
            
            _prefabCreated.Add( newObject );

            if (i == _construtcPrefabCount - 1)
            {
                _lastPosition = newObject.transform.position;
            }
        }
    }


    void Update()
    {
        if (_isMoving)
        {
            foreach (GameObject createdObject in _prefabCreated)
            {
                Vector3 moveDelta = _speed * Time.deltaTime;
                createdObject.transform.position += moveDelta;

                Vector3 dirToMyPosition = ((transform.position + _firstPositionOffset) - createdObject.transform.position).normalized;
                Vector3 myLook = transform.forward;

                if (Vector3.Dot(myLook, dirToMyPosition) > 0.0f)
                {
                    
                    createdObject.transform.position = (transform.position + _firstPositionOffset + _construtcPrefabOffset * _construtcPrefabCount); ;
                }
            }
        }
    }
}
