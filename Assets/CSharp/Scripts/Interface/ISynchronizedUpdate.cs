using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMonoBehaviour : MonoBehaviour
{
    protected void SynchronizedUpdate(Action<float> work)
    {
        if (Time.inFixedTimeStep == true)
        {
            Debug.Assert(false, "Fixed Update 단계에서 호출하면 안된다");
            Debug.Break();
        }

        int count = Mathf.FloorToInt(Time.deltaTime / Time.fixedDeltaTime);

        for (int i = 0; i < count; i++)
        {
            work(Time.fixedDeltaTime);
        }

        work(Time.deltaTime - (count * Time.fixedDeltaTime));
    }

    protected void SynchronizedUpdate(Action work)
    {
        if (Time.inFixedTimeStep == true)
        {
            Debug.Assert(false, "Fixed Update 단계에서 호출하면 안된다");
            Debug.Break();
        }

        int count = Mathf.FloorToInt(Time.deltaTime / Time.fixedDeltaTime);

        for (int i = 0; i < count; i++)
        {
            work();
        }

        work();
    }
}
