using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static AnimationFrameDataAsset;
using static StateGraphAsset;
using static StatScript;
using static LevelStatAsset;
using static ItemAsset_Weapon;
using Unity.VisualScripting;
using static UnityEditor.Rendering.InspectorCurveEditor;
using UnityEngine.Animations.Rigging;
using UnityEditor.Experimental.GraphView;

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
    public enum DamageReason
    {
        Ray,
        Collision,
    }

    public enum HitType
    {
        Blunt,
        Slash,
        End,
    }

    //public class DamageDescToApply
    //{
    //    public int _damage = 1;
    //    public int _damagePower = 1;
    //    public int _damagingStamina = 1;
    //}

    //public DamageDesc ShallowCopy()
    //{
    //    return MemberwiseClone() as DamageDesc;
    //}

    public DamageReason _damageReason = DamageReason.Collision;
    public HitType _hitType = HitType.Blunt;

    public float _damage = 1;
    public float _damagePower = 1;
    public float _damagingStamina = 1;
}


public enum CharacterType
{
    Player,
    Monster_Zombie,
}


public class CharacterScript : GameActorScript, IHitable
{
    [SerializeField] private CharacterType _characterType = CharacterType.Player;
    public CharacterType _CharacterType => _characterType;

    protected bool _dead = false;
    public bool GetDead() { return _dead; }


