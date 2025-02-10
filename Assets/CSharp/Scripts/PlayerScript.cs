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

        //HUD가 필요합니다
        if (_usingHUDPrefab != null)
        {
            GameObject canvasObject = UIManager.Instance.GetMainCanvasObject();
            GameObject newHUDObject = Instantiate(_usingHUDPrefab, canvasObject.transform);
            HUDScript newHUDScript = newHUDObject.GetComponent<HUDScript>();

            newHUDScript.HUDLinking(GCST<StatScript>());
        }







        //로직이 이곳에 온다면
        //플레이어의 준비는 끝난다.
        //하지만 준비를 위해 생성된 객체들의 데이터는 존재하지 않는다.
        //근데 일단 플레이어 자체를 안사리지게 한다...
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public override void DeadCall()
    {
        base.DeadCall();

        //씬전환 합시다...
        {
            CurtainCallControl_SimpleColor onDesc = new CurtainCallControl_SimpleColor();
            onDesc._target = false;
            onDesc._runningTime = 2.0f;
            onDesc._color = new Vector3(0.0f, 0.0f, 0.0f);
            CurtainCallControl_SimpleColor offDesc = new CurtainCallControl_SimpleColor();
            offDesc._target = true;
            offDesc._runningTime = 1.0f;
            offDesc._color = new Vector3(0.0f, 0.0f, 0.0f);

            SceneManagerWrapper.Instance.ChangeScene
            (
                "StageScene_Vil2",
                CurtainCallType.SimpleColorFadeInOut,
                onDesc,
                CurtainCallType.SimpleColorFadeInOut,
                offDesc
            );
        }
    }


    protected override void Update()
    {
        base.Update();

        //임시...휠 드래그 소모템 변경 체크
        {
            CheckUseableItemChange();
        }



        if (Input.GetKeyDown(KeyCode.H) == true)
        {
            DamageDesc testDamage = new DamageDesc();
            testDamage._damage = 100;
            DealMe_Test(testDamage);
        }


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
