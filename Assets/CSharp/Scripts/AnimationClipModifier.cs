using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class AnimationClipModifier : MonoBehaviour
{
    [SerializeField] private AnimationClip clipA = null;
    [SerializeField] private AnimationClip clipB = null;

#if UNITY_EDITOR
    public void AnimationClipAttach()
    {
        if (clipA == null || clipB == null)
        {
            return;
        }

        AnimationClip newClip = new AnimationClip();
        newClip.frameRate = clipA.frameRate;
        newClip.legacy = false; // Ensure it's a Humanoid-compatible clip

        float clipAEndTime = clipA.length;

        // First, add clipA's curves
        foreach (var binding in AnimationUtility.GetCurveBindings(clipA))
        {
            AnimationCurve curveA = AnimationUtility.GetEditorCurve(clipA, binding);
            if (curveA != null)
            {
                AnimationUtility.SetEditorCurve(newClip, binding, new AnimationCurve(curveA.keys));
            }
        }

        // Then, add clipB's curves offset by clipAEndTime
        foreach (var binding in AnimationUtility.GetCurveBindings(clipB))
        {
            AnimationCurve curveB = AnimationUtility.GetEditorCurve(clipB, binding);
            if (curveB != null)
            {
                AnimationCurve newCurve = new AnimationCurve();
                foreach (var key in curveB.keys)
                {
                    Keyframe newKey = new Keyframe(key.time + clipAEndTime, key.value, key.inTangent, key.outTangent);
                    newCurve.AddKey(newKey);
                }
                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            }
        }

        HumanPoseTransfer(clipA, newClip);
        HumanPoseTransfer(clipB, newClip, clipAEndTime);

        string savePath = "Assets/AAA----AttachedAnimation/NewClip.anim";

        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateAsset(newClip, savePath);
        AssetDatabase.SaveAssets();
    }

    private static void HumanPoseTransfer(AnimationClip sourceClip, AnimationClip targetClip, float timeOffset = 0f)
    {
        foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
        {
            if (binding.path == "" && binding.propertyName.StartsWith("m_LocalPosition"))
                continue; // Ignore root motion

            AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
            if (sourceCurve == null) continue;

            AnimationCurve newCurve = new AnimationCurve();
            foreach (var key in sourceCurve.keys)
            {
                newCurve.AddKey(new Keyframe(key.time + timeOffset, key.value, key.inTangent, key.outTangent));
            }

            AnimationUtility.SetEditorCurve(targetClip, binding, newCurve);
        }
    }


#endif

}
