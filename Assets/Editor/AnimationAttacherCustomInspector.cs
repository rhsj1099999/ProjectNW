using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationClipModifier))]
public class AnimationAttacherCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();


        if (GUILayout.Button("Generate AttachedAnimation"))
        {
            AnimationClipModifier generator = (AnimationClipModifier)target;
            generator.AnimationClipAttach();
        }
    }
}
