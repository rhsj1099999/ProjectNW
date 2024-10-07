using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunscript : MonoBehaviour
{
    [SerializeField] private Transform _followingTransformStartPoint = null;

    /*-------------------------------------------------------
    |TODO| = GunFire������ �÷��̾ ����ִ°Ͱ��� ������ ����?
    -------------------------------------------------------*/
    //GunFire���� ����, �Լ���
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
        //���콺�� Ŭ���ϸ� �Ѿ� �߻�
        if (Input.GetKey(KeyCode.Mouse0) == true && _coolTime < float.Epsilon)
        {
            Fire();

            _coolTime = _coolTimeOriginal;

            StartCoroutine("CooltimeCoroutine");

            //AimShake -> �ܼ��� ���콺�� Ʋ������°� �ƴ�
        }
    }

    public IEnumerator AimRestoreCoroutine()
    {
        return null;
    }

    public IEnumerator AimShakeCoroutine()
    {
        //���� �߻��ϸ�, n�ʵ��� ����ؼ� ���� �ִ� �ڷ�ƾ

        //�ٵ� �ƽ�ġ�� ��� �� ����



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
