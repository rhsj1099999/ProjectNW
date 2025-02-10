using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SceneChangeDoor_InField : MonoBehaviour
{
    /*--------------------------------------------------
    |NOTI & TOOD| LoginScene에 존재하는 Door와 비슷한데
    필드에서 Scene change 용도로 쓰고있다. 합칠방법은 없나?
    --------------------------------------------------*/

    [SerializeField] private GameObject _effectPrefabObject = null;
    [SerializeField] private GameObject _effectPrefabAttachScaler = null;

    [SerializeField] private RenderTexture _openDoorRenderTexture = null;
    [SerializeField] private string _targetStage = "None";

    private Animator _ownerAnimator = null;

    private IEnumerator CreateDoorCoroutine(float targetTime)
    {
        float targetTimeAcc = 0.0f;

        while (true)
        {
            targetTimeAcc += Time.deltaTime;

            if (targetTimeAcc >= targetTime)
            {
                {
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
                }

                break;
            }

            yield return null;
        }
    }


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
        StartCoroutine(CreateDoorCoroutine(stateInfo.length));
    }


    public void DoorCall()
    {
        _ownerAnimator.SetTrigger("Triggered");
    }


    public void SceneChange()
    {
        CurtainCallControl_SimpleColor onDesc = new CurtainCallControl_SimpleColor();
        onDesc._target = false;
        onDesc._runningTime = 2.0f;
        onDesc._color = new Vector3(1.0f, 1.0f, 1.0f);
        CurtainCallControl_SimpleColor offDesc = new CurtainCallControl_SimpleColor();
        offDesc._target = true;
        offDesc._runningTime = 1.0f;
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
}
