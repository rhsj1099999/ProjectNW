Shader "Custom/RedShader"
{
    Properties
    {
        // ���̴����� �ʿ��� �Ӽ��� ������ �� ������, �� ���� �ʿ� ����
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            // �⺻������ ���� ���������� ������
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; // ���ؽ� ������
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // Ŭ�� ���� ������
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // ���� -> Ŭ�� �������� ��ȯ
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 0, 0, 1); // ������ RGBA (1, 0, 0, 1)
            }
            ENDCG
        }
    }
}