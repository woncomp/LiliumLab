#pragma pack_matrix ( row_major )

/*
		struct ShaderData
		{
			public Matrix matWorld;
			public Matrix matView;
			public Matrix matProjection;
			public Vector4 lightDir;
			public Vector4 ambientColor;
			public Vector4 diffuseColor;
		}

			var data = new ShaderData();
			data.matView = ActiveCamera.ViewMatrix;
			data.matProjection = ActiveCamera.ProjectionMatrix;
			data.lightDir = LightDir4;
			data.ambientColor = new Vector4(ambient, ambient, ambient, 1);
			data.diffuseColor = new Vector4(diffuse, diffuse, diffuse, 1);
*/
cbuffer LiliumPerFrame : register(b1)
{
	float4x4 matView;
	float4x4 matProjection;
	float4 lightDir;
	float4 ambientColor;
	float4 diffuseColor;
};

cbuffer LiliumPerObject : register(b2)
{
	float4x4 matWorld;
};

struct VS_IN
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float2 texCoord : TEXCOORD;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
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
	color = ambientColor + l * diffuseColor;

	return baseMapValue * color;
}