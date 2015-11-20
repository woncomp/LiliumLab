cbuffer CustomDataBuffer : register(b0)
{
	float4x4 matWorld;
	float4x4 matView;
	float4x4 matProjection;
	float4x4 matWorldViewProj;
	float4 lightDir;
	float4 eyePos;
};

struct VS_IN
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float3 normalW : NORMAL;
};

PS_IN VS( VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = mul(matWorldViewProj, input.position);
	output.normalW = mul(matWorld, input.normal);

	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	float l = dot(input.normalW, lightDir.xyz);
	return float4(l, l, l, 1);
}