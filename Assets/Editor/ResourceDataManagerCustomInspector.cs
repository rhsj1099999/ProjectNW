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

        // Manager�� ����Ʈ�� ���
        manager._stateGraphAssets = foundObjects;

        //Debug.Log($"Found and loaded {foundObjects.Count} ScriptableObjects of type A.");
        //EditorUtility.SetDirty(manager); // ���� ���� ����
    }
}
