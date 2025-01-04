Shader "Custom/BlitShader"
{
    Properties
    {
        _MainTex("Source Texture", 2D) = "white" {}
        _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
    }
        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _ColorTint;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // 원본 텍스처에서 픽셀을 가져와 컬러 틴트 적용
                    fixed4 texColor = tex2D(_MainTex, i.uv);
                    return texColor * _ColorTint;
                }
                ENDCG
            }
        }
}