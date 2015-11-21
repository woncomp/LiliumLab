#pragma pack_matrix ( row_major )

cbuffer LiliumPerFrame : register(b1)
{
	float4x4 matView;
	float4x4 matProjection;
	float4 lightDir;
	float4 eyePos;
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