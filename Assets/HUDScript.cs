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
    |NOTI| HUD 가 초기화되는 순간, 동시에 연결해줄 StatBar들을 '미리' 지정해둡니다.
    이것 앞으로 어떤 HUD든, 그 HUD의 스타일이라고 생각합니다
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
            Debug.Assert(false, "인스펙터에서 버프 표시기를 설정하세요");
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
