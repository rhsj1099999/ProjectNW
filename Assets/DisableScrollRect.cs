using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisableScrollRect : ScrollRect
{
    /*-------------------------------------------------------------
    |NOTI| 아무것도 정의를 하지 않음으로서 드래그를 막는 스크립트입니다.
    -------------------------------------------------------------*/

    public override void OnDrag(PointerEventData eventData) { }
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
