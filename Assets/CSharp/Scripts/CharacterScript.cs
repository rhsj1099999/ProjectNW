using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static AnimationAttackFrameAsset;
using static StateGraphAsset;
using static StatScript;
using static ItemAsset_Weapon;
using Unity.VisualScripting;

public class StateContollerComponentDesc
{
    public CharacterScript _owner = null;
    public InputController _ownerInputController = null;
    public CharacterContollerable _ownerCharacterControllable = null;
    public CharacterAnimatorScript _ownerCharacterAnimatorScript = null;
    public AINavigationScript _ownerNavigationScript = null;
    public EnemyAIScript _ownerEnemyAIScript = null;
    public AimScript2 _ownerAimScript = null;
    public CharacterColliderScript _ownerCharacterColliderScript = null;
}


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
    
    Weapon_Reloading,
    Weapon_Fire,
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

[Serializable]
public class DamageDesc
{
    public class DamageDescToApply
    {
        public int _damage = 1;
        public int _damagePower = 1;
        public int _damagingStamina = 1;
    }

    public DamageDesc ShallowCopy()
    {
        return MemberwiseClone() as DamageDesc;
    }

    public float _damage = 1;
    public float _damagePower = 1;
    public float _damagingStamina = 1;
}




public class CharacterScript : GameActorScript, IHitable
{
    private bool _dead = false;
    public bool GetDead() { return _dead; }


