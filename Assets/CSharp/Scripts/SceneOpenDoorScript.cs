using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraDraggingDesc
{
    public Vector3 _targetPosition = Vector3.zero;
    public float _accel = 0.0f;
    public float _currVelocity = 0.0f;
    public float _targetTime = 0.0f;
    public float _timeACC = 0.0f;
    public float _maxVelocityPlus = -1.0f;
    public float _maxVelocityMinus = 1.0f;
}

public class SceneOpenDoorScript : MonoBehaviour
{
    public enum DoorState
    {
        Hide,
        Closed,
        End,
    }


    [SerializeField] private GameObject _effectPrefabObject = null;
    [SerializeField] private GameObject _effectPrefabAttachScaler = null;

    [SerializeField] private GameObject _cameraAttachPosition_DoorDir = null;
    [SerializeField] private GameObject _cameraAttachPosition_OppositeDir = null;

    [SerializeField] private RenderTexture _openDoorRenderTexture = null;
    [SerializeField] private string _targetStage = "None";

    private Animator _ownerAnimator = null;
    private DoorState _state = DoorState.Hide;

    private bool _isBusy = false;


    private void Awake()
    {
        if (_targetStage == "None")
        {
            Debug.Assert(false, "타겟 스테이지가 설정돼있지 않습니다");
            Debug.Break();
        }

        if (_openDoorRenderTexture == null)
        {
            Debug.Assert(false, "렌더텍스쳐가 없다");
            Debug.Break();
        }

        _ownerAnimator = GetComponent<Animator>();
    }


    public void DoorCall()
    {
        if (_isBusy == true)
        {
            return;
        }

        switch (_state)
        {
            case DoorState.Hide:
                {
                    AnimatorStateInfo stateInfo = _ownerAnimator.GetCurrentAnimatorStateInfo(0);
                    StartCoroutine(CreateDoorCoroutine(stateInfo.length));
                }
                break;

            case DoorState.Closed:
                {
                    StartCoroutine(OpenDoor());
                }
                break;
        }
    }


    public void SetTargetStage(string stageName) { _targetStage = stageName; }

    public IEnumerator OpenDoor()
    {
        _isBusy = true;

        //렌더 텍스쳐 드로잉 카메라 작업
        {
            Camera mainCamera = Camera.main;

            Transform mainCameraChild = mainCamera.transform.Find("SubCamera_OpenDoorLayer");

            if (mainCameraChild == null)
            {
                GameObject mainCameraObject = mainCamera.gameObject;
                GameObject subCameraObject = new GameObject("SubCamera_OpenDoorLayer");
                subCameraObject.transform.SetParent(mainCameraObject.transform);
                subCameraObject.transform.localPosition = Vector3.zero;
                subCameraObject.transform.localRotation = Quaternion.identity;
                mainCameraChild = subCameraObject.transform;

                Camera subCamera = mainCameraChild.gameObject.AddComponent<Camera>();
                subCamera.clearFlags = CameraClearFlags.SolidColor;
                subCamera.backgroundColor = Color.black;
                subCamera.targetTexture = _openDoorRenderTexture;
                subCamera.cullingMask = LayerMask.GetMask("OpenDoorLayer");
            }
        }


        //이펙트 생성
        {
            GameObject effectObject = Instantiate(_effectPrefabObject, _effectPrefabAttachScaler.transform);
            effectObject.transform.localPosition = Vector3.zero;
            effectObject.transform.localRotation = Quaternion.identity;
            effectObject.transform.localScale = Vector3.one;
        }


        //애니메이션 반영까지 최소 한프레임을 기다린다
        {
            _ownerAnimator.SetTrigger("Triggered");
            yield return new WaitForNextFrameUnit();
        }

        //문열기 작업 시작
        {
            float maxWaitTime = 5.0f;
            float maxWaitTimeAcc = 0.0f;
            bool animationTransitionFailed = false;

            while (true)
            {
                if (_ownerAnimator.IsInTransition(0) == false)
                {
                    break;
                }

                maxWaitTimeAcc += Time.deltaTime;

                if (maxWaitTimeAcc > maxWaitTime)
                {
                    animationTransitionFailed = true;
                    Debug.Assert(false, "Transition Waiting이 실패했습니다");
                    Debug.Break();
                    break;
                }

                yield return null;
            }


            if (animationTransitionFailed == true)
            {
                _state = DoorState.End;
                _isBusy = false;
                yield break;
            }

            AnimatorStateInfo stateInfo = _ownerAnimator.GetCurrentAnimatorStateInfo(0); //Open Node 반영이 됐을꺼다
            float time = stateInfo.length;
            float timeACC = 0.0f;

            while (true)
            {
                timeACC += Time.deltaTime;

                if (timeACC >= time)
                {
                    _state = DoorState.End;
                    _isBusy = false;
                    break;
                }

                yield return null;
            }
        }
    }

