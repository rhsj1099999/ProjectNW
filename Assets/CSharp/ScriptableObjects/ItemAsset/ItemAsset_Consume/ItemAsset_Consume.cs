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


    //������ �Դ� �ִϸ��̼��̴�
    //�̰� ������ ��ų�� �����Ѵ�.
    [SerializeField]
    private AnimationClip _usingItemAnimation_Phase1 = null;
    public AnimationClip _UsingItemAnimation_Phase1 => _usingItemAnimation_Phase1;

    private List<BuffAssetBase> _buffs = new List<BuffAssetBase>();
    public List<BuffAssetBase> _Buffs = new List<BuffAssetBase>();


    //�� �ִϸ��̼��� �����Ѵ�? ������ ReUseable �ִϸ��̼��̴�
    //���߿� ReUseable�� bool ������ ���� �ٸ� �뵵�� ������...
    [SerializeField]
    private AnimationClip _usingItemAnimation_Phase2 = null;
    public AnimationClip _UsingItemAnimation_Phase2 => _usingItemAnimation_Phase2;
}