    //��������Ʈ Ÿ�Ե�
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);

    //ĳ���� �߽�
    protected GameObject _characterHeart = null;

    //��ǥ ������Ʈ��
    public List<GameCharacterSubScript> _components = new List<GameCharacterSubScript>();
    private Dictionary<Type, Component> _mySubScripts = new Dictionary<Type, Component>();



    //�κ��丮
    [SerializeField] protected GameObject _inventoryUIPrefab = null;
    protected List<InventoryBoard> _inventoryBoardCached = new List<InventoryBoard>();

    //���� ����
    //���� �տ� ����ִ� = ��üȭ�� �ƴ� = GameObject �� ����ִµ� ����---------------------------
    protected GameObject _tempCurrLeftWeapon = null;
    protected GameObject _tempCurrRightWeapon = null;
    //----------------------------------------------------------------------------------------

    protected KeyCode _useItemKeyCode1 = KeyCode.N;
    protected KeyCode _useItemKeyCode2 = KeyCode.M;
    protected KeyCode _useItemKeyCode3 = KeyCode.Comma;
    protected KeyCode _useItemKeyCode4 = KeyCode.Period;

    protected KeyCode _changeRightHandWeaponHandlingKey = KeyCode.B;
    protected KeyCode _changeLeftHandWeaponHandlingKey = KeyCode.V;
    protected KeyCode _changeRightWeaponKey = KeyCode.T;
    protected KeyCode _changeLeftWeaponKey = KeyCode.R;

    [SerializeField] protected int _currLeftWeaponIndex = 0;
    [SerializeField] protected int _currRightWeaponIndex = 0;
    protected int _tempMaxWeaponSlot = 3;

    protected bool _tempUsingRightHandWeapon = false; //�ֱٿ� ����� ���Ⱑ �������Դϱ�?
    public bool _isRightWeaponAimed = false;
    public bool _isLeftWeaponAimed = false;








    public List<InventoryBoard> GetMyInventoryBoards() {return _inventoryBoardCached;}
    private void InitInventory()
    {
        InventoryBoard[] inventories = _inventoryUIPrefab.GetComponentsInChildren<InventoryBoard>();
        foreach (var item in inventories)
        {
            _inventoryBoardCached.Add(item);
        }
    }

    //�κ��丮 ������ �߰��ƴٸ� �� �Լ��� ���� ĳ�̵� ������Ʈ�� ������ ��
    public void ChangeInventoryInfo() {}



    public T GetCharacterSubcomponent<T>(bool isNullable = false) where T : Component
    {
        Component target = null;

        _mySubScripts.TryGetValue(typeof(T), out target);

        if (target == null && isNullable == true)
        {
            Debug.Assert(false, "���� ������Ʈ�� ã���� �ϰ��ִ�" + typeof(T));
            Debug.Break();
        }

        return (T)_mySubScripts[typeof(T)];
    }

    public T GCST<T>() where T : Component
    {
        return GetCharacterSubcomponent<T>();
    }

    public void AddCharacterSubComponent(GameCharacterSubScript subScript)
    {
        Type componentRealType = subScript.GetMyRealType();

        if (_mySubScripts.ContainsKey(componentRealType) == true)
        {
            Debug.Assert(false, "�ش� ��ũ��Ʈ�� �̹� �ֽ��ϴ�");
            Debug.Break();
        }

        _mySubScripts.Add(componentRealType, subScript);
    }

    public void AddCharacterSubComponent(Component subScript)
    {
        Type componentRealType = subScript.GetType();

        if (_mySubScripts.ContainsKey(componentRealType) == true)
        {
            Debug.Assert(false, "�ش� ��ũ��Ʈ�� �̹� �ֽ��ϴ�");
            Debug.Break();
        }

        _mySubScripts.Add(componentRealType, subScript);
    }


    protected virtual void Awake()
    {
        _characterHeart = new GameObject("CharacterHeart");
        Vector3 myPosition = transform.position;
        myPosition.y += 0.5f;
        _characterHeart.transform.position = myPosition;
        _characterHeart.transform.SetParent(transform);
        _characterHeart.layer = LayerMask.NameToLayer("CharacterHeart");
        SphereCollider heartCollider = _characterHeart.AddComponent<SphereCollider>();
        heartCollider.radius = 0.1f;
        heartCollider.includeLayers = 0;
        heartCollider.excludeLayers = ~0;
        heartCollider.isTrigger = true;

        foreach (var component in _components)
        {
            if (component == this)
            {
                continue;
            }

            if (component == null)
            {
                Debug.Assert(false, "null Component �̴�" + component.GetType());
                Debug.Break();
            }
            component.Init(this);
            AddCharacterSubComponent(component);
        }

        foreach (var component in _components)
        {
            component.SubScriptStart();
        }

        if (_inventoryUIPrefab != null)
        {
            InitInventory();
        }

        for (int i = 0; i < _tempMaxWeaponSlot; i++)
        {
            _tempLeftWeaponPrefabs.Add(null);
            _tempRightWeaponPrefabs.Add(null);
        }
    }




    //HP�� 0�̵Ǿ� �״� ������ �����մϴ�
    protected virtual void ZeroHPCall(CharacterScript killedBy)
    {
        _dead = true;

        if (killedBy != null)
        {
            killedBy.YouKillThisObject(gameObject);
        }
    }




    public virtual void YouKillThisObject(GameObject killObject)
    {
        AimScript2 aimScript = GetCharacterSubcomponent<AimScript2>(true);

        if (aimScript != null &&
            aimScript.GetLockOnObject() != null &&
            aimScript.GetLockOnObject().transform.parent.gameObject == killObject)
        {
            aimScript.OffAimState();
        }
    }




    public virtual void DeadCall()
    {
        // State Controller�� ��Ȱ��ȭ �Ѵ�.
        GetCharacterSubcomponent<StateContoller>().enabled = false;

        CharacterController ownerCharacterController = GetComponent<CharacterController>();
        if (ownerCharacterController != null)
        {
            ownerCharacterController.excludeLayers = ~(LayerMask.GetMask("StaticNavMeshLayer"));
        }

        // ��� �浹ó���� ��Ȱ��ȭ�Ѵ� (���� ����)
        //GetCharacterSubcomponent<CharacterController>().excludeLayers = );
    }



    #region WeaponSection




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

    protected List<ItemStoreDesc_Weapon> _tempLeftWeaponPrefabs = new List<ItemStoreDesc_Weapon>();
    protected List<ItemStoreDesc_Weapon> _tempRightWeaponPrefabs = new List<ItemStoreDesc_Weapon>();
    public ItemAsset_Weapon GetCurrentWeaponInfo(AnimatorLayerTypes layerType)
    {
        List<ItemStoreDesc_Weapon> target = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        int targetIndex = (layerType == AnimatorLayerTypes.RightHand)
            ? _currRightWeaponIndex
            : _currLeftWeaponIndex;

        if (target.Count <= targetIndex)
        {
            return null;
        }

        ItemStoreDescBase storeDesc = target[targetIndex];

        if (storeDesc == null)
        {
            return null;
        }

        return target[targetIndex]._itemAsset as ItemAsset_Weapon;
    }
    public ItemAsset_Weapon GetNextWeaponInfo(AnimatorLayerTypes layerType)
    {
        List<ItemStoreDesc_Weapon> target = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        int targetIndex = (layerType == AnimatorLayerTypes.RightHand)
            ? _currRightWeaponIndex
            : _currLeftWeaponIndex;

        targetIndex++;

        if (targetIndex >= _tempMaxWeaponSlot)
        {
            targetIndex = targetIndex % _tempMaxWeaponSlot;
        }

        if (target.Count <= targetIndex)
        {
            return null;
        }

        ItemStoreDescBase storeDesc = target[targetIndex];

        if (storeDesc == null)
        {
            return null;
        }

        return target[targetIndex]._itemAsset as ItemAsset_Weapon;
    }
    public ItemStoreDesc_Weapon GetNextWeaponStoreDesc(AnimatorLayerTypes layerType)
    {
        List<ItemStoreDesc_Weapon> target = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        int targetIndex = (layerType == AnimatorLayerTypes.RightHand)
            ? _currRightWeaponIndex
            : _currLeftWeaponIndex;

        targetIndex++;

        if (targetIndex >= _tempMaxWeaponSlot)
        {
            targetIndex = targetIndex % _tempMaxWeaponSlot;
        }

        if (target.Count <= targetIndex)
        {
            return null;
        }

        return target[targetIndex];
    }
    public ItemStoreDesc_Weapon GetCurrentWeaponStoreDesc(AnimatorLayerTypes layerType)
    {
        List<ItemStoreDesc_Weapon> target = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        int targetIndex = (layerType == AnimatorLayerTypes.RightHand)
            ? _currRightWeaponIndex
            : _currLeftWeaponIndex;

        if (target.Count <= targetIndex)
        {
            return null;
        }

        return target[targetIndex];

    }


    protected WeaponGrabFocus _tempGrabFocusType = WeaponGrabFocus.Normal;
    public WeaponGrabFocus GetGrabFocusType() { return _tempGrabFocusType; }
    public void ChangeGrabFocusType(WeaponGrabFocus targetType)
    {
        _tempGrabFocusType = targetType;
    }



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
    public void DestroyWeapon(AnimatorLayerTypes layerType)
    {
        StateGraphType targetType = StateGraphType.End;

        if (layerType == AnimatorLayerTypes.RightHand && _tempCurrRightWeapon != null)
        {
            Destroy(_tempCurrRightWeapon);
            _tempCurrRightWeapon = null;
            targetType = StateGraphType.WeaponState_RightGraph;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand && _tempCurrLeftWeapon != null)
        {
            Destroy(_tempCurrLeftWeapon);
            _tempCurrLeftWeapon = null;
            targetType = StateGraphType.WeaponState_LeftGraph;
        }


        StateGraphAsset basicAsset = GetCharacterSubcomponent<StateContoller>().GetBasicStateGraphes(targetType);
        if (basicAsset == null)
        {
            return;
        }

        GetCharacterSubcomponent<StateContoller>().EquipStateGraph(basicAsset, targetType);
    }

    #endregion WeaponSection

    public void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, ItemStoreDesc_Weapon nextItemStoreDesc)
    {
        if (layerType != AnimatorLayerTypes.RightHand &&layerType != AnimatorLayerTypes.LeftHand) {return;}

   
        WeaponSocketScript.SideType targetSide = (layerType == AnimatorLayerTypes.RightHand)
            ? WeaponSocketScript.SideType.Right
            : WeaponSocketScript.SideType.Left;

        //���� ã��

        ItemAsset_Weapon weaponItemAsset = (ItemAsset_Weapon)nextItemStoreDesc._itemAsset;

        Transform correctSocket = null;
        {
            Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

            WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");


            WeaponType targetType = weaponItemAsset._WeaponType;

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

            if (correctSocket == null)
            {
                Debug.Assert(false, "���⸦ ���� �� �ִ� ������ �����ϴ�");
                Debug.Break();
                return;
            }

        }

        //������ ������ ����, ����
        GameObject weaponModel = Instantiate(weaponItemAsset._ItemModel, transform);

        WeaponScript weaponScript = null;

        if (weaponItemAsset._WeaponType >= WeaponType.SmallGun && weaponItemAsset._WeaponType <= WeaponType.LargeGun)
        {
            weaponScript = weaponModel.AddComponent<Gunscript2>();
        }
        else
        {
            weaponScript = weaponModel.AddComponent<WeaponScript>();
        }

        {
            weaponScript.Init(nextItemStoreDesc);
            weaponScript.Equip(this, correctSocket);

            if (layerType == AnimatorLayerTypes.RightHand)
            {
                _tempCurrRightWeapon = weaponModel;
            }
            else
            {
                _tempCurrLeftWeapon = weaponModel;
            }

            StateGraphAsset stateGraphAsset = weaponItemAsset._WeaponStateGraph;

            StateGraphType stateGraphType = (layerType == AnimatorLayerTypes.RightHand == true)
                ? StateGraphType.WeaponState_RightGraph
                : StateGraphType.WeaponState_LeftGraph;

            //������ ��, ���±׷����� ��ü�Ѵ�.
            GCST<StateContoller>().EquipStateGraph(stateGraphAsset, stateGraphType);


            //������ ��, �ݶ��̴��� ������Ʈ �Ѵ�.
            ColliderScript weaponColliderScript = weaponModel.GetComponentInChildren<ColliderScript>();
            if (weaponColliderScript != null)
            {
                /*---------------------------------------------------------
                |NOTI| ���� ����, ���� �浹ü �ʿ����
                ---------------------------------------------------------*/

                ColliderAttachType colliderType = CalculateAttachType(weaponColliderScript.GetAttachType(), layerType);
                GCST<CharacterColliderScript>().ChangeCollider(colliderType, weaponColliderScript.gameObject);
                weaponColliderScript.gameObject.SetActive(false);
            }
        }
    }

    public void WeaponSwitchHand(AnimatorLayerTypes layerType, BodyPartBlendingWork work)
    {
        WeaponScript targetWeaponScript = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempCurrRightWeapon.GetComponent<WeaponScript>()
            : _tempCurrLeftWeapon.GetComponent<WeaponScript>();

        targetWeaponScript.Equip_OnSocket(work._weaponEquipTransform);
    }

    public virtual ColliderAttachType CalculateAttachType(ColliderAttachType attachType, AnimatorLayerTypes layerType)
    {
        ColliderAttachType type = ColliderAttachType.ENEND;

        if (attachType == ColliderAttachType.HumanoidLeftHand ||
            attachType == ColliderAttachType.HumanoidRightHand)
        {
            if (layerType == AnimatorLayerTypes.RightHand)
            {
                type = ColliderAttachType.HumanoidRightHand;
            }
            else
            {
                type = ColliderAttachType.HumanoidLeftHand;
            }
        }

        else if (attachType == ColliderAttachType.HumanoidLeftHandWeapon ||
            attachType == ColliderAttachType.HumanoidRightHandWeapon)
        {
            if (layerType == AnimatorLayerTypes.RightHand)
            {
                type = ColliderAttachType.HumanoidRightHandWeapon;
            }
            else
            {
                type = ColliderAttachType.HumanoidLeftHandWeapon;
            }
        }

        return type;
    }

    private void TriggerEnterWithWeapon(Collider other)
    {
        /*-------------------------------------------------------
        OnTirggerEnter -> TriggerEnterWithWeapon -> DealMe(���� ����ɰ���)
        -------------------------------------------------------*/

        /*-------------------------------------------------------
        other = ���� �ε��� ��ü�� ����(����, ���� ���� �ƴ�)
        ���� Ȯ���� ����
        -------------------------------------------------------*/


        CharacterScript otherCharacterScript = other.gameObject.GetComponentInParent<CharacterScript>();
        DamageDesc currentDamage = new DamageDesc();
        otherCharacterScript.CalculateMyCurrentWeaponDamage(ref currentDamage, other);

        
        DealMe_Final(currentDamage, otherCharacterScript.gameObject);
    }

    public void MoveWeapons()
    {
        Transform correctSocket_Left = null;
        Transform correctSocket_Right = null;

        //���� ã��
        {
            Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

            WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");

            foreach (var socketComponent in weaponSockets)
            {
                if (socketComponent._sideType == WeaponSocketScript.SideType.Left)
                {
                    //�޼� �����Դϴ�
                    correctSocket_Left = socketComponent.transform;
                }
                else
                {
                    correctSocket_Right = socketComponent.transform;
                }
            }

            if (correctSocket_Left == null || correctSocket_Right == null)
            {
                Debug.Assert(false, "��ã�ҵ�");
                Debug.Break();
            }
        }

        if (_tempCurrRightWeapon != null)
        {
            WeaponScript rightWeaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
            rightWeaponScript.Equip(this, correctSocket_Right);
        }

        if (_tempCurrLeftWeapon != null)
        {
            WeaponScript leftWeaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
            leftWeaponScript.Equip(this, correctSocket_Left);
        }
    }

    public virtual LayerMask CalculateWeaponColliderExcludeLayerMask(ColliderAttachType type, GameObject targetObject)
    {
        return 0;
    }


    public float GetStateChangingPercentage()
    {
        return GCST<CharacterAnimatorScript>().GetStateChangingPercentage();
    }



    protected virtual void Update()
    {
        //���� ���� ������Ʈ
        if (GCST<StateContoller>().enabled == true)
        {
            GCST<StateContoller>().DoWork();
        }

        //�⺻������ �߷��� ��� ������Ʈ �Ѵ�
        {
            GCST<CharacterContollerable>().GravityUpdate();
            GCST<CharacterContollerable>().ClearLatestVelocity();
        }
    }


    public void StateChanged(StateAsset nextState)
    {
        GCST<CharacterAnimatorScript>().StateChanged(nextState);
        GCST<CharacterColliderScript>().StateChanged();
        GCST<CharacterContollerable>().StateChanged();
    }


    public void SetWeapon(bool isRightWeapon, int index, ItemStoreDesc_Weapon weaponInfo)
    {
        List<ItemStoreDesc_Weapon> targetWeaponPrefabs = (isRightWeapon == true)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        targetWeaponPrefabs[index] = weaponInfo;

        int currIndex = (isRightWeapon == true)
            ? _currRightWeaponIndex
            : _currLeftWeaponIndex;

        if (currIndex != index)
        {
            return;
        }


        if (isRightWeapon == true)
        {
            _currRightWeaponIndex--;
            if (_currRightWeaponIndex <= 0)
            {
                _currRightWeaponIndex = _tempMaxWeaponSlot - 1;
            }
        }
        else
        {
            _currLeftWeaponIndex--;
            if (_currLeftWeaponIndex <= 0)
            {
                _currLeftWeaponIndex = _tempMaxWeaponSlot - 1;
            }
        }
        
        GCST<CharacterAnimatorScript>().CalculateBodyWorkType_ChangeWeapon(_tempGrabFocusType, isRightWeapon, -1, true);
    }

    public void CheckBehave(AdditionalBehaveType additionalBehaveType)
    {
        /*--------------------------------------------------
        |NOTI| �̰��� ���� �ൿ���� �����ϰ� LayerLock�� ����Լ���
        �ൿ�� �̸� �������� ����.
        --------------------------------------------------*/
        int currentAnimatorBusyLayerBitShift = GCST<CharacterAnimatorScript>().GetBusyLayer();

        switch (additionalBehaveType)
        {
            case AdditionalBehaveType.ChangeWeapon:
                {
                    if (UIManager.Instance.IsConsumeInput() == true)
                    {
                        return;
                    }

                    bool weaponChangeTry = false;
                    bool tempIsRightHandWeapon = false;
                    int nextWeaponIndex = 0;

                    if (Input.GetKeyDown(_changeLeftWeaponKey))
                    {
                        //�޼� ���� �������� ��ȯ
                        weaponChangeTry = true;

                        nextWeaponIndex = _currLeftWeaponIndex + 1;
                        if (nextWeaponIndex >= _tempMaxWeaponSlot)
                        {
                            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
                        }
                    }
                    else if (Input.GetKeyDown(_changeRightWeaponKey))
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

                    //���� ��ȯ�� �õ����� �ʾҴ�. �ƹ��ϵ� �Ͼ�� �������̴�.
                    if (weaponChangeTry == false) 
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
                    GCST<CharacterAnimatorScript>().CalculateBodyWorkType_ChangeWeapon(_tempGrabFocusType, tempIsRightHandWeapon, willUsingAnimatorLayer);
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
                        GCST<CharacterAnimatorScript>().CalculateBodyWorkType_ChangeFocus_ReleaseMode(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                    else
                    {
                        GCST<CharacterAnimatorScript>().CalculateBodyWorkType_ChangeFocus(isRightHandWeapon, willUsingAnimatorLayer);
                    }
                }
                break;

            case AdditionalBehaveType.UseItem_Drink:
                {
                    ItemAsset_Consume newTestingItem = null;

                    if (Input.GetKeyDown(_useItemKeyCode1) == true)
                    {
                        newTestingItem = ItemInfoManager.Instance.GetItemInfo(60) as ItemAsset_Consume;
                    }

                    if (newTestingItem == null)
                    {
                        return;
                    }

                    if (GCST<StateContoller>().GetCurrState()._myState._canUseItem == false)
                    {
                        return;
                    }


                    //��� ���� üũ
                    int willBusyLayer = 0;

                    {
                        ////���� �����۸����� �ʿ��� ���̾� üũ
                        //if (newTestingItem._UsingItemMustNotBusyLayers != null || newTestingItem._UsingItemMustNotBusyLayers.Count > 0)
                        //{
                        //    if (newTestingItem._UsingItemMustNotBusyLayer < 0)
                        //    {
                        //        newTestingItem._UsingItemMustNotBusyLayer = 0;

                        //        foreach (var item in newTestingItem._UsingItemMustNotBusyLayers)
                        //        {
                        //            newTestingItem._UsingItemMustNotBusyLayer = (newTestingItem._UsingItemMustNotBusyLayer | 1 << (int)item);
                        //        }
                        //    }

                        //    willBusyLayer = newTestingItem._UsingItemMustNotBusyLayer;
                        //}

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
                    GCST<CharacterAnimatorScript>().CalculateBodyWorkType_UseItem_Drink(_tempGrabFocusType, newTestingItem, willBusyLayer);
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

            case AdditionalBehaveType.Weapon_Reloading:
                {
                    //Debug.Log("������ �˻� ����");

                    GameObject currWeapon = (_tempUsingRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    KeyCode fireKeyCode = KeyCode.R;

                    if (Input.GetKeyDown(fireKeyCode) == false)
                    {
                        //Debug.Log("Ű�� �ȴ��ȴ�");
                        return;
                    }

                    if (currWeapon == null)
                    {
                        //Debug.Log("���Ⱑ ����?");
                        return;
                    }

                    Gunscript2 gunScript = currWeapon.GetComponent<Gunscript2>();

                    if (gunScript == null)
                    {
                        //Debug.Log("gun Script �� �ƴ� ���⿡ �� ������Ʈ�� ������?");
                        return;
                    }


                    ItemStoreDescBase ownerInventoryFirstMagazine = gunScript.FindMagazine();

                    if (ownerInventoryFirstMagazine == null)
                    {
                        //źâ�� �����
                        return;
                    }
                    

                    bool reloadCheck = gunScript.ReloadCheck();

                    if (reloadCheck == false)
                    {
                        //Debug.Log("���� ������ �� �� ����.");
                        return;
                    }

                    gunScript.StartReloadingProcess(ownerInventoryFirstMagazine);
                    //�߻�
                }
                break;

            case AdditionalBehaveType.Weapon_Fire:
                {
                    //Debug.Log("�߻�˻� ����");
                    //���� ������� ���⿡�� �߻��߳ĸ� Ȯ���ϴ� �Լ�
                    GameObject currWeapon = (_tempUsingRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    KeyCode fireKeyCode = (_tempUsingRightHandWeapon == true)
                        ? KeyCode.Mouse0
                        : KeyCode.Mouse1;

                    if (Input.GetKey(fireKeyCode) == false)
                    {
                        ////Debug.Log("Ű�� �ȴ��ȴ�");
                        return;
                    }

                    if (currWeapon == null)
                    {
                        Debug.Log("���Ⱑ ����?");
                        return;
                    }

                    Gunscript2 gunScript = currWeapon.GetComponent<Gunscript2>();

                    if (gunScript == null)
                    {
                        //Debug.Log("gun Script �� �ƴ� ���⿡ �� ������Ʈ�� ������?");
                        return;
                    }

                    bool weaponCanFire = gunScript.FireCheck();

                    if (weaponCanFire == false)
                    {
                        //Debug.Log("�߻� �غ� �ȵƴ�");
                        return;
                    }


                    {
                        //źâ�� ����
                        //return;
                    }


                    {
                        //źâ�� ź�̾���
                        //play ���� �Ҹ�
                        //return;
                    }


                    //Debug.Log("�߻��Ѵ�");

                    gunScript.Fire();
                    //�߻�
                }
                break;

            default:
                break;
        }
    }

    public virtual void CalculateMyCurrentWeaponDamage(ref DamageDesc damageDesc, Collider other)
    {
        StatScript myStat = GCST<StatScript>();

        if (damageDesc == null)
        {
            Debug.Assert(false, "damageDesc�� null�̿��� �ȵȴ�");
            Debug.Break();
            return;
        }

        /*-------------------------------------------------------
        �⺻->StatComponent(������ ���⼭ ����Ѵ�)
        �⺻ ���ݿ� ���� ������, ���׹̳� ���
        -------------------------------------------------------*/
        {
            damageDesc._damage = myStat.CalculateStatDamage();
            damageDesc._damagingStamina = myStat.CalculateStatDamagingStamina();
            damageDesc._damagePower = myStat.CalculatePower();
        }


        /*-------------------------------------------------------
        ���� ���(���� ����� �� ���ð��̴�)
        -------------------------------------------------------*/
        WeaponScript otherWeaponScript = other.GetComponentInParent<WeaponScript>();
        if (otherWeaponScript != null)
        {
            DamageDesc weaponDamageDest = otherWeaponScript.GetItemAsset()._WeaponDamageDesc;
            damageDesc._damage += weaponDamageDest._damage;
            damageDesc._damagingStamina += weaponDamageDest._damagingStamina;
            damageDesc._damagePower += weaponDamageDest._damagePower;
        }


        /*-------------------------------------------------------
        �ִϸ��̼� ���(������ ũ�� ���ð��̴�)
        -------------------------------------------------------*/
        StateContoller myStateController = GCST<StateContoller>();
        if (myStateController != null &&
            myStateController.GetCurrState()._myState._isAttackState == true)
        {
            DamageDesc attackMultiplyDesc = myStateController.GetCurrState()._myState._attackDamageMultiply;
            if (attackMultiplyDesc == null)
            {
                Debug.Log("���ݻ������� ���� �������� �ʾҴ�");
                attackMultiplyDesc = new DamageDesc();
            }

            damageDesc._damage *= attackMultiplyDesc._damage;
            damageDesc._damagingStamina *= attackMultiplyDesc._damagingStamina;
            damageDesc._damagePower *= attackMultiplyDesc._damagePower;
        }






        /*-------------------------------------------------------
        ������ ���(������� ��Ƽ� �ֵθ��� �� ���ð��̴�)
        -------------------------------------------------------*/
        if (_tempGrabFocusType != WeaponGrabFocus.Normal)
        {
            damageDesc._damage *= 1.2f;
            damageDesc._damagingStamina *= 1.2f;
            damageDesc._damagePower *= 1.2f;
        }







        /*-------------------------------------------------------
        ���� ���(�̱���)
        -------------------------------------------------------*/
        {

        }
    }

    public virtual void DealMe_Final(DamageDesc damage, GameObject caller)
    {
        StatScript statScript = GCST<StatScript>();


        /*------------------------------------------------
        |NOTO| �̰������� ������ �ǰ� ����, ���º��游 ����մϴ�.
        ------------------------------------------------*/

        Debug.Log("���� ������" + damage._damage);
        Debug.Log("���� ���׹̳�������" + damage._damagingStamina);
        Debug.Log("���� �Ŀ�" + damage._damagePower);

        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.Hit_Lvl_0;

        StateAsset currState = GCST<StateContoller>().GetCurrState();
        StateDesc currStateDesc = GCST<StateContoller>().GetCurrState()._myState;



        //�������̿������� ���� ��� ����
        {
            if (currStateDesc._isBlockState == true)
            {
                //���׹̳��� ����ϰ� ���ε��� ����մϴ�
                if (statScript._stamina >= damage._damagingStamina &&
                    statScript._roughness >= damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Reaction;
                }

                //���׹̳��� ����ѵ� ���ε��� �����մϴ�.
                else if (statScript._stamina >= damage._damagingStamina &&
                    statScript._roughness < damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Sliding;
                }

                //���ε��� ����ѵ� ���׹̳��� �����մϴ�.
                else if (statScript._stamina < damage._damagingStamina &&
                    statScript._roughness >= damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Crash;
                }

                //����� ���µ��� �����ͺ�
                StateAsset nextStateAsseet = null;
                List<LinkedStateAsset> linkedStates = GCST<StateContoller>().GetCurrStateGraph().GetGraphStates()[currState];
                foreach (LinkedStateAsset linkedState in linkedStates)
                {
                    if (linkedState._linkedState._myState._stateType == representType)
                    {
                        nextStateAsseet = linkedState._linkedState;
                        break;
                    }
                }

                //���׹̳��� �����ϰ� ���ε��� �����մϴ�. Ȥ�� ������°� �������� �ʽ��ϴ�
                if ((statScript._stamina < damage._damagingStamina && statScript._roughness < damage._damagePower) ||
                    nextStateAsseet == null)
                {
                    //�´� ���·� ���� �Ұǵ�
                    nextGraphType = StateGraphType.HitStateGraph;

                    float deltaRoughness = damage._damagePower - statScript._roughness;

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

                float deltaRoughness = damage._damagePower - statScript._roughness;

                if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //���ε��� ���� �����ϴ�
                {
                    representType = RepresentStateType.Hit_Lvl_0;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //���ε��� ���� �����ϴ�
                {
                    representType = RepresentStateType.Hit_Lvl_1;
                }
                else
                {
                    representType = RepresentStateType.Hit_Lvl_2;
                }
            }
        }


        /*--------------------------------------------------------------------------------------------------------------
        --------------------------------------��� �������� �����־�� �Ѵ�-------------------------------------------------------
        --------------------------------------------------------------------------------------------------------------*/

        int finalDamage = (int)damage._damage;

        statScript._hp -= finalDamage;
        if (statScript._hp <= 0)
        {
            Debug.Log("�׾���");

            //caller.
            ZeroHPCall(caller.GetComponent<CharacterScript>());


            //���󰥸�ŭ�� �������� �ް� �״´�
            if (representType == RepresentStateType.Hit_Lvl_2)
            {
                representType = RepresentStateType.DieThrow;
            }
            else
            {
                representType = RepresentStateType.DieNormal;
            }
            nextGraphType = StateGraphType.DieGraph;

            GCST<StateContoller>().TryChangeState(nextGraphType, representType);

            return;
        }

        gameObject.transform.LookAt(caller.transform.position);

        GCST<StateContoller>().TryChangeState(nextGraphType, representType);
    }

    public void WhenTriggerEnterWithWeaponCollider(Collider other)
    {
        if (_dead == true) {return;}

        
        if (AnimationAttackManager.Instance.TriggerEnterCheck(this, other) == false)
        {
            return;
        }

        //���� �浹�� �ƴ� �����浹�Դϴ�.
        TriggerEnterWithWeapon(other);

        //string tag = other.tag;

        //if (tag == "WeaponAttachedCollider")
        //{
        //    //���� �浹�� �ƴ� �����浹�Դϴ�.
        //    if (AnimationAttackManager.Instance.TriggerEnterCheck(this, other) == true)
        //    {
        //        TriggerEnterWithWeapon(other);
        //    }
        //    return;
        //}
    }


}
