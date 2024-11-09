using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static State;



public class AnimationFrameData
{
    public enum FrameDataType
    {

    }
}

public class AnimationHipCurve
{
    public AnimationHipCurve(AnimationClip clip)
    {
        const string _unityName_HipBone = "Hips";
        const string _unityName_HipBoneLocalPositionX = "RootT.x";
        const string _unityName_HipBoneLocalPositionY = "RootT.y";
        const string _unityName_HipBoneLocalPositionZ = "RootT.z";

        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        bool curveXFind = false, curveYFind = false, curveZFind = false;

        foreach (var binding in bindings)
        {
            if (curveXFind == true && curveYFind == true && curveZFind == true)
            { break; } //다 찾았습니다.

            if (binding.propertyName == _unityName_HipBoneLocalPositionX)
            { _animationHipCurveX = AnimationUtility.GetEditorCurve(clip, binding); curveXFind = true; }

            if (binding.propertyName == _unityName_HipBoneLocalPositionY)
            { _animationHipCurveY = AnimationUtility.GetEditorCurve(clip, binding); curveYFind = true; }

            if (binding.propertyName == _unityName_HipBoneLocalPositionZ)
            { _animationHipCurveZ = AnimationUtility.GetEditorCurve(clip, binding); curveZFind = true; }
        }

        Debug.Assert((curveXFind == true && curveYFind == true && curveZFind == true), "커브가 존재하지 않습니다");
    }
    public AnimationCurve _animationHipCurveX = null;
    public AnimationCurve _animationHipCurveY = null;
    public AnimationCurve _animationHipCurveZ = null;
}


public class ResourceDataManager : MonoBehaviour
{
    static private ResourceDataManager _instance;

    static public ResourceDataManager Instance
    {
        get 
        { 
            if (_instance == null)
            {
                GameObject newGameObject = new GameObject("ResourceDataManager");
                DontDestroyOnLoad(newGameObject);
                _instance = newGameObject.AddComponent<ResourceDataManager>();
            }

            return _instance;
        }
    }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy( _instance.gameObject );
        }
    }


    private Dictionary<AnimationClip, AnimationHipCurve> _animationHipData = new Dictionary<AnimationClip, AnimationHipCurve>();
    public void AddHipCurve(AnimationClip clip)
    {
        if (_animationHipData.ContainsKey(clip) == true)
        {
            Debug.Log("Curve가 이미 있다");
            return;
        }

        _animationHipData.Add(clip, new AnimationHipCurve(clip));

    }
    public AnimationHipCurve GetHipCurve(AnimationClip clip)
    {
        if ( _animationHipData.ContainsKey(clip) == false) 
        {
            return null;
        }

        return _animationHipData[clip];
    }


}
