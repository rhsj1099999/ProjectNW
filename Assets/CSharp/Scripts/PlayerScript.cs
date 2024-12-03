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
        |NOTI| 총 무기 뿐만 아니라 그냥 무기투적을 생각해서라도 TPS 시스템을 준비한다
        -----------------------------------------------------------------*/
        _inputController = GetComponent<InputController>();
        Debug.Assert(_inputController != null, "인풋 컨트롤러가 없다");
        ReadyAimSystem();
        _aimScript.enabled = false;
    }




    protected override void Update()
    {
        base.Update();
        //타임디버깅
        {
            if (Input.GetKeyDown(KeyCode.Slash))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.L))  // S키를 누르면 게임 속도를 느리게 함
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // 물리적 시간 업데이트
            }

            if (Input.GetKeyDown(KeyCode.O))  // R키를 누르면 게임 속도를 정상으로 복원
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        //임시 Hit 디버깅 코드
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

        //임시 양손잡기 코드
        {
            //ChangeWeaponHandling();
        }

        //임시 무기변경 코드 -> State가 BeHave를 체크해야한다.
        {
            //WeaponChangeCheck2();
        }

        //임시 아이템 사용 코드
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

        //현재 스텟, 버프, 데미지 타입등을 비교해봄
        {
            //체력이 더 달거나, 덜 달거나


            //BuffScript guardBuff = null;
            //_currBuffs.TryGetValue(BuffTypes./* TARGET BUFF*/ , out guardBuff);
            //if (guardBuff != null)
            //{
            //    GameObject result = guardBuff.CallFunc();
            //}

            //representType이 변경된다
            //willUsingHealthPoint가 변경된다
            //willUsingStaminaPoint가 변경된다.
        }

        StateAsset currState = _stateContoller.GetCurrState();
        StateDesc currStateDesc = _stateContoller.GetCurrState()._myState;

        //가드중이였나?
        if (currStateDesc._isBlockState == true)
        {
            //스테미나도 충분하고 강인도도 충분합니다
            if (_myStat._currStamina >= willUsingStamina &&
                _myStat._currRoughness >= willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Reaction;
            }

            //스테미나는 충분한데 강인도가 부족합니다.
            else if (_myStat._currStamina >= willUsingStamina &&
                _myStat._currRoughness < willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Sliding;
            }

            //강인도는 충분한데 스테미나가 부족합니다.
            else if (_myStat._currStamina < willUsingStamina &&
                _myStat._currRoughness >= willUsingRoughness)
            {
                nextGraphType = _stateContoller.GetCurrStateGraphType();
                representType = RepresentStateType.Blocked_Crash;
            }

            //연결된 상태들을 가져와봄
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

            //스테미나가 부족하고 강인도도 부족합니다. 혹은 연결상태가 존재하지 않습니다
            if ((_myStat._currStamina < willUsingStamina && _myStat._currRoughness < willUsingRoughness) ||
                nextStateAsseet == null)
            {
                //맞는 상태로 가긴 할건데
                nextGraphType = StateGraphType.HitStateGraph;

                int deltaRoughness = willUsingRoughness - _myStat._currRoughness;

                if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //강인도가 조금 부족하다
                {
                    representType = RepresentStateType.Hit_Lvl_0;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //강인도가 많이 부족하다
                {
                    representType = RepresentStateType.Hit_Lvl_1;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl2) //강인도가 심하게 부족하다
                {
                    representType = RepresentStateType.Hit_Lvl_2;
                }
            }
        }
        else
        {
            //맞는 상태로 가긴 할건데
            nextGraphType = StateGraphType.HitStateGraph;

            int deltaRoughness = willUsingRoughness - _myStat._currRoughness;

            if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //강인도가 조금 부족하다
            {
                representType = RepresentStateType.Hit_Lvl_0;
            }
            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //강인도가 많이 부족하다
            {
                representType = RepresentStateType.Hit_Lvl_1;
            }
            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl2) //강인도가 심하게 부족하다
            {
                representType = RepresentStateType.Hit_Lvl_2;
            }
        }

        _stateContoller.TryChangeState(nextGraphType, representType);
    }
}
