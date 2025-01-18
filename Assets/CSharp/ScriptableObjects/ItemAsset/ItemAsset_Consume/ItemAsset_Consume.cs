using System;
using UnityEngine;
using System.Collections.Generic;

//�������� ����Ҷ� �ʿ��� ������

[CreateAssetMenu(fileName = "ItemAsset_Consume", menuName = "Scriptable Object/Create_ItemAsset_Consume", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_Consume : ItemAsset
{
    [SerializeField]
    private List<AnimatorLayerTypes> _usingItemMustNotBusyLayers = null;
    public List<AnimatorLayerTypes> _UsingItemMustNotBusyLayers => _usingItemMustNotBusyLayers;

    [SerializeField]
    private int _usingItemMustNotBusyLayer = -1;
    public int _UsingItemMustNotBusyLayer => _usingItemMustNotBusyLayer;

    [SerializeField]
    private AnimationClip _usingItemAnimation = null;
    public AnimationClip _UsingItemAnimation => _usingItemAnimation;
}