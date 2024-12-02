using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Gunscript2 : WeaponScript
{
    /*------------------------------------------
    Component Section.
    ------------------------------------------*/
    [SerializeField]  private GameObject _firePosition = null;
    [SerializeField]  private GameObject _stockPosition = null;
    [SerializeField] UnityEvent _whenShootEvent = null;

    private AimScript2 _aimScript = null;

    // private GameObject _bullet = null;
    //[SerializeField] private Transform _followingTransformStartPoint = null;


    /*------------------------------------------
    Spec Section.
    ------------------------------------------*/
    [SerializeField] private bool _isAutomatic = false;
    [SerializeField] private float _coolTimeOriginal = 0.5f;
    [SerializeField] private float _dampingTime = 0.01f;
    [SerializeField] private float _aimShakeDurationOriginal = 0.1f;
    [SerializeField] private Vector2 _absAimShakeForceMin = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 _absAimShakeForceMax = new Vector2(0.3f, 0.3f);





    /*------------------------------------------
    런타임중 정보저장용 변수들
    ------------------------------------------*/
    private bool _isAimming = false;
    private float _coolTime = 0.0f;
    private float _aimShakeDuration = 0.0f;
    private Vector2 _aimShakeDampRef = new Vector2();
    private Vector2 _calculatedAimShakeForce = new Vector2();
    protected Transform _shoulderStock_Unity = null;





    void Update()
    {
        FireCheck();
    }






    public override void FollowSocketTransform()
    {
        if (_isAimming == true)
        {
            Vector3 deltaPosition = (_shoulderStock_Unity.position - _stockPosition.transform.position);
            transform.position += deltaPosition;

            Vector3 aimDir = (_aimScript.GetAimSatellite().transform.position - _shoulderStock_Unity.position);
            transform.forward = aimDir.normalized;
        }
        else
        {
            base.FollowSocketTransform();
        }
    }


    override public void Equip(PlayerScript itemOwner, Transform followTransform) 
    {
        base.Equip(itemOwner, followTransform);
        _shoulderStock_Unity = _ownerAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
        _aimScript = itemOwner.gameObject.GetComponent<AimScript2>();

        Debug.Assert(_aimScript != null, "Gun은 AimScrip가 반드시 있어야만 한다");
        Debug.Assert(_firePosition != null, "발사할곳이 없는데 이게 총입니까?");
        Debug.Assert(_stockPosition != null, "자세제어를 위해 견착위치가 필요합니다(권총도 마찬가지)");
        Debug.Assert(_ownerIKSkript != null, "Gun은 IK를 이용해야만 합니다");
    }

    override public void UnEquip()
    {
    }





    public void Fire()
    {
        //Do RayCast
    }

    public void FireCheck()
    {
        if (Input.GetKey(KeyCode.Mouse0) == true && _coolTime < float.Epsilon)
        {
            Fire();

            RayCheck();

            _coolTime = _coolTimeOriginal;
            
            StartCoroutine("AimShakeCoroutine");

            StartCoroutine("CooltimeCoroutine");

            _whenShootEvent.Invoke();
        }
    }

    private void RayCheck()
    {
        RaycastHit hit;

        int targetLayer = 
            (1 << LayerMask.NameToLayer("StaticNavMeshLayer")) |
            (1 << LayerMask.NameToLayer("HitCollider"));

        if (Physics.Raycast(_firePosition.transform.position, _firePosition.transform.forward, out hit, Mathf.Infinity, targetLayer) == false)
        {
            return;
        }

        IHitable hitable = hit.collider.gameObject.GetComponentInParent<IHitable>();

        if (hitable == null)
        {
            return;
        }

        //hitable.DealMe(1, gameObject);
    }

    public IEnumerator AimShakeCoroutine()
    {
        //에임 흔들림 정도 결정하기
        {
            float xShake = Random.Range(_absAimShakeForceMin.x, _absAimShakeForceMax.x);
            if (Random.Range(0, 2) == 1) { xShake *= -1.0f; }

            _calculatedAimShakeForce = new Vector2
            (
                xShake,
                Random.Range(_absAimShakeForceMin.y, _absAimShakeForceMax.y)
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


    override public void TurnOnAim()
    {
        /*------------------------------------------------------
        |TODO| Desc 받아와서 MainHandler에 해당하는 Desc를 꺼야한다
        ------------------------------------------------------*/
        _ownerIKSkript.OnIK(_createdIKTargets["RightHandIK"]);
    }


    override public void TurnOffAim()
    {
        /*------------------------------------------------------
        |TODO| Desc 받아와서 MainHandler에 해당하는 Desc를 꺼야한다
        ------------------------------------------------------*/
        _ownerIKSkript.OffIK(_createdIKTargets["RightHandIK"]);
    }

}
