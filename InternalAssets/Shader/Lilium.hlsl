#pragma pack_matrix ( row_major )

cbuffer LiliumPerFrame : register(b1)
{
	float4x4 matView;
	float4x4 matProjection;
	float4 lightDir;
	float4 eyePos;
	float4 lightAmbient;
	float4 lightDiffuse;
	float renderTargetWidth;
	float renderTargetHeight;
	float nearPlane;
	float farPlane;
};

cbuffer LiliumPerObject : register(b2)
{
	float4x4 matWorld;
	float4x4 matWorldInverseTranspose;
	float4x4 matWorldViewInverseTranspose;
};

struct VS_IN
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float2 texCoord : TEXCOORD;
};