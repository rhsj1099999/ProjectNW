using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisableScrollRect : ScrollRect
{
    /*-------------------------------------------------------------
    |NOTI| �ƹ��͵� ���Ǹ� ���� �������μ� �巡�׸� ���� ��ũ��Ʈ�Դϴ�.
    -------------------------------------------------------------*/

    public override void OnDrag(PointerEventData eventData) { }
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
