using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

public class Gunscript2 : WeaponScript
{
    /*------------------------------------------
    Component Section.
    ------------------------------------------*/
    private AimScript2 _aimScript = null;

    [SerializeField] private GameObject _firePosition = null;
    [SerializeField] private GameObject _stockPosition = null;

    private Coroutine _coolTimeCoroutine = null;
    private Coroutine _aimShakeCoroutine = null;
    private Coroutine _reloadingCoroutine = null;

    [SerializeField] private float _coolTimeOriginal = 0.1f;
    [SerializeField] private float _reloadingTimeOriginal = 0.1f;
    [SerializeField] private float _aimShakeDurationOriginal = 0.1f;
    [SerializeField] private Vector2 _absAimShakeForceMin = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 _absAimShakeForceMax = new Vector2(0.3f, 0.3f);

    [SerializeField] private float _dampingSpeed = 0.1f;
    private Vector3 _followPositionRef = Vector3.zero;
    RaycastHit[] _gunRayHit = new RaycastHit[2]; 

    /*------------------------------------------
    런타임중 정보저장용 변수들
    ------------------------------------------*/
    private bool _isAimed = false;
    private bool _isIK = false;


    private float _coolTime = 0.0f;
    private float _reloadingTime = 0.0f;
    private float _aimShakeDuration = 0.0f;
    private Vector2 _aimShakeDampRef = new Vector2();
    private Vector2 _calculatedAimShakeForce = new Vector2();
    protected Transform _shoulderStock_Unity = null;
    protected Transform _elbowPosition_Unity = null;
    private List<int> _firingEffectedLayer = new List<int>();


    protected override void LateUpdate()
    {
        FollowSocketTransform();
    }

    private bool GetAimStateChanged()
    {
        bool ret = false;

        bool targetSied = (_isRightHandWeapon == true)
            ? _owner._isRightWeaponAimed
            : _owner._isLeftWeaponAimed;

        ret = (_isAimed != targetSied);
        _isAimed = targetSied;

        return ret;
    }

    private bool GetIKStateChanged()
    {
        bool ret = false;

        bool isReloading = (_reloadingCoroutine != null);
        bool isAiming = (_isRightHandWeapon == true)
            ? _owner._isRightWeaponAimed
            : _owner._isLeftWeaponAimed;

        bool isIK = (isReloading == false && isAiming == true);

        ret = (_isIK != isIK);
        _isIK = isIK;

        return ret;
    }

