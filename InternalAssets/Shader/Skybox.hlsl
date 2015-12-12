#include <Lilium.hlsl>

cbuffer data : register(b0)
{
	float4x4 matWorld2;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 texCoord : TEXCOORD;
};

SamplerState s;
TextureCube texCube : register(t0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = mul(matWorld2, input.position);
	output.position = mul(matView, output.position);
	output.position = mul(matProjection, output.position);

	output.texCoord = input.position.xyz;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return texCube.Sample(s, normalize(input.texCoord));
}