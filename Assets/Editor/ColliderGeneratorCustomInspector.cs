using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NUnit.Framework;

[CustomEditor(typeof(ColliderGenerator))]
public class ColliderGeneratorCustomInspector : Editor
{
    private string _colliderName = "CapsuleCollider(Clone)";
    private GameObject _dubuggingCapsulePrefab = null;

    private CapsuleColliderDesc _creatingCapsuleCollider = new CapsuleColliderDesc();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.LabelField("Create Capsule Collider", GUI.skin.horizontalSlider); /*Collider Generate-Capsule------------------------------------------------------------*/
        /*Collider Generate-Capsule------------------------------------------------------------*/
        _dubuggingCapsulePrefab = (GameObject)EditorGUILayout.ObjectField("CapsulePrefab", _dubuggingCapsulePrefab, typeof(GameObject), true);
        _creatingCapsuleCollider._startTransform = (Transform)EditorGUILayout.ObjectField("ParentTransform", _creatingCapsuleCollider._startTransform, typeof(Transform), true);
        _creatingCapsuleCollider._endTransform = (Transform)EditorGUILayout.ObjectField("ChildTransformNullable", _creatingCapsuleCollider._endTransform, typeof(Transform), true);
        _creatingCapsuleCollider._radiusX = EditorGUILayout.FloatField("RadiusX", _creatingCapsuleCollider._radiusX);
        _creatingCapsuleCollider._radiusZ = EditorGUILayout.FloatField("RadiusZ", _creatingCapsuleCollider._radiusZ);
        _creatingCapsuleCollider._heightRatio = EditorGUILayout.FloatField("HeightRatio", _creatingCapsuleCollider._heightRatio);
        _colliderName = EditorGUILayout.TextField("NameForKey(SearchInParent)", _colliderName);

        if (GUILayout.Button("Generate Capsule Collider"))
        {
            ColliderGenerator generator = (ColliderGenerator)target;
            generator.GenerateCapsulecollider(_creatingCapsuleCollider, _colliderName, _dubuggingCapsulePrefab);
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
