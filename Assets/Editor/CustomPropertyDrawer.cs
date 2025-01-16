using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyInCodeAttribute))]
public class ReadOnlyInCodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(Application.isPlaying); // 런타임에 읽기 전용
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}