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

                // 애니메이션 클립 저장 경로
                string newAssetPath = Path.Combine(directory, assetElement.name + count + ".anim");

                // 애니메이션 클립 복사
                AssetDatabase.CreateAsset(Object.Instantiate(assetElement), newAssetPath);
                count++;
            }
        }

        // 변경 사항 적용
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
                legacy = originalClip.legacy // 원본의 Legacy 설정 복사
            };


            foreach (var binding in AnimationUtility.GetCurveBindings(originalClip))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);

                // Mirror 로직: X, Z 포지션과 일부 회전값 반전
                if (binding.propertyName.Contains("Position.x") || binding.propertyName.Contains("Position.z"))
                {
                    AnimationCurve invertedCurve = new AnimationCurve();
                    foreach (var key in curve.keys)
                    {
                        // Keyframe 값 반전
                        Keyframe invertedKey = new Keyframe(key.time, -key.value, -key.inTangent, -key.outTangent);
                        invertedCurve.AddKey(invertedKey);
                    }

                    curve = invertedCurve;
                }
                else if (binding.propertyName.Contains("Rotation"))
                {
                    // 쿼터니언의 특정 축 (Y, Z, W) 반전
                    if (binding.propertyName.Contains("y") || binding.propertyName.Contains("z") || binding.propertyName.Contains("w"))
                    {
                        AnimationCurve invertedCurve = new AnimationCurve();
                        foreach (var key in curve.keys)
                        {
                            // Keyframe 값 반전
                            Keyframe invertedKey = new Keyframe(key.time, -key.value, -key.inTangent, -key.outTangent);
                            invertedCurve.AddKey(invertedKey);
                        }

                        curve = invertedCurve;
                    }
                }

                // Mirror된 커브를 새 애니메이션 클립에 설정
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

        // 변경 사항 적용
        AssetDatabase.Refresh();
    }


}
