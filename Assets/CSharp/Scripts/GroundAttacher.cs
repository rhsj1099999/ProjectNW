using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-------------------------------------------
 * |Obsoleted|-------------------------------
-------------------------------------------*/

public class GroundAttacher : MonoBehaviour
{
    //[SerializeField] private GameObject _downRayPoint1 = null;
    //[SerializeField] private GameObject _downRayPoint2 = null;
    //[SerializeField] private GameObject _massCenter = null;
    //[SerializeField] private CharacterContollerable _controller = null;



    //private void Awake()
    //{
    //    if (_downRayPoint1 == null ||
    //        _downRayPoint2 == null ||
    //        _massCenter == null)
    //    {
    //        Debug.Assert(false, "���ӿ�����Ʈ�� �Ҵ��ϼ���");
    //        Debug.Break();
    //    }
    //}

    //RaycastHit[] firstHit;
    //RaycastHit[] secondHit;

    //private void LateUpdate()
    //{
    //    if (_controller.GetIsInAir() == false)
    //    {
    //        return;
    //    }

    //    float radius = 1.0f;

    //    int firstGrounded = Physics.SphereCastNonAlloc(_downRayPoint1.transform.position, radius, Vector3.down, firstHit, LayerMask.GetMask("StaticNavMeshLayer"));
    //    int secondGrounded = Physics.SphereCastNonAlloc(_downRayPoint2.transform.position, radius, Vector3.down, secondHit, LayerMask.GetMask("StaticNavMeshLayer"));


    //    if (firstGrounded >= 1 && secondGrounded >= 0)
    //    {
    //        Vector3 dir = (_downRayPoint2.transform.position - _downRayPoint1.transform.position).normalized;
    //        //�Ѵ� �پ��ִ�
    //        //�� �����̼��� ��->�� ����
    //    }
    //    else if (firstGrounded <= 1 && secondGrounded <= 0)
    //    {
    //        //�ִ��� ���鿡 ���ִ� �����̿������
    //    }
    //    else
    //    {
    //        //�ϳ��� �پ��ִ�
    //        //�� �����̼��� ����->�� / ��->���� ����
    //    }
    //}
}
