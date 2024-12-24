using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationAttackManager))]
public class AnimationAttackManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // ���� Inspector UI �׸���

        AnimationAttackManager manager = (AnimationAttackManager)target;

        if (GUILayout.Button("Load StateGraphes"))
        {
            LoadAllScriptableObjects(manager);
        }
    }

    private void LoadAllScriptableObjects(AnimationAttackManager manager)
    {
        // ������Ʈ���� ��� A ScriptableObject �˻�
        string[] guids = AssetDatabase.FindAssets("t:AnimationAttackFrameAsset");
        List<AnimationAttackFrameAsset> foundObjects = new List<AnimationAttackFrameAsset>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationAttackFrameAsset asset = AssetDatabase.LoadAssetAtPath<AnimationAttackFrameAsset>(path);
            if (asset != null)
            {
                foundObjects.Add(asset);
            }
        }

        // Manager�� ����Ʈ�� ���
        manager._animationAttackFrameList = foundObjects;

        //Debug.Log($"Found and loaded {foundObjects.Count} ScriptableObjects of type A.");
        //EditorUtility.SetDirty(manager); // ���� ���� ����
    }
}
