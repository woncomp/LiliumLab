cbuffer Lilium : register(b7)
{
	float renderTargetWidth;
	float renderTargetHeight;
};

struct VS_IN
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

PS_IN VS( VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = float4(input.position.xy, 0.5, 1);
	output.texCoord = input.texCoord;

	return output;
}