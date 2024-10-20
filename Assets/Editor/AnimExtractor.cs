using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimExtractor : EditorWindow
{
    //[MenuItem("Extract Animations From FBX")]
    // Start is called before the first frame update
    [MenuItem("Tools/Extract Animations From FBX")]
    public static void ExtractAnimations()
    {
        // ������ FBX ���ϵ�
        Object[] selectedObjects = Selection.objects;

        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (Path.GetExtension(assetPath).ToLower() == ".fbx")
            {
                // FBX�� ���Ե� ��� �ִϸ��̼� Ŭ���� ������
                Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip)
                    {
                        // ������ ���� ����
                        string directory = "Assets/ExtractedAnimations";
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // �ִϸ��̼� Ŭ�� ���� ���
                        string newAssetPath = Path.Combine(directory, asset.name + count + ".anim");

                        // �ִϸ��̼� Ŭ�� ����
                        AssetDatabase.CreateAsset(Object.Instantiate(asset), newAssetPath);
                        count++;
                    }
                }
            }
        }

        // ���� ���� ����
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
