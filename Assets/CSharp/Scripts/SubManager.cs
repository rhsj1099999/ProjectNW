using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubManager : MonoBehaviour
{
    public virtual void SubManagerUpdate() {}
    public virtual void SubManagerFixedUpdate() {}
    public virtual void SubManagerLateUpdate() {}


    public virtual void SubManagerAwake() { }
    public virtual void SubManagerStart() { }
}
