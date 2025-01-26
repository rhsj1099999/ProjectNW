using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NUnit.Framework;

[CustomEditor(typeof(ColliderGenerator))]
public class ColliderGeneratorCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.LabelField("Create Capsule Collider", GUI.skin.horizontalSlider); /*Collider Generate-Capsule------------------------------------------------------------*/
        /*Collider Generate-Capsule------------------------------------------------------------*/

        if (GUILayout.Button("Generate Capsule Collider"))
        {
            ColliderGenerator generator = (ColliderGenerator)target;
            generator.GenerateCapsulecollider();
        }


        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); /*EachColliderModify-----------------------*/
        ///*EachColliderModify-----------------------*/

        ///*EachColliderModify-----------------------*/
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); /*EachColliderModify-----------------------*/
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); /*Collider Generate-Capsule------------------------------------------------------------*/




        /*Collider Generate-Capsule------------------------------------------------------------*/
        //_colliderOwnerObject = (GameObject)EditorGUILayout.ObjectField("CapsulePrefab", _colliderOwnerObject, typeof(GameObject), true);
        

        /*Collider Generate-Capsule------------------------------------------------------------*/
        EditorGUILayout.LabelField("SaveColliderList", GUI.skin.horizontalSlider); /*Collider Generate-Capsule------------------------------------------------------------*/


        serializedObject.ApplyModifiedProperties();
    }

    private void FindGameObjects(string targetName, ref List<GameObject> list, GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == targetName)
            {
                list.Add(child.gameObject);
            }

            FindGameObjects(targetName, ref list, parent);
        }
    }

}
