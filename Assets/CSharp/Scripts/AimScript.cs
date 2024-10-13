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
    [SerializeField] private Vector2 smoothTime = new Vector2(0.05f, 0.05f); // �ε巴�� ȸ���� �ð�

    public CinemachineVirtualCameraBase _sightCamera = null;
    public CinemachineVirtualCameraBase _freeRunCamera = null;
    public CinemachineVirtualCameraBase _tpsCamera = null;

    private float _calaculatedValX = 0.0f;
    private float _calaculatedValY = 0.0f;

    private Vector2 currentAimRotation;
    private Vector2 currentVelocity; // SmoothDamp�� �ӵ� ������ ����

    private bool _isAim = false;


    private void Awake()
    {
        Debug.Assert(_inputController != null, "��ǲ��Ʈ�ѷ��� ����");
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
                Debug.Assert(false, "�̷��� �ȉ´�");
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



        if (Input.GetKeyDown(KeyCode.K) == true)
        {
            _aimState += 1;
            _aimState = (AimState)((int)_aimState % (int)AimState.ENEND); //Aim���� = AimScript�� �����ϵ���
        }
    }

    public void LateUpdate()
    {
        if (_isAim == true)
        {
            Vector2 mouseMove = _inputController._pr_mouseMove;
            Vector2 desiredAimRotation = new Vector2(mouseMove.x * _aimSpeed.x, mouseMove.y * _aimSpeed.y);

            AimRotation(desiredAimRotation);
        }
    }

    public void AimRotation(Vector2 rotatedValue) //���ڰ� = ���콺�� ���������� �������� �� ��
    {
        {
            //ĳ���� y�� ȸ�� (����ȸ��)
            //Ÿ�ٰ��� ���ŵƴ�.
            _calaculatedValY += rotatedValue.x; 
            //���� ������ ���� �����Ѵ�
            currentAimRotation.y = Mathf.SmoothDamp(currentAimRotation.y, _calaculatedValY, ref currentVelocity.y, smoothTime.x);
            _aimmingCharacter.transform.rotation = Quaternion.Euler(_aimmingCharacter.transform.rotation.x, currentAimRotation.y, 0f);
        }

        {
            //ĳ���� x�� ȸ�� (����ȸ��)
            //Ÿ�ٰ��� ���ŵƴ�.
            _calaculatedValX += rotatedValue.y;
            //���� ������ ���� �����Ѵ�
            currentAimRotation.x = Mathf.SmoothDamp(currentAimRotation.x, _calaculatedValX, ref currentVelocity.x, smoothTime.y);
            transform.localRotation = Quaternion.Euler(-currentAimRotation.x, transform.rotation.y, 0f);
        }

    }
}
