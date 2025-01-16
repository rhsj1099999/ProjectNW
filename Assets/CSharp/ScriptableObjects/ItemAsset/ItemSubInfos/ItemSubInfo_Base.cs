using System;
using UnityEngine;

public class ItemSubInfo : ScriptableObject
{
    //이걸 사용할 아이템 인포들
    [SerializeField] private ItemAsset _usingThisItemAssets = null;
    public ItemAsset _UsingThisItemAssets => _usingThisItemAssets;


    /*---------------------------------------------------------
     여기가 공통 부모니까

    타입이 있어야한다
     
    이름

    크기


     
    ---------------------------------------------------------*/
}