    //델리게이트 타입들
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);

    //캐릭터 중심
    protected GameObject _characterHeart = null;
    public GameObject _CharacterHeart => _characterHeart;

    //대표 컴포넌트들
    public List<GameCharacterSubScript> _components = new List<GameCharacterSubScript>();
    private Dictionary<Type, Component> _mySubScripts = new Dictionary<Type, Component>();



    //인벤토리
    [SerializeField] protected GameObject _inventoryUIPrefab = null;
    protected List<InventoryBoard> _inventoryBoardCached = new List<InventoryBoard>();

    //무기 관련
    //현재 손에 쥐고있다 = 객체화가 됐다 = GameObject 로 들고있는게 맞음---------------------------
    protected GameObject _tempCurrLeftWeapon = null;
    protected GameObject _tempCurrRightWeapon = null;
    //----------------------------------------------------------------------------------------

    protected Dictionary<AnimatorLayerTypes, GameObject> _currHandObject = new Dictionary<AnimatorLayerTypes, GameObject>();


    protected KeyCode _changeRightHandWeaponHandlingKey = KeyCode.B;
    protected KeyCode _changeLeftHandWeaponHandlingKey = KeyCode.V;

    protected KeyCode _changeRightWeaponKey = KeyCode.T;
    protected KeyCode _changeLeftWeaponKey = KeyCode.R;
    [SerializeField] protected int _currLeftWeaponIndex = 0;
    [SerializeField] protected int _currRightWeaponIndex = 0;
    protected int _tempMaxWeaponSlot = 3;

    protected KeyCode _useUseableItemKey = KeyCode.Z;
    protected string _changeUseableItemKey = "Mouse ScrollWheel";
    protected List<ItemStoreDescBase> _tempUseableItemDescs = new List<ItemStoreDescBase>();
    [SerializeField] protected int _currUseableItemIndex = 0;
    protected int _tempMaxUseableSlot = 6;

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

        //if (target == null && isNullable == true)
        //{
        //    Debug.Assert(false, "없는 컴포넌트를 찾으려 하고있다" + typeof(T));
        //    Debug.Break();
        //}

        return (T)target;
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
        for (int i = 0; i < _tempMaxUseableSlot; i++)
        {
            _tempUseableItemDescs.Add(null);
        }
    }




    //HP가 0이되어 죽는 연출을 시작합니다
    protected virtual void ZeroHPCall(CharacterScript killedBy)
    {
        _dead = true;

        AimScript2 aimScript = GetCharacterSubcomponent<AimScript2>(true);
        if (aimScript != null &&
            aimScript.GetAimState() == AimState.eLockOnAim &&
            aimScript.GetLockOnObject() != null)
        {
            aimScript.OffAimState();
        }

        if (killedBy != null)
        {
            killedBy.YouKillThisObject(gameObject);
        }
    }




    public virtual void YouKillThisObject(GameObject killObject)
    {
        AimScript2 aimScript = GetCharacterSubcomponent<AimScript2>(true);

        if (aimScript != null &&
            aimScript.GetAimState() == AimState.eLockOnAim &&
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
        gameObject.layer = LayerMask.NameToLayer("DeadObject");

        GCST<CharacterContollerable>().CharacterDie();
    }


    public virtual void CharacterRevive(int hp = 0)
    {
        _dead = false;
        StateContoller stateController = GCST<StateContoller>();
        stateController.enabled = true;

        //뒤로 누웠으면 뒤에서 누웠다 일어나는 자세로 연결, 앞으로 누웠으면 앞으로 누웠다 일어나는 자세로 연결
        {
            stateController.TryChangeState(StateGraphType.DieGraph, stateController._CurrLinkedStated.First()._linkedStateWrapper._linkedState);
        }
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


    public void DestroyHandObject(AnimatorLayerTypes layerType)
    {
        if (_currHandObject[layerType] == null)
        {
            return;
        }

        Destroy(_currHandObject[layerType]);

        _currHandObject.Remove(layerType);
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

        //아이템 버프 해제
        ItemAsset_Weapon weaponAsset = null;
        StatScript statScript = GCST<StatScript>();


        if (layerType == AnimatorLayerTypes.RightHand && _tempCurrRightWeapon != null)
        {
            WeaponScript weaponScript = _tempCurrRightWeapon.GetComponent<WeaponScript>();
            if (weaponScript != null)
            {
                weaponAsset = (ItemAsset_Weapon)weaponScript._ItemStoreInfo._itemAsset;
            }
            Destroy(_tempCurrRightWeapon);
            _tempCurrRightWeapon = null;
            targetType = StateGraphType.WeaponState_RightGraph;
        }
        else if (layerType == AnimatorLayerTypes.LeftHand && _tempCurrLeftWeapon != null)
        {
            WeaponScript weaponScript = _tempCurrLeftWeapon.GetComponent<WeaponScript>();
            if (weaponScript != null)
            {
                weaponAsset = (ItemAsset_Weapon)weaponScript._ItemStoreInfo._itemAsset;
            }

            Destroy(_tempCurrLeftWeapon);
            _tempCurrLeftWeapon = null;
            targetType = StateGraphType.WeaponState_LeftGraph;
        }

        foreach (BuffAssetBase buff in weaponAsset._Buffs)
        {
            statScript.RemoveBuff(buff, -1);
        }


        StateGraphAsset basicAsset = GetCharacterSubcomponent<StateContoller>().GetBasicStateGraphes(targetType);
        if (basicAsset == null)
        {
            return;
        }

        GetCharacterSubcomponent<StateContoller>().EquipStateGraph(basicAsset, targetType);


        //충돌체 업데이트
        {
            //장착한 후, 콜라이더를 업데이트 한다.

            CharacterModelDataInitializer myModelDataInitlaizer = GCST<CharacterAnimatorScript>()._CurrModelDataInitializer;

            ColliderAttachType colliderType = (layerType == AnimatorLayerTypes.RightHand)
                ? ColliderAttachType.HumanoidRightHand
                : ColliderAttachType.HumanoidLeftHand;

            WeaponColliderScript basicColliderScript = null;
            myModelDataInitlaizer._BasicColliders.TryGetValue(colliderType, out basicColliderScript);

            if (basicColliderScript != null) 
            {
                GCST<CharacterColliderScript>().ChangeCollider(colliderType, basicColliderScript.gameObject);
                basicColliderScript.gameObject.SetActive(false);
            }
        }

        //HUD Setting
        {
            InventoryHUDScript.InventoryHUDType targetHUDType = (layerType == AnimatorLayerTypes.RightHand)
                ? InventoryHUDScript.InventoryHUDType.RightWeapon
                : InventoryHUDScript.InventoryHUDType.LeftWeapon;

            InventoryHUDScript targetHUDScript = UIManager.Instance._CurrHUD.GetInventoryHUDScript(targetHUDType);

            targetHUDScript.SetImage(null);
        }
    }

    public void ApplyPotionBuff(List<string> buffNames)
    {
        StatScript statScript = GCST<StatScript>();
        foreach (string name in buffNames) 
        {
            statScript.ApplyBuff(LevelStatInfoManager.Instance.GetBuff(name), 1);
        }
    }

    public void ApplyPotionBuff(List<BuffAssetBase> buffs)
    {
        StatScript statScript = GCST<StatScript>();
        foreach (BuffAssetBase buff in buffs)
        {
            statScript.ApplyBuff(buff, 1);
        }
    }

    public void CreateWeaponModelAndEquip(AnimatorLayerTypes layerType, ItemStoreDescBase itemStoreDesc)
    {
        if (layerType != AnimatorLayerTypes.RightHand && layerType != AnimatorLayerTypes.LeftHand) {return;}

        if (itemStoreDesc._itemAsset._EquipType == ItemAsset.EquipType.UseAndComsumeableByCharacter)
        {
            Transform correctSocket = null; //무조건 오른손
            {
                Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "무기를 붙이려는데 모델이 없어서는 안된다");

                WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

                Debug.Assert(weaponSockets.Length > 0, "뭔가 붙여줄려는데 소켓이 없다");

                foreach (var socketComponent in weaponSockets)
                {
                    if (socketComponent._sideType != WeaponSocketScript.SideType.Right)
                    {
                        continue;
                    }

                    correctSocket = socketComponent.gameObject.transform;
                    break;
                }

                if (correctSocket == null)
                {
                    Debug.Assert(false, "무기를 붙일 수 있는 소켓이 없습니다");
                    Debug.Break();
                    return;
                }

            }

            //아이템 프리팹 생성, 장착
            GameObject itemModel = Instantiate(itemStoreDesc._itemAsset._ItemModel, correctSocket);

            _currHandObject.Add(layerType, itemModel);
        }

        if (itemStoreDesc._itemAsset._EquipType == ItemAsset.EquipType.Weapon)
        {
            WeaponSocketScript.SideType targetSide = (layerType == AnimatorLayerTypes.RightHand)
                ? WeaponSocketScript.SideType.Right
                : WeaponSocketScript.SideType.Left;

            ItemStoreDesc_Weapon nextItemStoreDesc = (ItemStoreDesc_Weapon)itemStoreDesc;
            
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

                //버프걸기
                {
                    ItemAsset_Weapon weaponAsset = (ItemAsset_Weapon)itemStoreDesc._itemAsset;
                    foreach (BuffAssetBase buff in weaponAsset._Buffs)
                    {
                        GCST<StatScript>().ApplyBuff(buff, 1);
                    }
                }

                //HUD 세팅
                InventoryHUDScript.InventoryHUDType targetHUDType = (layerType == AnimatorLayerTypes.RightHand)
                    ? InventoryHUDScript.InventoryHUDType.RightWeapon
                    : InventoryHUDScript.InventoryHUDType.LeftWeapon;

                InventoryHUDScript targetHUDScript = UIManager.Instance._CurrHUD.GetInventoryHUDScript(targetHUDType);

                targetHUDScript.SetImage(nextItemStoreDesc._itemAsset._ItemImage);
            }
        }
    }

    #endregion WeaponSection


    public void WeaponSwitchHand(AnimatorLayerTypes layerType, Transform oppositeTransform)
    {
        WeaponScript targetWeaponScript = (layerType == AnimatorLayerTypes.RightHand)
            ? _tempCurrRightWeapon.GetComponent<WeaponScript>()
            : _tempCurrLeftWeapon.GetComponent<WeaponScript>();

        targetWeaponScript.Equip_OnSocket(oppositeTransform);
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

    private void TriggerEnterWithWeapon(Collider other, bool isWeakPointCollider, ref Vector3 closetPoint, ref Vector3 hitNormal)
    {
        CharacterScript attackerCharacterScript = other.gameObject.GetComponentInParent<CharacterScript>();

        #region NullCheck
        if (attackerCharacterScript == null)
        {
            Debug.Assert(false, "attackerCharacterScript 이 널입니다?");
            Debug.Break();
        }

        if (other == null)
        {
            Debug.Assert(false, "other 이 널입니다?");
            Debug.Break();
        }
        #endregion NullCheck

        DamageDesc currentDamage = attackerCharacterScript.CalculateAttackerDamage(other, isWeakPointCollider);


        /*----------------------------------------------------------
        |NOTI| isWeakPointCollider 가 True 라면 부위에 의한치명타입니다
        ----------------------------------------------------------*/
        DealMe_Final(currentDamage, isWeakPointCollider, attackerCharacterScript, this, ref closetPoint, ref hitNormal);
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

    public virtual LayerMask CalculateWeaponColliderIncludeLayerMask()
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
        if (GCST<StatScript>().enabled == true)
        {
            GCST<StatScript>().StatScriptUpdate();
        }

        //현재 상태 업데이트
        if (GCST<StateContoller>().enabled == true)
        {
            GCST<StateContoller>().DoWork();
        }

        //기본적으로 중력은 계속 업데이트 한다
        {
            GCST<CharacterContollerable>().MoverUpdate();
        }
    }

    protected void CheckUseableItemChange()
    {
        if (UIManager.Instance.IsConsumeInput() == true)
        {
            return;
        }

        float axisVal = Input.GetAxis(_changeUseableItemKey);

        if (axisVal == 0.0f)
        {
            return;
        }

        int nextVal = (axisVal > 0.0f)
            ? 1
            : -1;

        _currUseableItemIndex += nextVal;

        if (_currUseableItemIndex >= _tempMaxUseableSlot)
        {
            _currUseableItemIndex %= _tempMaxUseableSlot;
        }

        if (_currUseableItemIndex < 0)
        {
            _currUseableItemIndex = _tempMaxUseableSlot - 1;
        }

        Sprite itemSprite = (_tempUseableItemDescs[_currUseableItemIndex] == null)
            ? null
            : _tempUseableItemDescs[_currUseableItemIndex]._itemAsset._ItemImage;

        InventoryHUDScript targetHUDScript = UIManager.Instance._CurrHUD.GetInventoryHUDScript(InventoryHUDScript.InventoryHUDType.Useable);

        targetHUDScript.SetImage(itemSprite);
    }


    public void StateChanged(StateAsset nextState)
    {
        /*-------------------------------------------------------
        |NOTI| AnimationSpeed가 엮여있음으로 가장먼저 업데이트합니다
        -------------------------------------------------------*/
        GCST<CharacterAnimatorScript>().StateChanged(nextState);


        GCST<CharacterColliderScript>().StateChanged();
        GCST<CharacterContollerable>().StateChanged();
    }




    public void SetEquipItem_Weapon(bool isRightWeapon, int index, ItemStoreDesc_Weapon weaponInfo)
    {
        List<ItemStoreDesc_Weapon> targetWeaponPrefabs = (isRightWeapon == true)
            ? _tempRightWeaponPrefabs
            : _tempLeftWeaponPrefabs;

        ItemStoreDesc_Weapon prev = targetWeaponPrefabs[index];

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
            if (_currRightWeaponIndex < 0)
            {
                _currRightWeaponIndex = _tempMaxWeaponSlot - 1;
            }
        }
        else
        {
            _currLeftWeaponIndex--;
            if (_currLeftWeaponIndex < 0)
            {
                _currLeftWeaponIndex = _tempMaxWeaponSlot - 1;
            }
        }

        GCST<CharacterAnimatorScript>().CalculateBodyWorkType_ChangeWeapon(_tempGrabFocusType, isRightWeapon, -1, true);
        return;
    }

    public void SetEquipItem_Useable(int index, ItemStoreDescBase useableItemDesc)
    {
        if ((index == _currUseableItemIndex) == true)
        {
            InventoryHUDScript targetHUDScript = UIManager.Instance._CurrHUD.GetInventoryHUDScript(InventoryHUDScript.InventoryHUDType.Useable);

            Sprite targetSprite = (useableItemDesc == null)
                ? null
                :useableItemDesc._itemAsset._ItemImage;

            targetHUDScript.SetImage(targetSprite);
        }

        _tempUseableItemDescs[index] = useableItemDesc;

        return;
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
                        break;
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
                        break;
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
                        break; //양손잡기 시도가 이루어지지 않았다. 아무일도 일어나지 않는다
                    }

                    GameObject targetWeapon = (isRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    if (targetWeapon == null)
                    {
                        break; //양손잡기를 시도했지만 무기가 없다.
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
                    if (UIManager.Instance.IsConsumeInput() == true)
                    {
                        break;
                    }

                    if (Input.GetKeyDown(_useUseableItemKey) == false)
                    {
                        break;
                    }

                    ItemStoreDescBase useableItemStoreDesc = _tempUseableItemDescs[_currUseableItemIndex];

                    if (useableItemStoreDesc == null)
                    {
                        break;
                    }

                    //사용 부위 체크
                    //int willBusyLayer = 0;
                    {
                        ////순수 아이템만으로 필요한 레이어 체크
                        //if (useableItemAsset._UsingItemMustNotBusyLayers != null || useableItemAsset._UsingItemMustNotBusyLayers.Count > 0)
                        //{
                        //    if (useableItemAsset._UsingItemMustNotBusyLayer < 0)
                        //    {
                        //        useableItemAsset._UsingItemMustNotBusyLayer = 0;

                        //        foreach (var item in useableItemAsset._UsingItemMustNotBusyLayers)
                        //        {
                        //            useableItemAsset._UsingItemMustNotBusyLayer = (useableItemAsset._UsingItemMustNotBusyLayer | 1 << (int)item);
                        //        }
                        //    }

                        //    willBusyLayer = newTestingItem._UsingItemMustNotBusyLayer;
                        //}

                        ////현재 무기 파지법에 의해 필요한 레이어 체크
                        //if (_tempCurrRightWeapon != null)
                        //{
                        //    willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        //}
                    }

                    int willBusyLayer = (1 << (int)AnimatorLayerTypes.RightHand | 1 << (int)AnimatorLayerTypes.Head);

                    if ((currentAnimatorBusyLayerBitShift & willBusyLayer) != 0)
                    {
                        return; //해당 부위들은 지금 할일이 있다
                    }

                    //Work를 담는다 이전에 Lock 계산을 끝낼것.
                    GCST<CharacterAnimatorScript>().CalculateBodyWorkType_UseItem_Drink(_tempGrabFocusType, useableItemStoreDesc, willBusyLayer);
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

    public virtual DamageDesc CalculateAttackerDamage(Collider other, bool isWeakPoint)
    {
        DamageDesc damageDesc = new DamageDesc();

        StatScript myStat = GCST<StatScript>();

        /*-------------------------------------------------------
        기본 스텟에 의한 데미지, 스테미나 계산
        -------------------------------------------------------*/
        {
            damageDesc._damage = myStat.CalculateStatDamage();
            damageDesc._damagingStamina = myStat.CalculateStatDamagingStamina();
            damageDesc._damagePower = myStat.CalculatePower();
        }

        myStat.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_AttackerBuffCheck, damageDesc, isWeakPoint, this, null);
        /*-------------------------------------------------------
        공격자에게 버프가 걸려있었나? (미구현입니다.)
        -------------------------------------------------------*/
        {

        }

        /*-------------------------------------------------------
        무기에 의한 데미지 (미구현입니다.)
        -------------------------------------------------------*/
        {
            //WeaponScript otherWeaponScript = other.GetComponentInParent<WeaponScript>();
            //if (otherWeaponScript != null)
            //{
            //    DamageDesc weaponDamageDest = otherWeaponScript.GetItemAsset()._WeaponDamageDesc;
            //    damageDesc._damage += weaponDamageDest._damage;
            //    damageDesc._damagingStamina += weaponDamageDest._damagingStamina;
            //    damageDesc._damagePower += weaponDamageDest._damagePower;
            //}
        }

        /*-------------------------------------------------------
        애니메이션 배수
        -------------------------------------------------------*/
        {
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
                damageDesc._hitType = attackMultiplyDesc._hitType;
            }
        }

        /*-------------------------------------------------------
        양손 배수(양손으로 잡아서 휘두르면 더 아플것이다)
        -------------------------------------------------------*/
        {
            if (_tempGrabFocusType != WeaponGrabFocus.Normal)
            {
                damageDesc._damage *= 1.2f;
                damageDesc._damagingStamina *= 1.2f;
                damageDesc._damagePower *= 1.2f;
            }
        }

        myStat.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_AttackerBuffCheck, damageDesc, isWeakPoint, this, null);

        return damageDesc;
    }


    private void CalculateNextState_Guard(ref StateGraphType nextGraphType, ref RepresentStateType representType, DamageDesc damage, StatScript statScript, StateAsset currState)
    {
        Debug.Log("가드버프 계산을 시작합니다");

        //int currRoughness = statScript.GetPassiveStat(PassiveStat.Roughness);
        int currRoughness = 7;
        


        //스테미나도 충분하고 강인도도 충분합니다
        if (statScript.GetActiveStat(ActiveStat.Stamina) >= damage._damagingStamina &&
            currRoughness >= damage._damagePower)
        {
            nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
            representType = RepresentStateType.Blocked_Reaction;
        }

        //스테미나는 충분한데 강인도가 부족합니다.
        else if (statScript.GetActiveStat(ActiveStat.Stamina) >= damage._damagingStamina &&
            currRoughness < damage._damagePower)
        {
            nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
            representType = RepresentStateType.Blocked_Sliding;
        }

        //강인도는 충분한데 스테미나가 부족합니다.
        else if (statScript.GetActiveStat(ActiveStat.Stamina) < damage._damagingStamina &&
            currRoughness >= damage._damagePower)
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
        if ((statScript.GetActiveStat(ActiveStat.Stamina) < damage._damagingStamina && statScript.GetPassiveStat(PassiveStat.Roughness) < damage._damagePower) ||
            nextStateAsseet == null)
        {
            Debug.Log("가드 버프가 걸렸지만 맞는상태로 갈껍니다");

            //맞는 상태로 가긴 할건데
            nextGraphType = StateGraphType.HitStateGraph;

            float deltaRoughness = damage._damagePower - statScript.GetPassiveStat(PassiveStat.Roughness);

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


    protected void DealMe_Test(DamageDesc damage)
    {
        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;
        StatScript statScript = GCST<StatScript>();

        //Step3. 데미지 적용
        {
            bool willDead = false;

            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_HP) <= 0)
            {
                statScript.ChangeActiveStat(ActiveStat.Hp, -(int)damage._damage);
            }


            if (statScript.GetActiveStat(ActiveStat.Hp) <= 0)
            {
                willDead = true;

                ZeroHPCall(this);

                nextGraphType = StateGraphType.DieGraph;

                if (GCST<StateContoller>().GetStateGraphes()[(int)StateGraphType.DieGraph] == null)
                {
                    Debug.Assert(false, "죽을건데 죽는 그래프가 없네요?");
                    Debug.Break();
                }

                if (representType == RepresentStateType.Hit_Lvl_2)
                {
                    representType = RepresentStateType.DieThrow;
                }
                else
                {
                    representType = RepresentStateType.DieNormal;
                }
            }




            //데미지 적용하는데, 상태는 어떻게 변경될까요?
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //CC면역이 걸려있지 않고,
                willDead == false) //다음에 죽지 않을 예정이라면 -> 죽을거였으면 죽는모션으로 간다는게 위에 계산돼있다
            {
                //다음 상태를 계산합니다.

                if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0)
                {
                    //가드중이였다 = 움찔관련 모션을 계산한다
                    CalculateNextState_Guard(ref nextGraphType, ref representType, damage, statScript, GCST<StateContoller>().GetCurrState());
                }
                else
                {
                    float deltaRoughness = damage._damagePower - statScript.GetPassiveStat(PassiveStat.Roughness);

                    if (deltaRoughness > 0)
                    {
                        if (deltaRoughness <= MyUtil.deltaRoughness_lvl0)
                        {
                            representType = RepresentStateType.Hit_Lvl_0;
                        }
                        else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1)
                        {
                            representType = RepresentStateType.Hit_Lvl_1;
                        }
                        else
                        {
                            representType = RepresentStateType.Hit_Lvl_2;
                        }
                    }
                }
            }


            if (willDead == true || //다음에 죽을 예정이거나
                (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && representType != RepresentStateType.End)) //CC면역이 걸려있지 않다면
            {
                //Vector3 toAttackerDir = (attacker.transform.position - transform.position);
                //toAttackerDir.y = 0.0f;
                //toAttackerDir = toAttackerDir.normalized;

                //GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(toAttackerDir));
                GCST<StateContoller>().TryChangeState(nextGraphType, representType);
            }
        }
    }


    /*--------------------------------------------------------------------
    |TOOD| DealMe_Final과 유사합니다. 두 함수의 공통적인 부분이 있지 않을까요?
    --------------------------------------------------------------------*/
    private IEnumerator RipositeCoroutine
        (
            bool isWeakPoint,
            CharacterScript attacker,
            CharacterScript victim,
            Vector3 closetPoint,
            Vector3 hitNormal,
            float timeTarget,
            RepresentStateType ripositeType
        )
    {
        float timeACC = 0.0f;

        while (true) 
        {
            timeACC += Time.deltaTime;

            if (timeACC >= timeTarget)
            {
                DealMe_Reposite(isWeakPoint, attacker, victim, ref closetPoint, ref hitNormal, ripositeType);
                break;
            }

            yield return null;
        }

    }







    public virtual void DealMe_Reposite(bool isWeakPoint, CharacterScript attacker, CharacterScript victim, ref Vector3 closetPoint, ref Vector3 hitNormal, RepresentStateType ripositeType)
    {
        DamageDesc damage = attacker.CalculateAttackerDamage(null, isWeakPoint);

        StatScript statScript = GCST<StatScript>();
        StatScript attackerStatScript = attacker.GCST<StatScript>();

        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_InvincibleCheck, damage, isWeakPoint, attacker, victim);
        // Step0 무적체크
        {
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible) > 0)
            {
                Debug.Log("무적이라서 씹었다");
                damage._damage = 0;
                damage._damagingStamina = 0;

                statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);
                return;
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);




        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;




        // Step1 피격자 버프체크
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_BuffCheck, damage, isWeakPoint, attacker, victim);
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_BuffCheck, damage, isWeakPoint, attacker, victim);




        // Step2 대미지 적용
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_ApplyDamage, damage, isWeakPoint, attacker, victim);
        {
            bool willDead = false;

            //데미지가 적용된다
            {
                if (statScript.GetPassiveStat(PassiveStat.IsInvincible_HP) <= 0)
                {
                    statScript.ChangeActiveStat(ActiveStat.Hp, -(int)damage._damage);
                }

                if (statScript.GetActiveStat(ActiveStat.Hp) <= 0)
                {
                    willDead = true;

                    ZeroHPCall(attacker);

                    nextGraphType = StateGraphType.DieGraph;

                    if (GCST<StateContoller>().GetStateGraphes()[(int)StateGraphType.DieGraph] == null)
                    {
                        Debug.Assert(false, "죽을건데 죽는 그래프가 없네요?");
                        Debug.Break();
                    }

                    switch (ripositeType)
                    {
                        case RepresentStateType.Riposte_BackStab:
                            representType = RepresentStateType.Hit_Riposte_BackStab;
                            break;
                        case RepresentStateType.Riposte_FrontStab:
                            representType = RepresentStateType.Hit_Riposte_FrontStab;
                            break;
                        case RepresentStateType.Riposte_Smash:
                            representType = RepresentStateType.Die_Riposte_Smash;
                            break;
                        default:
                            {
                                Debug.Assert(false, "Riposite가 아닌데 이 함수가 호출됐다고?");
                                Debug.Break();
                            }
                            break;
                    }
                }
            }

            bool isPostureOverPhase1 = false; //체간이 Phase1을 넘겼습니다.
            bool isPostureMax = false; //체간이 꽉 찼습니다.

            //체간 적용
            /*-------------------------------------------------------
            |TODO| 특별한 공식이 있으면 하세요. 지금은 바로 적용시킵니다
            -------------------------------------------------------*/
            if (willDead == false)
            {
                statScript.ChangeActiveStat(ActiveStat.PosturePercent, (int)damage._damagePower);
                isPostureOverPhase1 = (statScript.GetPassiveStat(PassiveStat.PostruePercentPhase1) <= statScript.GetActiveStat(ActiveStat.PosturePercent));
                isPostureMax = (statScript.GetActiveStat(ActiveStat.PosturePercent) == 100);
            }


            //데미지 적용하는데, 상태는 어떻게 변경될까요?
            //CC면역이 걸려있지 않고,
            //다음에 죽지 않을 예정이라면 -> 죽을거였으면 죽는모션으로 간다는게 위에 계산돼있다. 다시 계산하는건 낭비다
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && willDead == false)
            {
                /*-----------------------------------------------------
                |NOTI| Ver1... 체간없이 그냥 계산하는 버전 ... 이건 플레이어만 씁니다!!!
                -----------------------------------------------------*/
                if (_characterType == CharacterType.Player)
                {
                    if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0 &&
                        true /*가드 범위 내에서 맞았냐? 가드하는데 뒤에서 맞으면 실패임*/)
                    {
                        CalculateNextState_Guard(ref nextGraphType, ref representType, damage, statScript, GCST<StateContoller>().GetCurrState());
                    }
                    else
                    {
                        float deltaRoughness = damage._damagePower - statScript.GetPassiveStat(PassiveStat.Roughness);

                        if (deltaRoughness > 0)
                        {
                            if (deltaRoughness <= MyUtil.deltaRoughness_lvl0)
                            {
                                representType = RepresentStateType.Hit_Lvl_0;
                            }
                            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1)
                            {
                                representType = RepresentStateType.Hit_Lvl_1;
                            }
                            else
                            {
                                representType = RepresentStateType.Hit_Lvl_2;
                            }
                        }
                    }
                }

                /*-----------------------------------------------------
                |NOTI| Ver2... 체간을 계산하는 버전 ... 이건 몬스터 용도 입니다!
                -----------------------------------------------------*/
                else
                {
                    switch (ripositeType)
                    {
                        case RepresentStateType.Riposte_BackStab:
                            representType = RepresentStateType.Hit_Riposte_BackStab;
                            break;
                        case RepresentStateType.Riposte_FrontStab:
                            representType = RepresentStateType.Hit_Riposte_FrontStab;
                            break;
                        case RepresentStateType.Riposte_Smash:
                            representType = RepresentStateType.Hit_Riposte_Smash;
                            break;
                        default:
                            {
                                Debug.Assert(false, "Riposite가 아닌데 이 함수가 호출됐다고?");
                                Debug.Break();
                            }
                            break;
                    }
                }
            }

            attackerStatScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.ApplyDamage_AttackerCallback, damage, isWeakPoint, attacker, victim);

            //이펙트 생성을 위한 코드영역
            {
                string effectName = "HitSparkEffect";

                //공격에 의한 이펙트가 결정됩니다.
                switch (damage._hitType)
                {
                    case DamageDesc.HitType.Blunt:
                        {
                            if (isWeakPoint == true && damage._damageReason == DamageDesc.DamageReason.Ray)
                            {
                                effectName = "HitSparkCritical";
                            }
                        }
                        break;
                    case DamageDesc.HitType.Slash:
                        effectName = "SlashedSparkEffect";
                        break;
                    case DamageDesc.HitType.End:
                        {
                            //Debug.Assert(false, "상태 Asset에서 어떻게 때리는지 지정하세요. 기본값 '타격'으로 세팅됩니다");
                        }
                        break;
                }

                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);
            }


            //-------------------------------------------------------------------------------------------------------------
            if (willDead == true || //-----------------------------------------------------다음에 죽을 예정이거나,
                statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //-----CC면역이 걸려있지 않거나,
                (representType != RepresentStateType.End && representType != RepresentStateType.Hit_Riposte_BackStab && representType != RepresentStateType.Hit_Riposte_FrontStab)) 
                //(representType != RepresentStateType.End))
            { //-----------------------------------------------------------------------------------------------------------

                //해당 방향을 바라보면서 다음 결정된 상태를 연출하는 코드일 뿐이다

                

                if (representType != RepresentStateType.Hit_Riposte_BackStab && representType != RepresentStateType.Hit_Riposte_FrontStab)
                {
                    Vector3 toAttackerDir = (attacker.transform.position - transform.position);
                    toAttackerDir.y = 0.0f;
                    toAttackerDir = toAttackerDir.normalized;

                    GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(toAttackerDir));
                    GCST<StateContoller>().TryChangeState(nextGraphType, representType);
                }
                else
                {
                    
                    //CharacterAnimatorScript characterAnimatorScript = GCST<CharacterAnimatorScript>();
                    //int currFullBodyLayer = characterAnimatorScript.GetCurrFullBodyLayer();
                    //float currProgress = GCST<CharacterAnimatorScript>().GetAnimationProgress((int)AnimatorLayerTypes.FullBody);

                    //GCST<StateContoller>().TryChangeStateContinue(nextGraphType, representType);

                    //characterAnimatorScript.SetAnimationProgress(currFullBodyLayer, currProgress);
                }

                
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_ApplyDamage, damage, isWeakPoint, attacker, victim);
    }










    public virtual void DealMe_Final(DamageDesc damage, bool isWeakPoint, CharacterScript attacker, CharacterScript victim, ref Vector3 closetPoint, ref Vector3 hitNormal)
    {
        StatScript statScript = GCST<StatScript>();
        StatScript attackerStatScript = attacker.GCST<StatScript>();

        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_InvincibleCheck, damage, isWeakPoint, attacker, victim);
        // Step0 무적체크
        {
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible) > 0)
            {
                Debug.Log("무적이라서 씹었다");
                damage._damage = 0;
                damage._damagingStamina = 0;

                statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);
                return;
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);


        //무적이 걸려있었더라면, 애당초 패링이 성공할 수 없다.
        //그리고 패링 성공 여부 판정 시작은 피격자에서 시작
        if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("ParryBuff")) != null &&
            true /*패링 가능 공격이고  -- 일단 전부 패링 가능합니다. 내 강인도와 상대 공격의 Power의 차이? */ && 
            true /*타격점이 일정 각도 내 라면*/)
        {
            //이펙트 생성
            {
                string effectName = "HitSparkEffect";

                //공격에 의한 이펙트가 결정됩니다.
                switch (damage._hitType)
                {
                    case DamageDesc.HitType.Blunt:
                        {
                            if (isWeakPoint == true && damage._damageReason == DamageDesc.DamageReason.Ray)
                            {
                                effectName = "HitSparkCritical";
                            }
                        }
                        break;
                    case DamageDesc.HitType.Slash:
                        effectName = "SlashedSparkEffect";
                        break;
                    case DamageDesc.HitType.End:
                        {
                            //Debug.Assert(false, "상태 Asset에서 어떻게 때리는지 지정하세요. 기본값 '타격'으로 세팅됩니다");
                        }
                        break;
                }

                GameObject shieldWeapon = (GCST<StateContoller>().GetCurrStateGraphType() == StateGraphType.WeaponState_RightGraph)
                    ? _tempCurrRightWeapon
                    : _tempCurrLeftWeapon;

                closetPoint = shieldWeapon.transform.position; //방패피격점
                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);
                EffectManager.Instance.CreateEffect("ConeSparkEffect", hitNormal, closetPoint);
                EffectManager.Instance.CreateEffect("CartoonSparkEffect", hitNormal, attacker._CharacterHeart.transform.position);
            }

            //공격자 Stagger_Parried로 바꾸기
            {
                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                attackerStateController.TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Stagger_Parried);
                attacker.AfterDealMe();
            }

            return;
        }



        Debug.Log("들어온 데미지" + damage._damage);
        Debug.Log("들어온 스테미나데미지" + damage._damagingStamina);
        Debug.Log("들어온 파워" + damage._damagePower);


        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;

        // Step1 피격자 버프체크
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_BuffCheck, damage, isWeakPoint, attacker, victim);
        {
            if (isWeakPoint == true && damage._damageReason == DamageDesc.DamageReason.Ray)
            {
                damage._damage *= 10.0f;
                Debug.Log("---격발 치명타--- || 치명타데미지 : " + damage._damage);
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_BuffCheck, damage, isWeakPoint, attacker, victim);

        // Step2 대미지 적용
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_ApplyDamage, damage, isWeakPoint, attacker, victim);
        {
            if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0)
            {
                /*--------------------------------------------
                임시로 데미지 무효화함
                => 가드버프는 데미지를 올려주는 방식으로 접근할것
                --------------------------------------------*/
                damage._damage = 0;
            }

            bool willDead = false;

            //데미지가 적용된다
            {
                if (statScript.GetPassiveStat(PassiveStat.IsInvincible_HP) <= 0)
                {
                    statScript.ChangeActiveStat(ActiveStat.Hp, -(int)damage._damage);
                }

                if (statScript.GetActiveStat(ActiveStat.Hp) <= 0)
                {
                    willDead = true;

                    ZeroHPCall(attacker);

                    nextGraphType = StateGraphType.DieGraph;

                    if (GCST<StateContoller>().GetStateGraphes()[(int)StateGraphType.DieGraph] == null)
                    {
                        Debug.Assert(false, "죽을건데 죽는 그래프가 없네요?");
                        Debug.Break();
                    }

                    if (representType == RepresentStateType.Hit_Lvl_2)
                    {
                        representType = RepresentStateType.DieThrow;
                    }
                    else
                    {
                        representType = RepresentStateType.DieNormal;
                    }
                }
            }

            bool isPostureOverPhase1 = false; //체간이 Phase1을 넘겼습니다.
            bool isPostureMax = false; //체간이 꽉 찼습니다.

            //체간 적용
            /*-------------------------------------------------------
            |TODO| 특별한 공식이 있으면 하세요. 지금은 바로 적용시킵니다
            -------------------------------------------------------*/
            if (willDead == false)
            {
                statScript.ChangeActiveStat(ActiveStat.PosturePercent, (int)damage._damagePower);
                isPostureOverPhase1 = (statScript.GetPassiveStat(PassiveStat.PostruePercentPhase1) <= statScript.GetActiveStat(ActiveStat.PosturePercent));
                isPostureMax = (statScript.GetActiveStat(ActiveStat.PosturePercent) == 100);
            }

            bool isRipositeAttack = false;
            //데미지 적용하는데, 상태는 어떻게 변경될까요?
            //CC면역이 걸려있지 않고,
            //다음에 죽지 않을 예정이라면 -> 죽을거였으면 죽는모션으로 간다는게 위에 계산돼있다. 다시 계산하는건 낭비다
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && willDead == false)
            {
                /*-----------------------------------------------------
                |NOTI| Ver1... 체간없이 그냥 계산하는 버전 ... 이건 플레이어만 씁니다!!!
                -----------------------------------------------------*/
                if (_characterType == CharacterType.Player)
                {
                    if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0 &&
                        true /*가드 범위 내에서 맞았냐? 가드하는데 뒤에서 맞으면 실패임*/)
                    {
                        CalculateNextState_Guard(ref nextGraphType, ref representType, damage, statScript, GCST<StateContoller>().GetCurrState());
                    }
                    else
                    {
                        float deltaRoughness = damage._damagePower - statScript.GetPassiveStat(PassiveStat.Roughness);

                        if (deltaRoughness > 0)
                        {
                            if (deltaRoughness <= MyUtil.deltaRoughness_lvl0)
                            {
                                representType = RepresentStateType.Hit_Lvl_0;
                            }
                            else if (deltaRoughness <= MyUtil.deltaRoughness_lvl1)
                            {
                                representType = RepresentStateType.Hit_Lvl_1;
                            }
                            else
                            {
                                representType = RepresentStateType.Hit_Lvl_2;
                            }
                        }
                    }
                }

                /*-----------------------------------------------------
                |NOTI| Ver2... 체간을 계산하는 버전 ... 이건 몬스터 용도 입니다!
                -----------------------------------------------------*/
                else
                {
                    if (false/*보스입니다*/)
                    {

                    }
                    else
                    {
                        if (attackerStatScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("TryRiposite")) != null)
                        {
                            isRipositeAttack = true;

                            if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) != null)
                            {
                                //그로기 상태에서 맞았다 = 찍기가 발동된다
                                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_Smash);

                                //공격자의 상태는 Riposite로 바뀌었다.
                                {
                                    AnimationClip attackerRipositeStateAnimationClip = attackerStateController.GetCurrState()._myState._stateAnimationClip;
                                    Dictionary<FrameDataWorkType, List<AEachFrameData>> allFrameDatas = ResourceDataManager.Instance.GetAnimationAllFrameData(attackerRipositeStateAnimationClip);
                                    List<AEachFrameData> ripositeAttackFrameData = allFrameDatas[FrameDataWorkType.RipositeAttack];

                                    float time = ripositeAttackFrameData[0]._frameUp / attackerRipositeStateAnimationClip.frameRate;

                                    StartCoroutine(RipositeCoroutine(isWeakPoint, attacker, victim, closetPoint, hitNormal, time, attackerStateController.GetCurrState()._myState._stateType));
                                }
                            }

                            else if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("Stagger_Parried")) != null ||
                                false /*가드크래시다*/)
                            {
                                //가드 크래시 상태에서 맞았다 = 앞잡기가 발동된다.

                                Vector3 dirAttackerToVictim = (transform.position - attacker.transform.position);
                                dirAttackerToVictim.y = 0.0f;
                                dirAttackerToVictim = dirAttackerToVictim.normalized;
                                float angle = Mathf.Abs(Vector3.Angle(dirAttackerToVictim, transform.forward));

                                if (angle < 150.0f/*피격 위치가 앞잡기 가능 위치다*/)
                                {
                                    return;
                                }
                                    
                                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_FrontStab);

                                //Victim을 회전시킨다.
                                //Victim은 먼저 상태를 변경시켜야 한다.
                                {
                                    GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(Quaternion.AngleAxis(180f, transform.right) * dirAttackerToVictim));
                                    GCST<StateContoller>().TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Hit_Riposte_FrontStab);
                                }


                                //Attaker를 회전시킨다.
                                {
                                    attacker.GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                }


                                //공격자의 상태는 Riposite로 바뀌었다.
                                {
                                    AnimationClip attackerRipositeStateAnimationClip = attackerStateController.GetCurrState()._myState._stateAnimationClip;
                                    Dictionary<FrameDataWorkType, List<AEachFrameData>> allFrameDatas = ResourceDataManager.Instance.GetAnimationAllFrameData(attackerRipositeStateAnimationClip);
                                    List<AEachFrameData> ripositeAttackFrameData = allFrameDatas[FrameDataWorkType.RipositeAttack];

                                    float time = ripositeAttackFrameData[0]._frameUp / attackerRipositeStateAnimationClip.frameRate;

                                    StartCoroutine(RipositeCoroutine(isWeakPoint, attacker, victim, closetPoint, hitNormal, time, attackerStateController.GetCurrState()._myState._stateType));
                                }
                            }


                            else
                            {
                                Vector3 dirAttackerToVictim = (transform.position - attacker.transform.position);
                                dirAttackerToVictim.y = 0.0f;
                                dirAttackerToVictim = dirAttackerToVictim.normalized;
                                float angle = Mathf.Abs(Vector3.Angle(dirAttackerToVictim, transform.forward));

                                if (angle <= 15.0f/*피격 위치가 뒤잡기 가능 위치다*/)
                                {
                                    //뒤잡기 위치다 = 뒤잡기가 발동된다.
                                    StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                    attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_BackStab);

                                    //Victim을 회전시킨다.
                                    //Victim은 먼저 상태를 변경시켜야 한다.
                                    {
                                        GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                        GCST<StateContoller>().TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Hit_Riposte_BackStab);
                                    }


                                    //Attaker를 회전시킨다.
                                    {
                                        attacker.GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                    }


                                    //공격자의 상태는 Riposite로 바뀌었다.
                                    {
                                        AnimationClip attackerRipositeStateAnimationClip = attackerStateController.GetCurrState()._myState._stateAnimationClip;
                                        Dictionary<FrameDataWorkType, List<AEachFrameData>> allFrameDatas = ResourceDataManager.Instance.GetAnimationAllFrameData(attackerRipositeStateAnimationClip);
                                        List<AEachFrameData> ripositeAttackFrameData = allFrameDatas[FrameDataWorkType.RipositeAttack];

                                        float time = ripositeAttackFrameData[0]._frameUp / attackerRipositeStateAnimationClip.frameRate;

                                        StartCoroutine(RipositeCoroutine(isWeakPoint, attacker, victim, closetPoint, hitNormal, time, attackerStateController.GetCurrState()._myState._stateType));
                                    }
                                }
                            }


                        }
                        else
                        {
                            if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0 &&
                                true /*가드 범위 내에서 맞았냐? 가드하는데 뒤에서 맞으면 실패임*/)
                            {
                                //CalculateNextState_Guard(ref nextGraphType, ref representType, damage, statScript, GCST<StateContoller>().GetCurrState());
                            }
                            else if (isPostureMax == true && statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) == null)
                            {
                                //최초로 체간이 Max치를 찍었습니다. 다운됩니다.
                                    //체간 MaxBuff를 걸고...
                                    //주저앉는 (캐릭터마다 다름) 애니메이션으로 갑니다.
                                representType = RepresentStateType.Groggy;
                                statScript.ApplyBuff(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff"), 1);
                                EffectManager.Instance.CreateEffect("CartoonSparkEffect", Vector3.up, _characterHeart.transform.position);
                            }
                            else if (isPostureOverPhase1 == true && statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) == null)
                            {
                                //체간이 Phase1을 넘어섰을때 체간 버프가 없다! = 이때 경직이 들어갑니다
                                representType = RepresentStateType.Hit_Lvl_1;
                            }
                        }
                    }
                }
            }




            
            attackerStatScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.ApplyDamage_AttackerCallback, damage, isWeakPoint, attacker, victim);

            //이펙트 생성을 위한 코드영역
            {
                string effectName = "HitSparkEffect";

                //공격에 의한 이펙트가 결정됩니다.
                switch (damage._hitType)
                {
                    case DamageDesc.HitType.Blunt:
                        {
                            if (isWeakPoint == true && damage._damageReason == DamageDesc.DamageReason.Ray)
                            {
                                effectName = "HitSparkCritical";
                            }
                        }
                        break;
                    case DamageDesc.HitType.Slash:
                        effectName = "SlashedSparkEffect";
                        break;
                    case DamageDesc.HitType.End:
                        {
                            //Debug.Assert(false, "상태 Asset에서 어떻게 때리는지 지정하세요. 기본값 '타격'으로 세팅됩니다");
                        }
                        break;
                }

                if (representType == RepresentStateType.Blocked_Reaction ||
                    representType == RepresentStateType.Blocked_Sliding ||
                    representType == RepresentStateType.Blocked_Crash)
                {
                    string guardEffectName = "ConeSparkEffect";

                    if (representType == RepresentStateType.Blocked_Sliding ||
                        representType == RepresentStateType.Blocked_Crash)
                    {
                        guardEffectName = "CartoonSparkEffect";
                    }

                    GameObject shieldWeapon = (GCST<StateContoller>().GetCurrStateGraphType() == StateGraphType.WeaponState_RightGraph)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    closetPoint = shieldWeapon.transform.position; //방패피격점

                    EffectManager.Instance.CreateEffect(guardEffectName, hitNormal, closetPoint); //가드이펙트
                }

                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);

                if (victim.GCST<StatScript>().GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PerfectGuardBuff")) != null)
                {
                    EffectManager.Instance.CreateEffect("PerfectGuardSparkEffect", hitNormal, closetPoint);
                    attacker.AfterDealMe();

                    /*----------------------------------------------------
                    |TOOD| 계산공식 결정하기
                    ----------------------------------------------------*/
                    int refelctPower = 10;


                    attackerStatScript.ChangeActiveStat(ActiveStat.PosturePercent, refelctPower);
                    int afterAttackerPosture = attackerStatScript.GetActiveStat(ActiveStat.PosturePercent);
                    int attackerPosturePhase1 = attackerStatScript.GetPassiveStat(PassiveStat.PostruePercentPhase1);

                    if (afterAttackerPosture == 100)
                    {
                        StateContoller attackerStateController = attacker.GCST<StateContoller>();
                        attackerStateController.TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Groggy);
                        attackerStatScript.ApplyBuff(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff"), 1);
                        EffectManager.Instance.CreateEffect("CartoonSparkEffect", Vector3.up, attacker._CharacterHeart.transform.position);
                    }

                    else if (afterAttackerPosture >= attackerPosturePhase1)
                    {
                        //attacker의 공격이 튕긴다
                        StateContoller attackerStateController = attacker.GCST<StateContoller>();
                        attackerStateController.TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.AttackRecoil);
                    }
                }
            }


            if (isRipositeAttack == true)
            {
                return;
            }



            //-------------------------------------------------------------------------------------------------------------
            if (willDead == true || //-----------------------------------------------------다음에 죽을 예정이거나,
                statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //-----CC면역이 걸려있지 않거나,
                representType != RepresentStateType.End) //-------------------------------최소 한번은 상태변경이 성공했다면,
            { //-----------------------------------------------------------------------------------------------------------

                //해당 방향을 바라보면서 다음 결정된 상태를 연출하는 코드일 뿐이다

                Vector3 toAttackerDir = (attacker.transform.position - transform.position);
                toAttackerDir.y = 0.0f;
                toAttackerDir = toAttackerDir.normalized;

                GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(toAttackerDir));
                GCST<StateContoller>().TryChangeState(nextGraphType, representType);
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_ApplyDamage, damage, isWeakPoint, attacker, victim);
    }

    public void WhenTriggerEnterWithWeaponCollider(Collider other, bool isWeakCollider, Vector3 closetPoint, Vector3 hitNormal)
    {
        if (_dead == true) {return;}

        //연쇄 충돌이라면 무시합니다. 한번만 계산할겁니다.
        if (AnimationAttackManager.Instance.TriggerEnterCheck(this, other) == false)
        {
            return;
        }

        /*----------------------------------------------------
        |NOTI| isWeakCollider 변수가 함께 들어옵니다.
        피격시 이 충돌체가 WeakPoint였다면 true 입니다. (머리같은거)
        ----------------------------------------------------*/
        TriggerEnterWithWeapon(other, isWeakCollider, ref closetPoint, ref hitNormal);
    }



    public virtual void AfterDealMe()
    {
    }
}
