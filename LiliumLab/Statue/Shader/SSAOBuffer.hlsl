#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
	float linearDepth : TEXCOORD1;
	float3 normalV : TEXCOORD2;
};

struct PS_OUT
{
	float4 linearDepth : SV_Target0;
	float4 normalV : SV_Target1;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(input.position, matWorld);
	posV = mul(posW, matView);

	output.position = mul(posV, matProjection);

	output.normalW = mul(input.normal, matWorldInverseTranspose);
	output.texCoord = input.texCoord;

	output.linearDepth = (posV.z - nearPlane) / (farPlane - nearPlane);
	output.normalV = mul(input.normal, matWorldViewInverseTranspose);

	return output;
}

PS_OUT PS(PS_IN input)
{
	PS_OUT output = (PS_OUT)0;
	output.linearDepth.xyz = input.linearDepth;
	output.linearDepth.w = 1;
	output.normalV = float4(normalize(input.normalV) * 0.5 + 0.5, 1);

	return output;
}