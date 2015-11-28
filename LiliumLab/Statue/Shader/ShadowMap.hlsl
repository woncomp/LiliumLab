#include <Lilium.hlsl>

cbuffer _data : register(b0)
{
	float4x4 LightViewMatrix;
	float4x4 LightProjectionMatrix;
}

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
	float3 positionLCS : TEXCOORD1;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(LightViewMatrix, posW);

	output.position = mul(LightProjectionMatrix, posV);

	//output.normalW = mul(matWorldInverseTranspose, input.normal);
	//output.texCoord = input.texCoord;

	output.positionLCS = output.position;// / output.position.w;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float d = input.positionLCS.z * 10;

	return float4(d, d, d, 1);
}