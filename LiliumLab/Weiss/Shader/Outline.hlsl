#include <Lilium.hlsl>

cbuffer data
{
	float4 OutlineColor = float4(0, 0, 0, 0);
	float OutlineThickness = 1;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	float width = 0.001 * output.position.w * OutlineThickness;

	posW = mul(matWorld, input.position + float4(input.normal * width, 0));
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);

	// output.normalW = mul(input.normal, matWorld);
	// normalP = mul(output.normalW, matView);
	// normalP = mul(output.normalW, matProjection);

	// output.position = output.position + mul(OutlineThickness, normalP);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return OutlineColor;
}