using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshAssembler : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer gloveMesh; // 장착할 장갑 메쉬
    [SerializeField] private SkinnedMeshRenderer playerMesh; // 플레이어의 본을 가지고 있는 메쉬 렌더러
    public Animator characterAnimator; // 플레이어의 캐릭터의 Animator
    public Animator partsAnimator; // 플레이어의 캐릭터의 Animator

    public void AttachGlove()
    {
        // 장갑을 캐릭터 트랜스폼의 자식으로 부착
        gloveMesh.transform.SetParent(characterAnimator.transform.parent);
        gloveMesh.transform.position = characterAnimator.transform.position;

        gloveMesh.rootBone = characterAnimator.GetBoneTransform(HumanBodyBones.Hips); // 보통 Hips 본을 루트 본으로 설정합니다.

        Transform[] mappedBones = new Transform[gloveMesh.bones.Length];

        List<Transform> matchedBones = new List<Transform>();

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform playerBoneTransform = characterAnimator.GetBoneTransform((HumanBodyBones)i);

            Transform partBoneTransform = partsAnimator.GetBoneTransform((HumanBodyBones)i);

            //if (playerBoneTransform != null && partBoneTransform != null)
            //{
            //    //둘다일치한다.
            //    mappedBones[i] = playerBoneTransform;
            //}
            //else
            //{
            //    mappedBones[i] = gloveMesh.bones[i];
            //}

            if (playerBoneTransform != null && partBoneTransform != null)
            {
                //둘다일치한다.
                matchedBones.Add(playerBoneTransform);
            }
            else
            {
                matchedBones.Add(gloveMesh.bones[i]);
            }
        }

        gloveMesh.bones = matchedBones.ToArray();
    }




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) == true)
        {
            AttachGlove();
        }
    }
}
