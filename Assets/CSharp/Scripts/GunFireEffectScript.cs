using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireEffectScript : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer = null;
    [SerializeField] private ParticleSystem _muzzleEffect = null;

    [SerializeField] private float _speed = 100.0f;
    [SerializeField] private float _timeTarget = 0.2f;
    [SerializeField] private float _timeACC = 0.0f;
    

    private void Awake()
    {
        if (_lineRenderer == null)
        {
            Debug.Assert(false, "LineRenderer가 있어야합니다");
            Debug.Break();
        }
    }


    public void Fire(Vector3 endPosition)
    {
        _muzzleEffect.Play();
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, endPosition);
        StartCoroutine(DrawCoroutine());
    }

    private IEnumerator DrawCoroutine()
    {
        while (true) 
        {
            _timeACC += Time.deltaTime;
            if (_timeACC >= _timeTarget)
            {
                _timeACC = 0.0f;
                _lineRenderer.SetPosition(0, Vector3.zero);
                _lineRenderer.SetPosition(1, Vector3.zero);
                break;
            }


            Vector3 dir = (_lineRenderer.GetPosition(1) - _lineRenderer.GetPosition(0)).normalized;
            Vector3 nextPosition = _lineRenderer.GetPosition(0) + dir * Time.deltaTime * _speed;
            Vector3 dir2 = (_lineRenderer.GetPosition(1) - nextPosition);

            if (Vector3.Dot(dir, dir2) < 0.0f)
            {
                _timeACC = 0.0f;
                _lineRenderer.SetPosition(0, Vector3.zero);
                _lineRenderer.SetPosition(1, Vector3.zero);
                break;
            }

            _lineRenderer.SetPosition(0, nextPosition);

            yield return null;
        }
    }
}
