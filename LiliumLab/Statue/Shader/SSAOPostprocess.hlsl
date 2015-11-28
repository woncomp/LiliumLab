#include <LiliumPostprocess.hlsl>

static const int KERNEL_COUNT = 64;
static const float NOISE_SIZE = 64;

cbuffer data :register(b0)
{
	float radius;
	float4x4 matProjection;
	float3 kernels[KERNEL_COUNT];
};

SamplerState s : register(s0);
SamplerState sKernelRotations : register(s2);
Texture2D texPosV : register(t0);
Texture2D texNormalV : register(t1);
Texture2D texKernelRotations : register(t2);

float2 GetScreenCoord(float3 posVS)
{
	float4 offset = float4(posVS, 1.0);
	offset = mul(matProjection, offset);
	offset.xyz /= offset.w;
	offset.xyz = offset * 0.5 + 0.5;
	offset.y = 1- offset.y;
	return offset.xy;
}

float4 PS(PS_IN input) : SV_Target
{
	float3 geoPosVS = texPosV.Sample(s, input.texCoord);
	float3 geoNormalVS = texNormalV.Sample(s, input.texCoord) * 2 - 1;
	float3 randomVec = texKernelRotations.Sample(sKernelRotations, input.texCoord * float2(renderTargetWidth, renderTargetHeight) / NOISE_SIZE);

	float3 geoTangentVS = normalize(randomVec - geoNormalVS * dot(randomVec, geoNormalVS));
	float3 geoBinormalVS = cross(geoNormalVS, geoTangentVS);

	float3x3 tbn = float3x3(geoTangentVS, geoBinormalVS, geoNormalVS);

	float occlusion = 0;

	for(int i=0;i<KERNEL_COUNT;++i)
	{
		float3 kernelTS = kernels[i];
		float3 kernelVS = mul(kernelTS, tbn);
		float3 samplePosVS = geoPosVS + kernelVS * radius;

		float2 screenTexCoord = GetScreenCoord(samplePosVS);

		float4 sampleGeoPosVS = texPosV.Sample(s, screenTexCoord);

		float rangeInfluence = smoothstep(0.0, 1.0, radius / abs(geoPosVS.z - sampleGeoPosVS.z));
		float oo = sampleGeoPosVS.z < samplePosVS.z ? 1 : 0;
		occlusion += oo * rangeInfluence;
	}
	occlusion = 1 - occlusion / KERNEL_COUNT;

	float4 output;
	output.xyzw = occlusion * occlusion;
	return output;
}