using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.LookDev;
using static Unity.VisualScripting.Metadata;

public class ParentCurveSector
{
    public ParentCurveSector(Keyframe startKeyFrame, Keyframe endKeyFrame, int startFrame, int endFrame)
    {
        _startKeyFrame = startKeyFrame;
        _endKeyFrame = endKeyFrame;

        _startFrame = startFrame;
        _endFrame = endFrame;

        _startTime = _startKeyFrame.time;
        _endTime = _endKeyFrame.time;
    }

    public float _startTime = 0.0f;
    public float _endTime = 0.0f;

    public int _startFrame = 0;
    public int _endFrame = 0;

    public Keyframe _startKeyFrame;
    public Keyframe _endKeyFrame;

    public bool isIn(Keyframe targetChildKeyFrame)
    {
        if (targetChildKeyFrame.time >= _startTime && targetChildKeyFrame.time <= _endTime)
        {
            return true;
        }

        return false;
    }
}




public class AnimationClipEditor : MonoBehaviour
{
    public enum CurveType
    {
        Scale,
        Translation,
        Rotation,
    }
    public AnimationClip sourceClip;       // 기존 애니메이션 클립 (A)
    private float _animationFPS = 0.0f;


    [SerializeField] private List<string> _parentsOrdered = new List<string>();
    private Dictionary<string, Dictionary<string, AnimationCurve>> _bonePropertyCurves = new Dictionary<string, Dictionary<string, AnimationCurve>>();
    private Dictionary<string, Dictionary<string, Dictionary<string, AnimationCurve>>> _childs = new Dictionary<string, Dictionary<string, Dictionary<string, AnimationCurve>>>();

