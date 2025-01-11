using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : SubManager
{
    private static TimeScaler _instance = null;

    private int _fixedUpdateCountACC = 0;
    private int _updateCountACC = 0;
    private int _lateUpdateCountACC = 0;

    private float _fixedUpdateDeltaTimeACC = 0.0f;
    private float _updateDeltaTimeACC = 0.0f;
    private float _lateUpdateDeltaTimeACC = 0.0f;

    private float _fixedUpdateMaxACC = 0.0f;
    private float _updateMaxACC = 0.0f;
    private float _lateUpdateMaxACC = 0.0f;

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
        _fixedUpdateCountACC++;
        _fixedUpdateDeltaTimeACC += Time.deltaTime;
        _fixedUpdateMaxACC += Time.deltaTime;
    }

    public override void SubManagerUpdate()
    {
        _updateCountACC++;
        _updateDeltaTimeACC += Time.deltaTime;
        _updateMaxACC = Time.deltaTime;
    }

    public override void SubManagerLateUpdate()
    {
        _lateUpdateCountACC++;
        _lateUpdateDeltaTimeACC += Time.deltaTime;
    }

    public float GetCustomFixedDeltaTime()
    {
        if (_fixedUpdateMaxACC >= _updateMaxACC)
        {
            return 0.0f;
        }

        float delta = _updateMaxACC - _fixedUpdateMaxACC;

        if (delta >= Time.fixedDeltaTime)
        {
            return Time.fixedDeltaTime;
        }
        else
        {
            return delta;
        }
    }
}
