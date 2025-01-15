using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynchronizedUpdater : SubManager<SynchronizedUpdater>
{
    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerInit()
    {
        SingletonAwake();
    }

    public override void SubManagerLateUpdate()
    {
    }

    public override void SubManagerStart()
    {
    }

    public override void SubManagerUpdate()
    {
    }
}
