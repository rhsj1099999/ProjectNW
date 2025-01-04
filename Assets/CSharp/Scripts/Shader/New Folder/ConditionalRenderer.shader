Shader "Custom/ConditionalRender"
{
    Properties
    {
        _MainColor("Main Color", Color) = (1, 1, 1, 1) // 기본 색상
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

            fixed4 _MainColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #if defined(UNITY_UV_STARTS_AT_TOP)
                // 렌더 텍스처일 때만 렌더링
                return _MainColor; // 지정된 색상으로 렌더링
            #else
                // 화면(Frame Buffer)에는 렌더링하지 않음
                discard;
            #endif
        }
        ENDCG
    }
    }
}