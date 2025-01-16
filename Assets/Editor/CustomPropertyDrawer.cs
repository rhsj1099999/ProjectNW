using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyInCodeAttribute))]
public class ReadOnlyInCodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(Application.isPlaying); // ��Ÿ�ӿ� �б� ����
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
    }
}