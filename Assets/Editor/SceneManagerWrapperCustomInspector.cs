using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneManagerWrapper))]
public class SceneManagerWrapperCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 Inspector UI 그리기

        SceneManagerWrapper manager = (SceneManagerWrapper)target;

        if (GUILayout.Button("Load StateGraphes"))
        {
            LoadAllScriptableObjects(manager);
        }
    }



    private void LoadAllScriptableObjects(SceneManagerWrapper manager)
    {
        List<GameObject> curtainCallList = manager.GetCurtainCallList();
        
        GameObject[] allObjects = Resources.LoadAll<GameObject>("Prefabs/UIs/CurtainCalls");

        foreach (var obj in allObjects)
        {
            if (obj.GetComponent<CurtainCallBase>() == null)
            {
                continue;
            }

            curtainCallList.Add(obj);
        }
    }
}

