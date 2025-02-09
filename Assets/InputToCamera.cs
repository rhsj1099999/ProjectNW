using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputToCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _cineVirtual = null;

    private float _speed_X = 0.0f;
    private float _speed_Y = 0.0f;

    private void Awake()
    {
        if (_cineVirtual == null)
        {
            Debug.Assert(false, "CineMachine 이 없습니다");
            Debug.Break();
        }

        _speed_X = _cineVirtual.m_XAxis.m_MaxSpeed;
        _speed_Y = _cineVirtual.m_YAxis.m_MaxSpeed;
    }

    private void Update()
    {
        if (UIManager.Instance.IsConsumeInput() == true)
        {
            _cineVirtual.m_XAxis.m_MaxSpeed = 0.0f;
            _cineVirtual.m_YAxis.m_MaxSpeed = 0.0f;
        }
        else
        {
            _cineVirtual.m_XAxis.m_MaxSpeed = _speed_X;
            _cineVirtual.m_YAxis.m_MaxSpeed = _speed_Y;
        }
    }
}
