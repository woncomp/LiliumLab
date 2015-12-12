#include <Lilium.hlsl>


cbuffer _data :register(b0)
{
	float4 Color = float4(1, 1, 1, 1);
}

cbuffer data
{
	// float4 AmbientColor = float4(0.2, 0.2, 0.2, 1);
	// float4 DiffuseColor = float4(0.2, 0.2, 0.2, 1);
	float4 SpecularColor = float4(1, 1, 1, 1);
	float Shininess  = 2.2;
	float Ka = 0.5;
	float Kd = 0.4;
	float Ks = 0.8;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
	float3 eyeDirW : NORMAL1;
	float2 texCoord : TEXCOORD;
};

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
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 ambient;
	float4 diffuse;
	float4 specular;
	//float4 baseMapValue;
	float3 normalW;
	float3 eyeDirW;
	float3 reflectW;
	float l;

	//baseMapValue = baseMap.Sample(s, input.texCoord);
	ambient = lightAmbient * Ka;

	normalW = normalize(input.normalW);
	diffuse = lightDiffuse * saturate(dot(normalW, lightDir)) * Kd;

	reflectW = reflect(-lightDir.xyz, normalW);
	eyeDirW = normalize(input.eyeDirW);
	specular = SpecularColor * pow(saturate(dot(reflectW, eyeDirW)), Shininess) * Ks;

	return (ambient + diffuse + specular) * Color;
}