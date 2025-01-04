using UnityEngine;

[ExecuteInEditMode]
public class ScreenBlend : MonoBehaviour
{
    public Camera secondaryCamera; // 추가 화면을 찍을 카메라
    public Material blendMaterial; // 화면을 합성할 쉐이더 머티리얼

    private RenderTexture secondaryRenderTexture;

    void Start()
    {
        if (secondaryCamera != null)
        {
            // RenderTexture 생성 및 추가 카메라에 할당
            secondaryRenderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Default);
            secondaryRenderTexture.Create();
            secondaryCamera.targetTexture = secondaryRenderTexture;

            // 쉐이더에 RenderTexture 전달
            blendMaterial.SetTexture("_SecondTex", secondaryRenderTexture);
        }
    }

    private void OnDestroy()
    {
        // RenderTexture 해제
        if (secondaryRenderTexture != null)
        {
            secondaryRenderTexture.Release();
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blendMaterial != null)
        {
            // 기본 화면(source)과 추가 화면을 합성하여 최종 출력
            Graphics.Blit(source, destination, blendMaterial);
        }
        else
        {
            // 합성 없이 기본 화면 출력
            Graphics.Blit(source, destination);
        }
    }
}