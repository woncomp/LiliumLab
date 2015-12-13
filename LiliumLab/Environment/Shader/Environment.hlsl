#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float3 eyeDirW : NORMAL1;
	float3 texCoord : TEXCOORD;
};

SamplerState s;
TextureCube texCube : register(t0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	output.normalW = mul(matWorldInverseTranspose, input.normal);
	output.eyeDirW = (eyePos - input.position).xyz;
	output.texCoord = input.position;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 ambient;
	float4 diffuse;
	float4 specular;
	float4 baseMapValue;
	float3 normalW;
	float3 eyeDirW;
	float3 reflectW;
	float l;

	normalW = normalize(input.normalW);
	eyeDirW = normalize(input.eyeDirW);

	float3 r = reflect(-eyeDirW, normalW);
	baseMapValue = texCube.Sample(s, r);

	return baseMapValue;
}