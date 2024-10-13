using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gunscript : MonoBehaviour
{
    [SerializeField] private Transform _followingTransformStartPoint = null;

    /*-------------------------------------------------------
    |TODO| = GunFire관련은 플레이어가 들고있는것과는 관련이 없나?
    -------------------------------------------------------*/
    //GunFire관련 변수, 함수들
    ///////////////
    /////////
    //////
    /// 
    [SerializeField] private float _coolTime = 0.0f;
    [SerializeField] private float _coolTimeOriginal = 0.5f;
    [SerializeField] private bool _isAutomatic = false;
    [SerializeField] private GameObject _bullet = null;
    [SerializeField] private GameObject _aimObject = null;
    [SerializeField] private SightAttachScript _sightAttachScript = null;

    [SerializeField] private Vector3 _posDampRef = Vector3.zero;
    [SerializeField] private Quaternion _quaternionDampRef = Quaternion.identity;
    [SerializeField] private float _dampingTime = 0.01f;

    [SerializeField] private Vector2 _absAimShakeForce = Vector2.zero;
    [SerializeField] private float _absAimShakeForceXMin = 0.1f;
    [SerializeField] private float _absAimShakeForceXMax = 0.3f;
    [SerializeField] private float _absAimShakeForceYMin = 0.1f;
    [SerializeField] private float _absAimShakeForceYMax = 0.3f;
    [SerializeField] private float _absAimShakeForcef = 3.0f;
    [SerializeField] private float _aimShakeDuration = 0.0f;
    [SerializeField] private float _aimDampingForce = 0.0f;
    [SerializeField] private float _aimShakeDurationOriginal = 0.1f;
    [SerializeField] UnityEvent _whenShootEvent = null;

    private Vector2 _calculatedAimShakeForce = new Vector2();
    private Vector2 _aimShakeDampRef = new Vector2();

    private AimScript _aimScript = null;

    private void Awake()
    {
        _aimScript = _aimObject.GetComponent<AimScript>();
        Debug.Assert( _aimScript != null, "Gun은 AimScrip가 반드시 있어야만 한다");
    }
    public void Fire()
    {
        //Do RayCast
    }


    public void FireCheck()
    {
        //마우스를 클릭하면 총알 발사
        if (Input.GetKey(KeyCode.Mouse0) == true && _coolTime < float.Epsilon)
        {
            Fire();

            _coolTime = _coolTimeOriginal;
            
            StartCoroutine("AimShakeCoroutine");

            StartCoroutine("CooltimeCoroutine");

            _whenShootEvent.Invoke();
        }
    }

    public IEnumerator AimRestoreCoroutine()
    {
        return null;
    }

    public IEnumerator AimShakeCoroutine()
    {

        {

            _calculatedAimShakeForce = _absAimShakeForce;

            float randomDegree = Random.Range(0.0f, 3.14f);
            int isNeg = Random.Range(0, 2);
            float xShake = Random.Range(_absAimShakeForceXMin, _absAimShakeForceXMax);
            if (isNeg == 1)
            {
                xShake *= -1.0f;
            }
            _calculatedAimShakeForce = new Vector2
            (
                xShake,
                Random.Range(_absAimShakeForceYMin, _absAimShakeForceYMax)
            );
        }


        _aimShakeDuration = _aimShakeDurationOriginal;
        while (true)
        {
            _aimShakeDuration -= Time.deltaTime;
            if (_aimShakeDuration < float.Epsilon)
            {
                _aimShakeDuration = 0.0f;
                break;
            }
            _calculatedAimShakeForce = Vector2.SmoothDamp(_calculatedAimShakeForce, Vector2.zero, ref _aimShakeDampRef, _aimShakeDurationOriginal);
            _aimScript.AimRotation(_calculatedAimShakeForce);

            yield return null;
        }
    }

    public IEnumerator CooltimeCoroutine()
    {
        while (true) 
        {
            _coolTime -= Time.deltaTime;

            if (_coolTime <= float.Epsilon)
            {
                _coolTime = 0.0f;
                break;
            }

            yield return null;
        }
    }
    ///
    //////
    /////////
    ///////////////



    private void FixedUpdate()
    {
        
    }


    void Update()
    {
        FireCheck();
    }

    private void LateUpdate()
    {
        //if (_followingTransformStartPoint != null)
        //{
        //    transform.position = _followingTransformStartPoint.position;
        //    transform.rotation = _followingTransformStartPoint.rotation;
        //}

        //스무스 댐핑으로 바꾸기
        if (_followingTransformStartPoint != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, _followingTransformStartPoint.position, ref _posDampRef, _dampingTime);
            transform.rotation = _followingTransformStartPoint.rotation;
        }
    }

}
