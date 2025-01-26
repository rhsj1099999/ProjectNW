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


    //델리게이트 타입들
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);

    //캐릭터 중심
    protected GameObject _characterHeart = null;

    //대표 컴포넌트들
    public List<GameCharacterSubScript> _components = new List<GameCharacterSubScript>();
    private Dictionary<Type, Component> _mySubScripts = new Dictionary<Type, Component>();



    //인벤토리
    [SerializeField] protected GameObject _inventoryUIPrefab = null;
    protected List<InventoryBoard> _inventoryBoardCached = new List<InventoryBoard>();

    //무기 관련
    //현재 손에 쥐고있다 = 객체화가 됐다 = GameObject 로 들고있는데 맞음---------------------------
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

    protected bool _tempUsingRightHandWeapon = false; //최근에 사용한 무기가 오른손입니까?
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

    //인벤토리 개수가 추가됐다면 이 함수를 통해 캐싱된 컴포넌트를 변경할 것
    public void ChangeInventoryInfo() {}



    public T GetCharacterSubcomponent<T>(bool isNullable = false) where T : Component
    {
        Component target = null;

        _mySubScripts.TryGetValue(typeof(T), out target);

        if (target == null && isNullable == true)
        {
            Debug.Assert(false, "없는 컴포넌트를 찾으려 하고있다" + typeof(T));
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
            Debug.Assert(false, "해당 스크립트가 이미 있습니다");
            Debug.Break();
        }

        _mySubScripts.Add(componentRealType, subScript);
    }

    public void AddCharacterSubComponent(Component subScript)
    {
        Type componentRealType = subScript.GetType();

        if (_mySubScripts.ContainsKey(componentRealType) == true)
        {
            Debug.Assert(false, "해당 스크립트가 이미 있습니다");
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
                Debug.Assert(false, "null Component 이다" + component.GetType());
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




    //HP가 0이되어 죽는 연출을 시작합니다
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
        // State Controller를 비활성화 한다.
        GetCharacterSubcomponent<StateContoller>().enabled = false;

        CharacterController ownerCharacterController = GetComponent<CharacterController>();
        if (ownerCharacterController != null)
        {
            ownerCharacterController.excludeLayers = ~(LayerMask.GetMask("StaticNavMeshLayer"));
        }

        // 모든 충돌처리를 비활성화한다 (지면 빼고)
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
            Debug.Assert(false, "잘못된 호출입니다. 왼손, 오른손중 둘중 하나여야합니다");
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

        //소켓 찾기

        ItemAsset_Weapon weaponItemAsset = (ItemAsset_Weapon)nextItemStoreDesc._itemAsset;

        Transform correctSocket = null;
        {
            Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "무기를 붙이려는데 모델이 없어서는 안된다");

            WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");


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
                Debug.Assert(false, "무기를 붙일 수 있는 소켓이 없습니다");
                Debug.Break();
                return;
            }

        }

        //아이템 프리팹 생성, 장착
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

            //장착한 후, 상태그래프를 교체한다.
            GCST<StateContoller>().EquipStateGraph(stateGraphAsset, stateGraphType);


            //장착한 후, 콜라이더를 업데이트 한다.
            ColliderScript weaponColliderScript = weaponModel.GetComponentInChildren<ColliderScript>();
            if (weaponColliderScript != null)
            {
                /*---------------------------------------------------------
                |NOTI| 아직 방패, 총은 충돌체 필요없음
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
        OnTirggerEnter -> TriggerEnterWithWeapon -> DealMe(상태 변경될거임)
        -------------------------------------------------------*/

        /*-------------------------------------------------------
        other = 나와 부딪힌 객체가 무기(장판, 독뎀 등이 아닌)
        임이 확정인 상태
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

        //소켓 찾기
        {
            Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "무기를 붙이려는데 모델이 없어서는 안된다");

            WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

            Debug.Assert(weaponSockets.Length > 0, "무기를 붙이려는데 모델에 소켓이 없다");

            foreach (var socketComponent in weaponSockets)
            {
                if (socketComponent._sideType == WeaponSocketScript.SideType.Left)
                {
                    //왼손 소켓입니다
                    correctSocket_Left = socketComponent.transform;
                }
                else
                {
                    correctSocket_Right = socketComponent.transform;
                }
            }

            if (correctSocket_Left == null || correctSocket_Right == null)
            {
                Debug.Assert(false, "못찾았따");
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
        //현재 상태 업데이트
        if (GCST<StateContoller>().enabled == true)
        {
            GCST<StateContoller>().DoWork();
        }

        //기본적으로 중력은 계속 업데이트 한다
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
        |NOTI| 이곳은 다음 행동들을 예상하고 LayerLock을 잡는함수다
        행동을 미리 실행하지 말것.
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
                        //왼손 무기 다음으로 전환
                        weaponChangeTry = true;

                        nextWeaponIndex = _currLeftWeaponIndex + 1;
                        if (nextWeaponIndex >= _tempMaxWeaponSlot)
                        {
                            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
                        }
                    }
                    else if (Input.GetKeyDown(_changeRightWeaponKey))
                    {
                        //오른손 무기 다음으로 전환
                        weaponChangeTry = true;

                        nextWeaponIndex = _currRightWeaponIndex + 1;
                        if (nextWeaponIndex >= _tempMaxWeaponSlot)
                        {
                            nextWeaponIndex = nextWeaponIndex % _tempMaxWeaponSlot;
                        }

                        tempIsRightHandWeapon = true;
                    }

                    //무기 전환을 시도하지 않았다. 아무일도 일어나지 않을것이다.
                    if (weaponChangeTry == false) 
                    {
                        return;
                    }

                    int willUsingAnimatorLayer = 0;

                    //사용할 애니메이션 부위 체크
                    {
                        if (_tempGrabFocusType == WeaponGrabFocus.RightHandFocused ||
                            _tempGrabFocusType == WeaponGrabFocus.LeftHandFocused)
                        {
                            //현재 양손으로 잡고있었다.
                            willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                            willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        }
                        else
                        {
                            //한손으로 잡고있었다.
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
                        //지금 해당하는 부위들이 너무 바쁘다
                        return;
                    }

                    //Work를 담는다 이전에 Lock 계산을 끝낼것.
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
                        return; //양손잡기 시도가 이루어지지 않았다. 아무일도 일어나지 않는다
                    }

                    GameObject targetWeapon = (isRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    if (targetWeapon == null)
                    {
                        return; //양손잡기를 시도했지만 무기가 없다.
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
                    //사용할 레이어 계산
                    {
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.LeftHand);
                        willUsingAnimatorLayer = willUsingAnimatorLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                    }

                    if ((currentAnimatorBusyLayerBitShift & willUsingAnimatorLayer) != 0)
                    {
                        //지금 해당하는 부위들이 너무 바쁘다
                        return;
                    }

                    //Work를 담는다 이전에 Lock 계산을 끝낼것.
                    if (isRelease == true) //양손을 해제하려는 모드입니다
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


                    //사용 부위 체크
                    int willBusyLayer = 0;

                    {
                        ////순수 아이템만으로 필요한 레이어 체크
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

                        //현재 무기 파지법에 의해 필요한 레이어 체크
                        if (_tempCurrRightWeapon != null)
                        {
                            willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        }
                    }



                    if ((currentAnimatorBusyLayerBitShift & willBusyLayer) != 0)
                    {
                        return; //해당 부위들은 지금 할일이 있다
                    }

                    //Work를 담는다 이전에 Lock 계산을 끝낼것.
                    GCST<CharacterAnimatorScript>().CalculateBodyWorkType_UseItem_Drink(_tempGrabFocusType, newTestingItem, willBusyLayer);
                }
                break;

            case AdditionalBehaveType.UseItem_Break:
                {
                    Debug.Assert(false, "미구현입니다");
                    Debug.Break();
                    //Work를 담는다 이전에 Lock 계산을 끝낼것.
                    //CalculateBodyWorkType_UseItem_Break();
                }
                break;

            case AdditionalBehaveType.Weapon_Reloading:
                {
                    //Debug.Log("재장전 검사 시작");

                    GameObject currWeapon = (_tempUsingRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    KeyCode fireKeyCode = KeyCode.R;

                    if (Input.GetKeyDown(fireKeyCode) == false)
                    {
                        //Debug.Log("키가 안눌렸다");
                        return;
                    }

                    if (currWeapon == null)
                    {
                        //Debug.Log("무기가 없다?");
                        return;
                    }

                    Gunscript2 gunScript = currWeapon.GetComponent<Gunscript2>();

                    if (gunScript == null)
                    {
                        //Debug.Log("gun Script 가 아닌 무기에 이 스테이트가 쓰였다?");
                        return;
                    }


                    ItemStoreDescBase ownerInventoryFirstMagazine = gunScript.FindMagazine();

                    if (ownerInventoryFirstMagazine == null)
                    {
                        //탄창이 없어요
                        return;
                    }
                    

                    bool reloadCheck = gunScript.ReloadCheck();

                    if (reloadCheck == false)
                    {
                        //Debug.Log("총을 재장전 할 수 없다.");
                        return;
                    }

                    gunScript.StartReloadingProcess(ownerInventoryFirstMagazine);
                    //발사
                }
                break;

            case AdditionalBehaveType.Weapon_Fire:
                {
                    //Debug.Log("발사검사 시작");
                    //현재 사용중인 무기에게 발사했냐를 확인하는 함수
                    GameObject currWeapon = (_tempUsingRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    KeyCode fireKeyCode = (_tempUsingRightHandWeapon == true)
                        ? KeyCode.Mouse0
                        : KeyCode.Mouse1;

                    if (Input.GetKey(fireKeyCode) == false)
                    {
                        ////Debug.Log("키가 안눌렸다");
                        return;
                    }

                    if (currWeapon == null)
                    {
                        Debug.Log("무기가 없다?");
                        return;
                    }

                    Gunscript2 gunScript = currWeapon.GetComponent<Gunscript2>();

                    if (gunScript == null)
                    {
                        //Debug.Log("gun Script 가 아닌 무기에 이 스테이트가 쓰였다?");
                        return;
                    }

                    bool weaponCanFire = gunScript.FireCheck();

                    if (weaponCanFire == false)
                    {
                        //Debug.Log("발사 준비가 안됐다");
                        return;
                    }


                    {
                        //탄창이 없다
                        //return;
                    }


                    {
                        //탄창에 탄이없다
                        //play 딸깍 소리
                        //return;
                    }


                    //Debug.Log("발사한다");

                    gunScript.Fire();
                    //발사
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
            Debug.Assert(false, "damageDesc가 null이여선 안된다");
            Debug.Break();
            return;
        }

        /*-------------------------------------------------------
        기본->StatComponent(무조건 여기서 출발한다)
        기본 스텟에 의한 데미지, 스테미나 계산
        -------------------------------------------------------*/
        {
            damageDesc._damage = myStat.CalculateStatDamage();
            damageDesc._damagingStamina = myStat.CalculateStatDamagingStamina();
            damageDesc._damagePower = myStat.CalculatePower();
        }


        /*-------------------------------------------------------
        무기 배수(좋은 무기면 더 아플것이다)
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
        애니메이션 배수(동작이 크면 아플것이다)
        -------------------------------------------------------*/
        StateContoller myStateController = GCST<StateContoller>();
        if (myStateController != null &&
            myStateController.GetCurrState()._myState._isAttackState == true)
        {
            DamageDesc attackMultiplyDesc = myStateController.GetCurrState()._myState._attackDamageMultiply;
            if (attackMultiplyDesc == null)
            {
                Debug.Log("공격상태지만 값이 설정돼지 않았다");
                attackMultiplyDesc = new DamageDesc();
            }

            damageDesc._damage *= attackMultiplyDesc._damage;
            damageDesc._damagingStamina *= attackMultiplyDesc._damagingStamina;
            damageDesc._damagePower *= attackMultiplyDesc._damagePower;
        }






        /*-------------------------------------------------------
        파지법 배수(양손으로 잡아서 휘두르면 더 아플것이다)
        -------------------------------------------------------*/
        if (_tempGrabFocusType != WeaponGrabFocus.Normal)
        {
            damageDesc._damage *= 1.2f;
            damageDesc._damagingStamina *= 1.2f;
            damageDesc._damagePower *= 1.2f;
        }







        /*-------------------------------------------------------
        버프 배수(미구현)
        -------------------------------------------------------*/
        {

        }
    }

    public virtual void DealMe_Final(DamageDesc damage, GameObject caller)
    {
        StatScript statScript = GCST<StatScript>();


        /*------------------------------------------------
        |NOTO| 이곳에서는 데미지 피격 감쇄, 상태변경만 계산합니다.
        ------------------------------------------------*/

        Debug.Log("들어온 데미지" + damage._damage);
        Debug.Log("들어온 스테미나데미지" + damage._damagingStamina);
        Debug.Log("들어온 파워" + damage._damagePower);

        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.Hit_Lvl_0;

        StateAsset currState = GCST<StateContoller>().GetCurrState();
        StateDesc currStateDesc = GCST<StateContoller>().GetCurrState()._myState;



        //가드중이였을때의 상태 계산 로직
        {
            if (currStateDesc._isBlockState == true)
            {
                //스테미나도 충분하고 강인도도 충분합니다
                if (statScript._stamina >= damage._damagingStamina &&
                    statScript._roughness >= damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Reaction;
                }

                //스테미나는 충분한데 강인도가 부족합니다.
                else if (statScript._stamina >= damage._damagingStamina &&
                    statScript._roughness < damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Sliding;
                }

                //강인도는 충분한데 스테미나가 부족합니다.
                else if (statScript._stamina < damage._damagingStamina &&
                    statScript._roughness >= damage._damagePower)
                {
                    nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
                    representType = RepresentStateType.Blocked_Crash;
                }

                //연결된 상태들을 가져와봄
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

                //스테미나가 부족하고 강인도도 부족합니다. 혹은 연결상태가 존재하지 않습니다
                if ((statScript._stamina < damage._damagingStamina && statScript._roughness < damage._damagePower) ||
                    nextStateAsseet == null)
                {
                    //맞는 상태로 가긴 할건데
                    nextGraphType = StateGraphType.HitStateGraph;

                    float deltaRoughness = damage._damagePower - statScript._roughness;

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

                float deltaRoughness = damage._damagePower - statScript._roughness;

                if (deltaRoughness <= MyUtil.deltaRoughness_lvl0) //강인도가 조금 부족하다
                {
                    representType = RepresentStateType.Hit_Lvl_0;
                }
                else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1) //강인도가 많이 부족하다
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
        --------------------------------------모든 데미지는 계산돼있어야 한다-------------------------------------------------------
        --------------------------------------------------------------------------------------------------------------*/

        int finalDamage = (int)damage._damage;

        statScript._hp -= finalDamage;
        if (statScript._hp <= 0)
        {
            Debug.Log("죽었다");

            //caller.
            ZeroHPCall(caller.GetComponent<CharacterScript>());


            //날라갈만큼의 데미지를 받고 죽는다
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

        //연쇄 충돌이 아닌 최초충돌입니다.
        TriggerEnterWithWeapon(other);

        //string tag = other.tag;

        //if (tag == "WeaponAttachedCollider")
        //{
        //    //연쇄 충돌이 아닌 최초충돌입니다.
        //    if (AnimationAttackManager.Instance.TriggerEnterCheck(this, other) == true)
        //    {
        //        TriggerEnterWithWeapon(other);
        //    }
        //    return;
        //}
    }


}
