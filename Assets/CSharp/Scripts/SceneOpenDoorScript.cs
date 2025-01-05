using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOpenDoorScript : MonoBehaviour
{
    [SerializeField] private GameObject _effectPrefabObject = null;
    [SerializeField] private GameObject _cameraAttachPosition_DoorDir = null;
    [SerializeField] private GameObject _cameraAttachPosition_OppositeDir = null;

    [SerializeField] private RenderTexture _openDoorRenderTexture = null;

    private Animator _ownerAnimator = null;

    private void Awake()
    {
        _ownerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) == true)
        {
            OpenDoor();

            //CameraDragging();

            //CurtainCall();

            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_1");
        }
    }

    //카메라를 끌어오는 함수
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


    //씬 전환을 위한 커튼콜 함수
    //커튼콜을 직접 호출하면 안된다.
    //로딩을 호출했어야 한다.
    public void CurtainCall()
    {
        CurtainCallControl_SimpleColor desc = new CurtainCallControl_SimpleColor();

        desc._target = false;
        desc._runningTime = 2.0f;
        desc._color = new Vector3(1.0f, 1.0f, 1.0f);

        SceneManagerWrapper.Instance.CurtainCall(CurtainCallType.SimpleColorFadeInOut, desc);
    }



    //문을 여는 함수
    public void OpenDoor()
    {
        if (_openDoorRenderTexture == null)
        {
            return;
        }

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

        GameObject effect = Instantiate(_effectPrefabObject, transform);

        _ownerAnimator.SetTrigger("Triggered");
    }
}
