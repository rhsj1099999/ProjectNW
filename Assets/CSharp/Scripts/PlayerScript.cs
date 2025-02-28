using System.Collections.Generic;
using UnityEngine;


public class PlayerScript : CharacterScript
{
    [SerializeField] protected GameObject _usingHUDPrefab = null;

    [SerializeField] protected GameObject _interactionUIPrefab = null;



    protected override void Awake()
    {
        base.Awake();

        TriggerSender_UIInteraction triggerSender = GetComponentInChildren<TriggerSender_UIInteraction>();
        triggerSender.SubscribeMe(TriggerSender_UIInteraction.TriggerType.Enter, WhenTriggerEnterWithInteraction);
        triggerSender.SubscribeMe(TriggerSender_UIInteraction.TriggerType.Exit, WhenTriggerExitWithInteraction);

        //HUD�� �ʿ��մϴ�
        if (_usingHUDPrefab != null)
        {
            Canvas canvasObject = UIManager.Instance.Get2DCanvs();
            GameObject newHUDObject = Instantiate(_usingHUDPrefab, canvasObject.transform);
            HUDScript newHUDScript = newHUDObject.GetComponent<HUDScript>();

            newHUDScript.HUDLinking(GCST<StatScript>());
        }




        //������ �̰��� �´ٸ�
        //�÷��̾��� �غ�� ������.
        //������ �غ� ���� ������ ��ü���� �����ʹ� �������� �ʴ´�.
        //�ٵ� �ϴ� �÷��̾� ��ü�� �Ȼ縮���� �Ѵ�...
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public override void DeadCall()
    {
        base.DeadCall();

        //����ȯ �սô�...
        {
            CurtainCallControl_SimpleColor onDesc = new CurtainCallControl_SimpleColor();
            onDesc._target = false;
            onDesc._runningTime = 1.0f;
            onDesc._color = new Vector3(0.0f, 0.0f, 0.0f);
            CurtainCallControl_SimpleColor offDesc = new CurtainCallControl_SimpleColor();
            offDesc._target = true;
            offDesc._runningTime = 2.0f;
            offDesc._color = new Vector3(0.0f, 0.0f, 0.0f);

            SceneManagerWrapper.Instance.ChangeScene
            (
                "StageScene_Vil2",
                CurtainCallType.SimpleColorFadeInOut,
                onDesc,
                CurtainCallType.SimpleColorFadeInOut,
                offDesc,
                true
            );
        }
    }


    protected override void Update()
    {
        base.Update();

        //�ӽ�...�� �巡�� �Ҹ��� ���� üũ
        {
            CheckUseableItemChange();
        }



        //�κ��丮 �����ڵ�
        {
            if (GCST<InputController>().GetInventoryOpen() == true)
            {
                UIManager.Instance.TurnOnUI(_inventoryUIPrefab, UIManager.LayerOrder.InventorySomethingElse);
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
