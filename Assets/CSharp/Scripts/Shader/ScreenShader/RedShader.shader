Shader "Custom/RedShader"
{
    Properties
    {
        // 쉐이더에서 필요한 속성을 정의할 수 있지만, 이 경우는 필요 없음
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            // 기본적으로 고정 빨간색으로 렌더링
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; // 버텍스 포지션
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // 클립 공간 포지션
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // 월드 -> 클립 공간으로 변환
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 0, 0, 1); // 빨간색 RGBA (1, 0, 0, 1)
            }
            ENDCG
        }
    }
}