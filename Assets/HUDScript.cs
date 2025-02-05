using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{
    /*
     * ����
     * ����
     * ���� ������
    */

    [SerializeField] private BuffDisplayScript _buffDisplay = null;
    public BuffDisplayScript _BuffDisplay => _buffDisplay;

    private void Awake()
    {
        if (_buffDisplay == null) 
        {
            Debug.Assert(false, "�ν����Ϳ��� ���� ǥ�ñ⸦ �����ϼ���");
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
