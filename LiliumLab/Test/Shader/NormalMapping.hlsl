#include <Lilium.hlsl>

cbuffer data
{
	float SpecularPowerScale = 1;
	float3 ___;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 positionW : POSITION1;
	float3 normalW : NORMAL;
	float3 tangentW : TANGENT;
	float2 texCoord : TEXCOORD;
};

SamplerState s;
Texture2D baseMap : register(t0);
Texture2D bumpMap : register(t1);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;

	output.position = mul(input.position, matWorld);
	output.position = mul(output.position, matView);
	output.position = mul(output.position, matProjection);

	output.positionW = mul(input.position, matWorld);
	output.normalW = mul(input.normal, matWorld);
	output.tangentW = mul(input.tangent, matWorld);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 bumpMapValue;
	float4 baseMapValue;
	float3 normalW;
	float3 tangentW;
	float3 binormalW;
	float3x3 tbn;
	float3 lightDirT;
	float3 normalT;
	float3 eyeDirT;
	float3 halfT;
	float l;
	float4 diffuse;
	float4 specular;

	normalW = normalize(input.normalW);
	tangentW = normalize(input.tangentW);
	binormalW = cross(normalW, tangentW);
	tbn = float3x3(tangentW, binormalW, normalW);

	lightDirT = mul(tbn, lightDir);

	bumpMapValue = bumpMap.Sample(s, input.texCoord);
	normalT = bumpMapValue.xyz * 2 - 1;

	baseMapValue = baseMap.Sample(s, input.texCoord);
	l = saturate(dot(normalT, lightDirT));
	diffuse = l * lightDiffuse;

	eyeDirT = normalize(mul(tbn, (eyePos - input.positionW).xyz));
	halfT = (lightDirT + eyeDirT) * 0.5;
	l = pow(saturate(dot(halfT, normalT)), 1 / SpecularPowerScale);
	specular = float4(l, l, l, 1);

	return baseMapValue * (lightAmbient + diffuse) + specular;
}