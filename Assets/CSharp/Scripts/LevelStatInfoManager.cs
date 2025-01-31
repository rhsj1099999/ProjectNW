using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStatInfoManager : SubManager<LevelStatInfoManager>
{
    public List<LevelStatAsset> _levelStats_Init = new List<LevelStatAsset>();
    private Dictionary<int, LevelStatAsset> _levelStats = new Dictionary<int, LevelStatAsset>();

    private void ReadyLevelStatData()
    {
        foreach (LevelStatAsset statAsset in _levelStats_Init)
        {
            int level_key = statAsset._Level;

            if (_levelStats.ContainsKey(level_key) == true)
            {
                Debug.Assert(false, "�̹� �ش� ������ ���� �����Ͱ� �ִ�" + level_key);
                Debug.Break();
            }

            _levelStats.Add(level_key, statAsset);
            statAsset.PartailAwake_InitDict();
        }
    }


    public LevelStatAsset GetLevelStatAsset(int level)
    {
        if (_levelStats.ContainsKey(level) == false)
        {
            Debug.Assert(false, "���� ������ ���� �����͸� ��û�ߵ�/ ���� : " + level);
            Debug.Break();
        }

        return _levelStats[level];
    }



    public List<BuffAsset> _buffs_Init = new List<BuffAsset>();
    Dictionary<int, BuffAsset> _buffs = new Dictionary<int, BuffAsset>();
    Dictionary<string, int> _buffNames = new Dictionary<string, int>();


    public int GetBuffKey(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "���� BuffName�� ��û�߽��ϴ�");
            Debug.Break();
            return -1;
        }

        return _buffNames[buffName];
    }

    public BuffAsset GetBuff(int buffKey)
    {
        if (_buffs.ContainsKey(buffKey) == false)
        {
            Debug.Assert(false, "���� BuffKey�� ��û�߽��ϴ�");
            Debug.Break();
            return null;
        }

        return _buffs[buffKey];
    }

    public BuffAsset GetBuff(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "���� BuffName�� ��û�߽��ϴ�");
            Debug.Break();
            return null;
        }

        return _buffs[GetBuffKey(buffName)];
    }


    private void ReadyBuffs()
    {
        int key = 0;
        foreach (BuffAsset buff in _buffs_Init)
        {
            if (_buffNames.ContainsKey(buff._BuffName) == true)
            {
                Debug.Assert(false, "�ߺ��Ǵ� �̸��� �ֽ��ϴ�" + buff._BuffName);
                Debug.Break();
            }

            buff._buffKey = key;

            _buffNames.Add(buff._BuffName, key);

            _buffs.Add(key, buff);
        }
    }




    public override void SubManagerInit()
    {
        SingletonAwake();

        ReadyLevelStatData();
        ReadyBuffs();
    }

    public override void SubManagerFixedUpdate(){}
    public override void SubManagerLateUpdate(){}
    public override void SubManagerStart(){}
    public override void SubManagerUpdate() {}
}
