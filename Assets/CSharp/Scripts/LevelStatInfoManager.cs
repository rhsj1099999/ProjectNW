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
            int level_key = statAsset._ActiveStatDesc._Level;

            if (_levelStats.ContainsKey(level_key) == true)
            {
                Debug.Assert(false, "이미 해당 레벨에 대한 데이터가 있다" + level_key);
                Debug.Break();
            }

            _levelStats.Add(level_key, statAsset);
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



    public override void SubManagerInit()
    {
        SingletonAwake();

        ReadyLevelStatData();
    }
    public override void SubManagerFixedUpdate(){}
    public override void SubManagerLateUpdate(){}
    public override void SubManagerStart(){}
    public override void SubManagerUpdate() {}
}
