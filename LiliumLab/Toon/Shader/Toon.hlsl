#include <Lilium.hlsl>

cbuffer data
{
	float4 AmbientColor = float4(1, 1, 1, 1);
	float4 DiffuseColor = float4(1, 1, 1, 1);
	float4 SpecularColor = float4(0, 0, 0, 1);
	float Shininess = 5;
	float SphereAddIndensity = 0;
	float SphereMulIndensity = 0;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 positionW : POSITION1;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
};

SamplerState s;
Texture2D texColor : register(t0);
Texture2D texToon : register(t1);
Texture2D texSphere : register(t2);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;
	float4 posV;

	posW = mul(matWorld, input.position);
	posV = mul(matView, posW);

	output.position = mul(matProjection, posV);
	output.positionW = posW;
	output.normalW = mul(matWorldInverseTranspose, input.normal);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float l;
	float3 normalWS = normalize(input.normalW);

// color
	float4 color = texColor.Sample(s, input.texCoord);

// sphere
	float2 texCoordSph;
	texCoordSph.x = 0.5 + 0.5 * normalWS.x;
	texCoordSph.y = 0.5 - 0.5 * normalWS.y;
	float4 sphere = texSphere.Sample(s, texCoordSph);

	color *= lerp(1, sphere, SphereMulIndensity);
	color += (sphere * SphereAddIndensity);

// ambient
	float4 ambient = lightAmbient * AmbientColor;

// diffuse
	float2 texCoordToon = 0.5;
	texCoordToon.x = 0.5;
	texCoordToon.y = 0.5 - 0.5 * dot(normalWS, lightDir);

	float4 colorToon = texToon.Sample(s, texCoordToon);
	float4 diffuse = lightDiffuse * DiffuseColor;

// specular
	float3 eyeDirWS = normalize(eyePos - input.positionW);
	float3 halfVectorWS = (eyeDirWS + lightDir) * 0.5;
	l = saturate(dot(halfVectorWS, normalWS));
	l = pow(l, Shininess);
	float4 specular = lightDiffuse * SpecularColor * l;

// light
	color *= (ambient + diffuse);
	color *= colorToon;
	//color += specular;

	return color;
}