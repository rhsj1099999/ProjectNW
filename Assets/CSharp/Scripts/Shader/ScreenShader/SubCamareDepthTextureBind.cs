using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SubCamareDepthTextureBind : MonoBehaviour
{
    public Camera targetCamera = null;
    public Material _targetMaterial = null;    // Shader Graph�� ����ϴ� ��Ƽ����
    public RenderTexture _renderTexture;

    private void Awake()
    {
        ////_renderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        ////_renderTexture.name = "Test";
        ////_renderTexture.Create();
        ////targetCamera.targetTexture = _renderTexture;
        //targetCamera.depthTextureMode = DepthTextureMode.Depth;
        ////_targetMaterial.SetTexture("_SubCameraDepthTexture", _renderTexture);
    }

    private void Update()
    {
        var temp = targetCamera.clearFlags;
        int a = 10;
        //GL.Clear(true, true, Color.clear); // �÷��� ���� ���۸� �ʱ�ȭ
    }


    private void OnDestroy()
    {
        ////if (_renderTexture != null)
        ////{
        ////    _renderTexture.Release();
        ////}
    }
}
