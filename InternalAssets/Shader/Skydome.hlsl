#include <Lilium.hlsl>

cbuffer data : register(b0)
{
	float4x4 matWorld2;
	float4 bottomColor;
	float4 topColor;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

SamplerState s;
Texture2D baseMap : register(t0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = mul(matWorld2, input.position);
	output.position = mul(matView, output.position);
	output.position = mul(matProjection, output.position);

	float3 dir = normalize(input.position.xyz);
	output.texCoord = float2(0.5, dir.y * 0.5 + 0.5);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	//return baseMap.Sample(s, input.texCoord);
	return lerp(bottomColor, topColor, input.texCoord.y);
}