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
    [SerializeField] private GameObject _effectPrefabObject = null;
    [SerializeField] private GameObject _cameraAttachPosition_DoorDir = null;
    [SerializeField] private GameObject _cameraAttachPosition_OppositeDir = null;
    [SerializeField] private RenderTexture _openDoorRenderTexture = null;
    [SerializeField] private string _targetStage = "None";

    public void SetTargetState(string stageName) { _targetStage = stageName; }

    private bool _objectActivated = false;

    private Animator _ownerAnimator = null;

    private Coroutine _interactionCoroutine = null;

    

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

        AnimatorStateInfo stateInfo = _ownerAnimator.GetCurrentAnimatorStateInfo(0);

        StartCoroutine(ActivateCoroutine(stateInfo.length));
    }

    private IEnumerator ActivateCoroutine(float targetTime)
    {
        float targetTimeAcc = 0.0f;
        
        while (true) 
        {
            targetTimeAcc += Time.deltaTime;

            if (targetTimeAcc >= targetTime)
            {
                _objectActivated = true;
                break;
            }

            yield return null;
        }
    }

    private void Update()
    {
        CheckOpenDoor();
    }

    private IEnumerator StartOpenDoorProcedural()
    {
        yield return StartCoroutine(OpenDoor());

        CameraDragging();

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


    private void CheckOpenDoor()
    {
        if (_objectActivated == false)
        {
            return;
        }

        if (_interactionCoroutine != null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.H) == false)
        {
            return;
        }

        _interactionCoroutine = StartCoroutine(StartOpenDoorProcedural());
    }

    //카메라를 끌어오는 함수
    public void CameraDragging()
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



    //문을 여는 함수
    public IEnumerator OpenDoor()
    {
        Camera mainCamera = Camera.main;
        GameObject mainCameraObject = mainCamera.gameObject;
        GameObject subCameraObject = new GameObject("SubCamera_OpenDoorLayer");
        subCameraObject.transform.localPosition = Vector3.zero;
        subCameraObject.transform.localRotation = Quaternion.identity;
        subCameraObject.transform.SetParent(mainCameraObject.transform);

        Camera subCamera = subCameraObject.AddComponent<Camera>();
        subCamera.clearFlags = CameraClearFlags.SolidColor;
        subCamera.backgroundColor = Color.black;
        subCamera.targetTexture = _openDoorRenderTexture;
        subCamera.cullingMask = LayerMask.GetMask("OpenDoorLayer");
        
        Instantiate(_effectPrefabObject, transform);

        //애니메이션 반영까지 최소 한프레임을 기다린다
        {
            _ownerAnimator.SetTrigger("Triggered");
            yield return new WaitForNextFrameUnit();
        }

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
                break;
            }

            yield return null;
        }
    }
}
