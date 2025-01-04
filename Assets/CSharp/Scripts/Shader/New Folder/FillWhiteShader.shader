Shader "Custom/FillWhiteShader"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (1,1,1,1) // 기본 흰색
    }
        SubShader
    {
        Tags { "Queue" = "Overlay" } // 화면 최상단에 렌더링
        Pass
        {
            // Stencil 설정
            Stencil
            {
                Ref 1                // Stencil 값 1과 비교
                Comp Equal           // Stencil 값이 1인 영역만 렌더링
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FillColor;      // 흰색으로 채울 색상

            struct appdata_t
            {
                float4 vertex : POSITION;
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
                return _FillColor; // Stencil 값이 1인 영역을 흰색으로 채움
            }
            ENDCG
        }
    }
}