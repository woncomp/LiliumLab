#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;

	output.position = mul(input.position, matWorld);
	output.position = mul(output.position, matView);
	output.position = mul(output.position, matProjection);

	output.normalW = mul(input.normal, matWorldInverseTranspose);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float3 normalW;
	float l;

	normalW = normalize(input.normalW);
	l = saturate(dot(normalW, lightDir));
	return lightAmbient + l * lightDiffuse;
}