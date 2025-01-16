using System;
using UnityEngine;
using System.Collections.Generic;

//�������� ����Ҷ� �ʿ��� ������

[CreateAssetMenu(fileName = "ItemSubInfo_UsingInfo", menuName = "Scriptable Object/Create_ItemSubInfo_UsingInfo", order = int.MinValue)]
public class ItemSubInfo_UsingInfo : ItemSubInfo
{
    [SerializeField, ReadOnlyInCode]
    private List<AnimatorLayerTypes> _usingItemMustNotBusyLayers = null;

    [SerializeField, ReadOnlyInCode]
    private int _usingItemMustNotBusyLayer = -1;

    [SerializeField, ReadOnlyInCode]
    private AnimationClip _usingItemAnimation = null;
}