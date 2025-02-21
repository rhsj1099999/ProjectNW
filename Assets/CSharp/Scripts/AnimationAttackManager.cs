using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class AnimationAttackManager
{

}



//public class AnimationAttackManager : SubManager<AnimationAttackManager>
//{
//    //private Dictionary<GameObject/*other*/, HashSet<MonoBehaviour>> _collideRecord = new Dictionary<GameObject, HashSet<MonoBehaviour>>();

//    //public bool TriggerEnterCheck(MonoBehaviour caller, Collider other)
//    //{
//    //    bool ret = false;

//    //    HashSet<MonoBehaviour> list = null;

//    //    _collideRecord.TryGetValue(other.gameObject, out list);

//    //    if (list == null)
//    //    {
//    //        _collideRecord.Add(other.gameObject, new HashSet<MonoBehaviour>());
//    //        _collideRecord[other.gameObject].Add(caller);
//    //        ret = true;
//    //        return ret;
//    //    }

//    //    if (list.Contains(caller) == true)
//    //    {
//    //        ret = false;
//    //        return ret;
//    //    }

//    //    ret = true;
//    //    list.Add(caller);
//    //    return ret;
//    //}

//    //public void ClearCollider(GameObject collider)
//    //{
//    //    HashSet<MonoBehaviour> list = null;

//    //    _collideRecord.TryGetValue(collider, out list);

//    //    if (list == null)
//    //    {
//    //        return;
//    //    }

//    //    if (list.Count >= 100000)
//    //    {
//    //        _collideRecord[collider] = new HashSet<MonoBehaviour>(); //GC야 나중에 지워줘?
//    //    }
//    //    else
//    //    {
//    //        list.Clear();
//    //    }
//    //}

//    public override void SubManagerInit()
//    {
//        SingletonAwake();
//    }

//    public override void SubManagerUpdate() {}
//    public override void SubManagerFixedUpdate() {}
//    public override void SubManagerLateUpdate() {}
//    public override void SubManagerStart() {}
//}
