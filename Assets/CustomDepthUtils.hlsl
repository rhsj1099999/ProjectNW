#ifndef CUSTOMDEPTHUTILS_INCLUDED
#define CUSTOMDEPTHUTILS_INCLUDED

float LinearEyeDepth_float(float depth_float, float near_float, float far_float)
{
    float LinearEyeDepth_float = (2.0 * near_float * far_float) / (far_float + near_float - depth_float * (far_float - near_float));
    return LinearEyeDepth_float;
}

float3 ReconstructViewSpacePosition(float2 uv, float depth, float4x4 invProjection)
{
    float4 clipSpacePos = float4(uv * 2.0 - 1.0, depth, 1.0);
    float4 viewSpacePos = mul(invProjection, clipSpacePos);
    viewSpacePos /= viewSpacePos.w;
    return viewSpacePos.xyz;
}

float3 ReconstructWorldPosition(float3 viewSpacePos, float4x4 viewToWorld)
{
    float4 worldSpacePos = mul(viewToWorld, float4(viewSpacePos, 1.0));
    return worldSpacePos.xyz;
}

float ComputeDistanceToCamera(float3 worldPos, float3 cameraPos)
{
    return length(worldPos - cameraPos);
}

#endif // CUSTOM_DEPTH_UTILS_INCLUDED