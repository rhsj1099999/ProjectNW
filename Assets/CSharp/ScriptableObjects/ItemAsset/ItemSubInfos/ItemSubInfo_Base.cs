using System;
using UnityEngine;

public class ItemSubInfo : ScriptableObject
{
    //�̰� ����� ������ ������
    [SerializeField] private ItemAsset _usingThisItemAssets = null;
    public ItemAsset _UsingThisItemAssets => _usingThisItemAssets;
}
