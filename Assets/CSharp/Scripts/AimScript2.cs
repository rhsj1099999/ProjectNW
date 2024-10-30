using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimScript2 : MonoBehaviour
{
    private InputController _inputController = null;
    private GameObject _aimOribit = null; //x축 회전 조종 오브젝트임
    public GameObject GetAimOrbit() { return _aimOribit; }
    private GameObject _aimSatellite = null; //x축 회전 조종 오브젝트에 의해 상대회전하는 오브젝트. 카메라는 이것을 쳐다본다
    public GameObject GetAimSatellite() { return _aimSatellite; }

    private float _aimSatelliteZOffset = 1000.0f;
    

    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;

    private AimState _aimState = AimState.eTPSAim;

    private Vector2 currentAimRotation = Vector2.zero;
    private Vector2 _calaculatedVal = Vector2.zero;
    private Vector2 currentVelocity; // SmoothDamp의 속도 추적용 변수

    private bool _isAim = false;

    [SerializeField] private Vector2 _aimSpeed = new Vector2(1.0f, 1.0f);
    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private Vector2 smoothTime = new Vector2(0.05f, 0.05f); // 부드럽게 회전할 시간
    [SerializeField] private GameObject _aimSatelliteDebuggingPrefab = null;


    private void OnDisable()
    {
        RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
        Rig riggingComponent = GetComponentInChildren<Rig>();

        //Awake에서 만들어졌다고 가정합니다.

        riggingComponent.enabled = false;
        rigBuilderComponent.enabled = false;
    }


    private void OnEnable()
    {
        RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
        Rig riggingComponent = GetComponentInChildren<Rig>();

        //Awake에서 만들어졌다고 가정합니다.

        riggingComponent.enabled = true;
        rigBuilderComponent.enabled = true;

        rigBuilderComponent.Build();
    }


    private void Awake()
    {
        //조종자의 인풋 컨트롤러 세팅
        {
            _inputController = GetComponentInParent<InputController>();
            Debug.Assert(_inputController != null, "인풋컨트롤러가 없다");
        }

        //쳐다볼 게임 오브젝트 만들기
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

            //디버깅 메쉬 만들기
            {
                GameObject prefab = Resources.Load<GameObject>("RuntimePrefab/ForDebug/mesh_0.1");
                GameObject newObject = Instantiate(prefab);
                newObject.transform.position = _aimSatellite.transform.position;
                newObject.transform.SetParent(_aimSatellite.transform);
            }
        }

        //카메라 세팅
        {
            _tpsCamera = transform.Find("TPSCamera").GetComponent<CinemachineVirtualCameraBase>();
            Debug.Assert(_tpsCamera != null, "AimSystem을 사용하려면 tpsCamera가 있어야합니다.");
            _tpsCamera.LookAt = _aimSatellite.transform;

            _freeRunCamera = transform.Find("FreeRunCamera").GetComponent<CinemachineVirtualCameraBase>();
            Debug.Assert(_freeRunCamera != null, "AimSystem을 사용하려면 tpsCamera가 있어야합니다.");
        }

        //런타임 리깅세팅
        {
            RigBuilder rigBuilderComponent = GetComponentInChildren<RigBuilder>();
            Rig riggingComponent = GetComponentInChildren<Rig>();

            if (riggingComponent == null || rigBuilderComponent == null)
            {
                Debug.Assert(false, "아직 리깅이 없을때에 대해 처리하지 않았습니다");
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
         |TODO| 카메라 돌아오는 타이밍이랑 맞춰서 리셋하세요
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
                Debug.Assert(false, "데이터가 추가됐습니까?");
                break;
        }

        //Animator -> 다리 움직임이 바뀌어야한다.
        {

        }
    }

    public void AimRotation(Vector2 rotatedValue) //인자값 = 마우스가 움직였으니 움직여야 할 값
    {
        //캐릭터 y축 회전 (수평회전)
        {
            //타겟값이 갱신됐다.
            _calaculatedVal.y += rotatedValue.x;
            //따라서 적용할 값을 댐핑한다
            currentAimRotation.y = Mathf.SmoothDamp(currentAimRotation.y, _calaculatedVal.y, ref currentVelocity.y, smoothTime.x);
            transform.rotation = Quaternion.Euler(transform.rotation.x, currentAimRotation.y, 0f);
        }

        //캐릭터 x축 회전 (수평회전)
        {
            //타겟값이 갱신됐다.
            _calaculatedVal.x += rotatedValue.y;
            //따라서 적용할 값을 댐핑한다
            currentAimRotation.x = Mathf.SmoothDamp(currentAimRotation.x, _calaculatedVal.x, ref currentVelocity.x, smoothTime.y);
            _aimOribit.transform.localRotation = Quaternion.Euler(-currentAimRotation.x, _aimOribit.transform.rotation.y, 0f);
        }

    }
}
