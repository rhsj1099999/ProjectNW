using System.Collections.Generic;
using UnityEngine;
using static AnimationFrameDataAsset;
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


        //인벤토리 오픈코드
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



    public override LayerMask CalculateWeaponColliderIncludeLayerMask()
    {
        return LayerMask.GetMask("Monster");
    }


    private void WhenTriggerEnterWithInteraction(Collider other)
    {
        if (_interactionUIPrefab != null &&
            other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("상호작용 추가!");

            UICallScript component = other.gameObject.GetComponent<UICallScript>();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().AddList(component);
        }
    }

    private void WhenTriggerExitWithInteraction(Collider other)
    {
        if (_interactionUIPrefab != null &&
             other.gameObject.layer == LayerMask.NameToLayer("InteractionableCollider"))
        {
            Debug.Log("상호작용 제거!");

            UICallScript component = other.gameObject.GetComponent<UICallScript>();

            _interactionUIPrefab.GetComponentInChildren<InteractionUIListScript>().RemoveList(component);
        }
    }
}
