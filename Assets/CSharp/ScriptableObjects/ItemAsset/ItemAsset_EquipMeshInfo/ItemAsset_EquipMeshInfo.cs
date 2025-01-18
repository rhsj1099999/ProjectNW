using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemAsset_EquipMesh", menuName = "Scriptable Object/Create_ItemAsset_EquipMesh", order = (int)MyUtil.CustomToolOrder.CreateItemAsset)]
public class ItemAsset_EquipMesh : ItemAsset
{
    [SerializeField] private GameObject _equipmentPrefab = null;
    public GameObject _EquipmentPrefab => _equipmentPrefab;

    [SerializeField] private Avatar _equipmentAvatar = null;
    public Avatar _EquipmentAvatar => _equipmentAvatar;

    [SerializeField] private List<GameObject> _equipmentMeshes = new List<GameObject>();
    public List<GameObject> _EquipmentMeshes => _equipmentMeshes;
}
