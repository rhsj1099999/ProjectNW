using UnityEngine;
using UnityEngine.Rendering;

public class CameraCommandBuffer : MonoBehaviour
{
    public Camera mainCamera = null;            // 메인 카메라
    private CommandBuffer commandBuffer = null; // CommandBuffer 객체
    public LayerMask targetLayer = 0;        // Depth Texture를 캡처할 대상 레이어

    [SerializeField] private RenderTexture depthTexture = null;  // Depth Texture 저장

    void Start()
    {
        // RenderTexture 생성
        mainCamera = Camera.main;
        //depthTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Depth);
        depthTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Depth);
        depthTexture.Create();

        // CommandBuffer 생성
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Capture Depth for Specific Layer";
        
        commandBuffer.SetRenderTarget(depthTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        commandBuffer.SetViewProjectionMatrices(mainCamera.worldToCameraMatrix, mainCamera.projectionMatrix);

        commandBuffer.ClearRenderTarget(true, true, Color.black); // 초기화
        commandBuffer.ClearRenderTarget(true, true, Color.red); // 모든 픽셀을 빨간색으로 채우기
        Debug.Log("CommandBuffer executed.");
        

        // 특정 레이어만 렌더링
        Renderer[] renderers = FindRenderersInLayer(targetLayer);
        foreach (Renderer renderer in renderers)
        {
            commandBuffer.DrawRenderer(renderer, renderer.sharedMaterial);
        }

        Shader.SetGlobalTexture("_CDT", depthTexture);

        mainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    private Renderer[] FindRenderersInLayer(LayerMask layerMask)
    {
        // 특정 레이어의 오브젝트들을 검색
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var renderers = new System.Collections.Generic.List<Renderer>();

        foreach (GameObject obj in allObjects)
        {


            if (((1 << obj.layer) & layerMask) != 0)
            {
                Renderer renderer = obj.GetComponent<Renderer>();


                if (renderer != null)
                {
                    if (renderer.sharedMaterial == null)
                    {
                        Debug.LogWarning($"{renderer.name} has no material.");
                    }

                    renderers.Add(renderer);
                }
            }
        }

        return renderers.ToArray();
    }

    private void OnDestroy()
    {
        if (commandBuffer != null)
        {
            mainCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            commandBuffer.Dispose();
        }

        // RenderTexture 정리
        if (depthTexture != null)
        {
            depthTexture.Release();
            Destroy(depthTexture);
        }
    }



    //public Camera mainCamera = null; // 주 카메라
    //public RenderTexture renderTexture = null; // 렌더 텍스처
    //public LayerMask renderTextureLayer; // 렌더 텍스처에 렌더링할 레이어

    //private Material _targetMaterial = null;
    //private CommandBuffer commandBuffer;

    //void Start()
    //{
    //    mainCamera = Camera.main;
    //    commandBuffer = new CommandBuffer();
    //    commandBuffer.name = "Render To Texture";
    //    EnableRenderToTexture();
    //}

    //public void EnableRenderToTexture()
    //{
    //    // CommandBuffer 초기화
    //    commandBuffer.Clear();

    //    // 특정 레이어만 렌더링
    //    commandBuffer.SetRenderTarget(renderTexture);

    //    // 카메라의 특정 레이어만 CommandBuffer에 렌더링
    //    commandBuffer.ClearRenderTarget(true, true, Color.clear);
    //    commandBuffer.SetViewProjectionMatrices(mainCamera.worldToCameraMatrix, mainCamera.projectionMatrix);

    //    // 특정 레이어만 렌더링
    //    commandBuffer.SetGlobalFloat("_LayerMask", renderTextureLayer.value);

    //    // 명령 추가
    //    mainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    //}

    //public void DisableRenderToTexture()
    //{
    //    if (commandBuffer != null)
    //    {
    //        mainCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    //    }
    //}
}