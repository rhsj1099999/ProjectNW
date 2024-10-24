using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAlphaChange : MonoBehaviour
{
    // SkinnedMeshRenderer 참조
    public SkinnedMeshRenderer skinnedMeshRenderer;

    // Alpha 값을 조정할 수 있는 변수
    [Range(0f, 1f)] public float alpha = 1f;

    // OnValidate를 통해 Inspector에서 Alpha 값을 조정
    private void OnValidate()
    {
        if (skinnedMeshRenderer != null)
        {
            // SkinnedMeshRenderer의 Material에서 색상 가져오기
            Material material = skinnedMeshRenderer.sharedMaterial;
            if (material != null)
            {
                // 현재 색상 가져오기
                Color color = material.color;

                // alpha 값을 변경
                color.a = alpha;

                // 변경된 색상을 Material에 다시 적용
                material.color = color;

                // 만약 투명도를 지원하는 셰이더를 사용하지 않는다면, 투명 셰이더 설정을 확인
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
