using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelStatInfoManager))]
public class LevelStatInfoManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 Inspector UI 그리기

        LevelStatInfoManager manager = (LevelStatInfoManager)target;

        if (GUILayout.Button("Load_All_Level_Stat_Asset"))
        {
            LoadAllLevelStatAsset(manager);
        }

        if (GUILayout.Button("Load_All_Buff_Asset"))
        {
            LoadAllBuffAsset(manager);
        }
    }

    private void LoadAllLevelStatAsset(LevelStatInfoManager manager)
    {
        // 프로젝트에서 모든 A ScriptableObject 검색
        string[] guids = AssetDatabase.FindAssets("t:LevelStatAsset");
        List<LevelStatAsset> foundObjects = new List<LevelStatAsset>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelStatAsset asset = AssetDatabase.LoadAssetAtPath<LevelStatAsset>(path);
            if (asset != null)
            {
                foundObjects.Add(asset);
            }
        }

        manager._levelStats_Init = foundObjects;
    }




    private void LoadAllBuffAsset(LevelStatInfoManager manager)
    {
        // 프로젝트에서 모든 A ScriptableObject 검색
        string[] guids = AssetDatabase.FindAssets("t:BuffAsset");
        List<BuffAssetBase> foundObjects = new List<BuffAssetBase>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuffAssetBase asset = AssetDatabase.LoadAssetAtPath<BuffAssetBase>(path);
            if (asset != null)
            {
                foundObjects.Add(asset);
            }
        }

        manager._buffs_Init = foundObjects;
    }
}
