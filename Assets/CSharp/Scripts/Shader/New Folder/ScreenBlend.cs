using UnityEngine;

[ExecuteInEditMode]
public class ScreenBlend : MonoBehaviour
{
    public Camera secondaryCamera; // �߰� ȭ���� ���� ī�޶�
    public Material blendMaterial; // ȭ���� �ռ��� ���̴� ��Ƽ����

    private RenderTexture secondaryRenderTexture;

    void Start()
    {
        if (secondaryCamera != null)
        {
            // RenderTexture ���� �� �߰� ī�޶� �Ҵ�
            secondaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Default);
            secondaryRenderTexture.Create();
            secondaryCamera.targetTexture = secondaryRenderTexture;

            // ���̴��� RenderTexture ����
            blendMaterial.SetTexture("_SecondTex", secondaryRenderTexture);
        }
    }

    private void OnDestroy()
    {
        // RenderTexture ����
        if (secondaryRenderTexture != null)
        {
            secondaryRenderTexture.Release();
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blendMaterial != null)
        {
            // �⺻ ȭ��(source)�� �߰� ȭ���� �ռ��Ͽ� ���� ���
            Graphics.Blit(source, destination, blendMaterial);
        }
        else
        {
            // �ռ� ���� �⺻ ȭ�� ���
            Graphics.Blit(source, destination);
        }
    }
}