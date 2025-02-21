using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationFrameDataAsset;
using CollideKey = System.UInt32;


public class WeaponColliderManager : SubManager<WeaponColliderManager>
{
    public class WeaponCollideServiceDescBase
    {
        public Collider _collider = null; //Center는 여기서 사용할겁니다
        /*-------------------------------
        |NOTI| 가급적 박스를 사용하세요
            Box는 Sweep해도 Box와 '유사'합니다.
            Sphere Sweep -> overlap Capsule;
            Capsule -> overlap Box;
        -------------------------------*/
        public uint _checkedCount = 0;

        public Vector3 _extents = Vector3.zero;

        public Vector3 _checkedPosition = Vector3.zero;
        public Quaternion _checterRotation = Quaternion.identity;
    }


    public class WeaponCollideServiceTickAttackDesc : WeaponCollideServiceDescBase
    {
        /*-------------------------------------------------------
        |NOTI| 지속적으로 쿨타임마다 때리는 장판, 화염방사기 등등...
        -------------------------------------------------------*/
        public float _checkCollTime = 0.0f; //검사 쿨타임
        public float _checkedTime = 0.0f; //마지막으로 검사를 실행한 시간
    }


    private Dictionary<GameObject/*other*/, HashSet<MonoBehaviour>> _collideRecord = new Dictionary<GameObject, HashSet<MonoBehaviour>>();

    private CollideKey _collideKey = 0;
    private Stack<CollideKey> _usedCollideKey = new Stack<CollideKey>();

    public CollideKey GetWeaponCollideKey()
    {
        if (_usedCollideKey.Count > 0)
        {
            return _usedCollideKey.Pop();
        }

        return _collideKey++;
    }

    public void ReturnWeaponCollideKey(CollideKey key)
    {
        _usedCollideKey.Push(key);
    }

    public bool TriggerEnterCheck(MonoBehaviour caller_Victim, Collider other_Attacker)
    {
        bool ret = false;

        HashSet<MonoBehaviour> list = null;

        _collideRecord.TryGetValue(other_Attacker.gameObject, out list);

        if (list == null)
        {
            _collideRecord.Add(other_Attacker.gameObject, new HashSet<MonoBehaviour>());
            _collideRecord[other_Attacker.gameObject].Add(caller_Victim);
            ret = true;
            return ret;
        }

        if (list.Contains(caller_Victim) == true)
        {
            ret = false;
            return ret;
        }

        ret = true;
        list.Add(caller_Victim);
        return ret;
    }

    public void ClearCollider(GameObject collider)
    {
        HashSet<MonoBehaviour> list = null;

        _collideRecord.TryGetValue(collider, out list);

        if (list == null)
        {
            return;
        }

        if (list.Count >= 100000)
        {
            _collideRecord[collider] = new HashSet<MonoBehaviour>(); //GC야 나중에 지워줘?
        }
        else
        {
            list.Clear();
        }
    }


    public override void SubManagerFixedUpdate() {}

    public override void SubManagerInit()
    {
        SingletonAwake();
    }

    public override void SubManagerLateUpdate() {}
    public override void SubManagerStart() {}
    public override void SubManagerUpdate(){}



    private void ColliderCheck(WeaponCollideServiceDescBase desc)
    {
        Vector3 centerPosition = (desc._checkedPosition + desc._collider.bounds.center) / 2.0f;
        

    }
}
