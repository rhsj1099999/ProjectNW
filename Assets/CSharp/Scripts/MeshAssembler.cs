using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class MeshAssembler : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer gloveMesh; // ������ �尩 �޽�
    [SerializeField] private SkinnedMeshRenderer playerMesh; // �÷��̾��� ���� ������ �ִ� �޽� ������

    [SerializeField] private Avatar _source;
    [SerializeField] private Avatar _target;
    [SerializeField] private Transform _ownerTransform;

    public Animator characterAnimator; // �÷��̾��� ĳ������ Animator
    public Animator partsAnimator; // �÷��̾��� ĳ������ Animator

    //private bool isRecordReady = false;
    //private bool isRecorded = false;
    //private float ReadyTick = 5.0f;

    public void AttachGlove()
    {
        // �尩�� ĳ���� Ʈ�������� �ڽ����� ����
        //gloveMesh.transform.SetParent(characterAnimator.transform.parent);
        //gloveMesh.transform.position = characterAnimator.transform.position;

        //gloveMesh.rootBone = characterAnimator.GetBoneTransform(HumanBodyBones.Hips); // ���� Hips ���� ��Ʈ ������ �����մϴ�.
        //gloveMesh.bones[1] = characterAnimator.GetBoneTransform(HumanBodyBones.Hips); // ���� Hips ���� ��Ʈ ������ �����մϴ�.
        //gloveMesh.rootBone = _ownerTransform; // ���� Hips ���� ��Ʈ ������ �����մϴ�.
        //gloveMesh.bones[0] = _ownerTransform;
        //gloveMesh.bones[1] = characterAnimator.GetBoneTransform(HumanBodyBones.Hips);


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
                matchedBones.Add(partBoneTransform);
            }

            //if (playerBoneTransform != null && partBoneTransform != null)
            //{
            //    //�Ѵ���ġ�Ѵ�.
            //    matchedBones.Add(playerBoneTransform);
            //}
        }

        gloveMesh.bones = matchedBones.ToArray();
    }



    //void LateUpdate()
    //{




    //    List<Transform> debugBones_Character = new List<Transform>();
    //    List<string> CharacterBoneNameList = new List<string>();

    //    List<Transform> debugBones_Parts = new List<Transform>();
    //    List<string> PartsBoneNameList = new List<string>();

    //    List<Transform> debugBones_Parts_After = new List<Transform>();

    //    // HumanBodyBones �������� ��ȸ�Ͽ� ��� ���� �����մϴ�.
    //    foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
    //    {
    //        if (bone == HumanBodyBones.LastBone) continue; // None �� ����
    //        Transform sourceBone = characterAnimator.GetBoneTransform(bone);
    //        Transform targetBone = partsAnimator.GetBoneTransform(bone);




    //        if (sourceBone != null && targetBone != null)
    //        {
    //            debugBones_Character.Add(sourceBone);
    //            debugBones_Parts.Add(targetBone);
    //            CharacterBoneNameList.Add(sourceBone.name);
    //            PartsBoneNameList.Add(targetBone.name);

    //            targetBone.localPosition = sourceBone.localPosition;
    //            targetBone.localRotation = sourceBone.localRotation;
    //            targetBone.localScale = sourceBone.localScale;


    //            debugBones_Parts_After.Add(targetBone);
    //        }
    //    }

    //    int a = 10;


    //    {

    //        if (isRecordReady == true && isRecorded == false)
    //        {
    //            int i = 0;
    //            string filePath = "C:\\Users\\rhsj\\Downloads\\Test_Character.json";
    //            foreach (Transform transform in debugBones_Character)
    //            {
    //                //string 

    //                string serialize = "Character : ";
    //                serialize = CharacterBoneNameList[i];
    //                serialize += " position : ";
    //                serialize += transform.localPosition.ToString();
    //                serialize += " position : ";
    //                serialize += transform.localRotation.ToString();

    //                string json = JsonUtility.ToJson(serialize, true);

    //                File.AppendAllText(filePath, serialize + Environment.NewLine);
    //                i++;
    //            }


    //            i = 0;
    //            filePath = "C:\\Users\\rhsj\\Downloads\\Test_Parts.json";
    //            foreach (Transform transform in debugBones_Parts)
    //            {
    //                //string 

    //                string serialize = "Parts : ";
    //                serialize = PartsBoneNameList[i];
    //                serialize += " position : ";
    //                serialize += transform.localPosition.ToString();
    //                serialize += " rotation : ";
    //                serialize += transform.localRotation.ToString();

    //                string json = JsonUtility.ToJson(serialize, true);

    //                File.AppendAllText(filePath, serialize + Environment.NewLine);
    //                i++;
    //            }


    //            Debug.Log($"Transform data saved to: {filePath}");
    //            isRecordReady = false;
    //            isRecorded = true;
    //        }

    //        a = 20;
    //    }

    //    ReadyTick -= Time.deltaTime;
    //    if (ReadyTick < 0.0)
    //    {
    //        isRecordReady = true;
    //    }
    //}


    private void CalculateInterpolationMatrix()
    {

    }

    


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) == true)
        {
            AttachGlove();
        }

        //if (Input.GetKeyDown(KeyCode.C) == true)
        //{
        //    isRecordReady = true;
        //}

    }
}
