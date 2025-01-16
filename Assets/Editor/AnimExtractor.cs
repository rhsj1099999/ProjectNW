using UnityEngine;
using UnityEditor;
using System.IO;
using static Unity.VisualScripting.Member;

public class AnimExtractor : EditorWindow
{
    [MenuItem("Tools/Extract Animations From FBX")]
    public static void ExtractAnimations()
    {
        Object[] selectedObjects = Selection.objects;

        int count = 0;

        string directory = "Assets/ExtractedAnimations";

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        foreach (Object asset in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);

            if (Path.GetExtension(assetPath).ToLower() != ".fbx")
            {
                continue;
            }

            Object[] assetElements = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

            foreach (Object assetElement in assetElements)
            {
                if (assetElement is not AnimationClip)
                {
                    continue;
                }

                // �ִϸ��̼� Ŭ�� ���� ���
                string newAssetPath = Path.Combine(directory, assetElement.name + count + ".anim");

                // �ִϸ��̼� Ŭ�� ����
                AssetDatabase.CreateAsset(Object.Instantiate(assetElement), newAssetPath);
                count++;
            }
        }

        // ���� ���� ����
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Create Mirror Animation")]
    public static void CreateMirrorAnimation() 
    {
        string saveDirectory = "Assets/CreatedMirroredAnimation/";

        if (Directory.Exists(saveDirectory) == false) 
        {
            Directory.CreateDirectory(saveDirectory);
        }

        Object[] selectedObjects = Selection.objects;

        foreach (Object item in selectedObjects)
        {
            if (item is not AnimationClip)
            {
                continue;
            }


            AnimationClip originalClip = item as AnimationClip;

            AnimationClip newAnimationClip = new AnimationClip()
            {
                name = originalClip.name + "_Mirrored",
                legacy = originalClip.legacy // ������ Legacy ���� ����
            };


            foreach (var binding in AnimationUtility.GetCurveBindings(originalClip))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);

                // Mirror ����: X, Z �����ǰ� �Ϻ� ȸ���� ����
                if (binding.propertyName.Contains("Position.x") || binding.propertyName.Contains("Position.z"))
                {
                    AnimationCurve invertedCurve = new AnimationCurve();
                    foreach (var key in curve.keys)
                    {
                        // Keyframe �� ����
                        Keyframe invertedKey = new Keyframe(key.time, -key.value, -key.inTangent, -key.outTangent);
                        invertedCurve.AddKey(invertedKey);
                    }

                    curve = invertedCurve;
                }
                else if (binding.propertyName.Contains("Rotation"))
                {
                    // ���ʹϾ��� Ư�� �� (Y, Z, W) ����
                    if (binding.propertyName.Contains("y") || binding.propertyName.Contains("z") || binding.propertyName.Contains("w"))
                    {
                        AnimationCurve invertedCurve = new AnimationCurve();
                        foreach (var key in curve.keys)
                        {
                            // Keyframe �� ����
                            Keyframe invertedKey = new Keyframe(key.time, -key.value, -key.inTangent, -key.outTangent);
                            invertedCurve.AddKey(invertedKey);
                        }

                        curve = invertedCurve;
                    }
                }

                // Mirror�� Ŀ�긦 �� �ִϸ��̼� Ŭ���� ����
                newAnimationClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            newAnimationClip.wrapMode = originalClip.wrapMode;



            string newAssetPath = saveDirectory + newAnimationClip.name + ".anim";

            AssetDatabase.CreateAsset(Object.Instantiate(newAnimationClip), newAssetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }








    [MenuItem("Tools/ConvertAnimClip")]
    public static void ConvertAnimProperties()
    {
        Object[] selectedObjects = Selection.objects;

        foreach (Object asset in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);

            Object[] assetElements = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

            foreach (Object assetElement in assetElements)
            {
                if (assetElement is not AnimationClip)
                {
                    continue;
                }

                AnimationClip clip = (AnimationClip)assetElement;
            }
        }

        // ���� ���� ����
        AssetDatabase.Refresh();
    }


}
