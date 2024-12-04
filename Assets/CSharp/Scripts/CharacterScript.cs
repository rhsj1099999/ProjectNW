using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimatorBlendingDesc;
using static BodyPartBlendingWork;
using static StateGraphAsset;

public enum AnimatorLayerTypes
{
    FullBody = 0,
    LeftHand,
    RightHand,
    Head,
    Body,
    LeftLeg,
    RightLeg,
    End,
}


public enum AdditionalBehaveType
{
    ChangeWeapon,
    ChangeFocus,
    UseItem_Drink,
    UseItem_Break,
    UseItem_Throw,
}

public enum WeaponGrabFocus
{
    Normal,
    RightHandFocused,
    LeftHandFocused,
    DualGrab,
}

public class CoroutineLock
{
    public bool _isEnd = false;
}

public class DamageDesc
{
    public enum DamageType
    {
        Damage_Lvl_0 = 0,
        Damage_Lvl_1,
        Damage_Lvl_2,
        Damage_Lvl_3,
    }
    public int _damage = 0;
    public int _damagePower = 1;
    public int _dagingStamina = 0;
    public DamageType _damageType = DamageType.Damage_Lvl_0;
}







public class CharacterScript : MonoBehaviour, IHitable
{
    //��������Ʈ Ÿ�Ե�
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);


    //Buff ���� ������Ʈ��
    protected Dictionary<BuffTypes, BuffScript> _currBuffs = new Dictionary<BuffTypes, BuffScript>();
    protected StatScript _myStat = new StatScript();


    //��ǥ ������Ʈ
    [SerializeField] protected StateContoller _stateContoller = null;
    [SerializeField] protected CharacterMoveScript2 _characterMoveScript2 = null;
    [SerializeField] protected CharacterAnimatorScript _characterAnimatorScript = null;
    protected GameObject _characterModelObject = null; //�ִϸ����ʹ� �갡 �����ִ�
    public GameObject GetCharacterModleObject() { return _characterModelObject; }



    //Aim System
    protected AimScript2 _aimScript = null;
    protected bool _isAim = false;
    protected bool _isTargeting = false;
    public bool GetIsTargeting() { return _isTargeting; }






    #region WeaponSection
    /*---------------------------------------------------
    |TODO| Weapon�� ���õ� ��ũ��Ʈ�� ���� ������ ������
    //Weapon Section -> �̰� �ٸ� ������Ʈ�� ������(���� ��������ִ°� EquipmentBoard Ȥ�� Inventory)
    ---------------------------------------------------*/

    public void DestroyWeapon(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand && _tempCurrRightWeapon != null)
        {
            Destroy(_tempCurrRightWeapon);
            _tempCurrRightWeapon = null;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand && _tempCurrLeftWeapon != null)
        {
            Destroy(_tempCurrLeftWeapon);
            _tempCurrLeftWeapon = null;
        }
    }

    public void WeaponSwitchHand(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        WeaponScript targetWeaponScript = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempCurrRightWeapon.GetComponent<WeaponScript>()
            : _tempCurrLeftWeapon.GetComponent<WeaponScript>();

        targetWeaponScript.Equip_OnSocket(work._weaponEquipTransform);
    }

    public void ChangeGrabFocusType(WeaponGrabFocus targetType)
    {
        _tempGrabFocusType = targetType;
    }

    public void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, GameObject nextWeaponPrefab)
    {
        if (layerType != AnimatorLayerTypes.RightHand &&
            layerType != AnimatorLayerTypes.LeftHand)
        {
            return;
        }

        WeaponSocketScript.SideType targetSide = (layerType == AnimatorLayerTypes.RightHand)
            ? WeaponSocketScript.SideType.Right
            : WeaponSocketScript.SideType.Left;

        WeaponScript nextWeaponScript = nextWeaponPrefab.GetComponent<WeaponScript>();

        Debug.Assert(nextWeaponScript != null, "����� WeaponScript�� �־�� �Ѵ�");

        //���� ã��
        Transform correctSocket = null;
        {
            Debug.Assert(_characterModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");


            ItemInfo.WeaponType targetType = nextWeaponScript._weaponType;

            foreach (var socketComponent in weaponSockets)
            {
                if (socketComponent._sideType != targetSide)
                {
                    continue;
                }

                foreach (var type in socketComponent._equippableWeaponTypes)
                {
                    if (type == targetType)
                    {
                        correctSocket = socketComponent.gameObject.transform;
                        break;
                    }
                }
            }

            Debug.Assert(correctSocket != null, "���⸦ ���� �� �ִ� ������ �����ϴ�");
        }

        //������ ������ ����, ����
        GameObject newObject = Instantiate(nextWeaponPrefab);
        {
            nextWeaponScript = newObject.GetComponent<WeaponScript>();
            nextWeaponScript._weaponType = ItemInfo.WeaponType.MediumGun;
            nextWeaponScript.Equip(this, correctSocket);
            newObject.transform.SetParent(transform);

            if (layerType == AnimatorLayerTypes.RightHand)
            {
                _tempCurrRightWeapon = newObject;
            }
            else
            {
                _tempCurrLeftWeapon = newObject;
            }

            StateGraphAsset stateGraphAsset = nextWeaponScript._weaponStateGraph;

            StateGraphAsset.StateGraphType stateGraphType = (layerType == AnimatorLayerTypes.RightHand == true)
                ? StateGraphAsset.StateGraphType.WeaponState_RightGraph
                : StateGraphAsset.StateGraphType.WeaponState_LeftGraph;

            //������ ��, ���±׷����� ��ü�Ѵ�.
            _stateContoller.EquipStateGraph(stateGraphAsset, stateGraphType);
        }
    }

    #region GetNextWeaponPrefab
    public GameObject GetNextWeaponPrefab(AnimatorLayerTypes layerType)
    {
        if (layerType != AnimatorLayerTypes.LeftHand &&
            layerType != AnimatorLayerTypes.RightHand)
        {
            return null;
        }

        int nextWeaponIndex = (layerType == AnimatorLayerTypes.LeftHand)
            ? _currLeftWeaponIndex + 1
            : _currRightWeaponIndex + 1;

        if (nextWeaponIndex >= _tempMaxWeaponSlot)
        {
            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
        }

        GameObject weaponPrefab = (layerType == AnimatorLayerTypes.LeftHand)
            ? _tempLeftWeaponPrefabs[nextWeaponIndex]
            : _tempRightWeaponPrefabs[nextWeaponIndex];

        return weaponPrefab;
    }
    #endregion GetNextWeaponPrefab

    #region GetNextWeaponScript
    public WeaponScript GetNextWeaponScript(AnimatorLayerTypes layerType)
    {
        if (layerType != AnimatorLayerTypes.LeftHand &&
            layerType != AnimatorLayerTypes.RightHand)
        {
            return null;
        }

        int nextWeaponIndex = (layerType == AnimatorLayerTypes.LeftHand)
            ? _currLeftWeaponIndex + 1
            : _currRightWeaponIndex + 1;

        if (nextWeaponIndex >= _tempMaxWeaponSlot)
        {
            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
        }

        GameObject weaponPrefab = (layerType == AnimatorLayerTypes.LeftHand)
            ? _tempLeftWeaponPrefabs[nextWeaponIndex]
            : _tempRightWeaponPrefabs[nextWeaponIndex];

        if (weaponPrefab == null)
        {
            return null;
        }

        return weaponPrefab.GetComponent<WeaponScript>();
    }
    #endregion GetNextWeaponScript

    #region GetCurrentWeaponPrefab
    public GameObject GetCurrWeaponPrefab(AnimatorLayerTypes layerType)
    {
        if (layerType != AnimatorLayerTypes.LeftHand &&
            layerType != AnimatorLayerTypes.RightHand)
        {
            return null;
        }

        GameObject weaponPrefab = (layerType == AnimatorLayerTypes.LeftHand)
            ? _tempCurrLeftWeapon
            : _tempCurrRightWeapon;

        return weaponPrefab;
    }
    #endregion GetCurrentWeaponPrefab

    #region GetCurrentWeaponScript
    public WeaponScript GetCurrentWeaponScript(AnimatorLayerTypes layerType)
    {
        if (layerType != AnimatorLayerTypes.LeftHand &&
            layerType != AnimatorLayerTypes.RightHand)
        {
            return null;
        }

        GameObject weaponPrefab = (layerType == AnimatorLayerTypes.LeftHand)
            ? _tempCurrLeftWeapon
            : _tempCurrRightWeapon;

        if (weaponPrefab == null)
        {
            return null;
        }

        return weaponPrefab.GetComponent<WeaponScript>();
    }

    public WeaponScript GetCurrentWeaponScript(bool isRightHand)
    {
        GameObject weaponPrefab = (isRightHand == true)
            ? _tempCurrRightWeapon
            : _tempCurrLeftWeapon;

        if (weaponPrefab == null)
        {
            return null;
        }

        return weaponPrefab.GetComponent<WeaponScript>();
    }
    #endregion GetCurrentWeaponScript


    [SerializeField] protected List<GameObject> _tempLeftWeaponPrefabs = new List<GameObject>();
    [SerializeField] protected List<GameObject> _tempRightWeaponPrefabs = new List<GameObject>();
    protected KeyCode _changeRightHandWeaponHandlingKey = KeyCode.B;
    protected KeyCode _changeLeftHandWeaponHandlingKey = KeyCode.V;
    protected KeyCode _useItemKeyCode1 = KeyCode.N;
    protected KeyCode _useItemKeyCode2 = KeyCode.M;
    protected KeyCode _useItemKeyCode3 = KeyCode.Comma;
    protected KeyCode _useItemKeyCode4 = KeyCode.Period;
    protected int _currLeftWeaponIndex = 0;
    protected int _currRightWeaponIndex = 0;
    protected int _tempMaxWeaponSlot = 3;
    protected GameObject _tempCurrLeftWeapon = null;
    protected GameObject _tempCurrRightWeapon = null;
    public GameObject GetLeftWeapon() { return _tempCurrLeftWeapon; }
    public GameObject GetRightWeapon() { return _tempCurrRightWeapon; }
    public GameObject GetCurrentWeapon(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _tempCurrRightWeapon;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _tempCurrLeftWeapon;
        }
        else
        {
            return null;
        }
    }
    public GameObject GetLeftWeaponPrefab() { return _tempLeftWeaponPrefabs[_currLeftWeaponIndex]; }
    public GameObject GetRightWeaponPrefab() { return _tempRightWeaponPrefabs[_currRightWeaponIndex]; }
    public GameObject GetCurrentWeaponPrefab(AnimatorLayerTypes layerType)
    {
        if (layerType == AnimatorLayerTypes.RightHand)
        {
            return _tempRightWeaponPrefabs[_currRightWeaponIndex];
        }
        else if (layerType == AnimatorLayerTypes.LeftHand)
        {
            return _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
        }
        else
        {
            return null;
        }
    }
    protected WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }
    protected bool _tempUsingRightHandWeapon = false; //�ֱٿ� ����� ���Ⱑ �������Դϱ�?
    public bool GetLatestWeaponUse() { return _tempUsingRightHandWeapon; }
    public void SetLatestWeaponUse(bool isRightHandWeapon)
    {
        _tempUsingRightHandWeapon = isRightHandWeapon;
    }
    public void IncreaseWeaponIndex(AnimatorLayerTypes layerType)
    {
        if (layerType != AnimatorLayerTypes.LeftHand &&
            layerType != AnimatorLayerTypes.RightHand)
        {
            Debug.Assert(false, "�߸��� ȣ���Դϴ�. �޼�, �������� ���� �ϳ������մϴ�");
            Debug.Break();
            return;
        }

        if (layerType == AnimatorLayerTypes.LeftHand)
        {
            _currLeftWeaponIndex++;
            if (_currLeftWeaponIndex >= _tempMaxWeaponSlot)
            {
                _currLeftWeaponIndex %= _tempMaxWeaponSlot;
            }
            //_tempCurrLeftWeapon = _tempLeftWeaponPrefabs[_currLeftWeaponIndex];
        }
        else
        {
            _currRightWeaponIndex++;
            if (_currRightWeaponIndex >= _tempMaxWeaponSlot)
            {
                _currRightWeaponIndex %= _tempMaxWeaponSlot;
            }
            //_tempCurrRightWeapon = _tempRightWeaponPrefabs[_currRightWeaponIndex];
        }
    }
    #endregion WeaponSection






    protected virtual void Awake()
    {
        _characterMoveScript2 = GetComponent<CharacterMoveScript2>();
        Debug.Assert(_characterMoveScript2 != null, "CharacterMove ������Ʈ ����");

        _stateContoller = GetComponent<StateContoller>();
        Debug.Assert(_stateContoller != null, "StateController�� ����");

        Animator animator = GetComponentInChildren<Animator>();
        _characterModelObject = animator.gameObject;
    }

    protected virtual void Update()
    {
        //���� ���� ������Ʈ
        {
            _stateContoller.DoWork();
        }

        //�⺻������ �߷��� ��� ������Ʈ �Ѵ�
        {
            _characterMoveScript2.GravityUpdate();
            _characterMoveScript2.ClearLatestVelocity();
        }
    }


    public void StateChanged(StateAsset nextState)
    {
        _characterAnimatorScript.StateChanged(nextState._myState._stateAnimationClip);
    }






    protected void ReadyAimSystem()
    {
        if (_aimScript == null)
        {
            _aimScript = transform.gameObject.AddComponent<AimScript2>();
        }
        _aimScript.enabled = true;
    }


    public void CheckBehave(AdditionalBehaveType additionalBehaveType)
    {
        /*--------------------------------------------------
        |NOTI| �̰��� ���� �ൿ���� �����ϰ� LayerLock�� ����Լ���
        �ൿ�� �̸� �������� ����.
        --------------------------------------------------*/
        int currentAnimatorBusyLayerBitShift = _characterAnimatorScript.GetBusyLayer();

        switch (additionalBehaveType)
        {
            case AdditionalBehaveType.ChangeWeapon:
                {
                    bool weaponChangeTry = false;
                    bool tempIsRightHandWeapon = false;
                    int nextWeaponIndex = 0;

                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        //�޼� ���� �������� ��ȯ
                        weaponChangeTry = true;

                        nextWeaponIndex = _currLeftWeaponIndex + 1;
                        if (nextWeaponIndex >= _tempMaxWeaponSlot)
                        {
                            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.T))
                    {
                        //������ ���� �������� ��ȯ
                        weaponChangeTry = true;

                        nextWeaponIndex = _currRightWeaponIndex + 1;
                        if (nextWeaponIndex >= _tempMaxWeaponSlot)
                        {
                            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
                        }

                        tempIsRightHandWeapon = true;
                    }

                    if (weaponChangeTry == false) //���� ��ȯ�� �õ����� �ʾҴ�. �ƹ��ϵ� �Ͼ�� �������̴�.
                    {
                        return;
                    }

                    int willUsingAnimatorLayer = 0;

                    //����� �ִϸ��̼� ���� üũ
                    {
                        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused ||
                            _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
                        {
                            //���� ������� ����־���.
                            willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                            willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        }
                        else
                        {
                            //�Ѽ����� ����־���.
                            if (tempIsRightHandWeapon == true)
                            {
                                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                            }
                            else
                            {
                                willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                            }
                        }
                    }

                    if ((currentAnimatorBusyLayerBitShift & willUsingAnimatorLayer) != 0)
                    {
                        //���� �ش��ϴ� �������� �ʹ� �ٻڴ�
                        return;
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    _characterAnimatorScript.CalculateBodyWorkType_ChangeWeapon(_tempGrabFocusType, tempIsRightHandWeapon, willUsingAnimatorLayer);
                }
                break;

            case AdditionalBehaveType.ChangeFocus:
                {
                    bool isChangeWeaponHandlingTry = false;
                    bool isRightHandWeapon = false;

                    if (Input.GetKeyDown(_changeRightHandWeaponHandlingKey) == true)
                    {
                        isChangeWeaponHandlingTry = true;
                        isRightHandWeapon = true;
                    }
                    else if (Input.GetKeyDown(_changeLeftHandWeaponHandlingKey) == true)
                    {
                        isChangeWeaponHandlingTry = true;
                    }

                    if (isChangeWeaponHandlingTry == false)
                    {
                        return; //������ �õ��� �̷������ �ʾҴ�. �ƹ��ϵ� �Ͼ�� �ʴ´�
                    }

                    GameObject targetWeapon = (isRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    if (targetWeapon == null)
                    {
                        return; //�����⸦ �õ������� ���Ⱑ ����.
                    }

                    bool isRelease = false;

                    if (isRightHandWeapon == true)
                    {
                        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused)
                        {
                            isRelease = true;
                        }
                    }
                    else
                    {
                        if (_tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
                        {
                            isRelease = true;
                        }
                    }

                    int willUsingAnimatorLayer = 0;
                    //����� ���̾� ���
                    {
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                    }

                    if ((currentAnimatorBusyLayerBitShift & willUsingAnimatorLayer) != 0)
                    {
                        //���� �ش��ϴ� �������� �ʹ� �ٻڴ�
                        return;
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    if (isRelease == true) //����� �����Ϸ��� ����Դϴ�
                    {
                        _characterAnimatorScript.CalculateBodyWorkType_ChangeFocus_ReleaseMode(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                    else
                    {
                        _characterAnimatorScript.CalculateBodyWorkType_ChangeFocus(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                }
                break;

            case AdditionalBehaveType.UseItem_Drink:
                {
                    ItemInfo newTestingItem = null;

                    if (Input.GetKeyDown(_useItemKeyCode1) == true)
                    {
                        newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
                    }
                    else if (Input.GetKeyDown(_useItemKeyCode2) == true)
                    {
                        newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
                    }
                    else if (Input.GetKeyDown(_useItemKeyCode3) == true)
                    {
                        newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
                    }
                    else if (Input.GetKeyDown(_useItemKeyCode4) == true)
                    {
                        newTestingItem = ItemInfoManager.Instance.GetItemInfo(60);
                    }

                    if (newTestingItem == null)
                    {
                        return;
                    }

                    if (_stateContoller.GetCurrState()._myState._canUseItem == false)
                    {
                        return;
                    }


                    //��� ���� üũ
                    int willBusyLayer = 0;

                    {
                        //���� �����۸����� �ʿ��� ���̾� üũ
                        if (newTestingItem._usingItemMustNotBusyLayers != null || newTestingItem._usingItemMustNotBusyLayers.Count > 0)
                        {
                            if (newTestingItem._usingItemMustNotBusyLayer < 0)
                            {
                                newTestingItem._usingItemMustNotBusyLayer = 0;

                                foreach (var item in newTestingItem._usingItemMustNotBusyLayers)
                                {
                                    newTestingItem._usingItemMustNotBusyLayer = (newTestingItem._usingItemMustNotBusyLayer | 1 << (int)item);
                                }
                            }

                            willBusyLayer = newTestingItem._usingItemMustNotBusyLayer;
                        }

                        //���� ���� �������� ���� �ʿ��� ���̾� üũ
                        if (_tempCurrRightWeapon != null)
                        {
                            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        }
                    }



                    if ((currentAnimatorBusyLayerBitShift & willBusyLayer) != 0)
                    {
                        return; //�ش� �������� ���� ������ �ִ�
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    _characterAnimatorScript.CalculateBodyWorkType_UseItem_Drink(_tempGrabFocusType, newTestingItem, willBusyLayer);
                }
                break;

            case AdditionalBehaveType.UseItem_Break:
                {
                    Debug.Assert(false, "�̱����Դϴ�");
                    Debug.Break();
                    //Work�� ��´� ������ Lock ����� ������.
                    //CalculateBodyWorkType_UseItem_Break();
                }
                break;

            default:
                break;
        }
    }

    public virtual void DealMe(DamageDesc damage, GameObject caller)
    {
    }
}
