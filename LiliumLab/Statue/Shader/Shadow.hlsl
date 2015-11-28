#include <LiliumPostprocess.hlsl>

static const int BACK_WEIGHT = 100;
static const int2 PCF_OFFSETS[9] = {-1, -1, -1, 0, -1, 1, 0, -1, 0, 0, 0, 1, 1, -1, 1, 0, 1, 1};

cbuffer _ShadowMapData : register(b0)
{
	float4x4 LightViewMatrix;
	float4x4 LightProjectionMatrix;
	float ShadowBias;
	float3 LightPosV;
	int ShadowMapSize;
}

SamplerState s;
Texture2D texPosW : register(t0);
Texture2D texShadow : register(t1);
Texture2D texNormalV : register(t2);
Texture2D texPosV : register(t3);

float GetShadowIndensity(float2 texCoord, int x, int y)
{
}

float4 PS(PS_IN input) : SV_Target
{
	float4 posWS = texPosW.Sample(s, input.texCoord);

	float4 posLVS = mul(LightViewMatrix, posWS);
	float4 posLCS = mul(LightProjectionMatrix, posLVS);

	float2 texCoordShadow;
	texCoordShadow.x = 0.5 + 0.5 * posLCS.x;
	texCoordShadow.y = 0.5 - 0.5 * posLCS.y;

	float c = 0;
	for(int i=0;i<9;++i)
	{
		int2 pcfOffset = PCF_OFFSETS[i];
		float2 texCoord = texCoordShadow;
		texCoord.x += pcfOffset.x / (float)ShadowMapSize;
		texCoord.y += pcfOffset.y / (float)ShadowMapSize;
		float shadowDepth = texShadow.Sample(s, texCoord).x;
		float fragmentDepth = posLCS.z / posLCS.w;

		c += (shadowDepth + ShadowBias > fragmentDepth) ? 0 : 0.5;
	}
	c /= 9.0;

	float3 normalVS = texNormalV.Sample(s, input.texCoord).xyz * 2 - 1;
	float3 posVS = texPosV.Sample(s, input.texCoord).xyz;
	float3 lightDirVS = normalize(LightPosV - posVS);

	float backLight = dot(lightDirVS, normalVS);
	if(backLight < 0) c = 0.5;

	return float4(0, 0, 0, c);
}