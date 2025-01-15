using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;


public enum AimState
{
    eTPSAim,
    eSightAim,
    eLockOnAim,
    ENEND,
};

public class AimScript2 : GameCharacterSubScript
{
    /*-----------------------------------------------
    ���� ������
    -----------------------------------------------*/
    private const float _aimSatelliteZOffset = 5.0f;
    private const float _lockOnRadius = 10.0f;
    private const float _lockOnMaxDegreeX = 45.0f;
    private const float _lockOnMaxDegreeY = 45.0f;



    [SerializeField] private float _lockOnHardLimitZoneDegree = 150.0f;
    [SerializeField] private GameObject _debuggingMeshObject = null;




    /*-----------------------------------------------
    ������Ʈ��
    -----------------------------------------------*/
    private GameObject _ownerCharacterHeart = null;
    private GameObject _ownerGameObject = null;

    private bool _isRiggingOn = false;




    /*-----------------------------------------------
    �ǽð����� �Ĵٺ� ��ü��
    -----------------------------------------------*/
    private GameObject _aimOribit = null; //x�� ȸ�� ���� ������Ʈ��
    private GameObject _aimSatellite = null; //x�� ȸ�� ���� ������Ʈ�� ���� ���ȸ���ϴ� ������Ʈ. ī�޶�� �̰��� �Ĵٺ���
    public GameObject GetAimOrbit() { return _aimOribit; }
    public GameObject GetAimSatellite() { return _aimSatellite; }
    private GameObject _lockedOnObject = null;
    public GameObject GetLockOnObject() { return _lockedOnObject; }

    


    /*-----------------------------------------------
    ī�޶� ������
    -----------------------------------------------*/
    public CinemachineFreeLook _freeRunCamera = null;
    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;
    public CinemachineFreeLook _lockOnCamera = null;




    /*-----------------------------------------------
    ��Ÿ�� ���� ������
    -----------------------------------------------*/
    private AimState _aimState = AimState.ENEND;
    public bool GetIsAim() 
    {
        if (_aimState == AimState.ENEND)
        {
            return false;
        }
        return true;
    }
    



    /*-----------------------------------------------
    ��Ÿ�� ���� ������_��ġ����
    -----------------------------------------------*/
    [SerializeField] private Vector2 _aimSpeed = new Vector2(3.0f, 1.0f);
    [SerializeField] private Vector2 smoothTime = new Vector2(0.05f, 0.05f); // �ε巴�� ȸ���� �ð�
    private Vector2 currentAimRotation = Vector2.zero;
    private Vector2 _calaculatedVal = Vector2.zero;
    private Vector2 currentVelocity; // SmoothDamp�� �ӵ� ������ ����


    public override void Init(CharacterScript owner)
    {
        _myType = typeof(AimScript2);
        _owner = owner;

        //�Ĵٺ� ���� ������Ʈ �����
        {
            _aimOribit = new GameObject("AimOrbit");
            _aimOribit.transform.SetParent(transform);
            _aimOribit.transform.rotation = transform.rotation;
            _aimOribit.transform.position = transform.position;

            Vector3 orbitPosition = _aimOribit.transform.position;
            Animator ownerAnimator = _owner.GCST<CharacterAnimatorScript>().GetCurrActivatedAnimator();
            orbitPosition.y = ownerAnimator.GetBoneTransform(HumanBodyBones.Chest).position.y;
            _aimOribit.transform.position = orbitPosition;

            _aimSatellite = new GameObject("AimSatellite");
            _aimSatellite.transform.SetParent(_aimOribit.transform);
            Vector3 aimSatellitePosition = _aimOribit.transform.position;
            aimSatellitePosition += _aimOribit.transform.forward * _aimSatelliteZOffset;
            _aimSatellite.transform.position = aimSatellitePosition;

            if (_debuggingMeshObject != null)
            {
                Instantiate(_debuggingMeshObject, _aimSatellite.transform);
            }
        }

        //ī�޶� ����
        {
            _tpsCamera = transform.Find("TPSCamera").GetComponent<CinemachineVirtualCameraBase>();
            Debug.Assert(_tpsCamera != null, "AimSystem�� ����Ϸ��� tpsCamera�� �־���մϴ�.");
            _tpsCamera.LookAt = _aimSatellite.transform;

            _freeRunCamera = transform.Find("FreeRunCamera").GetComponent<CinemachineFreeLook>();
            Debug.Assert(_freeRunCamera != null, "AimSystem�� ����Ϸ��� _freeRunCamera �־���մϴ�.");
            _ownerGameObject = _freeRunCamera.LookAt.gameObject;

            _lockOnCamera = transform.Find("LockOnCamera").GetComponent<CinemachineFreeLook>();
            Debug.Assert(_lockOnCamera != null, "AimSystem�� ����Ϸ��� LockOnCamera �־���մϴ�.");
        }
    }


