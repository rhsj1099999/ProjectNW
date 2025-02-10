using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SceneChangeDoor_InField : MonoBehaviour
{
    /*--------------------------------------------------
    |NOTI & TOOD| LoginScene�� �����ϴ� Door�� ����ѵ�
    �ʵ忡�� Scene change �뵵�� �����ִ�. ��ĥ����� ����?
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
                    //���� �ؽ��� ����� ī�޶� �۾�
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


                    //����Ʈ ����
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
            Debug.Assert(false, "Ÿ�� ���������� ���������� �ʽ��ϴ�");
            Debug.Break();
        }

        if (_openDoorRenderTexture == null)
        {
            Debug.Assert(false, "�����ؽ��İ� ����");
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
