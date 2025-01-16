using System;
using UnityEngine;
using System.Collections.Generic;

//아이템을 사용할때 필요한 정보들

[CreateAssetMenu(fileName = "ItemSubInfo_UsingInfo", menuName = "Scriptable Object/Create_ItemSubInfo_UsingInfo", order = int.MinValue)]
public class ItemSubInfo_UsingInfo : ItemSubInfo
{
    [SerializeField]
    private List<AnimatorLayerTypes> _usingItemMustNotBusyLayers = null;

    [SerializeField]
    private int _usingItemMustNotBusyLayer = -1;

    [SerializeField]
    private AnimationClip _usingItemAnimation = null;
}