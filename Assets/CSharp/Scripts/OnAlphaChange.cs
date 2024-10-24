using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAlphaChange : MonoBehaviour
{
    // SkinnedMeshRenderer ����
    public SkinnedMeshRenderer skinnedMeshRenderer;

    // Alpha ���� ������ �� �ִ� ����
    [Range(0f, 1f)] public float alpha = 1f;

    // OnValidate�� ���� Inspector���� Alpha ���� ����
    private void OnValidate()
    {
        if (skinnedMeshRenderer != null)
        {
            // SkinnedMeshRenderer�� Material���� ���� ��������
            Material material = skinnedMeshRenderer.sharedMaterial;
            if (material != null)
            {
                // ���� ���� ��������
                Color color = material.color;

                // alpha ���� ����
                color.a = alpha;

                // ����� ������ Material�� �ٽ� ����
                material.color = color;

                // ���� ������ �����ϴ� ���̴��� ������� �ʴ´ٸ�, ���� ���̴� ������ Ȯ��
                material.SetFloat("_Mode", 3); // 3 = Transparent
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
    }
}
