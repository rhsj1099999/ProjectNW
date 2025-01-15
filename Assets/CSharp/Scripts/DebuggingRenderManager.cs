using System.Collections.Generic;
using UnityEngine;

public interface IDebugRender
{
    public void AddMe(GameObject obj)
    {
        DebuggingRenderManager.Instance?.RegistDebuggingObject(obj);
    }
    public void RemoveMe(GameObject obj)
    {
        DebuggingRenderManager.Instance?.RemoveDebuggingObject(obj);
    }
}

public class DebuggingRenderManager : SubManager<DebuggingRenderManager>
{
    private bool _debuggingRender = false;

    private HashSet<GameObject> _debuggingObjects = new HashSet<GameObject>();

    public override void SubManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F8) == true)
        {
            _debuggingRender = !_debuggingRender;

            foreach (GameObject obj in _debuggingObjects)
            {
                obj.SetActive(_debuggingRender);
            }
        }
    }

    public void RegistDebuggingObject(GameObject obj)
    {
        _debuggingObjects.Add(obj);

        obj.SetActive(_debuggingRender);
    }

    public void RemoveDebuggingObject(GameObject obj)
    {
        _debuggingObjects.Remove(obj);
    }

    public override void SubManagerLateUpdate()
    {
    }

    public override void SubManagerStart()
    {
    }

    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerInit()
    {
        SingletonAwake();
    }
}
