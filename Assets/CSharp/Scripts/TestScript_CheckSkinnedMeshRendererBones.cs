using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript_CheckSkinnedMeshRendererBones : MonoBehaviour
{

    private SkinnedMeshRenderer _myRenderer = null;


    private void Awake()
    {
        _myRenderer = GetComponent<SkinnedMeshRenderer>();
    }


    private void ChangeBone()
    {
        // 연결된 모든 뼈 출력
        Transform[] bones = _myRenderer.bones;
        Animator animator = GetComponentInParent<CharacterAnimatorScript>().GetCurrActivatedAnimator();

        HashSet<Transform> transforms = new HashSet<Transform>();
        foreach (Transform bone in bones)
        {
            transforms.Add(bone);
        }

        

        //for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        //{
        //    HumanBodyBones boneIndex = (HumanBodyBones)i;

        //    animator.GetBoneTransform(boneIndex)
        //}
    }




    void Update()
    {
        if (_myRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer가 지정되지 않았습니다.");
            return;
        }

        // 루트 뼈 출력
        if (_myRenderer.rootBone != null)
        {
            Debug.Log($"Root Bone: {_myRenderer.rootBone.name}");
        }
        else
        {
            Debug.Log("Root Bone이 설정되지 않았습니다.");
        }

        // 연결된 모든 뼈 출력
        Transform[] bones = _myRenderer.bones;
        foreach (Transform bone in bones)
        {
            Debug.Log($"Bone: {bone.name}");
        }
    }
}
