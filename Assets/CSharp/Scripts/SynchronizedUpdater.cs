using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynchronizedUpdater : SubManager
{
    static private SynchronizedUpdater _instance = null;

    public static SynchronizedUpdater Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<SynchronizedUpdater>();
                singletonObject.name = typeof(SynchronizedUpdater).ToString();

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



    public override void SubManagerUpdate()
    {
        base.SubManagerUpdate();
    }
}
