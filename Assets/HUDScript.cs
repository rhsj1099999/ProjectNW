using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    /*
     * 여러
     * 가지
     * 스택 막대기들
    */

    [SerializeField] private BuffDisplayScript _buffDisplay = null;
    public BuffDisplayScript _BuffDisplay => _buffDisplay;

    private void Awake()
    {
        if (_buffDisplay == null) 
        {
            Debug.Assert(false, "인스펙터에서 버프 표시기를 설정하세요");
            Debug.Break();
        }


        UIManager.Instance.SetHUD(this);
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null) 
        {
            UIManager.Instance.DestroyHUD(this);
        }
    }



    
}