    void Start()
    {
        _animationFPS = sourceClip.frameRate;

        foreach (var boneName in _parentsOrdered)
        {
            FindBoneEndWith(boneName);
        }

        foreach (KeyValuePair<string, Dictionary<string, AnimationCurve>> pair in _bonePropertyCurves)
        {
            FindChildBones(pair.Key);
        }

        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, AnimationCurve>>> pair in _childs)
        {
            Dictionary<string, AnimationCurve> parentPropertyCurve = _bonePropertyCurves[pair.Key];

            //부모 회전커브 xyzw 만들어놓기
            List<AnimationCurve> parentCurves = new List<AnimationCurve>();
            {
                parentCurves.Add(null);
                parentCurves.Add(null);
                parentCurves.Add(null);
                parentCurves.Add(null);

                foreach (KeyValuePair<string, AnimationCurve> parentProperty in parentPropertyCurve)
                {
                    if (parentProperty.Key.ToLower().Contains("rotation") == false)
                    {
                        continue;
                    }

                    if (parentProperty.Key.ToLower().EndsWith("x") == true)
                    {
                        Debug.Assert(parentCurves[0] == null, "이미 있다");
                        parentCurves[0] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("y") == true)
                    {
                        Debug.Assert(parentCurves[1] == null, "이미 있다");
                        parentCurves[1] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("z") == true)
                    {
                        Debug.Assert(parentCurves[2] == null, "이미 있다");
                        parentCurves[2] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("w") == true)
                    {
                        Debug.Assert(parentCurves[3] == null, "이미 있다");
                        parentCurves[3] = parentProperty.Value;
                    }
                }
            }

            string parentPathName = pair.Key;

            Dictionary<string, Dictionary<string, AnimationCurve>> totalChildrenCurves = pair.Value;

            foreach (KeyValuePair<string, Dictionary<string, AnimationCurve>> childPair in totalChildrenCurves)
            {
                string childrenPathName = childPair.Key;

                Dictionary<string, AnimationCurve> children = childPair.Value;

                //자식의 keyFrame들을 복사한 리스트 만들어놓기. 그리고 이름도 만들어둔다;
                List<string> childrenPropertyNames = new List<string>();
                List<List<Keyframe>> childKeyFrames = new List<List<Keyframe>>();
                {
                    childKeyFrames.Add(new List<Keyframe>());
                    childKeyFrames.Add(new List<Keyframe>());
                    childKeyFrames.Add(new List<Keyframe>());
                    childKeyFrames.Add(new List<Keyframe>());
                    childrenPropertyNames.Add("None");
                    childrenPropertyNames.Add("None");
                    childrenPropertyNames.Add("None");
                    childrenPropertyNames.Add("None");

                    foreach (KeyValuePair<string, AnimationCurve> eachCurvePair in children)
                    {
                        if (eachCurvePair.Key.ToLower().Contains("rotation") == false)
                        {
                            continue;
                        }

                        if (eachCurvePair.Key.ToLower().EndsWith("x") == true)
                        {
                            Debug.Assert(childKeyFrames[0].Count == 0, "이미 있다");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[0].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[0] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("y") == true)
                        {
                            Debug.Assert(childKeyFrames[1].Count == 0, "이미 있다");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[1].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[1] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("z") == true)
                        {
                            Debug.Assert(childKeyFrames[2].Count == 0, "이미 있다");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[2].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[2] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("w") == true)
                        {
                            Debug.Assert(childKeyFrames[3].Count == 0, "이미 있다");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[3].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[3] = eachCurvePair.Key;
                        }
                    }
                }

                CalculateNewCurveForChild3(parentCurves, ref childKeyFrames, CurveType.Rotation);

                //이제 childkeyFrames의 정보가 갱신됐다.

                List<AnimationCurve> newChildRotationCurves = new List<AnimationCurve>();

                newChildRotationCurves.Add(new AnimationCurve(childKeyFrames[0].ToArray()));
                newChildRotationCurves.Add(new AnimationCurve(childKeyFrames[1].ToArray()));
                newChildRotationCurves.Add(new AnimationCurve(childKeyFrames[2].ToArray()));
                newChildRotationCurves.Add(new AnimationCurve(childKeyFrames[3].ToArray()));

                ReplaceCurve_rotations(childrenPathName, childrenPropertyNames, newChildRotationCurves);
            }
        }
    }




    private void CalculateNewCurveForChild3(List<AnimationCurve> parentCurves, ref List<List<Keyframe>> newKeyFrames, CurveType curveType)
    {
        AnimationCurve representParentCurve = parentCurves[0];

        //부모 커브 섹터를 미리 선언해놓는다.
        List<ParentCurveSector> parentCurveSectors = new List<ParentCurveSector>();
        {
            for (int j = 0; j < representParentCurve.keys.Length; j++)
            {
                int startIndex = j;
                int nextIndex = j + 1;

                ParentCurveSector newSector = new ParentCurveSector(representParentCurve.keys[startIndex], representParentCurve.keys[nextIndex], startIndex, nextIndex);
                parentCurveSectors.Add(newSector);

                if (nextIndex == representParentCurve.keys.Length - 1)
                {
                    float lastTime = representParentCurve.keys[nextIndex].time;
                    float animationTime = sourceClip.length;
                    float deltaABS = Mathf.Abs(animationTime - lastTime);
                    Debug.Assert(deltaABS <= float.Epsilon, "마지막 시간이 애니메이션 시간과 일치하지 않습니다");
                    break;
                }
            }
        }

        List<Keyframe> representChildCurve = newKeyFrames[0];



        for (int i = 0; i < representChildCurve.Count; ++i)
        {
            Keyframe childKeyFrame = representChildCurve[i];

            int childKeyFrameSectorIndex = -1;
            ParentCurveSector parentSector = FindParentCurveSector(parentCurveSectors, childKeyFrame, childKeyFrameSectorIndex);
            Debug.Assert(parentSector != null, "속한 부모 섹터가 없습니다");

            Quaternion childQuaternion = new Quaternion
            (
                newKeyFrames[0][i].value,
                newKeyFrames[1][i].value,
                newKeyFrames[2][i].value,
                newKeyFrames[3][i].value
            );

            int startFrame = parentSector._startFrame;
            Quaternion parentStartQuat = new Quaternion
            (
            parentCurves[0].keys[startFrame].value,
            parentCurves[1].keys[startFrame].value,
            parentCurves[2].keys[startFrame].value,
            parentCurves[3].keys[startFrame].value
            );

            int endFrame = parentSector._endFrame;
            Quaternion parentEndQuat = new Quaternion
            (
            parentCurves[0].keys[endFrame].value,
            parentCurves[1].keys[endFrame].value,
            parentCurves[2].keys[endFrame].value,
            parentCurves[3].keys[endFrame].value
            );

            Quaternion parentQuat = InterpolateQuaternion(parentStartQuat, parentEndQuat, parentSector._startTime, parentSector._endTime, childKeyFrame.time);
            Quaternion multiedQuat = parentQuat * childQuaternion;

            Keyframe newKeyFrame_X = new Keyframe();
            newKeyFrame_X = newKeyFrames[0][i];
            newKeyFrame_X.value = multiedQuat.x;
            newKeyFrames[0][i] = newKeyFrame_X;

            Keyframe newKeyFrame_Y = new Keyframe();
            newKeyFrame_Y = newKeyFrames[1][i];
            newKeyFrame_Y.value = multiedQuat.y;
            newKeyFrames[1][i] = newKeyFrame_Y;

            Keyframe newKeyFrame_Z = new Keyframe();
            newKeyFrame_Z = newKeyFrames[2][i];
            newKeyFrame_Z.value = multiedQuat.z;
            newKeyFrames[2][i] = newKeyFrame_Z;

            Keyframe newKeyFrame_W = new Keyframe();
            newKeyFrame_W = newKeyFrames[3][i];
            newKeyFrame_W.value = multiedQuat.w;
            newKeyFrames[3][i] = newKeyFrame_W;
        }
    }




    Quaternion InterpolateQuaternion(Quaternion start, Quaternion end, float startTime, float endTime, float currentTime)
    {
        Debug.Assert(currentTime >= startTime && currentTime <= endTime, "사이값이 아닙니다");
        float t = (currentTime - startTime) / (endTime - startTime);
        return Quaternion.Slerp(start, end, t);
    }



    public bool HasKeyframeAtTime2(Keyframe[] childrenKeys, float parentTime, ref int resultFrame)
    {
        int index = 0;
        foreach (Keyframe childrenKeyFrame in childrenKeys)
        {
            if (Mathf.Abs(parentTime - childrenKeyFrame.time) <= 0.001f)
            {
                resultFrame = index;
                return true;
            }
            index++;
        }

        return false;
    }


    public bool HasKeyframeAtTime(AnimationCurve childrenCurve, float parentTime, ref int resultFrame)
    {
        int index = 0;
        foreach (Keyframe childrenKeyFrame in childrenCurve.keys)
        {
            if (Mathf.Abs(parentTime - childrenKeyFrame.time) <= 0.001f)
            {
                resultFrame = index;
                return true;
            }
            index++;
        }

        return false;
    }






    private void FindUpKeyFrame2(Keyframe[] childrenKeyFrames, float time, ref int targetFrame)
    {
        targetFrame = -1;

        for (int i = 0; i < childrenKeyFrames.Length; i++)
        {
            float childrenTime = childrenKeyFrames[i].time;
            if (time < childrenTime)
            {
                targetFrame = i;
                return;
            }
        }

        targetFrame = -1;
    }


    private void FindUpKeyFrame(AnimationCurve childrenCurve, float time, ref int targetFrame)
    {
        targetFrame = -1;

        for (int i = 0; i < childrenCurve.keys.Length; i++)
        {
            float childrenTime = childrenCurve.keys[i].time;
            if (time < childrenTime)
            {
                targetFrame = i;
                return;
            }
        }

        targetFrame = -1;
    }

    public void ReplaceCurve(string path, string propertyName, AnimationCurve newCurve)
    {
        // 기존 커브를 대체합니다.
        var binding = new EditorCurveBinding
        {
            path = path,
            type = typeof(Transform), // 예: Transform. 필요에 따라 타입을 조정하세요.
            propertyName = propertyName
        };

        Debug.Assert(AnimationUtility.GetEditorCurve(sourceClip, binding) != null, "해당 커브가 없습니다");

        AnimationUtility.SetEditorCurve(sourceClip, binding, newCurve);
    }


    public void ReplaceCurve_rotations(string path, List<string> propertyName, List<AnimationCurve> newRotationCurves)
    {
        for (int i = 0; i < 4; i++)
        {
            var binding = new EditorCurveBinding
            {
                path = path,
                type = typeof(Transform),
                propertyName = propertyName[i]
            };

            Debug.Assert(AnimationUtility.GetEditorCurve(sourceClip, binding) != null, "해당 커브가 없습니다");

            AnimationUtility.SetEditorCurve(sourceClip, binding, newRotationCurves[i]);
        }

    }



    private bool isChildBone(string parentBoneName, string childBoneName)
    {
        int parentLength = parentBoneName.Length;

        if (childBoneName.StartsWith(parentBoneName) == false) //부모뼈로 시작하지 않는다
        {
            return false;
        }


        if (childBoneName.Length <= parentBoneName.Length) //이름길이가 이하다
        {
            return false;
        }


        for (int i = parentLength + 1; i < childBoneName.Length; i++)
        {
            if (childBoneName[i] == '/')
            {
                return false;
            }
        }

        return true;
    }


    void FindChildBones(string parentBoneName)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

        foreach (EditorCurveBinding binding in bindings)
        {
            
            // Master 뼈의 Position, Rotation, Scale 관련 속성만 필터링
            if (isChildBone(parentBoneName, binding.path) == true)
            {
                if (_childs.ContainsKey(parentBoneName) == false)
                {
                    _childs.Add(parentBoneName, new Dictionary<string, Dictionary<string, AnimationCurve>>());
                }

                Dictionary<string, Dictionary<string, AnimationCurve>> childBones = _childs[parentBoneName];


                if (childBones.ContainsKey(binding.path) == false)
                {
                    childBones.Add(binding.path, new Dictionary<string, AnimationCurve>());
                }

                Dictionary<string, AnimationCurve> properties = childBones[binding.path];

                if (properties.ContainsKey(binding.propertyName) == true)
                {
                    //Debug.Log("커브가 이미 있다");
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

                properties.Add(binding.propertyName, curve);
            }
        }
    }


    void FindBoneEndWith(string boneName)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

        foreach (EditorCurveBinding binding in bindings)
        {
            // Master 뼈의 Position, Rotation, Scale 관련 속성만 필터링
            if (binding.path.EndsWith(boneName) == true)
            {
                if (_bonePropertyCurves.ContainsKey(binding.path) == false)
                {
                    _bonePropertyCurves.Add(binding.path, new Dictionary<string, AnimationCurve>());
                }

                Dictionary<string, AnimationCurve> properties = _bonePropertyCurves[binding.path];

                if (properties.ContainsKey(binding.propertyName) == true)
                {
                    //Debug.Log("커브가 이미 있다");
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

                properties.Add(binding.propertyName, curve);
            }
        }
    }



    private ParentCurveSector FindParentCurveSector(List<ParentCurveSector> parentSectors, Keyframe childKeyFrame, int? target = null)
    {
        int index = 0;

        foreach (ParentCurveSector parentSector in parentSectors)
        {
            if (parentSector.isIn(childKeyFrame) == true)
            {
                if (target != null)
                {
                    target = index;
                }
                return parentSector;
            }

            index++;
        }

        Debug.Assert(false, "속하는 키프레임이 하나도 없습니까?");
        return null;
    }




    // 부모의 Curve와 자식의 Curve를 이용해 새로운 Curve를 계산하는 함수
    //private void CalculateNewCurveForChild(AnimationCurve parentCurve, AnimationCurve childCurve, ref List<Keyframe> newKeyFrames, CurveType curveType)
    //{
    //    for (int i = 0; i < parentCurve.keys.Length; i++)
    //    {
    //        int resultFrame = 0;

    //        Keyframe parentKeyFrame = parentCurve.keys[i];

    //        float parentTime = parentKeyFrame.time;
    //        float newValue = 0.0f;


    //        Keyframe newChildKeyFrame = new Keyframe();
    //        newChildKeyFrame.time = parentTime;

    //        if (HasKeyframeAtTime(childCurve, parentTime, ref resultFrame) == true) //자식에 해당 타이밍에 키프레임이 있다
    //        {
    //            Keyframe childKeyFrame = newKeyFrames[resultFrame];

    //            switch (curveType)
    //            {
    //                case CurveType.Scale:
    //                    newValue = parentKeyFrame.value * childKeyFrame.value;
    //                    break;
    //                case CurveType.Translation:
    //                    newValue = parentKeyFrame.value + childKeyFrame.value;
    //                    break;
    //                case CurveType.Rotation:
    //                    newValue = parentKeyFrame.value * childKeyFrame.value;
    //                    break;
    //            }

    //            newChildKeyFrame.value = newValue;
    //            newKeyFrames[resultFrame] = newChildKeyFrame;
    //        }
    //        else //자식에 해당 타이밍에 키프레임이 없다 -> 새로 끼워넣어줘야한다.
    //        {
    //            int newResult = 0;
    //            FindUpKeyFrame(childCurve, parentTime, ref newResult);
    //            newChildKeyFrame.value = parentKeyFrame.value;
    //            newKeyFrames.Insert(newResult, newChildKeyFrame);
    //        }
    //    }
    //}



    //Dictionary<string, AnimationCurve> ReturnFindBoneEndWith(string boneName)
    //{
    //    Dictionary<string, AnimationCurve> retCurve;

    //    EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

    //    foreach (EditorCurveBinding binding in bindings)
    //    {
    //        // Master 뼈의 Position, Rotation, Scale 관련 속성만 필터링
    //        if (binding.path.EndsWith(boneName) == true)
    //        {
    //            if (_bonePropertyCurves.ContainsKey(binding.path) == false)
    //            {
    //                _bonePropertyCurves.Add(binding.path, new Dictionary<string, AnimationCurve>());
    //            }

    //            Dictionary<string, AnimationCurve> properties = _bonePropertyCurves[binding.path];

    //            if (properties.ContainsKey(binding.propertyName) == true)
    //            {
    //                Debug.Log("커브가 이미 있다");
    //            }

    //            AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

    //            properties.Add(binding.propertyName, curve);
    //        }
    //    }

    //    return retCurve;
    //}


    //path = 뼈 이름처럼 쓴다.
    //Path 끝에 /로 끝나지 않는다

    //void ExtractRootTransformData()
    //{
    //    // 모든 CurveBinding 가져오기
    //    EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

    //    foreach (EditorCurveBinding binding in bindings)
    //    {
    //        // Master 뼈의 Position, Rotation, Scale 관련 속성만 필터링
    //        if (binding.path == masterBoneName &&
    //            (binding.propertyName.Contains("localPosition") ||
    //             binding.propertyName.Contains("localRotation") ||
    //             binding.propertyName.Contains("localScale")))
    //        {
    //            // 해당 커브의 AnimationCurve 가져오기
    //            AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

    //            Debug.Log($"Property: {binding.propertyName}");

    //            // 키프레임 데이터 출력
    //            foreach (Keyframe key in curve.keys)
    //            {
    //                Debug.Log($"Time: {key.time}, Value: {key.value}");
    //            }


    //            foreach (Keyframe key in curve.keys)
    //            {
    //                float time = key.time;
    //                int frame = Mathf.RoundToInt(time * _animationFPS); // 시간 정보를 프레임으로 변환
    //                Debug.Log($"Frame: {frame}, Time: {time:F2}s, Value: {key.value}");
    //            }

    //        }
    //    }
    //}

    //private void CalculateNewCurveForChild2(List<AnimationCurve> parentCurves, ref List<List<Keyframe>> newKeyFrames, CurveType curveType)
    //{
    //    AnimationCurve representParentCurve = parentCurves[0];

    //    List<ParentCurveSector> parentCurveSectors = new List<ParentCurveSector>();

    //    for (int j = 0; j < representParentCurve.keys.Length; j++)
    //    {
    //        int startIndex = j;
    //        int nextIndex = j + 1;

    //        ParentCurveSector newSector = new ParentCurveSector(representParentCurve.keys[startIndex], representParentCurve.keys[nextIndex]);
    //        parentCurveSectors.Add(newSector);

    //        if (nextIndex == representParentCurve.keys.Length - 1)
    //        {break;}
    //    }

    //    for (int i = 0; i < representParentCurve.keys.Length; i++)
    //    {
    //        Keyframe parentKeyFrame = representParentCurve.keys[i];
    //        float parentTime = parentKeyFrame.time;

    //        int resultFrame = 0;

    //        int parentUpKeyFrame = 0;

    //        int childrenUpKeyFrame = 0;

    //        bool isHasChildKeyFrame = HasKeyframeAtTime2(newKeyFrames[0].ToArray(), parentTime, ref resultFrame);

    //        if (isHasChildKeyFrame == false) //보간을 할껀데 부모에는 키프레임이 있지만 자식에게는 없다
    //        {
    //            Quaternion parentQuaternion = new Quaternion(parentCurves[0].keys[i].value, parentCurves[1].keys[i].value, parentCurves[2].keys[i].value, parentCurves[3].keys[i].value);

    //            Quaternion childrenQuaternion = Quaternion.identity; //보간 될 키프레임 정보

    //            //자식 커브 기준으로 해당 시간을 최초로 뛰어넘는 키 인덱스를 내놔라
    //            FindUpKeyFrame2(newKeyFrames[0].ToArray(), parentTime, ref childrenUpKeyFrame);


    //            Keyframe copyFromNext = new Keyframe();

    //            if (childrenUpKeyFrame < 0)  //그 시간을 넘어선 키프레임이 없다. 마지막 키프레임을 기준으로 한다.
    //            {
    //                int lastKeyFrameIndex = newKeyFrames[0].Count - 1;
    //                childrenQuaternion = new Quaternion(newKeyFrames[0][lastKeyFrameIndex].value, newKeyFrames[1][lastKeyFrameIndex].value, newKeyFrames[2][lastKeyFrameIndex].value, newKeyFrames[3][lastKeyFrameIndex].value);
    //                copyFromNext = newKeyFrames[0][lastKeyFrameIndex];
    //            }
    //            else
    //            {
    //                float time_Up = (newKeyFrames[0][childrenUpKeyFrame].time - newKeyFrames[0][childrenUpKeyFrame - 1].time);
    //                float time_Down = (1.0f - newKeyFrames[0][childrenUpKeyFrame - 1].time);
    //                float time = time_Up / time_Down;

    //                Quaternion childQuaternionBehind = new Quaternion(newKeyFrames[0][childrenUpKeyFrame].value, newKeyFrames[1][childrenUpKeyFrame].value, newKeyFrames[2][childrenUpKeyFrame].value, newKeyFrames[3][childrenUpKeyFrame].value);
    //                Quaternion childQuaternionFront = new Quaternion(newKeyFrames[0][childrenUpKeyFrame - 1].value, newKeyFrames[1][childrenUpKeyFrame - 1].value, newKeyFrames[2][childrenUpKeyFrame - 1].value, newKeyFrames[3][childrenUpKeyFrame - 1].value);
    //                childrenQuaternion = Quaternion.Slerp(childQuaternionBehind, childQuaternionFront, time);

    //                copyFromNext = newKeyFrames[0][childrenUpKeyFrame - 1];
    //            }

    //            if (childrenUpKeyFrame < 0)
    //            {
    //                Keyframe newKeyFrame_X = new Keyframe();
    //                newKeyFrame_X = copyFromNext;
    //                newKeyFrame_X.time = parentTime;
    //                newKeyFrame_X.value = childrenQuaternion.x;
    //                newKeyFrames[0].Add(newKeyFrame_X);

    //                Keyframe newKeyFrame_Y = new Keyframe();
    //                newKeyFrame_Y = copyFromNext;
    //                newKeyFrame_Y.time = parentTime;
    //                newKeyFrame_Y.value = childrenQuaternion.y;
    //                newKeyFrames[1].Add(newKeyFrame_Y);

    //                Keyframe newKeyFrame_Z = new Keyframe();
    //                newKeyFrame_Z = copyFromNext;
    //                newKeyFrame_Z.time = parentTime;
    //                newKeyFrame_Z.value = childrenQuaternion.z;
    //                newKeyFrames[2].Add(newKeyFrame_Z);

    //                Keyframe newKeyFrame_W = new Keyframe();
    //                newKeyFrame_W = copyFromNext;
    //                newKeyFrame_W.time = parentTime;
    //                newKeyFrame_W.value = childrenQuaternion.w;
    //                newKeyFrames[3].Add(newKeyFrame_W);
    //            }
    //            else 
    //            {
    //                Keyframe newKeyFrame_X = new Keyframe();
    //                newKeyFrame_X = copyFromNext;
    //                newKeyFrame_X.time = parentTime;
    //                newKeyFrame_X.value = childrenQuaternion.x;
    //                newKeyFrames[0].Insert(childrenUpKeyFrame, newKeyFrame_X);

    //                Keyframe newKeyFrame_Y = new Keyframe();
    //                newKeyFrame_Y = copyFromNext;
    //                newKeyFrame_Y.time = parentTime;
    //                newKeyFrame_Y.value = childrenQuaternion.y;
    //                newKeyFrames[1].Insert(childrenUpKeyFrame, newKeyFrame_Y);

    //                Keyframe newKeyFrame_Z = new Keyframe();
    //                newKeyFrame_Z = copyFromNext;
    //                newKeyFrame_Z.time = parentTime;
    //                newKeyFrame_Z.value = childrenQuaternion.z;
    //                newKeyFrames[2].Insert(childrenUpKeyFrame, newKeyFrame_Z);

    //                Keyframe newKeyFrame_W = new Keyframe();
    //                newKeyFrame_W = copyFromNext;
    //                newKeyFrame_W.time = parentTime;
    //                newKeyFrame_W.value = childrenQuaternion.w;
    //                newKeyFrames[3].Insert(childrenUpKeyFrame, newKeyFrame_W);
    //            }
    //        }

    //        //이제 이곳에 도달하면 부모 키프레임이 있는곳에는 반드시 자식 키프레임이 있다.

    //        //부모 키프레임을 곱해야하는 타겟들을 찾는다.
    //        List<int> targetFrames = new List<int>();
    //        {
    //            //int startFrame = 0;
    //            //HasKeyframeAtTime2(newKeyFrames[0].ToArray(), parentTime, ref startFrame);
    //            //Debug.Assert(startFrame > 0, "없었으면 아까 넣었습니다 반드시 있어야합니다");

    //            //if (i == representParentCurve.keys.Length - 1) //지금이 마지막 키프레임입니다
    //            //{

    //            //}


    //            //for (int j = 0; j < ; j++)
    //            //{

    //            //}
    //        }


    //    }
    //}





}