    public override void SubScriptStart()
    {
        //��Ÿ�� ���뼼��
        {
            RigBuilder characterRigBuilder = _owner.GCST<CharacterAnimatorScript>().GetCharacterRigBuilder();
            Rig characterRig = _owner.GCST<CharacterAnimatorScript>().GetCharacterRig();
            SetRigging(characterRigBuilder, characterRig);
        }

        OffAimState();
    }



    public void SetRigging(RigBuilder characterRigBuilder, Rig characterRig)
    {
        if (characterRig == null || characterRigBuilder == null)
        {
            Debug.Assert(false, "���� ������ �������� ���� ó������ �ʾҽ��ϴ�");
        }

        MultiAimConstraint[] constrainComponents = characterRig.gameObject.GetComponentsInChildren<MultiAimConstraint>();

        foreach (var component in constrainComponents)
        {
            var sources = component.data.sourceObjects;

            WeightedTransform newSource = new WeightedTransform
            {
                transform = _aimSatellite.transform,
                weight = 1.0f
            };

            sources.Clear();
            sources.Add(newSource);

            component.data.sourceObjects = sources;
        }

        characterRigBuilder.Build();
        _isRiggingOn = false;
        characterRig.enabled = false;
        characterRigBuilder.enabled = false;
    }


    void Update()
    {
        if (_isRiggingOn == true)
        {
            if (_aimState == AimState.eLockOnAim)
            {
                _aimOribit.transform.LookAt(_lockedOnObject.transform.position);
            }
            else
            {
                Vector2 mouseMove = _owner.GCST<InputController>()._pr_mouseMove;
                Vector2 desiredAimRotation = new Vector2(mouseMove.x * _aimSpeed.x, mouseMove.y * _aimSpeed.y);
                AimRotation(desiredAimRotation);
            }
        }

        if (_aimState == AimState.eLockOnAim)
        {
            LockOnUpdate();
        }
    }


    private void LockOnUpdate()
    {
        Vector3 targetPosition = _lockedOnObject.transform.position;
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 ownerPosition = _ownerGameObject.transform.position;

        Vector3 ownerToTarget = (targetPosition - ownerPosition);
        Vector3 ownerToCamera = (cameraPosition - ownerPosition);

        Vector3 ownerToTargetDir = ownerToTarget.normalized;
        Vector3 ownerToCameraDir = ownerToCamera.normalized;

        Vector3 ownerToTargetPlaneDir = ownerToTargetDir;
        ownerToTargetPlaneDir.y = 0.0f;
        ownerToTargetPlaneDir = ownerToTargetPlaneDir.normalized;

        Vector3 ownerToCameraPlaneDir = ownerToCameraDir;
        ownerToCameraPlaneDir.y = 0.0f;
        ownerToCameraPlaneDir = ownerToCameraPlaneDir.normalized;


        float angle = Mathf.Abs(Vector3.Angle(ownerToCameraPlaneDir, ownerToTargetPlaneDir));

        if (angle > _lockOnHardLimitZoneDegree)
        {
            return;
        }

        float deltaAngle = _lockOnHardLimitZoneDegree - angle;

        bool isLeft = (Vector3.Cross(ownerToTargetPlaneDir, ownerToCameraPlaneDir).y < 0.0f);
        if (isLeft == true) 
        {
            deltaAngle *= -1.0f;
        }

        _lockOnCamera.m_XAxis.Value += deltaAngle;
    }


