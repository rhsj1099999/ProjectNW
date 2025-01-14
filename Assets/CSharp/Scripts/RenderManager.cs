using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.Universal.ScriptableRenderer;

public class RenderManager : SubManager
{
    public string featureName; // Ȱ��ȭ/��Ȱ��ȭ�Ϸ��� Render Feature�� �̸�
    private ScriptableRendererFeature targetFeature;

    private static RenderManager _instance = null;

    public static RenderManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject newGameObject = new GameObject("RenderManager");
                _instance = newGameObject.AddComponent<RenderManager>();
                DontDestroyOnLoad(newGameObject);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    public override void SubManagerInit()
    {

    }


    void Start()
    {

        //// ���� Universal Renderer ��������
        //var renderer = (UniversalRenderer)UniversalRenderPipeline.asset.scriptableRenderer;

        //// rendererFeatures �ʵ忡 ���÷������� ����
        //var field = typeof(UniversalRenderer).GetField("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        //if (field == null)
        //{
        //    Debug.LogError("Cannot access 'rendererFeatures'. Ensure the UniversalRenderer is correctly set up.");
        //    return;
        //}

        //// rendererFeatures ��� ��������
        //var features = (System.Collections.Generic.List<ScriptableRendererFeature>)field.GetValue(renderer);
        //foreach (var feature in features)
        //{
        //    if (feature.name == featureName)
        //    {
        //        targetFeature = feature;
        //        break;
        //    }
        //}

        //if (targetFeature == null)
        //{
        //    Debug.LogWarning($"Render Feature '{featureName}' not found.");
        //}
    }

    public void SetFeatureEnabled(bool enabled)
    {
        if (targetFeature != null)
        {
            targetFeature.SetActive(enabled);
        }
    }

}
