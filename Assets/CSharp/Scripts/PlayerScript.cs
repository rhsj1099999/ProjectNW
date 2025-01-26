using System.Collections.Generic;
using UnityEngine;
using static AnimationAttackFrameAsset;
using static StateGraphAsset;


public class PlayerScript : CharacterScript
{
    [SerializeField] protected GameObject _interactionUIPrefab = null;




    protected override void Awake()
    {
        base.Awake();

        TriggerSender_UIInteraction triggerSender = GetComponentInChildren<TriggerSender_UIInteraction>();
        triggerSender.SubscribeMe(TriggerSender_UIInteraction.TriggerType.Enter, WhenTriggerEnterWithInteraction);
        triggerSender.SubscribeMe(TriggerSender_UIInteraction.TriggerType.Exit, WhenTriggerExitWithInteraction);
    }

    protected override void Update()
    {
        base.Update();


        //�ӽ� Hit ����� �ڵ�
        {
            if (Input.GetKeyDown(KeyCode.H) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl0;
                DealMe_Final(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.J) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl1;
                DealMe_Final(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.K) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl2;
                DealMe_Final(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.M) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 0;
                tempTestDamage._damagingStamina = 200;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl0;
                DealMe_Final(tempTestDamage, this.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.Period) == true)
            {
                DamageDesc tempTestDamage = new DamageDesc();
                tempTestDamage._damage = 100;
                tempTestDamage._damagingStamina = 0;
                tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl3;
                DealMe_Final(tempTestDamage, this.gameObject);
            }
        }


        //�κ��丮 �����ڵ�
        {
            if (GCST<InputController>().GetInventoryOpen() == true)
            {
                UIManager.Instance.TurnOnUI(_inventoryUIPrefab);
            }
        }

        //TryLockOn
        if (GCST<AimScript2>() != null && Input.GetKeyDown(KeyCode.Mouse2) == true)
        {
            bool isAimed = GCST<AimScript2>().GetIsAim();

            isAimed = !isAimed;

            if (isAimed == true)
            {
                GCST<AimScript2>().OnAimState(AimState.eLockOnAim);
            }
            else
            {
                if (GCST<AimScript2>().GetAimState() != AimState.eTPSAim)
                {
                    GCST<AimScript2>().OffAimState();
                }
            }
        }
    }



    public override LayerMask CalculateWeaponColliderExcludeLayerMask(ColliderAttachType type, GameObject targetObject)
    {
        int ret = LayerMask.GetMask("Monster");
        return ret;
    }


    private void WhenTriggerEnterWithInteraction(Collider other)
    {
        if (_interactionUIPrefab != null &&
            other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("��ȣ�ۿ� �߰�!");

            UICallScript component = other.gameObject.GetComponent<UICallScript>();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().AddList(component);
        }
    }

    private void WhenTriggerExitWithInteraction(Collider other)
    {
        if (_interactionUIPrefab != null &&
             other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("��ȣ�ۿ� ����!");

            UICallScript component = other.gameObject.GetComponent<UICallScript>();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().RemoveList(component);
        }
    }
}
