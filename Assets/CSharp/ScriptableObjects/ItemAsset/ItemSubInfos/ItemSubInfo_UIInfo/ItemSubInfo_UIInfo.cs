using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ItemSubInfo_UIInfo", menuName = "Scriptable Object/Create_ItemSubInfo_UIInfo", order = int.MinValue)]
public class ItemSubInfo_UIInfo : ItemSubInfo
{
    [SerializeField] private Image _itemImage = null;
    public Image _ItemImage => _itemImage;
}
