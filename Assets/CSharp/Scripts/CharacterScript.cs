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


    //��������Ʈ Ÿ�Ե�
    public delegate void Action_Int(int param0);
    public delegate void Action_LayerType(AnimatorLayerTypes layerType);

    //ĳ���� �߽�
    protected GameObject _characterHeart = null;
    public GameObject _CharacterHeart => _characterHeart;

    //��ǥ ������Ʈ��
    public List<GameCharacterSubScript> _components = new List<GameCharacterSubScript>();
    private Dictionary<Type, Component> _mySubScripts = new Dictionary<Type, Component>();



    //�κ��丮
    [SerializeField] protected GameObject _inventoryUIPrefab = null;
    protected List<InventoryBoard> _inventoryBoardCached = new List<InventoryBoard>();

    //���� ����
    //���� �տ� ����ִ� = ��üȭ�� �ƴ� = GameObject �� ����ִ°� ����---------------------------
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

        //if (target == null && isNullable == true)
        //{
        //    Debug.Assert(false, "���� ������Ʈ�� ã���� �ϰ��ִ�" + typeof(T));
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
        for (int i = 0; i < _tempMaxUseableSlot; i++)
        {
            _tempUseableItemDescs.Add(null);
        }
    }




    //HP�� 0�̵Ǿ� �״� ������ �����մϴ�
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
        // State Controller�� ��Ȱ��ȭ �Ѵ�.
        GetCharacterSubcomponent<StateContoller>().enabled = false;
        gameObject.layer = LayerMask.NameToLayer("DeadObject");

        GCST<CharacterContollerable>().CharacterDie();
    }


    public virtual void CharacterRevive(int hp = 0)
    {
        _dead = false;
        StateContoller stateController = GCST<StateContoller>();
        stateController.enabled = true;

        //�ڷ� �������� �ڿ��� ������ �Ͼ�� �ڼ��� ����, ������ �������� ������ ������ �Ͼ�� �ڼ��� ����
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

        //������ ���� ����
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


        //�浹ü ������Ʈ
        {
            //������ ��, �ݶ��̴��� ������Ʈ �Ѵ�.

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
            Transform correctSocket = null; //������ ������
            {
                Debug.Assert(GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject() != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");

                WeaponSocketScript[] weaponSockets = GCST<CharacterAnimatorScript>().GetCurrActivatedModelObject().GetComponentsInChildren<WeaponSocketScript>();

                Debug.Assert(weaponSockets.Length > 0, "���� �ٿ��ٷ��µ� ������ ����");

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
                    Debug.Assert(false, "���⸦ ���� �� �ִ� ������ �����ϴ�");
                    Debug.Break();
                    return;
                }

            }

            //������ ������ ����, ����
            GameObject itemModel = Instantiate(itemStoreDesc._itemAsset._ItemModel, correctSocket);

            _currHandObject.Add(layerType, itemModel);
        }

        if (itemStoreDesc._itemAsset._EquipType == ItemAsset.EquipType.Weapon)
        {
            WeaponSocketScript.SideType targetSide = (layerType == AnimatorLayerTypes.RightHand)
                ? WeaponSocketScript.SideType.Right
                : WeaponSocketScript.SideType.Left;

            ItemStoreDesc_Weapon nextItemStoreDesc = (ItemStoreDesc_Weapon)itemStoreDesc;
            
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

                //�����ɱ�
                {
                    ItemAsset_Weapon weaponAsset = (ItemAsset_Weapon)itemStoreDesc._itemAsset;
                    foreach (BuffAssetBase buff in weaponAsset._Buffs)
                    {
                        GCST<StatScript>().ApplyBuff(buff, 1);
                    }
                }

                //HUD ����
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
            Debug.Assert(false, "attackerCharacterScript �� ���Դϴ�?");
            Debug.Break();
        }

        if (other == null)
        {
            Debug.Assert(false, "other �� ���Դϴ�?");
            Debug.Break();
        }
        #endregion NullCheck

        DamageDesc currentDamage = attackerCharacterScript.CalculateAttackerDamage(other, isWeakPointCollider);


        /*----------------------------------------------------------
        |NOTI| isWeakPointCollider �� True ��� ������ ����ġ��Ÿ�Դϴ�
        ----------------------------------------------------------*/
        DealMe_Final(currentDamage, isWeakPointCollider, attackerCharacterScript, this, ref closetPoint, ref hitNormal);
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
        //���� ���� ������Ʈ
        if (GCST<StatScript>().enabled == true)
        {
            GCST<StatScript>().StatScriptUpdate();
        }

        //���� ���� ������Ʈ
        if (GCST<StateContoller>().enabled == true)
        {
            GCST<StateContoller>().DoWork();
        }

        //�⺻������ �߷��� ��� ������Ʈ �Ѵ�
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
        |NOTI| AnimationSpeed�� ������������ ������� ������Ʈ�մϴ�
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
                        break;
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
                        break;
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
                        break; //������ �õ��� �̷������ �ʾҴ�. �ƹ��ϵ� �Ͼ�� �ʴ´�
                    }

                    GameObject targetWeapon = (isRightHandWeapon == true)
                        ? _tempCurrRightWeapon
                        : _tempCurrLeftWeapon;

                    if (targetWeapon == null)
                    {
                        break; //�����⸦ �õ������� ���Ⱑ ����.
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

                    //��� ���� üũ
                    //int willBusyLayer = 0;
                    {
                        ////���� �����۸����� �ʿ��� ���̾� üũ
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

                        ////���� ���� �������� ���� �ʿ��� ���̾� üũ
                        //if (_tempCurrRightWeapon != null)
                        //{
                        //    willBusyLayer = willBusyLayer | (1 << (int)AnimatorLayerTypes.RightHand);
                        //}
                    }

                    int willBusyLayer = (1 << (int)AnimatorLayerTypes.RightHand | 1 << (int)AnimatorLayerTypes.Head);

                    if ((currentAnimatorBusyLayerBitShift & willBusyLayer) != 0)
                    {
                        return; //�ش� �������� ���� ������ �ִ�
                    }

                    //Work�� ��´� ������ Lock ����� ������.
                    GCST<CharacterAnimatorScript>().CalculateBodyWorkType_UseItem_Drink(_tempGrabFocusType, useableItemStoreDesc, willBusyLayer);
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

    public virtual DamageDesc CalculateAttackerDamage(Collider other, bool isWeakPoint)
    {
        DamageDesc damageDesc = new DamageDesc();

        StatScript myStat = GCST<StatScript>();

        /*-------------------------------------------------------
        �⺻ ���ݿ� ���� ������, ���׹̳� ���
        -------------------------------------------------------*/
        {
            damageDesc._damage = myStat.CalculateStatDamage();
            damageDesc._damagingStamina = myStat.CalculateStatDamagingStamina();
            damageDesc._damagePower = myStat.CalculatePower();
        }

        myStat.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_AttackerBuffCheck, damageDesc, isWeakPoint, this, null);
        /*-------------------------------------------------------
        �����ڿ��� ������ �ɷ��־���? (�̱����Դϴ�.)
        -------------------------------------------------------*/
        {

        }

        /*-------------------------------------------------------
        ���⿡ ���� ������ (�̱����Դϴ�.)
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
        �ִϸ��̼� ���
        -------------------------------------------------------*/
        {
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
                damageDesc._hitType = attackMultiplyDesc._hitType;
            }
        }

        /*-------------------------------------------------------
        ��� ���(������� ��Ƽ� �ֵθ��� �� ���ð��̴�)
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
        Debug.Log("������� ����� �����մϴ�");

        //int currRoughness = statScript.GetPassiveStat(PassiveStat.Roughness);
        int currRoughness = 7;
        


        //���׹̳��� ����ϰ� ���ε��� ����մϴ�
        if (statScript.GetActiveStat(ActiveStat.Stamina) >= damage._damagingStamina &&
            currRoughness >= damage._damagePower)
        {
            nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
            representType = RepresentStateType.Blocked_Reaction;
        }

        //���׹̳��� ����ѵ� ���ε��� �����մϴ�.
        else if (statScript.GetActiveStat(ActiveStat.Stamina) >= damage._damagingStamina &&
            currRoughness < damage._damagePower)
        {
            nextGraphType = GCST<StateContoller>().GetCurrStateGraphType();
            representType = RepresentStateType.Blocked_Sliding;
        }

        //���ε��� ����ѵ� ���׹̳��� �����մϴ�.
        else if (statScript.GetActiveStat(ActiveStat.Stamina) < damage._damagingStamina &&
            currRoughness >= damage._damagePower)
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
        if ((statScript.GetActiveStat(ActiveStat.Stamina) < damage._damagingStamina && statScript.GetPassiveStat(PassiveStat.Roughness) < damage._damagePower) ||
            nextStateAsseet == null)
        {
            Debug.Log("���� ������ �ɷ����� �´»��·� �����ϴ�");

            //�´� ���·� ���� �Ұǵ�
            nextGraphType = StateGraphType.HitStateGraph;

            float deltaRoughness = damage._damagePower - statScript.GetPassiveStat(PassiveStat.Roughness);

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


    protected void DealMe_Test(DamageDesc damage)
    {
        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;
        StatScript statScript = GCST<StatScript>();

        //Step3. ������ ����
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
                    Debug.Assert(false, "�����ǵ� �״� �׷����� ���׿�?");
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




            //������ �����ϴµ�, ���´� ��� ����ɱ��?
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //CC�鿪�� �ɷ����� �ʰ�,
                willDead == false) //������ ���� ���� �����̶�� -> �����ſ����� �״¸������ ���ٴ°� ���� �����ִ�
            {
                //���� ���¸� ����մϴ�.

                if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0)
                {
                    //�������̿��� = ������� ����� ����Ѵ�
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


            if (willDead == true || //������ ���� �����̰ų�
                (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && representType != RepresentStateType.End)) //CC�鿪�� �ɷ����� �ʴٸ�
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
    |TOOD| DealMe_Final�� �����մϴ�. �� �Լ��� �������� �κ��� ���� �������?
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
        // Step0 ����üũ
        {
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible) > 0)
            {
                Debug.Log("�����̶� �þ���");
                damage._damage = 0;
                damage._damagingStamina = 0;

                statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);
                return;
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);




        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;




        // Step1 �ǰ��� ����üũ
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_BuffCheck, damage, isWeakPoint, attacker, victim);
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_BuffCheck, damage, isWeakPoint, attacker, victim);




        // Step2 ����� ����
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_ApplyDamage, damage, isWeakPoint, attacker, victim);
        {
            bool willDead = false;

            //�������� ����ȴ�
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
                        Debug.Assert(false, "�����ǵ� �״� �׷����� ���׿�?");
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
                                Debug.Assert(false, "Riposite�� �ƴѵ� �� �Լ��� ȣ��ƴٰ�?");
                                Debug.Break();
                            }
                            break;
                    }
                }
            }

            bool isPostureOverPhase1 = false; //ü���� Phase1�� �Ѱ���ϴ�.
            bool isPostureMax = false; //ü���� �� á���ϴ�.

            //ü�� ����
            /*-------------------------------------------------------
            |TODO| Ư���� ������ ������ �ϼ���. ������ �ٷ� �����ŵ�ϴ�
            -------------------------------------------------------*/
            if (willDead == false)
            {
                statScript.ChangeActiveStat(ActiveStat.PosturePercent, (int)damage._damagePower);
                isPostureOverPhase1 = (statScript.GetPassiveStat(PassiveStat.PostruePercentPhase1) <= statScript.GetActiveStat(ActiveStat.PosturePercent));
                isPostureMax = (statScript.GetActiveStat(ActiveStat.PosturePercent) == 100);
            }


            //������ �����ϴµ�, ���´� ��� ����ɱ��?
            //CC�鿪�� �ɷ����� �ʰ�,
            //������ ���� ���� �����̶�� -> �����ſ����� �״¸������ ���ٴ°� ���� �����ִ�. �ٽ� ����ϴ°� �����
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && willDead == false)
            {
                /*-----------------------------------------------------
                |NOTI| Ver1... ü������ �׳� ����ϴ� ���� ... �̰� �÷��̾ ���ϴ�!!!
                -----------------------------------------------------*/
                if (_characterType == CharacterType.Player)
                {
                    if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0 &&
                        true /*���� ���� ������ �¾ҳ�? �����ϴµ� �ڿ��� ������ ������*/)
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
                |NOTI| Ver2... ü���� ����ϴ� ���� ... �̰� ���� �뵵 �Դϴ�!
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
                                Debug.Assert(false, "Riposite�� �ƴѵ� �� �Լ��� ȣ��ƴٰ�?");
                                Debug.Break();
                            }
                            break;
                    }
                }
            }

            attackerStatScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.ApplyDamage_AttackerCallback, damage, isWeakPoint, attacker, victim);

            //����Ʈ ������ ���� �ڵ念��
            {
                string effectName = "HitSparkEffect";

                //���ݿ� ���� ����Ʈ�� �����˴ϴ�.
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
                            //Debug.Assert(false, "���� Asset���� ��� �������� �����ϼ���. �⺻�� 'Ÿ��'���� ���õ˴ϴ�");
                        }
                        break;
                }

                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);
            }


            //-------------------------------------------------------------------------------------------------------------
            if (willDead == true || //-----------------------------------------------------������ ���� �����̰ų�,
                statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //-----CC�鿪�� �ɷ����� �ʰų�,
                (representType != RepresentStateType.End && representType != RepresentStateType.Hit_Riposte_BackStab && representType != RepresentStateType.Hit_Riposte_FrontStab)) 
                //(representType != RepresentStateType.End))
            { //-----------------------------------------------------------------------------------------------------------

                //�ش� ������ �ٶ󺸸鼭 ���� ������ ���¸� �����ϴ� �ڵ��� ���̴�

                

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
        // Step0 ����üũ
        {
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible) > 0)
            {
                Debug.Log("�����̶� �þ���");
                damage._damage = 0;
                damage._damagingStamina = 0;

                statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);
                return;
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_InvincibleCheck, damage, isWeakPoint, attacker, victim);


        //������ �ɷ��־������, �ִ��� �и��� ������ �� ����.
        //�׸��� �и� ���� ���� ���� ������ �ǰ��ڿ��� ����
        if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("ParryBuff")) != null &&
            true /*�и� ���� �����̰�  -- �ϴ� ���� �и� �����մϴ�. �� ���ε��� ��� ������ Power�� ����? */ && 
            true /*Ÿ������ ���� ���� �� ���*/)
        {
            //����Ʈ ����
            {
                string effectName = "HitSparkEffect";

                //���ݿ� ���� ����Ʈ�� �����˴ϴ�.
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
                            //Debug.Assert(false, "���� Asset���� ��� �������� �����ϼ���. �⺻�� 'Ÿ��'���� ���õ˴ϴ�");
                        }
                        break;
                }

                GameObject shieldWeapon = (GCST<StateContoller>().GetCurrStateGraphType() == StateGraphType.WeaponState_RightGraph)
                    ? _tempCurrRightWeapon
                    : _tempCurrLeftWeapon;

                closetPoint = shieldWeapon.transform.position; //�����ǰ���
                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);
                EffectManager.Instance.CreateEffect("ConeSparkEffect", hitNormal, closetPoint);
                EffectManager.Instance.CreateEffect("CartoonSparkEffect", hitNormal, attacker._CharacterHeart.transform.position);
            }

            //������ Stagger_Parried�� �ٲٱ�
            {
                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                attackerStateController.TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Stagger_Parried);
                attacker.AfterDealMe();
            }

            return;
        }



        Debug.Log("���� ������" + damage._damage);
        Debug.Log("���� ���׹̳�������" + damage._damagingStamina);
        Debug.Log("���� �Ŀ�" + damage._damagePower);


        StateGraphType nextGraphType = StateGraphType.HitStateGraph;
        RepresentStateType representType = RepresentStateType.End;

        // Step1 �ǰ��� ����üũ
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_BuffCheck, damage, isWeakPoint, attacker, victim);
        {
            if (isWeakPoint == true && damage._damageReason == DamageDesc.DamageReason.Ray)
            {
                damage._damage *= 10.0f;
                Debug.Log("---�ݹ� ġ��Ÿ--- || ġ��Ÿ������ : " + damage._damage);
            }
        }
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.After_BuffCheck, damage, isWeakPoint, attacker, victim);

        // Step2 ����� ����
        statScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.Before_ApplyDamage, damage, isWeakPoint, attacker, victim);
        {
            if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0)
            {
                /*--------------------------------------------
                �ӽ÷� ������ ��ȿȭ��
                => ��������� �������� �÷��ִ� ������� �����Ұ�
                --------------------------------------------*/
                damage._damage = 0;
            }

            bool willDead = false;

            //�������� ����ȴ�
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
                        Debug.Assert(false, "�����ǵ� �״� �׷����� ���׿�?");
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

            bool isPostureOverPhase1 = false; //ü���� Phase1�� �Ѱ���ϴ�.
            bool isPostureMax = false; //ü���� �� á���ϴ�.

            //ü�� ����
            /*-------------------------------------------------------
            |TODO| Ư���� ������ ������ �ϼ���. ������ �ٷ� �����ŵ�ϴ�
            -------------------------------------------------------*/
            if (willDead == false)
            {
                statScript.ChangeActiveStat(ActiveStat.PosturePercent, (int)damage._damagePower);
                isPostureOverPhase1 = (statScript.GetPassiveStat(PassiveStat.PostruePercentPhase1) <= statScript.GetActiveStat(ActiveStat.PosturePercent));
                isPostureMax = (statScript.GetActiveStat(ActiveStat.PosturePercent) == 100);
            }

            bool isRipositeAttack = false;
            //������ �����ϴµ�, ���´� ��� ����ɱ��?
            //CC�鿪�� �ɷ����� �ʰ�,
            //������ ���� ���� �����̶�� -> �����ſ����� �״¸������ ���ٴ°� ���� �����ִ�. �ٽ� ����ϴ°� �����
            if (statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && willDead == false)
            {
                /*-----------------------------------------------------
                |NOTI| Ver1... ü������ �׳� ����ϴ� ���� ... �̰� �÷��̾ ���ϴ�!!!
                -----------------------------------------------------*/
                if (_characterType == CharacterType.Player)
                {
                    if (statScript.GetPassiveStat(PassiveStat.IsGuard) > 0 &&
                        true /*���� ���� ������ �¾ҳ�? �����ϴµ� �ڿ��� ������ ������*/)
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
                |NOTI| Ver2... ü���� ����ϴ� ���� ... �̰� ���� �뵵 �Դϴ�!
                -----------------------------------------------------*/
                else
                {
                    if (false/*�����Դϴ�*/)
                    {

                    }
                    else
                    {
                        if (attackerStatScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("TryRiposite")) != null)
                        {
                            isRipositeAttack = true;

                            if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) != null)
                            {
                                //�׷α� ���¿��� �¾Ҵ� = ��Ⱑ �ߵ��ȴ�
                                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_Smash);

                                //�������� ���´� Riposite�� �ٲ����.
                                {
                                    AnimationClip attackerRipositeStateAnimationClip = attackerStateController.GetCurrState()._myState._stateAnimationClip;
                                    Dictionary<FrameDataWorkType, List<AEachFrameData>> allFrameDatas = ResourceDataManager.Instance.GetAnimationAllFrameData(attackerRipositeStateAnimationClip);
                                    List<AEachFrameData> ripositeAttackFrameData = allFrameDatas[FrameDataWorkType.RipositeAttack];

                                    float time = ripositeAttackFrameData[0]._frameUp / attackerRipositeStateAnimationClip.frameRate;

                                    StartCoroutine(RipositeCoroutine(isWeakPoint, attacker, victim, closetPoint, hitNormal, time, attackerStateController.GetCurrState()._myState._stateType));
                                }
                            }

                            else if (statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("Stagger_Parried")) != null ||
                                false /*����ũ���ô�*/)
                            {
                                //���� ũ���� ���¿��� �¾Ҵ� = ����Ⱑ �ߵ��ȴ�.

                                Vector3 dirAttackerToVictim = (transform.position - attacker.transform.position);
                                dirAttackerToVictim.y = 0.0f;
                                dirAttackerToVictim = dirAttackerToVictim.normalized;
                                float angle = Mathf.Abs(Vector3.Angle(dirAttackerToVictim, transform.forward));

                                if (angle < 150.0f/*�ǰ� ��ġ�� ����� ���� ��ġ��*/)
                                {
                                    return;
                                }
                                    
                                StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_FrontStab);

                                //Victim�� ȸ����Ų��.
                                //Victim�� ���� ���¸� ������Ѿ� �Ѵ�.
                                {
                                    GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(Quaternion.AngleAxis(180f, transform.right) * dirAttackerToVictim));
                                    GCST<StateContoller>().TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Hit_Riposte_FrontStab);
                                }


                                //Attaker�� ȸ����Ų��.
                                {
                                    attacker.GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                }


                                //�������� ���´� Riposite�� �ٲ����.
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

                                if (angle <= 15.0f/*�ǰ� ��ġ�� ����� ���� ��ġ��*/)
                                {
                                    //����� ��ġ�� = ����Ⱑ �ߵ��ȴ�.
                                    StateContoller attackerStateController = attacker.GCST<StateContoller>();
                                    attacker.GCST<StateContoller>().TryChangeState(attackerStateController.GetCurrStateGraphType(), RepresentStateType.Riposte_BackStab);

                                    //Victim�� ȸ����Ų��.
                                    //Victim�� ���� ���¸� ������Ѿ� �Ѵ�.
                                    {
                                        GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                        GCST<StateContoller>().TryChangeState(StateGraphType.HitStateGraph, RepresentStateType.Hit_Riposte_BackStab);
                                    }


                                    //Attaker�� ȸ����Ų��.
                                    {
                                        attacker.GCST<CharacterContollerable>().CharacterRotate(Quaternion.LookRotation(dirAttackerToVictim));
                                    }


                                    //�������� ���´� Riposite�� �ٲ����.
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
                                true /*���� ���� ������ �¾ҳ�? �����ϴµ� �ڿ��� ������ ������*/)
                            {
                                //CalculateNextState_Guard(ref nextGraphType, ref representType, damage, statScript, GCST<StateContoller>().GetCurrState());
                            }
                            else if (isPostureMax == true && statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) == null)
                            {
                                //���ʷ� ü���� Maxġ�� ������ϴ�. �ٿ�˴ϴ�.
                                    //ü�� MaxBuff�� �ɰ�...
                                    //�����ɴ� (ĳ���͸��� �ٸ�) �ִϸ��̼����� ���ϴ�.
                                representType = RepresentStateType.Groggy;
                                statScript.ApplyBuff(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff"), 1);
                                EffectManager.Instance.CreateEffect("CartoonSparkEffect", Vector3.up, _characterHeart.transform.position);
                            }
                            else if (isPostureOverPhase1 == true && statScript.GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PostureMaxBuff")) == null)
                            {
                                //ü���� Phase1�� �Ѿ���� ü�� ������ ����! = �̶� ������ ���ϴ�
                                representType = RepresentStateType.Hit_Lvl_1;
                            }
                        }
                    }
                }
            }




            
            attackerStatScript.InvokeDamagingProcessDelegate(DamagingProcessDelegateType.ApplyDamage_AttackerCallback, damage, isWeakPoint, attacker, victim);

            //����Ʈ ������ ���� �ڵ念��
            {
                string effectName = "HitSparkEffect";

                //���ݿ� ���� ����Ʈ�� �����˴ϴ�.
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
                            //Debug.Assert(false, "���� Asset���� ��� �������� �����ϼ���. �⺻�� 'Ÿ��'���� ���õ˴ϴ�");
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

                    closetPoint = shieldWeapon.transform.position; //�����ǰ���

                    EffectManager.Instance.CreateEffect(guardEffectName, hitNormal, closetPoint); //��������Ʈ
                }

                EffectManager.Instance.CreateEffect(effectName, hitNormal, closetPoint);

                if (victim.GCST<StatScript>().GetRuntimeBuffAsset(LevelStatInfoManager.Instance.GetBuff("PerfectGuardBuff")) != null)
                {
                    EffectManager.Instance.CreateEffect("PerfectGuardSparkEffect", hitNormal, closetPoint);
                    attacker.AfterDealMe();

                    /*----------------------------------------------------
                    |TOOD| ������ �����ϱ�
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
                        //attacker�� ������ ƨ���
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
            if (willDead == true || //-----------------------------------------------------������ ���� �����̰ų�,
                statScript.GetPassiveStat(PassiveStat.IsInvincible_Stance) <= 0 && //-----CC�鿪�� �ɷ����� �ʰų�,
                representType != RepresentStateType.End) //-------------------------------�ּ� �ѹ��� ���º����� �����ߴٸ�,
            { //-----------------------------------------------------------------------------------------------------------

                //�ش� ������ �ٶ󺸸鼭 ���� ������ ���¸� �����ϴ� �ڵ��� ���̴�

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

        //���� �浹�̶�� �����մϴ�. �ѹ��� ����Ұ̴ϴ�.
        if (AnimationAttackManager.Instance.TriggerEnterCheck(this, other) == false)
        {
            return;
        }

        /*----------------------------------------------------
        |NOTI| isWeakCollider ������ �Բ� ���ɴϴ�.
        �ǰݽ� �� �浹ü�� WeakPoint���ٸ� true �Դϴ�. (�Ӹ�������)
        ----------------------------------------------------*/
        TriggerEnterWithWeapon(other, isWeakCollider, ref closetPoint, ref hitNormal);
    }



    public virtual void AfterDealMe()
    {
    }
}
