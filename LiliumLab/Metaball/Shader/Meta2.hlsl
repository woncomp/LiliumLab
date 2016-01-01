#include <Lilium.hlsl>

cbuffer _data
{
	float Threshold = 0.5;
	float UVScale = 1;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
	float2 texCoord2 : TEXCOORD1;
};

SamplerState s;
Texture2D baseMap : register(t0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	output.normalW = mul(matWorldInverseTranspose, input.normal);
	output.texCoord = input.texCoord;
	output.texCoord2 = input.texCoord * 2.0 - 1.0;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float c = baseMap.Sample(s, input.texCoord).r;

	float4 normalSS = 0;
	normalSS.xy = input.texCoord2 * UVScale;
	//normalSS.z = sqrt(1.0 - dot(input.texCoord2, input.texCoord2));
	normalSS.w = c;

	return normalSS;
}