#include <Lilium.hlsl>

cbuffer data
{
	float SSAO = 1;
	float4 Color = float4(1, 1, 1, 1);
}

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
	float4 positionCS : POSITION1;
};

SamplerState s;
Texture2D baseMap : register(t0);
Texture2D ssaoMap : register(t1);

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

	output.positionCS = output.position;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float l;

// ambient
	float2 texCoordScreenSpace = input.positionCS.xy / input.positionCS.w;
	texCoordScreenSpace.x = 0.5 + 0.5*texCoordScreenSpace.x;
	texCoordScreenSpace.y = 0.5 - 0.5*texCoordScreenSpace.y;
	float ambientIndensity = ssaoMap.Sample(s, texCoordScreenSpace);
	ambientIndensity = lerp(1, ambientIndensity, SSAO);
	float4 ambient = lightAmbient * ambientIndensity;

// diffuse
	float3 normalW = normalize(input.normalW);

	l = saturate(dot(normalW, lightDir));
	float4 diffuse = l * lightDiffuse;

// color
	float4 baseMapValue = baseMap.Sample(s, input.texCoord);
	return baseMapValue * (ambient + diffuse) * Color;
}