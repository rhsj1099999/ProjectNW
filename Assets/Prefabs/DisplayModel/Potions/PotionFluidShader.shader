Shader "PotionFluidShader/Mat"
{
    Properties
    {
        // �⺻ Material���� �ʿ� ������, Shader���� �� ���� �о�� ��
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

            // Shader���� ����� ���� ���� (MaterialPropertyBlock���� ���� ����)
            float4 _BaseColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _BaseColor; // ���� ������Ʈ�� ����� ����
            }
            ENDCG
        }
    }
}