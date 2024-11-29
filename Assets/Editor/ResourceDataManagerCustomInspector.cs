using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourceDataManager))]
public class ResourceDataManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 Inspector UI 그리기

        ResourceDataManager manager = (ResourceDataManager)target;

        if (GUILayout.Button("Load StateGraphes"))
        {
            LoadAllScriptableObjects(manager);
        }
    }

    private void LoadAllScriptableObjects(ResourceDataManager manager)
    {
        // 프로젝트에서 모든 A ScriptableObject 검색
        string[] guids = AssetDatabase.FindAssets("t:StateGraphAsset");
        List<StateGraphAsset> foundObjects = new List<StateGraphAsset>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StateGraphAsset asset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(path);
            if (asset != null)
            {
                foundObjects.Add(asset);
            }
        }

        // Manager의 리스트에 등록
        manager._stateGraphAssets = foundObjects;

        //Debug.Log($"Found and loaded {foundObjects.Count} ScriptableObjects of type A.");
        //EditorUtility.SetDirty(manager); // 변경 사항 저장
    }
}
