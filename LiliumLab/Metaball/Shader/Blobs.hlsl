
static const float GAUSSIANTEXSIZE = 64;

struct VS_IN
{
	float4 position : POSITION;
	float2 curr : TEXCOORD0;
	float2 back : TEXCOORD1;
	float3 color : COLOR0;
	float size : COLOR1;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float2 curr : TEXCOORD0;
	float2 back : TEXCOORD1;
	float3 color : COLOR0;
	float size : COLOR1;
};

struct PS_OUT
{
	float4 normal : SV_Target0;
	float4 color : SV_Target1;
};

SamplerState s;
Texture2D texGaussian;
Texture2D texNormal;
Texture2D texColor;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = input.position;
	output.curr = input.curr;
	output.back = input.back;
	output.color = input.color;
	output.size = input.size;

	return output;
}

float SampleGaussianLinear(float2 texCoord)
{
	float2 pixelpos = texCoord * GAUSSIANTEXSIZE;

	float2 lerps = frac(pixelpos);
	float psz = 1.0 / GAUSSIANTEXSIZE;//pixel size in uv space

	float3 origin = float3((pixelpos - lerps/GAUSSIANTEXSIZE ) / GAUSSIANTEXSIZE, 0);

	float2 sourcevals[2];
	sourcevals[0].x = texGaussian.Sample(s, origin).x;
	sourcevals[1].x = texGaussian.Sample(s, origin + float3(psz, 0, 0)).x;

	origin += float3(0, psz, 0);
	sourcevals[0].y = texGaussian.Sample(s, origin).x;
	sourcevals[1].y = texGaussian.Sample(s, origin + float3(psz, 0, 0)).x;

	float2 row = lerp(sourcevals[0], sourcevals[1], lerps.x);
	return lerp(row[0], row[1], lerps.y);
}

PS_OUT PS(PS_IN input)
{
	PS_OUT output = (PS_OUT)0;

	float weight = SampleGaussianLinear(input.curr);

    // Get the old data
	float4 oldNormalData = texNormal.Sample(s, input.back);
	float4 oldColorData = texColor.Sample(s, input.back);

	// Generate new surface data
	float4 newNormalData = float4((input.curr.x-0.5) * input.size, (input.curr.y-0.5) * input.size, 0, 1) * weight;
	float4 newColorData = float4(input.color, 1) * weight;

	output.normal = oldNormalData + newNormalData;
	output.color = oldColorData + newColorData;

	return output;
}