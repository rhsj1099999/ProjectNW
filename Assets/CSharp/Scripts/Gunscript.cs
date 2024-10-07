using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Vector2 _absAimShakeForce = Vector2.zero;

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

            StartCoroutine("CooltimeCoroutine");

            //AimShake -> 단순히 마우스를 틀어버리는게 아님
        }
    }

    public IEnumerator AimRestoreCoroutine()
    {
        return null;
    }

    public IEnumerator AimShakeCoroutine()
    {
        //총을 발사하면, n초동안 계속해서 힘을 주는 코루틴

        //근데 맥스치를 벗어날 순 없음



        return null;
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
    void Update()
    {
        if (_followingTransformStartPoint != null)
        {
            transform.position = _followingTransformStartPoint.position;
            transform.rotation = _followingTransformStartPoint.rotation;
        }



        FireCheck();
    }



}
