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

	baseMapValue = texCube.Sample(s, normalize(input.texCoord));

	normalW = normalize(input.normalW);
	//diffuse = DiffuseColor * lightDiffuse * saturate(dot(normalW, lightDir)) * Kd;

	//reflectW = reflect(-lightDir.xyz, normalW);
	eyeDirW = normalize(input.eyeDirW);
	float rimRange = 0.3;
	float rimIndensity = 0.4;
	float rim = clamp(dot(normalW, eyeDirW), 0, rimRange);
	float4 rimColor = float4(1, 1, 1, 1);
	rimColor *= (1 - rim / rimRange) * rimIndensity;
	rimColor.a = 1;

	return baseMapValue + rimColor;
}