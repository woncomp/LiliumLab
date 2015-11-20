#pragma pack_matrix( row_major )

cbuffer data : register(b0)
{
	float4x4 matViewProj;
};

struct VS_IN
{
	float4 position : POSITION;
	float4 color : COLOR;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PS_IN VS(VS_IN i)
{
	PS_IN o = (PS_IN)0;

	o.position = mul(i.position, matViewProj);
	o.color = i.color;

	return o;
}

float4 PS(PS_IN i) : SV_Target
{
	return i.color;
}