    private void CameraDragging()
    {
        CameraDraggingDesc newDesc = new CameraDraggingDesc();

        float dirToDoor = Vector3.Distance(Camera.main.transform.position, _cameraAttachPosition_DoorDir.transform.position);
        float dirToOpposite = Vector3.Distance(Camera.main.transform.position, _cameraAttachPosition_OppositeDir.transform.position);

        if (dirToDoor >= dirToOpposite)
        {
            newDesc._targetPosition = _cameraAttachPosition_OppositeDir.transform.position;
        }
        else
        {
            newDesc._targetPosition = _cameraAttachPosition_DoorDir.transform.position;
        }

        newDesc._accel = 10.0f;
        newDesc._currVelocity = 2.0f;

        StartCoroutine(CameraDraggingCorouting(newDesc));
    }

    private IEnumerator CreateDoorCoroutine(float targetTime)
    {
        _isBusy = true;

        float targetTimeAcc = 0.0f;

        while (true) 
        {
            targetTimeAcc += Time.deltaTime;

            if (targetTimeAcc >= targetTime)
            {
                _state = DoorState.Closed;
                _isBusy = false;
                break;
            }

            yield return null;
        }
    }

    public void SceneChange(bool isCameraDraggingNeed)
    {
        if (isCameraDraggingNeed == true)
        {
            CameraDragging();
        }

        CurtainCallControl_SimpleColor onDesc = new CurtainCallControl_SimpleColor();
        onDesc._target = false;
        onDesc._runningTime = 2.0f;
        onDesc._color = new Vector3(1.0f, 1.0f, 1.0f);
        CurtainCallControl_SimpleColor offDesc = new CurtainCallControl_SimpleColor();
        offDesc._target = true;
        offDesc._runningTime = 2.0f;
        offDesc._color = new Vector3(1.0f, 1.0f, 1.0f);

        SceneManagerWrapper.Instance.ChangeScene
        (
            _targetStage,
            CurtainCallType.SimpleColorFadeInOut,
            onDesc,
            CurtainCallType.SimpleColorFadeInOut,
            offDesc
        );
    }

    private IEnumerator CameraDraggingCorouting(CameraDraggingDesc desc)
    {
        float deltaTime = Time.deltaTime;

        while (true) 
        {
            Vector3 currCemeraPosition = Camera.main.transform.position;

            Vector3 dir = (desc._targetPosition - currCemeraPosition).normalized;

            desc._currVelocity += Time.deltaTime * desc._accel;

            if (desc._maxVelocityPlus > 0.0f)
            {
                desc._currVelocity = Mathf.Clamp(desc._currVelocity, 0.0f, desc._maxVelocityPlus);
            }

            if (desc._maxVelocityMinus < 0.0f)
            {
                desc._currVelocity = Mathf.Clamp(desc._currVelocity, desc._maxVelocityMinus, 0.0f);
            }

            Camera.main.transform.position += deltaTime * desc._currVelocity * dir;

            Vector3 afterDir = (desc._targetPosition - Camera.main.transform.position).normalized;

            if (Vector3.Dot(afterDir, dir) <= 0.0f)
            {
                Camera.main.transform.position = desc._targetPosition;
                break;
            }

            Vector3 afterPlaneDir = afterDir;
            afterPlaneDir.y = 0.0f;


            Camera.main.transform.LookAt(Camera.main.transform.position + afterPlaneDir);

            yield return null;
        }
    }
}
