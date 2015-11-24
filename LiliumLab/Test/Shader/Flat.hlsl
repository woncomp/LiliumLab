#include <Lilium.hlsl>

/*
		struct ShaderData
		{
			public Matrix matWorld;
			public Matrix matView;
			public Matrix matProjection;
			public Vector4 lightDir;
			public Vector4 lightAmbient;
			public Vector4 lightDiffuse;
		}

			var data = new ShaderData();
			data.matView = ActiveCamera.ViewMatrix;
			data.matProjection = ActiveCamera.ProjectionMatrix;
			data.lightDir = LightDir4;
			data.lightAmbient = new Vector4(ambient, ambient, ambient, 1);
			data.lightDiffuse = new Vector4(diffuse, diffuse, diffuse, 1);
*/

cbuffer data
{
	float4 SomeColor = float4(1, 0, 0, 1);
	float TestValue = 0.5;
	float3 ___;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	nointerpolation float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
};

SamplerState s;
Texture2D baseMap : register(t0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4 posW;

	output.position = mul(input.position, matWorld);
	output.position = mul(output.position, matView);
	output.position = mul(output.position, matProjection);

	output.normalW = mul(input.normal, matWorld);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 color;
	float4 baseMapValue;
	float3 normalW;
	float l;

	baseMapValue = baseMap.Sample(s, input.texCoord);
	normalW = normalize(input.normalW);
	l = saturate(dot(normalW, lightDir));
	color = lightAmbient + l * lightDiffuse;

	return baseMapValue * color * TestValue * SomeColor;
}