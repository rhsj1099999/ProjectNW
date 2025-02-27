using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScript : MonoBehaviour
{
    private InputController _inputController = null;
    private GameObject _aimmingCharacter = null;

    [SerializeField] private Vector2 _aimSpeed = new Vector2(1.0f, 1.0f);
    //[SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private Vector2 smoothTime = new Vector2(0.05f, 0.05f); // 부드럽게 회전할 시간

    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;
    private AimState _aimState = AimState.eTPSAim;

    private float _calaculatedValX = 0.0f;
    private float _calaculatedValY = 0.0f;

    private Vector2 currentAimRotation;
    private Vector2 currentVelocity; // SmoothDamp의 속도 추적용 변수

    private bool _isAim = false;

    private void Awake()
    {
        _inputController = GetComponentInParent<InputController>();
        Debug.Assert(_inputController != null, "인풋컨트롤러가 없다");
    }

    public void OffAimState()
    {
        bool currAim = false;
        if (_isAim == currAim)
        {
            return;
        }
        
        _isAim = currAim;

        _sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;
        _freeRunCamera.enabled = true;

        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0f);
    }

    public void OnAimState()
    {
        bool currAim = true;
        if (_isAim == currAim)
        {
            return;
        }

        _isAim = currAim;

        _sightCamera.enabled = false;
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
    }

    void Update()
    {
        if (_isAim == true)
        {
            Vector2 mouseMove = _inputController._pr_mouseMove;
            Vector2 desiredAimRotation = new Vector2(mouseMove.x * _aimSpeed.x, mouseMove.y * _aimSpeed.y);

            AimRotation(desiredAimRotation);
        }

        //bool isAimed = Input.GetButton(_aimKey);

        //if (isAimed != _isAim) 
        //{
        //    if (isAimed == true)
        //    {
        //        OnAimState();
        //    }
        //    else
        //    {
        //        OffAimState();
        //    }
        //}
        //_isAim = isAimed;

        //if (Input.GetKeyDown(KeyCode.K) == true)
        //{
        //    _aimState += 1;
        //    _aimState = (AimState)((int)_aimState % (int)AimState.ENEND); //Aim상태 = AimScript가 관리하도록
        //}
    }

    public void LateUpdate()
    {

    }

    public void AimRotation(Vector2 rotatedValue) //인자값 = 마우스가 움직였으니 움직여야 할 값
    {
        //캐릭터 y축 회전 (수평회전)
        {
            _calaculatedValY += rotatedValue.x; 
            currentAimRotation.y = Mathf.SmoothDamp(currentAimRotation.y, _calaculatedValY, ref currentVelocity.y, smoothTime.x);
            _aimmingCharacter.transform.rotation = Quaternion.Euler(_aimmingCharacter.transform.rotation.x, currentAimRotation.y, 0f);
        }

        //캐릭터 x축 회전 (수평회전)
        {
            _calaculatedValX += rotatedValue.y;
            currentAimRotation.x = Mathf.SmoothDamp(currentAimRotation.x, _calaculatedValX, ref currentVelocity.x, smoothTime.y);
            transform.localRotation = Quaternion.Euler(-currentAimRotation.x, transform.rotation.y, 0f);
        }

    }
}
