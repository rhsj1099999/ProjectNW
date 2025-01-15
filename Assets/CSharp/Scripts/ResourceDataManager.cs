using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum FrameCheckType
{
    Up, Under, Between,
    End,
}

public enum FrameDataType
{
    NextAttackMotion,
    StateChangeReadyLikeIdle, //Act Like IdleState
    End,
}

[Serializable]
public class FrameData
{
    public int _frameUp = -1;
    public int _frameUnder = -1;
    public FrameCheckType _frameCheckType = FrameCheckType.Up;

    public bool FrameCheck(int currAnimationFrame)
    {
        switch(_frameCheckType)
        {
            case FrameCheckType.Up:
                {
                    if (currAnimationFrame > _frameUp)
                    {
                        return true;
                    }
                    return false;
                }

            case FrameCheckType.Under:
                {
                    if (currAnimationFrame < _frameUnder)
                    {
                        return true;
                    }
                    return false;
                }

            case FrameCheckType.Between:
                if (currAnimationFrame > _frameUp && currAnimationFrame < _frameUnder)
                {
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}


public class ResourceDataManager : SubManager<ResourceDataManager>
{

    public override void SubManagerInit()
    {
        SingletonAwake();

        ReadyAnimationHipCurve();
        ReadyAnimationFrameData();
        ReadyStateData();
        ReadyZeroFrameAnimations();
        StateGraphAssets();
        ReadyAvatarMasks();
    }

    /*-----------------------------------
    Data Section _ Hip Curve
    -----------------------------------*/

    private Dictionary<AnimationClip, AnimationHipCurveAsset> _animationHipData = new Dictionary<AnimationClip, AnimationHipCurveAsset>();
    public List<AnimationHipCurveAsset> _animationHipCurveList = new List<AnimationHipCurveAsset>();

    private void ReadyAnimationHipCurve()
    {
        foreach (AnimationHipCurveAsset item in _animationHipCurveList)
        {
            if (_animationHipData.ContainsKey(item._clip) == true)
            {
                Debug.Log("해당 데이터가 이미 있다" + item._clip.name);
            }
            _animationHipData.Add(item._clip, item);
        }
    }

    public void AddHipCurve(AnimationClip clip)
    {
        AnimationHipCurveAsset newAsset = ScriptableObject.CreateInstance<AnimationHipCurveAsset>();
#if UNITY_EDITOR

        if (_animationHipData.ContainsKey(clip) == true)
        {
            return;
        }

        AllocateAnimationHipCurve(clip, newAsset);
        
        string filename = clip.name;
        string assetPath = "Assets/CSharp/ScriptableObjects/AnimationHipCurve/Created/" + filename + ".asset";

        if (AssetDatabase.LoadAssetAtPath<AnimationHipCurveAsset>(assetPath) != null)
        {
            Debug.Log("이미있습니다. 건너뜁니다" + clip.name);
            return;
        }

        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.SaveAssets();
#endif
    }

#if UNITY_EDITOR
    public void AllocateAnimationHipCurve(AnimationClip clip, AnimationHipCurveAsset asset)
    {
        Debug.Assert(clip != null, "clip이 null입니다");
        asset._clip = clip;
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
            { asset._animationHipCurveX = AnimationUtility.GetEditorCurve(clip, binding); curveXFind = true; }

            if (binding.propertyName == _unityName_HipBoneLocalPositionY)
            { asset._animationHipCurveY = AnimationUtility.GetEditorCurve(clip, binding); curveYFind = true; }

            if (binding.propertyName == _unityName_HipBoneLocalPositionZ)
            { asset._animationHipCurveZ = AnimationUtility.GetEditorCurve(clip, binding); curveZFind = true; }
        }

        Debug.Assert((curveXFind == true && curveYFind == true && curveZFind == true), "커브가 존재하지 않습니다");
    }
#endif

    public AnimationHipCurveAsset GetHipCurve(AnimationClip clip)
    {
        if (_animationHipData.ContainsKey(clip) == false)
        {
            Debug.Assert(false, "찾으려는 애니메이션이 없습니다" + clip.name);
            Debug.Break();
            return null;
        }

        return _animationHipData[clip];
    }


    /*-----------------------------------
    Data Section _ Frame Data
    -----------------------------------*/
    [SerializeField] private List<AnimationClipWrapperAsset> _animationFrameDataAssetSettings = new List<AnimationClipWrapperAsset>();
    private Dictionary<AnimationClip, Dictionary<FrameDataType, FrameData>> _animationFrameDatas = new Dictionary<AnimationClip, Dictionary<FrameDataType, FrameData>>();

    private void ReadyAnimationFrameData()
    {
        foreach (var dataWrappers in _animationFrameDataAssetSettings)
        {
            foreach (var wrappedData in dataWrappers._list)
            {
                AnimationClip animationClip = wrappedData._animationClip;

                foreach (var data in wrappedData._dataAssetWrapper)
                {
                    FrameData frameData = data._dataAsset;
                    FrameDataType frameDataType = data._frameDataType;

                    if (_animationFrameDatas.ContainsKey(animationClip) == false)
                    {
                        _animationFrameDatas.Add(animationClip, new Dictionary<FrameDataType, FrameData>());
                    }

                    Dictionary<FrameDataType, FrameData> dataByClip = _animationFrameDatas[animationClip];

                    Debug.Assert(dataByClip.ContainsKey(frameDataType) == false, "데이터가 겹칩니다. anim : " + animationClip.name + "type : " + frameDataType.ToString());

                    dataByClip.Add(frameDataType, frameData);
                }
            }
        }
    }

    public FrameData GetAnimationFrameData(AnimationClip clip, FrameDataType type)
    {
        Debug.Assert(_animationFrameDatas.ContainsKey(clip) != false, "clip에 대한 정보가 하나도 없다" + clip.name);

        Dictionary<FrameDataType, FrameData> dataByClip = _animationFrameDatas[clip];

        Debug.Assert(dataByClip.ContainsKey(type) != false, "clip 내에 해당 type에 대한 정보가 없다" + clip.name + type.ToString());

        return dataByClip[type];
    }



    /*-----------------------------------
    Data Section _ Avatar Masks
    -----------------------------------*/
    [SerializeField] private List<AvatarMask> _avatarMasks = new List<AvatarMask>();
    private Dictionary<string, AvatarMask> _readiedMasks = new Dictionary<string, AvatarMask>();

    private void ReadyAvatarMasks()
    {
        foreach (var mask in _avatarMasks) 
        {
            if (_readiedMasks.ContainsKey(mask.name) == true)
            {
                Debug.Assert(false, "AvatarMask 이미 있습니다" + mask.name);
                Debug.Break();
                continue;
            }

            _readiedMasks.Add(mask.name, mask);
        }
    }

    public AvatarMask GetAvatarMask(string name) 
    {
        return _readiedMasks[name];
    }





    /*-----------------------------------
    Data Section _ State Data
    -----------------------------------*/
    [SerializeField] private List<StateAsset> _stateAsset = new List<StateAsset>();
    private Dictionary<StateAsset, State> _created = new Dictionary<StateAsset, State>();
    
    private void ReadyStateData()
    {
        foreach (StateAsset stateAsset in _stateAsset)
        {
            State newState = new State(stateAsset);

            if (_created.ContainsKey(stateAsset) != false)
            {
                Debug.Log("State Asset에게서 이미 만들어진 State 가 있다");
                continue;
            }

            _created.Add(stateAsset, newState);
        }
    }


    public State GetState(StateAsset stateAsset)
    {
        if (_created.ContainsKey(stateAsset) == false)
        {
            Debug.Log("사용하려는 State가 만들어지지 않았다" + stateAsset.name);

            State newState = new State(stateAsset);

            _created.Add(stateAsset, newState);
        }
        

        return _created[stateAsset];
    }




    /*-----------------------------------
    Data Section _ Zero Frames
    -----------------------------------*/
    [SerializeField] private List<AnimationClip> _totalZeroFrameAnimations = new List<AnimationClip>();
    private Dictionary<string, AnimationClip> _zeroFrameAnimationClips = new Dictionary<string, AnimationClip>();
    private void ReadyZeroFrameAnimations()
    {
        string zeroFrame = "_ZeroFrame";

        foreach (AnimationClip zeroFrameAnimarions in _totalZeroFrameAnimations)
        {
            if (zeroFrameAnimarions.name.EndsWith(zeroFrame) == false)
            {
                Debug.Assert(false, "0프레임이 아닌 애니메이션을 등록하려 합니다");
                continue;
            }

            string targetName = zeroFrameAnimarions.name.Substring(0, zeroFrameAnimarions.name.Length - zeroFrame.Length);

            if (_zeroFrameAnimationClips.ContainsKey(targetName) == true)
            {
                Debug.Assert(false, "이미 0프레임 애니메이션이있습니다");
                continue;
            }

            _zeroFrameAnimationClips.Add(targetName, zeroFrameAnimarions);
        }
    }

    public AnimationClip GetZeroFrameAnimation(string animationClipName)
    {
        Debug.Assert(_zeroFrameAnimationClips.ContainsKey(animationClipName) == true, "찾으려는 0프레임 애니메이션이 없습니다");

        return _zeroFrameAnimationClips[animationClipName];
    }



    /*-----------------------------------
    Data Section _ State GraphSection
    -----------------------------------*/
    public List<StateGraphAsset> _stateGraphAssets = new List<StateGraphAsset>();

    private void StateGraphAssets()
    {
        foreach (StateGraphAsset stateGraphAsset in _stateGraphAssets)
        {
            stateGraphAsset.InitlaizeGraphAsset();
        }
    }

    public override void SubManagerUpdate()
    {
    }

    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerLateUpdate()
    {
    }

    public override void SubManagerStart()
    {
    }
}
