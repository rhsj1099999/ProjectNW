using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHUDScript : MonoBehaviour
{
    public enum InventoryHUDType
    {
        LeftWeapon,
        RightWeapon,
        Useable,
        End,
    }

    [SerializeField] private Image _itemImageComponent = null;
    [SerializeField] private InventoryHUDType _inventoryHUDType = InventoryHUDType.End;
    public InventoryHUDType _InventoryHUDType => _inventoryHUDType;

    private void Awake()
    {
        if ( _itemImageComponent == null )
        {
            Debug.Assert(false, "�������� ������ UI�� �����ϼ���");
            Debug.Break();
        }

        if (_inventoryHUDType == InventoryHUDType.End)
        {
            Debug.Assert(false, "HUD UI Type�� �����ϼ���");
            Debug.Break();
        }

        _itemImageComponent.preserveAspect = true;

        _itemImageComponent.enabled = !(_itemImageComponent.sprite == null);
    }

    public void SetImage(Sprite itemSprite)
    {
        _itemImageComponent.sprite = itemSprite;

        _itemImageComponent.enabled = !(_itemImageComponent.sprite == null);
    }
}
