using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourceDataManager))]
public class ResourceDataManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // ���� Inspector UI �׸���

        ResourceDataManager manager = (ResourceDataManager)target;

        if (GUILayout.Button("Load StateGraphes"))
        {
            LoadAllScriptableObjects(manager);
        }

        if (GUILayout.Button("Load AnimationHipCurveData"))
        {
            LoadAllAnimationHipCurveAsset(manager);
        }
    }

    private void LoadAllScriptableObjects(ResourceDataManager manager)
    {
        // ������Ʈ���� ��� A ScriptableObject �˻�
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

        manager._stateGraphAssets = foundObjects;
    }



    private void LoadAllAnimationHipCurveAsset(ResourceDataManager manager)
    {
        // ������Ʈ���� ��� A ScriptableObject �˻�
        string[] guids = AssetDatabase.FindAssets("t:AnimationHipCurveAsset");
        List<AnimationHipCurveAsset> foundObjects = new List<AnimationHipCurveAsset>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationHipCurveAsset asset = AssetDatabase.LoadAssetAtPath<AnimationHipCurveAsset>(path);
            if (asset != null)
            {
                manager._animationHipCurveList.Add(asset);
            }
        }

    }
}
