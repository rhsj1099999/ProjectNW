using System.Collections.Generic;
using UnityEngine;
using static AnimationAttackFrameAsset;
using static StateGraphAsset;


public class PlayerScript : CharacterScript
{
    [SerializeField] protected GameObject _interactionUIPrefab = null;
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

            if (Input.GetKeyDown(KeyCode.Period) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 100;
                tempTestDamage._dagingStamina = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl3;
                tempTestDamage._damageType = DamageDesc.DamageType.Damage_Lvl_3;
                DealMe(tempTestDamage, this.gameObject);
            }
        }


        //인벤토리 오픈코드
        {
            if (_inputController.GetInventoryOpen() == true)
            {
                UIManager.Instance.TurnOnUI(_inventoryUIPrefab);
            }
        }

        //TryLockOn
        if (_aimScript != null && Input.GetKeyDown(KeyCode.Mouse2) == true)
        {
            bool isAimed = _aimScript.GetIsAim();

            isAimed = !isAimed;

            if (isAimed == true)
            {
                _aimScript.OnAimState(AimState.eLockOnAim);
            }
            else
            {
                if (_aimScript.GetAimState() != AimState.eTPSAim)
                {
                    _aimScript.OffAimState();
                }
            }
        }
    }



    public override LayerMask CalculateWeaponColliderExcludeLayerMask(ColliderAttachType type, GameObject targetObject)
    {
        int ret = LayerMask.GetMask("Monster");
        return ret;
    }


    /*----------------------------------
    HitAble Section
    ----------------------------------*/
    public override void DealMe(DamageDesc damage, GameObject caller)
    {
        base.DealMe(damage, caller);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (_interactionUIPrefab != null &&
            other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("상호작용 추가!");

            UIInteractionableScript component = other.gameObject.GetComponent<UIInteractionableScript>();

            InteractionUIDesc interactionDesc = component.GetInteractionUIDesc();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().AddList(other, interactionDesc, component);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (_interactionUIPrefab != null &&
            other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("상호작용 제거!");

            UIInteractionableScript component = other.gameObject.GetComponent<UIInteractionableScript>();

            InteractionUIDesc interactionDesc = component.GetInteractionUIDesc();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().RemoveList(other, interactionDesc, component);
        }
    }
}
