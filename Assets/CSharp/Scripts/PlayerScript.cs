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
        |NOTI| �� ���� �Ӹ� �ƴ϶� �׳� ���������� �����ؼ��� TPS �ý����� �غ��Ѵ�
        -----------------------------------------------------------------*/
        _inputController = GetComponent<InputController>();
        Debug.Assert(_inputController != null, "��ǲ ��Ʈ�ѷ��� ����");
        ReadyAimSystem();
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


        //�κ��丮 �����ڵ�
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
            Debug.Log("��ȣ�ۿ� �߰�!");

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
            Debug.Log("��ȣ�ۿ� ����!");

            UIInteractionableScript component = other.gameObject.GetComponent<UIInteractionableScript>();

            InteractionUIDesc interactionDesc = component.GetInteractionUIDesc();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().RemoveList(other, interactionDesc, component);
        }
    }
}
