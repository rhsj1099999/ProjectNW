Shader "PotionFluidShader/Mat"
{
    Properties
    {
        // 기본 Material에는 필요 없지만, Shader에서 이 값을 읽어야 함
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            // Shader에서 사용할 변수 선언 (MaterialPropertyBlock에서 변경 가능)
            float4 _BaseColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _BaseColor; // 개별 오브젝트에 적용될 색상
            }
            ENDCG
        }
    }
}