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
    public AnimationClip sourceClip;       // ���� �ִϸ��̼� Ŭ�� (A)
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

            //�θ� ȸ��Ŀ�� xyzw ��������
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
                        Debug.Assert(parentCurves[0] == null, "�̹� �ִ�");
                        parentCurves[0] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("y") == true)
                    {
                        Debug.Assert(parentCurves[1] == null, "�̹� �ִ�");
                        parentCurves[1] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("z") == true)
                    {
                        Debug.Assert(parentCurves[2] == null, "�̹� �ִ�");
                        parentCurves[2] = parentProperty.Value;
                    }
                    else if (parentProperty.Key.ToLower().EndsWith("w") == true)
                    {
                        Debug.Assert(parentCurves[3] == null, "�̹� �ִ�");
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

                //�ڽ��� keyFrame���� ������ ����Ʈ ��������. �׸��� �̸��� �����д�;
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
                            Debug.Assert(childKeyFrames[0].Count == 0, "�̹� �ִ�");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[0].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[0] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("y") == true)
                        {
                            Debug.Assert(childKeyFrames[1].Count == 0, "�̹� �ִ�");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[1].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[1] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("z") == true)
                        {
                            Debug.Assert(childKeyFrames[2].Count == 0, "�̹� �ִ�");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[2].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[2] = eachCurvePair.Key;
                        }
                        else if (eachCurvePair.Key.ToLower().EndsWith("w") == true)
                        {
                            Debug.Assert(childKeyFrames[3].Count == 0, "�̹� �ִ�");
                            for (int i = 0; i < eachCurvePair.Value.keys.Length; i++)
                            {
                                childKeyFrames[3].Add(eachCurvePair.Value.keys[i]);
                            }
                            childrenPropertyNames[3] = eachCurvePair.Key;
                        }
                    }
                }

                CalculateNewCurveForChild3(parentCurves, ref childKeyFrames, CurveType.Rotation);

                //���� childkeyFrames�� ������ ���ŵƴ�.

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

        //�θ� Ŀ�� ���͸� �̸� �����س��´�.
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
                    Debug.Assert(deltaABS <= float.Epsilon, "������ �ð��� �ִϸ��̼� �ð��� ��ġ���� �ʽ��ϴ�");
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
            Debug.Assert(parentSector != null, "���� �θ� ���Ͱ� �����ϴ�");

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
        Debug.Assert(currentTime >= startTime && currentTime <= endTime, "���̰��� �ƴմϴ�");
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
        // ���� Ŀ�긦 ��ü�մϴ�.
        var binding = new EditorCurveBinding
        {
            path = path,
            type = typeof(Transform), // ��: Transform. �ʿ信 ���� Ÿ���� �����ϼ���.
            propertyName = propertyName
        };

        Debug.Assert(AnimationUtility.GetEditorCurve(sourceClip, binding) != null, "�ش� Ŀ�갡 �����ϴ�");

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

            Debug.Assert(AnimationUtility.GetEditorCurve(sourceClip, binding) != null, "�ش� Ŀ�갡 �����ϴ�");

            AnimationUtility.SetEditorCurve(sourceClip, binding, newRotationCurves[i]);
        }

    }



    private bool isChildBone(string parentBoneName, string childBoneName)
    {
        int parentLength = parentBoneName.Length;

        if (childBoneName.StartsWith(parentBoneName) == false) //�θ���� �������� �ʴ´�
        {
            return false;
        }


        if (childBoneName.Length <= parentBoneName.Length) //�̸����̰� ���ϴ�
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
            
            // Master ���� Position, Rotation, Scale ���� �Ӽ��� ���͸�
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
                    //Debug.Log("Ŀ�갡 �̹� �ִ�");
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
            // Master ���� Position, Rotation, Scale ���� �Ӽ��� ���͸�
            if (binding.path.EndsWith(boneName) == true)
            {
                if (_bonePropertyCurves.ContainsKey(binding.path) == false)
                {
                    _bonePropertyCurves.Add(binding.path, new Dictionary<string, AnimationCurve>());
                }

                Dictionary<string, AnimationCurve> properties = _bonePropertyCurves[binding.path];

                if (properties.ContainsKey(binding.propertyName) == true)
                {
                    //Debug.Log("Ŀ�갡 �̹� �ִ�");
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

        Debug.Assert(false, "���ϴ� Ű�������� �ϳ��� �����ϱ�?");
        return null;
    }




    // �θ��� Curve�� �ڽ��� Curve�� �̿��� ���ο� Curve�� ����ϴ� �Լ�
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

    //        if (HasKeyframeAtTime(childCurve, parentTime, ref resultFrame) == true) //�ڽĿ� �ش� Ÿ�ֿ̹� Ű�������� �ִ�
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
    //        else //�ڽĿ� �ش� Ÿ�ֿ̹� Ű�������� ���� -> ���� �����־�����Ѵ�.
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
    //        // Master ���� Position, Rotation, Scale ���� �Ӽ��� ���͸�
    //        if (binding.path.EndsWith(boneName) == true)
    //        {
    //            if (_bonePropertyCurves.ContainsKey(binding.path) == false)
    //            {
    //                _bonePropertyCurves.Add(binding.path, new Dictionary<string, AnimationCurve>());
    //            }

    //            Dictionary<string, AnimationCurve> properties = _bonePropertyCurves[binding.path];

    //            if (properties.ContainsKey(binding.propertyName) == true)
    //            {
    //                Debug.Log("Ŀ�갡 �̹� �ִ�");
    //            }

    //            AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

    //            properties.Add(binding.propertyName, curve);
    //        }
    //    }

    //    return retCurve;
    //}


    //path = �� �̸�ó�� ����.
    //Path ���� /�� ������ �ʴ´�

    //void ExtractRootTransformData()
    //{
    //    // ��� CurveBinding ��������
    //    EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);

    //    foreach (EditorCurveBinding binding in bindings)
    //    {
    //        // Master ���� Position, Rotation, Scale ���� �Ӽ��� ���͸�
    //        if (binding.path == masterBoneName &&
    //            (binding.propertyName.Contains("localPosition") ||
    //             binding.propertyName.Contains("localRotation") ||
    //             binding.propertyName.Contains("localScale")))
    //        {
    //            // �ش� Ŀ���� AnimationCurve ��������
    //            AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);

    //            Debug.Log($"Property: {binding.propertyName}");

    //            // Ű������ ������ ���
    //            foreach (Keyframe key in curve.keys)
    //            {
    //                Debug.Log($"Time: {key.time}, Value: {key.value}");
    //            }


    //            foreach (Keyframe key in curve.keys)
    //            {
    //                float time = key.time;
    //                int frame = Mathf.RoundToInt(time * _animationFPS); // �ð� ������ ���������� ��ȯ
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

    //        if (isHasChildKeyFrame == false) //������ �Ҳ��� �θ𿡴� Ű�������� ������ �ڽĿ��Դ� ����
    //        {
    //            Quaternion parentQuaternion = new Quaternion(parentCurves[0].keys[i].value, parentCurves[1].keys[i].value, parentCurves[2].keys[i].value, parentCurves[3].keys[i].value);

    //            Quaternion childrenQuaternion = Quaternion.identity; //���� �� Ű������ ����

    //            //�ڽ� Ŀ�� �������� �ش� �ð��� ���ʷ� �پ�Ѵ� Ű �ε����� ������
    //            FindUpKeyFrame2(newKeyFrames[0].ToArray(), parentTime, ref childrenUpKeyFrame);


    //            Keyframe copyFromNext = new Keyframe();

    //            if (childrenUpKeyFrame < 0)  //�� �ð��� �Ѿ Ű�������� ����. ������ Ű�������� �������� �Ѵ�.
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

    //        //���� �̰��� �����ϸ� �θ� Ű�������� �ִ°����� �ݵ�� �ڽ� Ű�������� �ִ�.

    //        //�θ� Ű�������� ���ؾ��ϴ� Ÿ�ٵ��� ã�´�.
    //        List<int> targetFrames = new List<int>();
    //        {
    //            //int startFrame = 0;
    //            //HasKeyframeAtTime2(newKeyFrames[0].ToArray(), parentTime, ref startFrame);
    //            //Debug.Assert(startFrame > 0, "�������� �Ʊ� �־����ϴ� �ݵ�� �־���մϴ�");

    //            //if (i == representParentCurve.keys.Length - 1) //������ ������ Ű�������Դϴ�
    //            //{

    //            //}


    //            //for (int j = 0; j < ; j++)
    //            //{

    //            //}
    //        }


    //    }
    //}





}
