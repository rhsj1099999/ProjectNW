using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public interface ISubManager
{
    public abstract void SubManagerUpdate();
    public abstract void SubManagerFixedUpdate();
    public abstract void SubManagerLateUpdate();
    public abstract void SubManagerInit();
    public abstract void SubManagerStart();
}

public abstract class SubManager<T> : MonoBehaviour, ISubManager where T : SubManager<T>
{
    protected static T _instance = null;

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    public void SingletonAwake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = (T)this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    

    public abstract void SubManagerUpdate();
    public abstract void SubManagerFixedUpdate();
    public abstract void SubManagerLateUpdate();
    public abstract void SubManagerInit();
    public abstract void SubManagerStart();
}
