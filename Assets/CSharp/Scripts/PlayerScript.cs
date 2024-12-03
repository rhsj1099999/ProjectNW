using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static StateGraphAsset;






public class PlayerScript : CharacterScript
{
    [SerializeField] protected InputController _inputController = null;


    protected override void Awake()
    {
        base.Awake();

        /*-----------------------------------------------------------------
        |NOTI| �� ���� �Ӹ� �ƴ϶� �׳� ���������� �����ؼ��� TPS �ý����� �غ��Ѵ�
        -----------------------------------------------------------------*/
        _inputController = GetComponent<InputController>();
        Debug.Assert(_inputController != null, "��ǲ ��Ʈ�ѷ��� ����");
        ReadyAimSystem();
        _aimScript.enabled = false;
    }




    protected override void Update()
    {
        base.Update();
        //Ÿ�ӵ����
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.L))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.O))  // RŰ�� ������ ���� �ӵ��� �������� ����
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        //�ӽ� Hit ����� �ڵ�
        {
            if (Input.GetKeyDown(KeyCode.H) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl0;
                tempTestDamage._damageType = DamageDesc.DamageType.Damage_Lvl_0;
                DealMe(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.J) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl1;
                tempTestDamage._damageType = DamageDesc.DamageType.Damage_Lvl_1;
                DealMe(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.K) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl2;
                tempTestDamage._damageType = DamageDesc.DamageType.Damage_Lvl_2;
                DealMe(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.M) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._dagingStamina = 200;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl0;
                tempTestDamage._damageType = DamageDesc.DamageType.Damage_Lvl_0;
                DealMe(tempTestDamage, this.gameObject);
            }
        }

        //�ӽ� ������ �ڵ�
        {
            //ChangeWeaponHandling();
        }

        //�ӽ� ���⺯�� �ڵ� -> State�� BeHave�� üũ�ؾ��Ѵ�.
        {
            //WeaponChangeCheck2();
        }

        //�ӽ� ������ ��� �ڵ�
        {
            //UseItemCheck();
        }

        //Temp Aim
        if (_aimScript != null && _aimScript.enabled == true)
        {
            //bool isAimed = Input.GetButton("Fire2");

            //if (isAimed != _isAim)
            //{
            //    _isAim = isAimed;

            //    if (isAimed == true)
            //    {
            //        _aimScript.OnAimState();

            //        if (_currRightWeaponIndex == 1)
            //        {
            //            _tempRightWeapons[1].TurnOnAim();
            //        }
            //    }
            //    else
            //    {
            //        _aimScript.OffAimState();

            //        if (_tempRightWeapons[_currRightWeaponIndex] != null)
            //        {
            //            _tempRightWeapons[_currRightWeaponIndex].TurnOffAim();
            //        }
            //    }
            //}
        }
    }

    /*----------------------------------
    HitAble Section
    ----------------------------------*/
    public override void DealMe(DamageDesc damage, GameObject caller)
    {
        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.Hit_Lvl_0;

        int willUsingHP = damage._damage;
        int willUsingStamina = damage._dagingStamina;
        int willUsingRoughness = damage._damagePower;

        //���� ����, ����, ������ Ÿ�Ե��� ���غ�
        {
            //ü���� �� �ްų�, �� �ްų�


            //BuffScript guardBuff = null;
            //_currBuffs.TryGetValue(BuffTypes./* TARGET BUFF*/ , out guardBuff);
            //if (guardBuff != null)
            //{
            //    GameObject result = guardBuff.CallFunc();
            //}

            //representType�� ����ȴ�
            //willUsingHealthPoint�� ����ȴ�
            //willUsingStaminaPoint�� ����ȴ�.
        }

        StateAsset currState = _stateContoller.GetCurrState();
        StateDesc currStateDesc = _stateContoller.GetCurrState()._myState;

        //�������̿���?
        if (currStateDesc._isBlockState == true)
        {
            //���׹̳��� ����ϰ� ���ε��� ����մϴ�
            if (_myStat._currStamina >= willUsingStamina &&
                _myStat._currRoughness >= willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Reaction;
            }

            //���׹̳��� ����ѵ� ���ε��� �����մϴ�.
            else if (_myStat._currStamina >= willUsingStamina &&
                _myStat._currRoughness < willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Sliding;
            }

            //���ε��� ����ѵ� ���׹̳��� �����մϴ�.
            else if (_myStat._currStamina < willUsingStamina &&
                _myStat._currRoughness >= willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Crash;
            }

            //����� ���µ��� �����ͺ�
            StateAsset nextStateAsseet = null;
            List<LinkedStateAsset> linkedStates = _stateContoller.GetCurrStateGraph().GetGraphStates()[currState];
            foreach (LinkedStateAsset linkedState in linkedStates)
            {
                if (linkedState._linkedState._myState._stateType == representType)
                {
                    nextStateAsseet = linkedState._linkedState;
                    break;
                }
            }

            //���׹̳��� �����ϰ� ���ε��� �����մϴ�. Ȥ�� ������°� �������� �ʽ��ϴ�
            if ((_myStat._currStamina < willUsingStamina && _myStat._currRoughness < willUsingRoughness) ||
                nextStateAsseet == null)
            {
                //�´� ���·� ���� �Ұǵ�
                nextGraphType = StateGraphType.HitStateGraph;

                int deltaRoughness = willUsingRoughness - _myStat._currRoughness;

                if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //���ε��� ���� �����ϴ�
                {
                    representType = RepresentStateType.Hit_Lvl_0;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //���ε��� ���� �����ϴ�
                {
                    representType = RepresentStateType.Hit_Lvl_1;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl2) //���ε��� ���ϰ� �����ϴ�
                {
                    representType = RepresentStateType.Hit_Lvl_2;
                }
            }
        }
        else
        {
            //�´� ���·� ���� �Ұǵ�
            nextGraphType = StateGraphType.HitStateGraph;

            int deltaRoughness = willUsingRoughness - _myStat._currRoughness;

            if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //���ε��� ���� �����ϴ�
            {
                representType = RepresentStateType.Hit_Lvl_0;
            }
            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //���ε��� ���� �����ϴ�
            {
                representType = RepresentStateType.Hit_Lvl_1;
            }
            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl2) //���ε��� ���ϰ� �����ϴ�
            {
                representType = RepresentStateType.Hit_Lvl_2;
            }
        }

        _stateContoller.TryChangeState(nextGraphType, representType);
    }
}
