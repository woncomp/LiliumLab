static const int MAX_BONES = 48;

cbuffer LiliumSkinData : register(b3)
{
	float4x4 matBonePalette[MAX_BONES];
};

struct VS_IN_SKIN
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
	float2 texCoord : TEXCOORD;
	float4 boneWeight : TEXCOORD1;
	int4 boneIndex : TEXCOORD2;
};

float4 SkinningVertex(VS_IN_SKIN input)
{
	float4x4 matBone0 = matBonePalette[input.boneIndex[0]] * input.boneWeight[0];
	float4x4 matBone1 = matBonePalette[input.boneIndex[1]] * input.boneWeight[1];
	float4x4 matBone2 = matBonePalette[input.boneIndex[2]] * input.boneWeight[2];
	float4x4 matBone3 = matBonePalette[input.boneIndex[3]] * input.boneWeight[3];
	return mul(matBone0 + matBone1 + matBone2 + matBone3, input.position);
}