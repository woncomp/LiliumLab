#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL0;
	float3 eyeDirW : NORMAL1;
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
	output.normalW = mul(matWorldInverseTranspose, input.normal);
	output.eyeDirW = eyePos - posW;
	output.texCoord = normalize(input.position.xyz);

	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return texCube.Sample(s, input.texCoord);
}

float4 PS_Sphere( PS_IN input ) :SV_Target
{
	float3 eyeDirWS = normalize(input.eyeDirW);
	float3 normalWS = normalize(input.normalW);
	float3 r = reflect(-eyeDirWS, normalWS);

	return texCube.Sample(s, r);
}