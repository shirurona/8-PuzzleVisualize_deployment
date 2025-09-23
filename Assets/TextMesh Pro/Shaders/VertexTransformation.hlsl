//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

StructuredBuffer<float4x4> _Matrices;

void VertexTransformation_float(float4 in_positionOS, float instanceID, out float3 out_positionWS)
{
    float4x4 matri = _Matrices[instanceID];
    
    float3 positionWS = mul(matri, float4(in_positionOS.xyz, 1.0)).xyz;
    //half3 normalWS = normalize(mul((float3x3)matri, normalOS));
    //float4 positionCS = TransformWorldToHClip(positionWS);
    
    //out_positionCS = positionCS;
    out_positionWS = positionWS;
    //out_normalWS = normalWS;
    //out_uv = in_uv;
}