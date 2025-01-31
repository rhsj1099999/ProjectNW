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
                Debug.Assert(false, "이미 해당 레벨에 대한 데이터가 있다" + level_key);
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
            Debug.Assert(false, "없는 레벨에 대한 데이터를 요청했따/ 레벨 : " + level);
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
            Debug.Assert(false, "없는 BuffName을 요청했습니다");
            Debug.Break();
            return -1;
        }

        return _buffNames[buffName];
    }

    public BuffAsset GetBuff(int buffKey)
    {
        if (_buffs.ContainsKey(buffKey) == false)
        {
            Debug.Assert(false, "없는 BuffKey을 요청했습니다");
            Debug.Break();
            return null;
        }

        return _buffs[buffKey];
    }

    public BuffAsset GetBuff(string buffName)
    {
        if (_buffNames.ContainsKey(buffName) == false)
        {
            Debug.Assert(false, "없는 BuffName을 요청했습니다");
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
                Debug.Assert(false, "중복되는 이름이 있습니다" + buff._BuffName);
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
