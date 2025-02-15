using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    private StatScript _owner = null;

    [SerializeField] private BuffDisplayScript _buffDisplay = null;
    public BuffDisplayScript _BuffDisplay => _buffDisplay;
    /*--------------------------------------------------------------------------------------------------
    |NOTI| HUD �� �ʱ�ȭ�Ǵ� ����, ���ÿ� �������� StatBar���� '�̸�' �����صӴϴ�.
    �̰� ������ � HUD��, �� HUD�� ��Ÿ���̶�� �����մϴ�
    --------------------------------------------------------------------------------------------------*/
    [SerializeField] private List<StatBarScript> _statBarScriptsForInit = new List<StatBarScript>();
    [SerializeField] private List<InventoryHUDScript> _inventoryHUDsForInit = new List<InventoryHUDScript>();
    private Dictionary<InventoryHUDScript.InventoryHUDType, InventoryHUDScript> _inventoryHUDs = new Dictionary<InventoryHUDScript.InventoryHUDType, InventoryHUDScript>();


    public InventoryHUDScript GetInventoryHUDScript(InventoryHUDScript.InventoryHUDType type)
    {
        return _inventoryHUDs[type];
    }


    public void HUDLinking(StatScript caller)
    {
        _owner = caller;

        UIManager.Instance.SetHUD(gameObject, this);

        if (_buffDisplay == null)
        {
            Debug.Assert(false, "�ν����Ϳ��� ���� ǥ�ñ⸦ �����ϼ���");
            Debug.Break();
        }

        foreach (StatBarScript statBarScript in _statBarScriptsForInit)
        {
            statBarScript.StatLinking(_owner);
        }

        foreach (InventoryHUDScript inventoryHUDScript in _inventoryHUDsForInit)
        {
            _inventoryHUDs.Add(inventoryHUDScript._InventoryHUDType, inventoryHUDScript);
        }
    }



    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.DestroyHUD(this);
        }
    }
}