    public void OnAimState(AimState targetAimState)
    {
        if (_ownerCharacterHeart == null)
        {
            _ownerCharacterHeart =_owner.gameObject.transform.Find("CharacterHeart").gameObject;
            if (_ownerCharacterHeart == null)
            {
                Debug.Assert(false, "CharacterHeart�� �����ϴ�");
                Debug.Break();
                return;
            }
        }

        bool isSuccrss = false;

        switch (targetAimState)
        {
            case AimState.eSightAim:
                isSuccrss = Check_TurnOnAim_SightAim();
                break;

            case AimState.eTPSAim:
                isSuccrss = Check_TurnOnAim_TPSAim();
                break;

            case AimState.eLockOnAim:
                isSuccrss = Check_TurnOnAim_LockOnAim();
                break;

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        if (isSuccrss == false) 
        {
            return;
        }

        _aimState = targetAimState;

        TurnOffAllCamera();

        switch (targetAimState)
        {
            case AimState.eSightAim:
                TurnOnAim_SightAim();
                break;

            case AimState.eTPSAim:
                TurnOnAim_TPSAim();
                break;

            case AimState.eLockOnAim:
                TurnOnAim_LockOnAim();
                break;

            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }
    }

    public AimState GetAimState()
    {
        return _aimState;
    }

    private bool Check_TurnOnAim_TPSAim()
    {
        return true;
    }

    private bool Check_TurnOnAim_SightAim()
    {
        return true;
    }

    private bool Check_TurnOnAim_LockOnAim()
    {
        _lockedOnObject = null;

        Vector3 cameraPosition = Camera.main.transform.position;
        

        //�÷��̾ ���� �� �ִ� �浹���̾ ���� �� �ִ� ���(gameObject)�� ���� �����ͺ���
        HashSet<GameObject> result = new HashSet<GameObject>();
        {
            int monsterLayerMask = LayerMask.GetMask("CharacterHeart");

            Collider[] colliders = Physics.OverlapSphere(cameraPosition, _lockOnRadius, monsterLayerMask); // �� ������ �浹 ���� ����

            foreach (Collider collider in colliders)
            {
                if (collider.GetComponentInParent<CharacterScript>().GetDead() == true)
                {
                    continue;
                }

                GameObject obj = collider.gameObject;

                if (obj == _ownerCharacterHeart)
                {
                    continue;
                }

                if (result.Contains(obj) == true)
                {
                    continue;
                }

                result.Add(obj);
            }

            if (result.Count <= 0)
            {
                return false;
            }
        }

        //�� ������Ʈ���� ������ ���� ���� ���� ���� �����ϴ��� �˻��ϸ� �Ÿ��� ���ÿ� üũ
        {
            float minDistance = -1.0f;
            foreach (GameObject target in result)
            {
                Vector3 enemyPosition = target.transform.position;

                Vector3 cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);

                Vector3 cameraLookDir = Camera.main.transform.forward.normalized;

                Vector3 cameraToEnemyDir = (enemyPosition - cameraPosition).normalized;

                Vector3 cameraLookPlaneDir = cameraLookDir;
                cameraLookPlaneDir.y = 0.0f;
                cameraLookPlaneDir = cameraLookPlaneDir.normalized;

                Vector3 cameraToEnemyPlaneDir = cameraToEnemyDir;
                cameraToEnemyPlaneDir.y = 0.0f;
                cameraToEnemyPlaneDir = cameraToEnemyPlaneDir.normalized;

                float xDegree = Vector3.Angle(cameraLookPlaneDir, cameraToEnemyPlaneDir);

                if (Mathf.Abs(xDegree) > _lockOnMaxDegreeX)
                {
                    continue;
                }

                //ī�޶��� Look, Right�� ������ ����� ��������
                Vector3 cameraLookVerticalPlaneVector = Vector3.Cross(cameraUpVector, cameraLookDir).normalized;
                float similarities = Vector3.Dot(cameraLookVerticalPlaneVector, cameraToEnemyDir);
                Vector3 toVerticalPlaneVector = cameraLookVerticalPlaneVector * similarities;
                Vector3 projectedVector = cameraToEnemyDir + toVerticalPlaneVector;

                float yDegree = Vector3.Angle(cameraLookDir, projectedVector);

                if (Mathf.Abs(yDegree) > _lockOnMaxDegreeY)
                {
                    continue;
                }

                float distance = (enemyPosition - cameraPosition).magnitude;
                if (distance < minDistance || minDistance <= 0.0f)
                {
                    minDistance = distance;
                    _lockedOnObject = target;
                }
            }
        }

        return (_lockedOnObject != null);
    }

