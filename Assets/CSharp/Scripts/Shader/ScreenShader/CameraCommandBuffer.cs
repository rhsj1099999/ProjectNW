using UnityEngine;
using UnityEngine.Rendering;

public class CameraCommandBuffer : MonoBehaviour
{
    public Camera mainCamera = null;            // ���� ī�޶�
    private CommandBuffer commandBuffer = null; // CommandBuffer ��ü
    public LayerMask targetLayer = 0;        // Depth Texture�� ĸó�� ��� ���̾�

    [SerializeField] private RenderTexture depthTexture = null;  // Depth Texture ����

    void Start()
    {
        // RenderTexture ����
        mainCamera = Camera.main;
        //depthTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Depth);
        depthTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Depth);
        depthTexture.Create();

        // CommandBuffer ����
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Capture Depth for Specific Layer";
        
        commandBuffer.SetRenderTarget(depthTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        commandBuffer.SetViewProjectionMatrices(mainCamera.worldToCameraMatrix, mainCamera.projectionMatrix);

        commandBuffer.ClearRenderTarget(true, true, Color.black); // �ʱ�ȭ
        commandBuffer.ClearRenderTarget(true, true, Color.red); // ��� �ȼ��� ���������� ä���
        Debug.Log("CommandBuffer executed.");
        

        // Ư�� ���̾ ������
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
        // Ư�� ���̾��� ������Ʈ���� �˻�
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

        // RenderTexture ����
        if (depthTexture != null)
        {
            depthTexture.Release();
            Destroy(depthTexture);
        }
    }



    //public Camera mainCamera = null; // �� ī�޶�
    //public RenderTexture renderTexture = null; // ���� �ؽ�ó
    //public LayerMask renderTextureLayer; // ���� �ؽ�ó�� �������� ���̾�

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
    //    // CommandBuffer �ʱ�ȭ
    //    commandBuffer.Clear();

    //    // Ư�� ���̾ ������
    //    commandBuffer.SetRenderTarget(renderTexture);

    //    // ī�޶��� Ư�� ���̾ CommandBuffer�� ������
    //    commandBuffer.ClearRenderTarget(true, true, Color.clear);
    //    commandBuffer.SetViewProjectionMatrices(mainCamera.worldToCameraMatrix, mainCamera.projectionMatrix);

    //    // Ư�� ���̾ ������
    //    commandBuffer.SetGlobalFloat("_LayerMask", renderTextureLayer.value);

    //    // ��� �߰�
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