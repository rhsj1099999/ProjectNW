using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationFrameDataAsset;
using CollideKey = System.UInt32;


public class WeaponColliderManager : SubManager<WeaponColliderManager>
{
    public class WeaponCollideServiceDescBase
    {
        public Collider _collider = null; //Center�� ���⼭ ����Ұ̴ϴ�
        /*-------------------------------
        |NOTI| ������ �ڽ��� ����ϼ���
            Box�� Sweep�ص� Box�� '����'�մϴ�.
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
        |NOTI| ���������� ��Ÿ�Ӹ��� ������ ����, ȭ������ ���...
        -------------------------------------------------------*/
        public float _checkCollTime = 0.0f; //�˻� ��Ÿ��
        public float _checkedTime = 0.0f; //���������� �˻縦 ������ �ð�
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
            _collideRecord[collider] = new HashSet<MonoBehaviour>(); //GC�� ���߿� ������?
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
