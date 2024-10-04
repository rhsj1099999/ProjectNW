using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScript : MonoBehaviour
{
    [SerializeField] private AimState _aimState = AimState.eTPSAim;
    [SerializeField] private Vector2 _aimSpeed = new Vector2(1.0f, 1.0f);
    [SerializeField] private InputController _inputController = null;
    [SerializeField] private string _aimKey = "Fire2";
    [SerializeField] private GameObject _aimmingCharacter = null;

    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;

    private bool _isAim = false;


    private void Awake()
    {
        Debug.Assert(_inputController != null, "인풋컨트롤러가 없다");
    }

    public void AimRotation()
    {

    }

    public void OffAimState()
    {
        _sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;
        _freeRunCamera.enabled = true;

        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0f);
    }

    public void OnAimState()
    {
        _sightCamera.enabled = false;
        _freeRunCamera.enabled = false;
        _tpsCamera.enabled = false;

        switch (_aimState)
        {
            default:
                break;
            case AimState.eSightAim:
                _sightCamera.enabled = true;
                break;
            case AimState.eTPSAim:
                _tpsCamera.enabled = true;
                break;
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

            //정조준 X축회전
            _aimmingCharacter.transform.rotation *= Quaternion.Euler(0.0f, mouseMove.x * _aimSpeed.x, 0f);
            //정조준 Y축회전
            transform.localRotation *= Quaternion.Euler(-mouseMove.y * _aimSpeed.y, 0.0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.K) == true)
        {
            _aimState += 1;
            _aimState = (AimState)((int)_aimState % (int)AimState.ENEND); //Aim상태 = AimScript가 관리하도록
        }
    }
}
