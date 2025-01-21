using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] UnityEvent _whenShootEvent = null;
    [SerializeField] private GameObject _firePosition = null;
    [SerializeField] private GameObject _stockPosition = null;

    private Coroutine _coolTimeCoroutine = null;
    private Coroutine _aimShakeCoroutine = null;


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
    private float _coolTime = 0.0f;
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

    public override void FollowSocketTransform()
    {
        bool targetSied = (_isRightHandWeapon == true)
            ? _owner._isRightWeaponAimed
            : _owner._isLeftWeaponAimed;

        bool changed = false;

        if (_isAimed != targetSied)
        {
            changed = true;
            _isAimed = targetSied;
        }
        

        if (changed == true) 
        {
            //aim�� �ٲ� ������ �����̸� �������� �ؾ��մϱ�?
            _aimScript.TurnOnRigging(targetSied);
            _aimScript.ResetAimRotation();
            CalculateAimIK(_isAimed); //1. IK�� �ؾ��մϴ�.
            
            if (_isAimed == true)
            {
                _aimScript.OnAimState(AimState.eTPSAim);
            }
            else 
            {
                _aimScript.OffAimState();
            }

        }

        if (targetSied == false)
        {
            base.FollowSocketTransform(); //�տ� ����
            return; //�Ϲ� = �տ� ���δ�.
        }
        else
        {
            Vector3 gunPosition = _stockPosition.transform.position;
            Vector3 stockPosition = _shoulderStock_Unity.transform.position;
            Vector3 deltaPosition = stockPosition - gunPosition;

            
            Vector3 targetPosition = transform.position + deltaPosition;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _followPositionRef, _dampingSpeed * Time.smoothDeltaTime);


            //�ѱ��� Aim������ �ٶ󺸰� �Ұ̴ϴ�. �ٵ� Animation�� ���� �̼��� ������ �Ұ̴ϴ�.
            DelicateRotationControl();
        }
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
    public void Reload()
    {
        {
            //�ִϸ��̼� ��ü�۾�
            //IK�� ��Ȱ��ȭ�ϰ� ���⸦ �տ� �ٿ����Ѵ�.
            //
        }

        //-----------------------�����б���--------------------
        ReloadAnimation();//                                  |
        //-----------------------�����б���--------------------

        {
            //�κ��丮�� źâ ��ü(Ȥ�� �׿� ���õ� �۾���);
        }
    }



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

        return true;
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
        StartCoolTime();

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
        StartAimShake();

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
