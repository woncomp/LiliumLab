#include <Lilium.hlsl>

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 posW : POSITION1;
	float3 posV : POSITION2;
	float3 normalV : NORMAL;
};

struct PS_OUT
{
	float4 posW : SV_Target0;
	float4 posV : SV_Target1;
	float4 normalV : SV_Target2;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	output.posW = posW;
	output.posV = posV;
	output.normalV = mul(matWorldViewInverseTranspose, input.normal);

	return output;
}

PS_OUT PS(PS_IN input)
{
	PS_OUT output = (PS_OUT)0;
	output.posW = float4(input.posW, 1);
	output.posV = float4(input.posV, 1);
	output.normalV = float4(normalize(input.normalV) * 0.5 + 0.5, 1);

	return output;
}