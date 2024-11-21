using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ItemAsset", menuName = "Scriptable Object/CreateItemAsset", order = int.MinValue)]
public class ItemAsset : ScriptableObject
{
    [SerializeField] public ItemInfo _itemInfo;
}
