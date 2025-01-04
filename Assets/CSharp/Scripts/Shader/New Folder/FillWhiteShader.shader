Shader "Custom/FillWhiteShader"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (1,1,1,1) // �⺻ ���
    }
        SubShader
    {
        Tags { "Queue" = "Overlay" } // ȭ�� �ֻ�ܿ� ������
        Pass
        {
            // Stencil ����
            Stencil
            {
                Ref 1                // Stencil �� 1�� ��
                Comp Equal           // Stencil ���� 1�� ������ ������
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FillColor;      // ������� ä�� ����

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
                return _FillColor; // Stencil ���� 1�� ������ ������� ä��
            }
            ENDCG
        }
    }
}