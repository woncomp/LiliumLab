#include <Lilium.hlsl>

cbuffer data : register(b0)
{
	float4x4 ShadowWorldTransform;
	float ShadowIndensity = 0.5;
	float3 ___;
};

struct PS_IN
{
	float4 position : SV_POSITION;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;

	output.position = mul(input.position, ShadowWorldTransform);
	output.position = mul(output.position, matView);
	output.position = mul(output.position, matProjection);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return float4(0, 0, 0, ShadowIndensity);
}