Shader "Custom/DoorMaskShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            // Stencil 설정
            Stencil
            {
                Ref 1                // Stencil 값 (1)
                Comp Always          // 항상 Stencil Buffer에 값 쓰기
                Pass Replace         // 현재 픽셀의 Stencil 값을 1로 설정
            }

        // 문 자체는 렌더링하지 않음
        ColorMask 0            // 색상 데이터 쓰지 않음
    }
    }
}