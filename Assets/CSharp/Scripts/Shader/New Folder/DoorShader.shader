Shader "Custom/DoorMaskShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            // Stencil ����
            Stencil
            {
                Ref 1                // Stencil �� (1)
                Comp Always          // �׻� Stencil Buffer�� �� ����
                Pass Replace         // ���� �ȼ��� Stencil ���� 1�� ����
            }

        // �� ��ü�� ���������� ����
        ColorMask 0            // ���� ������ ���� ����
    }
    }
}