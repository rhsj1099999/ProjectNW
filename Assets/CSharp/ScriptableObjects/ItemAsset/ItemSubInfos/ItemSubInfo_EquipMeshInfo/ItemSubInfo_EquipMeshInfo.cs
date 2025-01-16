using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemSubInfo_EquipMesh", menuName = "Scriptable Object/Create_ItemSubInfo_EquipMesh", order = int.MinValue)]
public class ItemSubInfo_EquipMesh : ItemSubInfo
{
    [SerializeField] private GameObject _equipmentPrefab = null;
    public GameObject _EquipmentPrefab => _equipmentPrefab;

    [SerializeField] private Avatar _equipmentAvatar = null;
    public Avatar _EquipmentAvatar => _equipmentAvatar;

    [SerializeField] private List<GameObject> _equipmentMeshes = new List<GameObject>();
    public List<GameObject> _EquipmentMeshes => _equipmentMeshes;
}
