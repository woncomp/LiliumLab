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

SamplerState s;
Texture2D baseMap;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = input.position;
	output.texCoord = input.texCoord;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return baseMap.Sample(s, input.texCoord);
}