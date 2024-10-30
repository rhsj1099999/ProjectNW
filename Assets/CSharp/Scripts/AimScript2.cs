using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimScript2 : MonoBehaviour
{
    private InputController _inputController = null;
    private GameObject _aimOribit = null; //x�� ȸ�� ���� ������Ʈ��
    public GameObject GetAimOrbit() { return _aimOribit; }
    private GameObject _aimSatellite = null; //x�� ȸ�� ���� ������Ʈ�� ���� ���ȸ���ϴ� ������Ʈ. ī�޶�� �̰��� �Ĵٺ���
    public GameObject GetAimSatellite() { return _aimSatellite; }

    private float _aimSatelliteZOffset = 1000.0f;
    

    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;

    private AimState _aimState = AimState.eTPSAim;

    private Vector2 currentAimRotation = Vector2.zero;
    private Vector2 _calaculatedVal = Vector2.zero;
    private Vector2 currentVelocity; // SmoothDamp�� �ӵ� ������ ����

    private bool _isAim = false;

    [SerializeField] private Vector2 _aimSpeed = new Vector2(1.0f, 1.0f);
    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private Vector2 smoothTime = new Vector2(0.05f, 0.05f); // �ε巴�� ȸ���� �ð�
    [SerializeField] private GameObject _aimSatelliteDebuggingPrefab = null;


    private void OnDisable()
    {
        RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
        Rig riggingComponent = GetComponentInChildren<Rig>();

        //Awake���� ��������ٰ� �����մϴ�.

        riggingComponent.enabled = false;
        rigBuilderComponent.enabled = false;
    }


    private void OnEnable()
    {
        RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
        Rig riggingComponent = GetComponentInChildren<Rig>();

        //Awake���� ��������ٰ� �����մϴ�.

        riggingComponent.enabled = true;
        rigBuilderComponent.enabled = true;

        rigBuilderComponent.Build();
    }


    private void Awake()
    {
        //�������� ��ǲ ��Ʈ�ѷ� ����
        {
            _inputController = GetComponentInParent<InputController>();
            Debug.Assert(_inputController != null, "��ǲ��Ʈ�ѷ��� ����");
        }

        //�Ĵٺ� ���� ������Ʈ �����
        {
            _aimOribit = new GameObject("AimOrbit");
            _aimOribit.transform.SetParent(transform);
            _aimOribit.transform.rotation = transform.rotation;
            _aimOribit.transform.position = transform.position;

            CharacterController ownerCharacterController = gameObject.GetComponent<CharacterController>();
            if (ownerCharacterController != null)
            {
                Vector3 orbitPosition = _aimOribit.transform.position;
                orbitPosition.y = ownerCharacterController.center.y;
                _aimOribit.transform.position = orbitPosition;
            }

            _aimSatellite = new GameObject("AimSatellite");
            _aimSatellite.transform.SetParent(_aimOribit.transform);
            Vector3 aimSatellitePosition = _aimOribit.transform.position;
            aimSatellitePosition += _aimOribit.transform.forward * _aimSatelliteZOffset;
            _aimSatellite.transform.position = aimSatellitePosition;

            //����� �޽� �����
            {
                GameObject prefab = Resources.Load<GameObject>("RuntimePrefab/ForDebug/mesh_0.1");
                GameObject newObject = Instantiate(prefab);
                newObject.transform.position = _aimSatellite.transform.position;
                newObject.transform.SetParent(_aimSatellite.transform);
            }
        }

        //ī�޶� ����
        {
            _tpsCamera = transform.Find("TPSCamera").GetComponent<CinemachineVirtualCameraBase>();
            Debug.Assert(_tpsCamera != null, "AimSystem�� ����Ϸ��� tpsCamera�� �־���մϴ�.");
            _tpsCamera.LookAt = _aimSatellite.transform;

            _freeRunCamera = transform.Find("FreeRunCamera").GetComponent<CinemachineVirtualCameraBase>();
            Debug.Assert(_freeRunCamera != null, "AimSystem�� ����Ϸ��� tpsCamera�� �־���մϴ�.");
        }

        //��Ÿ�� ���뼼��
        {
            RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
            Rig riggingComponent = GetComponentInChildren<Rig>();

            if (riggingComponent == null || rigBuilderComponent == null)
            {
                Debug.Assert(false, "���� ������ �������� ���� ó������ �ʾҽ��ϴ�");
            }

            MultiAimConstraint[] constrainComponents = riggingComponent.gameObject.GetComponentsInChildren<MultiAimConstraint>();

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

            rigBuilderComponent.Build();
        }
    }


    void Update()
    {
        bool isAimed = Input.GetButton(_aimKey);

        if (isAimed != _isAim)
        {
            if (isAimed == true)
            {
                OnAimState();
            }
            else
            {
                OffAimState();
            }
        }

        _isAim = isAimed;

        if (_isAim == true)
        {
            Vector2 mouseMove = _inputController._pr_mouseMove;
            Vector2 desiredAimRotation = new Vector2(mouseMove.x * _aimSpeed.x, mouseMove.y * _aimSpeed.y);

            AimRotation(desiredAimRotation);
        }
    }


    public void OffAimState()
    {
        //_sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;
       
        _freeRunCamera.enabled = true;


        /*---------------------------------------------------------------------
         |TODO| ī�޶� ���ƿ��� Ÿ�̹��̶� ���缭 �����ϼ���
        ---------------------------------------------------------------------*/
        {
            _aimOribit.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0f);
            currentAimRotation = Vector2.zero;
            _calaculatedVal = Vector2.zero;
        }

    }


    public void OnAimState()
    {
        //_sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;

        switch (_aimState)
        {
            case AimState.eSightAim:
                _sightCamera.enabled = true;
                break;
            case AimState.eTPSAim:
                _tpsCamera.enabled = true;
                break;
            default:
                Debug.Assert(false, "�����Ͱ� �߰��ƽ��ϱ�?");
                break;
        }

        //Animator -> �ٸ� �������� �ٲ����Ѵ�.
        {

        }
    }

    public void AimRotation(Vector2 rotatedValue) //���ڰ� = ���콺�� ���������� �������� �� ��
    {
        //ĳ���� y�� ȸ�� (����ȸ��)
        {
            //Ÿ�ٰ��� ���ŵƴ�.
            _calaculatedVal.y += rotatedValue.x;
            //���� ������ ���� �����Ѵ�
            currentAimRotation.y = Mathf.SmoothDamp(currentAimRotation.y, _calaculatedVal.y, ref currentVelocity.y, smoothTime.x);
            transform.rotation = Quaternion.Euler(transform.rotation.x, currentAimRotation.y, 0f);
        }

        //ĳ���� x�� ȸ�� (����ȸ��)
        {
            //Ÿ�ٰ��� ���ŵƴ�.
            _calaculatedVal.x += rotatedValue.y;
            //���� ������ ���� �����Ѵ�
            currentAimRotation.x = Mathf.SmoothDamp(currentAimRotation.x, _calaculatedVal.x, ref currentVelocity.x, smoothTime.y);
            _aimOribit.transform.localRotation = Quaternion.Euler(-currentAimRotation.x, _aimOribit.transform.rotation.y, 0f);
        }

    }
}
