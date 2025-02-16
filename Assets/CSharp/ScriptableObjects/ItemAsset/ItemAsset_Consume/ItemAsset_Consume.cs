using System;
using UnityEngine;
using System.Collections.Generic;

//아이템을 사용할때 필요한 정보들

[CreateAssetMenu(fileName = "ItemAsset_Consume", menuName = "Scriptable Object/Create_ItemAsset_Consume", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Consume : ItemAsset
{
    [SerializeField]
    private List<AnimatorLayerTypes> _usingItemMustNotBusyLayers = null;
    public List<AnimatorLayerTypes> _UsingItemMustNotBusyLayers => _usingItemMustNotBusyLayers;

    [SerializeField]
    private int _usingItemMustNotBusyLayer = -1;
    public int _UsingItemMustNotBusyLayer => _usingItemMustNotBusyLayer;


    //순전히 먹는 애니메이션이다
    //이게 끝나면 스킬을 적용한다.
    [SerializeField]
    private AnimationClip _usingItemAnimation_Phase1 = null;
    public AnimationClip _UsingItemAnimation_Phase1 => _usingItemAnimation_Phase1;

    private List<BuffAssetBase> _buffs = new List<BuffAssetBase>();
    public List<BuffAssetBase> _Buffs = new List<BuffAssetBase>();


    //이 애니메이션이 존재한다? 지금은 ReUseable 애니메이션이다
    //나중에 ReUseable을 bool 변수로 빼고 다른 용도로 쓰던가...
    [SerializeField]
    private AnimationClip _usingItemAnimation_Phase2 = null;
    public AnimationClip _UsingItemAnimation_Phase2 => _usingItemAnimation_Phase2;
}