    private void TurnOnAim_TPSAim()
    {
        _tpsCamera.enabled = true;
        //TurnOnRigging(true);
    }

    private void TurnOnAim_SightAim()
    {
        _sightCamera.enabled = true;
        //TurnOnRigging(true);
    }

    private void TurnOnAim_LockOnAim()
    {
        _lockOnCamera.enabled = true;
        _lockOnCamera.LookAt = _lockedOnObject.transform;

        _lockOnCamera.m_XAxis.Value = _freeRunCamera.m_XAxis.Value;
        _lockOnCamera.m_YAxis.Value = 1.0f;
    }





    public void TurnOnRigging(bool isOn)
    {
        RigBuilder characterRigBuilder = _owner.GCST<CharacterAnimatorScript>().GetCharacterRigBuilder();
        Rig characterRig = _owner.GCST<CharacterAnimatorScript>().GetCharacterRig();

        if (_isRiggingOn == isOn)
        {
            return;
        }

        _isRiggingOn = isOn;

        if (isOn == true)
        {
            float weight = (isOn == true)
                ? 1.0f
                : 0.0f;

            characterRig.weight = weight;

            characterRigBuilder.Build();
        }

        characterRig.enabled = isOn;
        characterRigBuilder.enabled = isOn;

    }

    private void TurnOffAllCamera()
    {
        //_sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;
        _lockOnCamera.enabled = false;
    }




    public void OffAimState()
    {
        TurnOffAllCamera();

        switch (_aimState)
        {
            case AimState.eTPSAim:
                {
                    _freeRunCamera.m_XAxis.Value = _lockOnCamera.m_XAxis.Value;
                    _freeRunCamera.m_YAxis.Value = _lockOnCamera.m_YAxis.Value;
                    _aimOribit.transform.localRotation = Quaternion.identity;
                }
                break;

            case AimState.eSightAim:
                break;

            case AimState.eLockOnAim:
                {
                    _freeRunCamera.m_XAxis.Value = _lockOnCamera.m_XAxis.Value;
                    _freeRunCamera.m_YAxis.Value = _lockOnCamera.m_YAxis.Value;
                }
                break;

            case AimState.ENEND:
                break;

            default:
                {
                    Debug.Assert(false, "���� ī�޶� -> FreeRunCamera ������ �ۼ��ϼ���");
                    Debug.Break();
                }
                break;
        }

        _aimState = AimState.ENEND;

        _freeRunCamera.enabled = true;
        _freeRunCamera.LookAt = _ownerGameObject.transform;
        _lockedOnObject = null;
    }


    public void ResetAimRotation()
    {
        _calaculatedVal.x = 0.0f;
        currentAimRotation.x = 0.0f;
    }

    public void AimRotation(Vector2 rotatedValue) //���ڰ� = ���콺�� ���������� �������� �� ��
    {
        if (_aimState == AimState.eLockOnAim)
        {
            return;
        }

        //ĳ���� y�� ȸ�� (����ȸ�� = Ʈ������ ȸ��)
        {
            _calaculatedVal.y += rotatedValue.x;
            //Ÿ�ٰ��� ���ŵƴ�. ���� ������ ���� �����Ѵ�
            currentAimRotation.y = Mathf.SmoothDamp(currentAimRotation.y, _calaculatedVal.y, ref currentVelocity.y, smoothTime.x);

            _owner.GCST<CharacterContollerable>().CharacterRotate(Quaternion.Euler(transform.rotation.x, currentAimRotation.y, 0f));
        }

        //ĳ���� x�� ȸ�� (����ȸ�� = ����ȸ��)
        {
            _calaculatedVal.x += rotatedValue.y;
            //Ÿ�ٰ��� ���ŵƴ�. ���� ������ ���� �����Ѵ�
            currentAimRotation.x = Mathf.SmoothDamp(currentAimRotation.x, _calaculatedVal.x, ref currentVelocity.x, smoothTime.y);
            _aimOribit.transform.localRotation = Quaternion.Euler(-currentAimRotation.x, _aimOribit.transform.rotation.y, 0f);
        }
    }
}
