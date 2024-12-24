using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationAttackManager))]
public class AnimationAttackManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 Inspector UI 그리기

        AnimationAttackManager manager = (AnimationAttackManager)target;

        if (GUILayout.Button("Load StateGraphes"))
        {
            LoadAllScriptableObjects(manager);
        }
    }

    private void LoadAllScriptableObjects(AnimationAttackManager manager)
    {
        // 프로젝트에서 모든 A ScriptableObject 검색
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

        // Manager의 리스트에 등록
        manager._animationAttackFrameList = foundObjects;

        //Debug.Log($"Found and loaded {foundObjects.Count} ScriptableObjects of type A.");
        //EditorUtility.SetDirty(manager); // 변경 사항 저장
    }
}
