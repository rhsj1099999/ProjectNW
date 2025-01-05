using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAttackManager : SubManager
{
    static private AnimationAttackManager _instance;

    static public AnimationAttackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject newGameObject = new GameObject("AnimationAttackManager");
                DontDestroyOnLoad(newGameObject);
                _instance = newGameObject.AddComponent<AnimationAttackManager>();
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

        ReadyAnimationAttackFrames();
    }

    public List<AnimationAttackFrameAsset> _animationAttackFrameList = new List<AnimationAttackFrameAsset>();
    private Dictionary<AnimationClip, List<AnimationAttackFrameAsset.AttackFrameDesc>> _animationAttackFrame = new Dictionary<AnimationClip, List<AnimationAttackFrameAsset.AttackFrameDesc>>();
    private Dictionary<GameObject/*other*/, HashSet<MonoBehaviour>> _collideRecord = new Dictionary<GameObject, HashSet<MonoBehaviour>>();

    private void ReadyAnimationAttackFrames()
    {
        foreach (AnimationAttackFrameAsset item in _animationAttackFrameList)
        {
            if (_animationAttackFrame.ContainsKey(item._animation) == true)
            {
                Debug.Assert(false, "�ߺ��Ǵ� �����Ͱ� �ֽ��ϴ�" + item._animation.name);
                Debug.Break();
                return;
            }

            _animationAttackFrame.Add(item._animation, item._frameDesc);
        }
    }

    public List<AnimationAttackFrameAsset.AttackFrameDesc> GetAttackFrameDesc(AnimationClip animation)
    {
        if (_animationAttackFrame.ContainsKey(animation) == false)
        {
            Debug.Assert(false, "ã������ Animation�� Attack ������ �����ϴ�" + animation.name);
            Debug.Break();
            return null;
        }

        return _animationAttackFrame[animation];
    }

    public bool TriggerEnterCheck(MonoBehaviour caller, Collider other)
    {
        //�ѹ� �ε�������, ���Ⱑ ���ٰ� �ٽ� ������ �ٽ� Enter�� �����ϰ�
        //������ �帣�� ���� ����
        bool ret = false;

        HashSet<MonoBehaviour> list = null;

        _collideRecord.TryGetValue(other.gameObject, out list);

        if (list == null)
        {
            _collideRecord.Add(other.gameObject, new HashSet<MonoBehaviour>());
            _collideRecord[other.gameObject].Add(caller);
            ret = true;
            return ret;
        }

        if (list.Contains(caller) == true)
        {
            ret = false;
            return ret;
        }

        ret = true;
        list.Add(caller);
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
}
