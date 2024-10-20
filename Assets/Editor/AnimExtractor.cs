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
        // 선택한 FBX 파일들
        Object[] selectedObjects = Selection.objects;

        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (Path.GetExtension(assetPath).ToLower() == ".fbx")
            {
                // FBX에 포함된 모든 애니메이션 클립을 가져옴
                Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip)
                    {
                        // 추출할 폴더 지정
                        string directory = "Assets/ExtractedAnimations";
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // 애니메이션 클립 저장 경로
                        string newAssetPath = Path.Combine(directory, asset.name + count + ".anim");

                        // 애니메이션 클립 복사
                        AssetDatabase.CreateAsset(Object.Instantiate(asset), newAssetPath);
                        count++;
                    }
                }
            }
        }

        // 변경 사항 적용
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
