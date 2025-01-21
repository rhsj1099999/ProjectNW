using System;
using UnityEngine;
using System.Collections.Generic;
using static ItemAsset_Weapon;

//아이템을 사용할때 필요한 정보들

[CreateAssetMenu(fileName = "ItemSubInfo_GunAnimation", menuName = "Scriptable Object/Create_ItemSubInfo_GunAnimation", order = (int)MyUtil.CustomToolOrder.CreateItemSubInfo)]
public class ItemSubInfo_GunAnimation : ScriptableObject
{
    [SerializeField] private ItemAsset _usingThisAsset;
    public ItemAsset _UsingThisAsset => _usingThisAsset;


    /*------------------------------------------
    PutAway/Draw AnimationClips Section.
    ------------------------------------------*/
    [SerializeField] private AnimationClip _fireAnimation = null;
    public AnimationClip _FireAnimation => _fireAnimation;

    [SerializeField] private AnimationClip _reloadAnimation = null;
    public AnimationClip _ReloadAnimation => _reloadAnimation;
}
