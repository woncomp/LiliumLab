#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 texCoord : TEXCOORD;
};

SamplerState s;
TextureCube texCube;

PS_IN VS( VS_IN input)
{
	PS_IN output = (PS_IN)0;

	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	output.texCoord = normalize(input.position.xyz);

	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return texCube.Sample(s, input.texCoord);
}