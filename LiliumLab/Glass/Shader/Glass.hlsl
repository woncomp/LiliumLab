#include <Lilium.hlsl>

cbuffer data
{
	float RefractionRatio = 1;
	float Transparency = 0.5;
	float4 GlassColor = float4(0.5, 0.5, 0.7, 1);
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL0;
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
	//output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float3 normalW = normalize(input.normalW);
	float3 eyeDirW = normalize(input.eyeDirW);

	float3 r = reflect(-eyeDirW, normalW);
	float4 reflectColor = texCube.Sample(s, r);

	float cos1 = dot(eyeDirW, normalW);
	float sin1 = sqrt(1 - cos1 * cos1);

	float sin2 = saturate(RefractionRatio * sin1);
	float cos2 = sqrt(1 - sin2 * sin2);

	float3 cosv = -normalW;
	float3 sinv = normalize(cross(cross(eyeDirW, normalW), normalW));

	float3 refractDir = cos2 * cosv + sin2 * sinv;
	float4 refractColor = texCube.Sample(s, refractDir);

	float4 color = reflectColor * sin1 + refractColor * (1 - sin2) + GlassColor * sin2;
	color.a = Transparency;

	return color;
}