    public override void FollowSocketTransform()
    {
        if (GetAimStateChanged() == true)
        {
            //바뀌었다.
            _aimScript.ResetAimRotation();
            if (_isAimed == true)
            {
                _aimScript.OnAimState(AimState.eTPSAim);
            }
            else
            {
                _aimScript.OffAimState();
            }
            _aimScript.TurnOnRigging(_isAimed);
        }


        if (GetIKStateChanged() == true)
        {
            //바뀌었다.
            CalculateAimIK(_isIK); //1. IK를 해야합니다.
        }

        if (_isAimed == true && _reloadingCoroutine == null)
        {
            Vector3 gunPosition = _stockPosition.transform.position;
            Vector3 stockPosition = _shoulderStock_Unity.transform.position;
            Vector3 deltaPosition = stockPosition - gunPosition;


            Vector3 targetPosition = transform.position + deltaPosition;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _followPositionRef, _dampingSpeed * Time.smoothDeltaTime);


            //총구를 Aim지점을 바라보게 할겁니다. 근데 Animation에 따라서 미세한 조정을 할겁니다.
            DelicateRotationControl();
        }
        else
        {
            base.FollowSocketTransform(); //손에 붙음
        }
    }


    public bool ReloadCheck()
    {
        if (_reloadingCoroutine != null) /*이미 재장전중이다*/
        {
            return false;
        }

        return true;
    }


    public ItemStoreDescBase FindMagazine()
    {
        ItemStoreDescBase ret = null;

        List<InventoryBoard> ownerInventoryBoards = _owner.GetMyInventoryBoards();

        //캐릭터에 인벤토리가 하나도 없다 = 갈아끼울 수 있는 탄창이 존재할리 없다
        if (ownerInventoryBoards.Count <= 0) 
        {
            return ret;
        }

        foreach (InventoryBoard inventoryBoard in ownerInventoryBoards)
        {
            Dictionary<int, SortedDictionary<int, ItemStoreDescBase>> ownerItems = inventoryBoard._Items;

            foreach (KeyValuePair<int, SortedDictionary<int, ItemStoreDescBase>> pair in ownerItems)
            {
                ItemAsset itemAsset = ItemInfoManager.Instance.GetItemInfo(pair.Key);

                if (itemAsset._ItemType != ItemAsset.ItemType.Magazine)
                {
                    continue;
                }

                ItemAsset_Bullet.BulletType magazineBulletType = ((ItemAsset_Magazine)itemAsset)._MagazineType;

                if (((ItemAsset_Weapon)_ItemStoreInfo._itemAsset)._UsingBulletType != magazineBulletType)
                {
                    continue;
                }

                SortedDictionary<int, ItemStoreDescBase> sameKeyItems = pair.Value;

                if (sameKeyItems.Count <= 0) 
                {
                    continue;
                }

                ret = sameKeyItems.First().Value;
                break;
            }
        }

        return ret;
    }

    private void DelicateRotationControl()
    {
        float stateChangingWeight = _owner.GetStateChangingPercentage();
        if (stateChangingWeight <= 0.0f || stateChangingWeight >= 1.0f) 
        {
            transform.LookAt(_aimScript.GetAimSatellite().transform.position);
            return;
        }

        transform.rotation = MyUtil.LootAtPercentageRotation(transform, _aimScript.GetAimSatellite().transform.position, stateChangingWeight);
    }


    private void CalculateAimIK(bool isAimed)
    {
        AnimatorLayerTypes oppositeLayer = (_isRightHandWeapon == true)
            ? AnimatorLayerTypes.LeftHand
            : AnimatorLayerTypes.RightHand;

        if (isAimed == false)
        {
            //다 꺼주세요
            _ownerIKSkript.SwitchOnOffIK(this, false);
            return;
        }

        //반대손에 뭔가를 들고있습니까의 bool 변수
        bool isOppositeHandBusy = (_owner.GetCurrentWeapon(oppositeLayer) != null);

        if (isOppositeHandBusy == true)
        {
            AvatarIKGoal type = (_isRightHandWeapon == true)
                ? AvatarIKGoal.RightHand
                : AvatarIKGoal.LeftHand;

            //들고있는 손만 켜주세요
            _ownerIKSkript.SwitchOnOffIK(this, true, false, type);
        }
        else
        {
            //다 켜주세요
            _ownerIKSkript.SwitchOnOffIK(this, true);
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<AvatarIKGoal, IKTargetDesc> iks in _createdIKTargets)
        {
            _ownerIKSkript.DestroyIK(this);
        }
    }


    override public void Equip(CharacterScript itemOwner, Transform followTransform) 
    {
        base.Equip(itemOwner, followTransform);

        //견착위치
        {
            _stockPosition = transform.Find("StockPosition").gameObject;
            Debug.Assert(_stockPosition != null, "자세제어를 위해 견착위치가 필요합니다(권총도 마찬가지)");

            _shoulderStock_Unity = (_isRightHandWeapon == true)
              ? _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.RightUpperArm)
              : _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.LeftUpperArm);

            _elbowPosition_Unity = (_isRightHandWeapon == true)
              ? _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.RightLowerArm)
              : _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.LeftLowerArm);
        }

        //조준 스크립트, 리깅관련
        {
            _aimScript = itemOwner.gameObject.GetComponent<AimScript2>();
            Debug.Assert(_aimScript != null, "Gun은 AimScrip가 반드시 있어야만 한다");
        }


        //IK 세팅
        {
            GameObject ownerModelObject = _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedModelObject();
            _ownerIKSkript = ownerModelObject.gameObject.GetComponent<IKScript>();
            Debug.Assert(_ownerIKSkript != null, "Gun은 IK를 이용해야만 합니다");
            base.InitIK();
        }

        //발사 위치 세팅
        {
            _firePosition = transform.Find("FirePosition").gameObject;
            Debug.Assert(_firePosition != null, "발사할곳이 없는데 이게 총입니까?");
        }
    }

    #region Fire

    public void Fire()
    {
        ItemStoreDesc_Weapon_Gun gunStoredDesc = (ItemStoreDesc_Weapon_Gun)_ItemStoreInfo;
        ItemAsset_Bullet firstBullet = (ItemAsset_Bullet)gunStoredDesc._myMagazine._bullets.First()._itemAsset;
        gunStoredDesc._myMagazine._bullets.RemoveAt(gunStoredDesc._myMagazine._bullets.Count - 1);

        //데미지는 총알에 의해 결정된다
        {

        }

        RayCheck(firstBullet);

        StartAimShake();
        if (_aimShakeCoroutine == null)
        {
            _aimShakeCoroutine = StartCoroutine(AimShakeCoroutine());
        }


        StartCoolTime();
        if (_coolTimeCoroutine == null)
        {
            _coolTimeCoroutine = StartCoroutine(CooltimeCoroutine());
        }


        //-----------------------로직분기점--------------------
        FireAnimation();//                                    |
        //-----------------------로직분기점--------------------
    }

    public bool FireCheck()
    {
        KeyCode fireKeyCode = (_isRightHandWeapon == true)
            ? KeyCode.Mouse0
            : KeyCode.Mouse1;

        bool fireKeyCheck = false;
        
        ItemAsset_Weapon weaponAsset = _itemStoreInfo._itemAsset as ItemAsset_Weapon;

        fireKeyCheck = (weaponAsset._IsAutomaticGun == true)
            ? Input.GetKey(fireKeyCode)
            : Input.GetKeyDown(fireKeyCode);

        if (fireKeyCheck == false)
        {
            return false;
        }

        if (((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine == null)
        {
            //탄창이 없는데요
            return false;
        }

        if (((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine._bullets.Count <= 0)
        {
            //탄창은 있는데 탄이 없는데요
            return false;
        }
        

        if (_coolTime > 0.0f) 
        {
            return false;
        }

        if (_reloadingCoroutine != null)
        {
            //재장전중이다.
            return false;
        }

        return true;
    }

    private void FireAnimation()
    {
        _firingEffectedLayer.Clear();
        CharacterAnimatorScript ownerCharacterAnimatorScript = _owner.GCST<CharacterAnimatorScript>();

        AvatarMask myAvatarMask = null;

        if (_isRightHandWeapon == true)
        {
            if (_owner.GetCurrentWeapon(AnimatorLayerTypes.LeftHand) != null)
            {
                myAvatarMask = ResourceDataManager.Instance.GetAvatarMask("UpperBodyExceptLeft");
            }
            else
            {
                myAvatarMask = ResourceDataManager.Instance.GetAvatarMask("UpperBody");
            }
        }
        else 
        {
            if (_owner.GetCurrentWeapon(AnimatorLayerTypes.RightHand) != null)
            {
                myAvatarMask = ResourceDataManager.Instance.GetAvatarMask("UpperBodyExceptRight");
            }
            else
            {
                myAvatarMask = ResourceDataManager.Instance.GetAvatarMask("UpperBody");
            }
        }

        /*---------------------------------------------
        |NOTI| 반동 제어 = 반동 제어 잘될수록 애니메이션을
        덜 섞을겁니다.
        ---------------------------------------------*/
        float GunRecoilPower = 1.0f;

        ownerCharacterAnimatorScript.RunAdditivaAnimationClip(myAvatarMask, ResourceDataManager.Instance.GetGunAnimation(((ItemAsset_Weapon)_ItemStoreInfo._itemAsset))._FireAnimation, false, GunRecoilPower);
    }


    private void RayCheck(ItemAsset_Bullet firedBullet)
    {
        RaycastHit hit;

        int targetLayer = LayerMask.GetMask("StaticNavMeshLayer") | _owner.CalculateWeaponColliderIncludeLayerMask();

        /*--------------------------------------------------------------
        |TODO| TPS 게임에서 에임 비틀림을 어떻게 해결할까요?
        --------------------------------------------------------------*/
        Vector3 rayStartPosition = Camera.main.transform.position;
        Vector3 rayDir = Camera.main.transform.forward;
        //Vector3 rayStartPosition = _firePosition.transform.position;
        //Vector3 rayDir = _firePosition.transform.forward;

        int retCount = Physics.RaycastNonAlloc(rayStartPosition, rayDir, _gunRayHit, 1000.0f, targetLayer, QueryTriggerInteraction.Collide);

        //아무것도 충돌하지 않았어요
        if (retCount == 0)
        {
            return;
        }

        for (int i = 0; i < retCount; i++)
        {
            HitColliderScript hitColliderScript = _gunRayHit[i].collider.GetComponent<HitColliderScript>();

            if (hitColliderScript == null)
            {
                continue;
            }

            //if (false/*아군이면 종료합니다. 아군이 총알을 막을 수 있습니다*/)
            //{
            //    return
            //}

            DamageDesc tempTestDamage = new DamageDesc();
            tempTestDamage._damageReason = DamageDesc.DamageReason.Ray;
            tempTestDamage._damage = firedBullet._BulletDamage;
            tempTestDamage._damagePower = MyUtil.deltaRoughness_lvl1;

            hitColliderScript.CollisionDirectly(tempTestDamage, this.gameObject);

            return;
        }
    }

    public IEnumerator CooltimeCoroutine()
    {
        while (true)
        {
            _coolTime -= Time.deltaTime;

            if (_coolTime <= float.Epsilon)
            {
                _coolTime = 0.0f;
                _coolTimeCoroutine = null;
                break;
            }

            yield return null;
        }
    }
    #endregion Fire

    #region Reload

    public void StartReloadingProcess(ItemStoreDescBase ownerFirstMagazine)
    {
        if (ownerFirstMagazine == null)
        {
            Debug.Assert(false, "탄창을 찾을 수 없는데 장전프로세스가 시작이 됐습니까?");
            Debug.Break();
        }


        int targetX = -1;
        int targetY = -1;
        bool isRotated = false;
        InventoryBoard targetBoard = null;
        if (((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine != null)
        {
            List<InventoryBoard> ownerInventoryBoards = _owner.GetMyInventoryBoards();
            foreach (InventoryBoard inventoryBoard in ownerInventoryBoards)
            {
                if (inventoryBoard.CheckInventorySpace_MustOpt(((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine._itemAsset, ref targetX, ref targetY, ref isRotated) == false)
                {
                    continue;
                }

                targetBoard = inventoryBoard;
                break;
            }
        }


        /*-----------------------------------------------------------------
        |TODO| 재장전시 일단 여기서 아이템정보를 세팅하고 갑니다.
        후에 디테일을 수정할때는 이곳을 삭제하세요
        -----------------------------------------------------------------*/
        if (((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine != null)
        {//총에 기존 탄창이 장착이 돼 있을때

            if (targetBoard != null)
            {//기존 탄창을 넣을 수 있는 공간이 있다면

                ((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine._isRotated = isRotated;
                targetBoard.AddItemUsingForcedIndex(((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine, targetX, targetY, null);
            }
            else
            {//없으면 버려라
                ItemInfoManager.Instance.DropItemToField(transform, ((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine);
            }
        }

        ((ItemStoreDesc_Weapon_Gun)_ItemStoreInfo)._myMagazine = ownerFirstMagazine as ItemStoreDesc_Magazine;
        List<GameObject> itemUIs = ownerFirstMagazine._owner.GetItemUIs(ownerFirstMagazine);
        foreach (GameObject itemUI in itemUIs)
        {
            StartCoroutine(itemUI.GetComponent<ItemUI>().DestroyCoroutine());
        }
        ownerFirstMagazine._owner.DeleteOnMe(ownerFirstMagazine);


        _reloadingCoroutine = StartCoroutine(ReloadCoroutine(ownerFirstMagazine));
    }

    private IEnumerator ReloadCoroutine(ItemStoreDescBase ownerFirstMagazine)
    {
        CoroutineLock lastCoroutineLock = null;
        CharacterAnimatorScript ownerCharacterAnimatorScript = _owner.GCST<CharacterAnimatorScript>();
        lastCoroutineLock = ownerCharacterAnimatorScript.CalculateBodyWorkType_WeaponReload(true);

        if (lastCoroutineLock != null)
        {
            while (lastCoroutineLock._isEnd == false)
            {
                yield return null;
            }
        }


        ReloadAnimation();

        AnimationClip reloadingAnimation = ResourceDataManager.Instance.GetGunAnimation(((ItemAsset_Weapon)_ItemStoreInfo._itemAsset))._ReloadAnimation;
        _reloadingTimeOriginal = reloadingAnimation.length;
        _reloadingTime = _reloadingTimeOriginal;

        while (true)
        {
            _reloadingTime -= Time.deltaTime;

            if (_reloadingTime <= 0.0f)
            {
                _reloadingTime = 0.0f;
                _reloadingCoroutine = null;
                break;
            }

            yield return null;
        }
    }

    private void ReloadAnimation()
    {
        //레이어 무시하고 재장전은 그냥 다써라
        //한손으로 재장전할수는 없다

        AvatarMask myAvatarMask = ResourceDataManager.Instance.GetAvatarMask("UpperBody");
        CharacterAnimatorScript ownerCharacterAnimatorScript = _owner.GCST<CharacterAnimatorScript>();
        ownerCharacterAnimatorScript.RunAdditivaAnimationClip(myAvatarMask, ResourceDataManager.Instance.GetGunAnimation(((ItemAsset_Weapon)_ItemStoreInfo._itemAsset))._ReloadAnimation, false, 1.0f/*Weight*/);
    }
    #endregion Reload

    #region AimShake

    private void StartCoolTime()
    {
        _coolTime = _coolTimeOriginal;
    }

    private void StartAimShake()
    {
        //에임 흔들림 정도 결정하기
        {
            float xShake = Random.Range(_absAimShakeForceMin.x, _absAimShakeForceMax.x);
            if (Random.Range(0, 2) == 1) { xShake *= -1.0f; }

            _calculatedAimShakeForce = new Vector2
            (
                xShake,
                Random.Range(_absAimShakeForceMin.y, _absAimShakeForceMax.y)
            );
        }

        _aimShakeDuration = _aimShakeDurationOriginal;
    }

    public IEnumerator AimShakeCoroutine()
    {
        while (true)
        {
            _aimShakeDuration -= Time.deltaTime;

            if (_aimShakeDuration < float.Epsilon)
            {
                _aimShakeDuration = 0.0f;
                _aimShakeCoroutine = null;
                break;
            }

            _calculatedAimShakeForce = Vector2.SmoothDamp(_calculatedAimShakeForce, Vector2.zero, ref _aimShakeDampRef, _aimShakeDurationOriginal);
            _aimScript.AimRotation(_calculatedAimShakeForce);

            yield return null;
        }
    }
    #endregion AimShake
}
