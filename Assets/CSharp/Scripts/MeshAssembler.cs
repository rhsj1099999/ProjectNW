using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshAssembler : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer gloveMesh; // ������ �尩 �޽�
    [SerializeField] private SkinnedMeshRenderer playerMesh; // �÷��̾��� ���� ������ �ִ� �޽� ������
    public Animator characterAnimator; // �÷��̾��� ĳ������ Animator
    public Animator partsAnimator; // �÷��̾��� ĳ������ Animator

    public void AttachGlove()
    {
        // �尩�� ĳ���� Ʈ�������� �ڽ����� ����
        gloveMesh.transform.SetParent(characterAnimator.transform.parent);
        gloveMesh.transform.position = characterAnimator.transform.position;

        gloveMesh.rootBone = characterAnimator.GetBoneTransform(HumanBodyBones.Hips); // ���� Hips ���� ��Ʈ ������ �����մϴ�.

        Transform[] mappedBones = new Transform[gloveMesh.bones.Length];

        List<Transform> matchedBones = new List<Transform>();

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform playerBoneTransform = characterAnimator.GetBoneTransform((HumanBodyBones)i);

            Transform partBoneTransform = partsAnimator.GetBoneTransform((HumanBodyBones)i);

            //if (playerBoneTransform != null && partBoneTransform != null)
            //{
            //    //�Ѵ���ġ�Ѵ�.
            //    mappedBones[i] = playerBoneTransform;
            //}
            //else
            //{
            //    mappedBones[i] = gloveMesh.bones[i];
            //}

            if (playerBoneTransform != null && partBoneTransform != null)
            {
                //�Ѵ���ġ�Ѵ�.
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
