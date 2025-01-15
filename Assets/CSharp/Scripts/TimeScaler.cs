using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TimeScaler : SubManager<TimeScaler>
{

    private int _fixedUpdateCountACC = 0;
    private int _updateCountACC = 0;


    private double _fixedUpdateDeltaTimeACC = 0.0f;
    private double _updateDeltaTimeACC = 0.0f;


    private double _DT_currFixedUpdate = 0.0f;
    private double _DT_prevFixedUpdate = 0.0f;

    private double _DT_currUpdate = 0.0f;
    private double _DT_prevUpdate = 0.0f;


    public Action<float> _timeChanged = null;
    public void AddTimeChangeDelegate(Action<float> func)
    {
        _timeChanged += func;
    }
    public void RemoveTimeChangeDelegate(Action<float> func)
    {
        _timeChanged -= func;
    }


    public override void SubManagerInit()
    {
        SingletonAwake();
    }



    public override void SubManagerFixedUpdate()
    {
        _fixedUpdateCountACC++;
        _fixedUpdateDeltaTimeACC += Time.deltaTime;

        _DT_prevFixedUpdate = _DT_currFixedUpdate;
        _DT_currFixedUpdate = Time.deltaTime;
    }



    public override void SubManagerUpdate()
    {
        //타임디버깅
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.01f;
            }

            if (Input.GetKeyDown(KeyCode.L))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.O))  // R키를 누르면 게임 속도를 정상으로 복원
            {
                Time.timeScale = 1.0f;
            }

            _timeChanged?.Invoke(Time.timeScale);
        }

        _updateCountACC++;
        _updateDeltaTimeACC += Time.deltaTime;

        _DT_prevUpdate = _DT_currUpdate;
        _DT_currUpdate = Time.deltaTime;
    }

    public override void SubManagerLateUpdate()
    {
    }


    public override void SubManagerStart()
    {
    }
}
