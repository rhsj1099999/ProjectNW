using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatScript
{
    public class StatScriptDesc
    {
        public int _hp = 10;       //ü��
        public int _stamina = 10;  //����
        public int _roughness = 1;  //���ε�(�ڼ��� �����Ϸ��� ��)
        public int _strength = 1;
        public int _level = 1;
    }







    public StatScriptDesc _baseDesc = new StatScriptDesc();
    public StatScriptDesc _runtimeDesc = new StatScriptDesc();

    public StatScriptDesc GetBaseStatDesc() { return _baseDesc; }
    public StatScriptDesc GetRuntimeStatDesc(){return _runtimeDesc;}

    public int CalculateStatDamage()
    {
        return 1;
    }

    public int CalculateStatDamagingStamina()
    {
        return 1;
    }

    public int CalculatePower()
    {
        return 1;
    }
}
