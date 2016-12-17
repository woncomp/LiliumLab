#include <Lilium.hlsl>

cbuffer data
{
	float4 SomeColor = float4(1, 0, 0, 1);
	float TessFactor = 8;
	float3 ___;
};

struct HS_IN
{
	float4 position : POSITION;
	float3 normalW : NORMAL;
	float2 texCoord : TEXCOORD;
};

struct HULL_CONSTANT
{
	float edge[4] : SV_TessFactor;
	float inside[2] : SV_InsideTessFactor;

	float3 TextureUV[16] : TEXCOORD;
};

struct DS_IN
{
	float4 position : POSITION;
	float3 normalW : NORMAL;
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

HS_IN VS(VS_IN input)
{
	HS_IN output = (HS_IN)0;
	// float4 posW;
	// float4 posV;

	// posW = mul(matWorld, input.position);
	// posV = mul(matView, posW);

	// output.position = mul(matProjection, posV);

	// output.normalW = mul(matWorldInverseTranspose, input.normal);
	// output.texCoord = input.texCoord;

	output.position = input.position;
	output.normalW = mul(matWorldInverseTranspose, input.normal);
	output.texCoord = input.texCoord;

	return output;
}

HULL_CONSTANT HS_C(InputPatch<HS_IN, 16> patch, uint patchID : SV_PrimitiveID)
{
	HULL_CONSTANT output = (HULL_CONSTANT)0;

	[unroll]
	for(int i=0;i<4;++i) output.edge[i] = TessFactor;
	[unroll]
	for(int i=0;i<2;++i) output.inside[i] = TessFactor;

	[unroll]
	for(int i=0;i<16;++i)
	{
		output.TextureUV[i].xy = patch[i].texCoord;
	}

	return output;
}

[domain("quad")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(16)]
[patchconstantfunc("HS_C")]
DS_IN HS(InputPatch<HS_IN, 16> patch, uint outputID : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
	DS_IN output = (DS_IN)0;

	output.position = patch[outputID].position;
	output.normalW = patch[outputID].normalW;
	output.texCoord = patch[outputID].texCoord;

	return output;
}

void DeCasteljau(float u, float3 p0, float3 p1, float3 p2, float3 p3, out float3 p, out float3 t)
{
	float3 q0 = lerp(p0, p1, u);
	float3 q1 = lerp(p1, p2, u);
	float3 q2 = lerp(p2, p3, u);
	float3 r0 = lerp(q0, q1, u);
	float3 r1 = lerp(q1, q2, u);
	t = r0 - r1;
	p = lerp(r0, r1, u);
}

void DeCasteljauBicubic(float2 uv, float3 p[16], out float3 result, out float3 normal)
{
	float3 p0, p1, p2, p3;
	float3 t0, t1, t2, t3;

	DeCasteljau(uv.x, p[ 0], p[ 1], p[ 2], p[ 3], p0, t0);
	DeCasteljau(uv.x, p[ 4], p[ 5], p[ 6], p[ 7], p1, t1);
	DeCasteljau(uv.x, p[ 8], p[ 9], p[10], p[11], p2, t2);
	DeCasteljau(uv.x, p[12], p[13], p[14], p[15], p3, t3);

	float3 du, dv, tmp;
	DeCasteljau(uv.y, p0, p1, p2, p3, result, dv);
	DeCasteljau(uv.y, t0, t1, t2, t3, du, tmp);

	normal = normalize(cross(du, dv));
}

[domain("quad")]
PS_IN DS(HULL_CONSTANT c, const OutputPatch<DS_IN, 16> patch, float2 uv : SV_DomainLocation)
{
	PS_IN output = (PS_IN)0;

	float3 p[16];

	[unroll]
	for(int i=0; i<16; ++i)
	{
		p[i] = patch[i].position;
	}

	float3 position, normal;

	DeCasteljauBicubic(uv, p, position, normal);

	float3 texCoord, dummy;
	DeCasteljauBicubic(uv, c.TextureUV, texCoord, dummy);

	output.position = mul(matWorldViewProjection, float4(position, 1));
	output.normalW = mul(matWorldInverseTranspose, normal);
	output.texCoord = texCoord;

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

	return baseMapValue * color;
}