using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : SubManager
{
    private static TimeScaler _instance = null;

    private int _fixedUpdateCountACC = 0;
    private int _updateCountACC = 0;
    private int _lateUpdateCountACC = 0;
    private int _currentFrameFixedUpdateCalled = 0;

    private double _fixedUpdateDeltaTimeACC = 0.0f;
    private double _updateDeltaTimeACC = 0.0f;
    private double _lateUpdateDeltaTimeACC = 0.0f;

    private double _fixedUpdateMaxACC = 0.0f;
    private double _updateMaxACC = 0.0f;
    private double _prevUpdateMaxACC = 0.0f;
    private double _lateUpdateMaxACC = 0.0f;

    public static TimeScaler Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<TimeScaler>();
                singletonObject.name = typeof(TimeScaler).ToString();

                DontDestroyOnLoad(singletonObject);
            }

            return _instance;
        }
    }

    public override void SubManagerAwake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public override void SubManagerFixedUpdate()
    {
        _fixedUpdateMaxACC += Time.deltaTime;
        _currentFrameFixedUpdateCalled++;
        _fixedUpdateCountACC++;
        _fixedUpdateDeltaTimeACC += Time.deltaTime;
    }

    public override void SubManagerUpdate()
    {
        //타임디버깅
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.L))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.O))  // R키를 누르면 게임 속도를 정상으로 복원
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }


        //Phase 1
        {
            double delta = _updateDeltaTimeACC - _fixedUpdateDeltaTimeACC;
            double step = delta / Time.fixedDeltaTime;

            if (Math.Abs(delta) >= Time.fixedDeltaTime)
            {
                int a = 10;
            }
        }


        _prevUpdateMaxACC = _updateMaxACC;
        _updateMaxACC = Time.deltaTime;

        _updateCountACC++;
        _updateDeltaTimeACC += Time.deltaTime;

        //Phase 2
        {
            double delta = _updateDeltaTimeACC - _fixedUpdateDeltaTimeACC;
            double step = delta / Time.fixedDeltaTime;

            if (Math.Abs(delta) >= Time.fixedDeltaTime)
            {
                int a = 10;
            }
        }
    }

    public override void SubManagerLateUpdate()
    {
        
        if (_updateMaxACC < _fixedUpdateMaxACC)
        {
            //Debug.Assert(false, "fixed가 넘어섰다");
            //Debug.Break();
        }

        double delta = _updateDeltaTimeACC - _fixedUpdateDeltaTimeACC;
        double step = delta / Time.fixedDeltaTime;

        if (Math.Abs(delta) >= Time.fixedDeltaTime)
        {
            int a = 10;
        }



        _lateUpdateCountACC++;
        _lateUpdateDeltaTimeACC += Time.deltaTime;
        _currentFrameFixedUpdateCalled = 0;
        _fixedUpdateMaxACC = 0.0f;
        //_updateMaxACC = 0.0f;
    }

    public float GetSynchronizedDeltaTime()
    {
        //if (_fixedUpdateMaxACC >= _updateMaxACC)
        //{
        //    return 0.0f;
        //}

        //float delta = _updateMaxACC - _fixedUpdateMaxACC;

        //if (delta >= Time.fixedDeltaTime)
        //{
        //    return Time.fixedDeltaTime;
        //}
        //else
        //{
        //    return delta;
        //}

        return 0.0f;
    }
}
