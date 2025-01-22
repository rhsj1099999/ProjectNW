using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.PostProcessing.HistogramMonitor;

public class Gunscript2 : WeaponScript
{
    /*------------------------------------------
    Component Section.
    ------------------------------------------*/
    private AimScript2 _aimScript = null;

    [SerializeField] UnityEvent _whenShootEvent = null;
    [SerializeField] private GameObject _firePosition = null;
    [SerializeField] private GameObject _stockPosition = null;

    private Coroutine _coolTimeCoroutine = null;
    private Coroutine _aimShakeCoroutine = null;
    private Coroutine _reloadingCoroutine = null;


    //[SerializeField] private AnimationClip _shootAnimationClip = null;
    //[SerializeField] private AnimationClip _reloadingClip = null;
    // private GameObject _bullet = null;
    //[SerializeField] private Transform _followingTransformStartPoint = null;


    /*------------------------------------------
    Spec Section.
    ------------------------------------------*/
    //[SerializeField] private bool _isAutomatic = false;
    //[SerializeField] private float _dampingTime = 0.01f;

    [SerializeField] private float _coolTimeOriginal = 0.1f;
    [SerializeField] private float _reloadingTimeOriginal = 0.1f;
    [SerializeField] private float _aimShakeDurationOriginal = 0.1f;
    [SerializeField] private Vector2 _absAimShakeForceMin = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 _absAimShakeForceMax = new Vector2(0.3f, 0.3f);

    [SerializeField] private float _dampingSpeed = 0.1f;
    private Vector3 _followPositionRef = Vector3.zero;






    /*------------------------------------------
    런타임중 정보저장용 변수들
    ------------------------------------------*/
    //private bool _isAimShaking = false;
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



    public override bool isUsingMe()
    {
        //격발하면 AimShake가 다 끝날떄까지 안놔줄거임
        return false;
    }



    public override bool isWeaponUseReady()
    {
        if (_coolTime >= float.Epsilon)
        {
            return false;
        }

        return base.isWeaponUseReady();
    }



    public override void UseWeapon()
    {
        //Fire();

        //RayCheck();
    }

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
        /*이미 재장전중이다*/
        if (_reloadingCoroutine != null)
        {
            return false;
        }

        if (false/*탄창이 없어요*/)
        {
            return false;
        }

        return true;
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


        {
            //탄창에 탄알 하나 감소
        }

        //RayCheck();
    }

    public bool FireCheck()
    {
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
        _owner.CalculateAffectingLayer(this, ref _firingEffectedLayer);
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

        ownerCharacterAnimatorScript.RunAdditivaAnimationClip(myAvatarMask, ResourceDataManager.Instance.GetGunAnimation(_ItemInfo)._FireAnimation, false, GunRecoilPower);
    }


    private void RayCheck()
    {
        RaycastHit hit;

        int targetLayer =
            (1 << LayerMask.NameToLayer("StaticNavMeshLayer")) |
            (1 << LayerMask.NameToLayer("HitCollider"));

        if (Physics.Raycast(_firePosition.transform.position, _firePosition.transform.forward, out hit, Mathf.Infinity, targetLayer) == false)
        {
            return;
        }

        IHitable hitable = hit.collider.gameObject.GetComponentInParent<IHitable>();

        if (hitable == null)
        {
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
    public void StartReloadingProcess()
    {
        _reloadingCoroutine = StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
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


        ////-----------------------로직분기점--------------------
        ReloadAnimation();//                                  |
        ////-----------------------로직분기점--------------------


        AnimationClip reloadingAnimation = ResourceDataManager.Instance.GetGunAnimation(_ItemInfo)._ReloadAnimation;
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
        float GunRecoilPower = 1.0f;
        CharacterAnimatorScript ownerCharacterAnimatorScript = _owner.GCST<CharacterAnimatorScript>();
        ownerCharacterAnimatorScript.RunAdditivaAnimationClip(myAvatarMask, ResourceDataManager.Instance.GetGunAnimation(_ItemInfo)._ReloadAnimation, false, GunRecoilPower);
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
