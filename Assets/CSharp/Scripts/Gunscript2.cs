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
    ��Ÿ���� ��������� ������
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
        //�ݹ��ϸ� AimShake�� �� ���������� �ȳ��ٰ���
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
            //�ٲ����.
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
            //�ٲ����.
            CalculateAimIK(_isIK); //1. IK�� �ؾ��մϴ�.
        }

        if (_isAimed == true && _reloadingCoroutine == null)
        {
            Vector3 gunPosition = _stockPosition.transform.position;
            Vector3 stockPosition = _shoulderStock_Unity.transform.position;
            Vector3 deltaPosition = stockPosition - gunPosition;


            Vector3 targetPosition = transform.position + deltaPosition;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _followPositionRef, _dampingSpeed * Time.smoothDeltaTime);


            //�ѱ��� Aim������ �ٶ󺸰� �Ұ̴ϴ�. �ٵ� Animation�� ���� �̼��� ������ �Ұ̴ϴ�.
            DelicateRotationControl();
        }
        else
        {
            base.FollowSocketTransform(); //�տ� ����
        }
    }

    public bool ReloadCheck()
    {
        /*�̹� ���������̴�*/
        if (_reloadingCoroutine != null)
        {
            return false;
        }

        if (false/*źâ�� �����*/)
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
            //�� ���ּ���
            _ownerIKSkript.SwitchOnOffIK(this, false);
            return;
        }

        //�ݴ�տ� ������ ����ֽ��ϱ��� bool ����
        bool isOppositeHandBusy = (_owner.GetCurrentWeapon(oppositeLayer) != null);

        if (isOppositeHandBusy == true)
        {
            AvatarIKGoal type = (_isRightHandWeapon == true)
                ? AvatarIKGoal.RightHand
                : AvatarIKGoal.LeftHand;

            //����ִ� �ո� ���ּ���
            _ownerIKSkript.SwitchOnOffIK(this, true, false, type);
        }
        else
        {
            //�� ���ּ���
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

        //������ġ
        {
            _stockPosition = transform.Find("StockPosition").gameObject;
            Debug.Assert(_stockPosition != null, "�ڼ���� ���� ������ġ�� �ʿ��մϴ�(���ѵ� ��������)");

            _shoulderStock_Unity = (_isRightHandWeapon == true)
              ? _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.RightUpperArm)
              : _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.LeftUpperArm);

            _elbowPosition_Unity = (_isRightHandWeapon == true)
              ? _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.RightLowerArm)
              : _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedAnimator().GetBoneTransform(HumanBodyBones.LeftLowerArm);
        }

        //���� ��ũ��Ʈ, �������
        {
            _aimScript = itemOwner.gameObject.GetComponent<AimScript2>();
            Debug.Assert(_aimScript != null, "Gun�� AimScrip�� �ݵ�� �־�߸� �Ѵ�");
        }


        //IK ����
        {
            GameObject ownerModelObject = _owner.GetComponentInChildren<CharacterAnimatorScript>().GetCurrActivatedModelObject();
            _ownerIKSkript = ownerModelObject.gameObject.GetComponent<IKScript>();
            Debug.Assert(_ownerIKSkript != null, "Gun�� IK�� �̿��ؾ߸� �մϴ�");
            base.InitIK();
        }

        //�߻� ��ġ ����
        {
            _firePosition = transform.Find("FirePosition").gameObject;
            Debug.Assert(_firePosition != null, "�߻��Ұ��� ���µ� �̰� ���Դϱ�?");
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


        //-----------------------�����б���--------------------
        FireAnimation();//                                    |
        //-----------------------�����б���--------------------


        {
            //źâ�� ź�� �ϳ� ����
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
            //���������̴�.
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
        |NOTI| �ݵ� ���� = �ݵ� ���� �ߵɼ��� �ִϸ��̼���
        �� �����̴ϴ�.
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


        ////-----------------------�����б���--------------------
        ReloadAnimation();//                                  |
        ////-----------------------�����б���--------------------


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
        //���̾� �����ϰ� �������� �׳� �ٽ��
        //�Ѽ����� �������Ҽ��� ����

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
        //���� ��鸲 ���� �����ϱ�
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
