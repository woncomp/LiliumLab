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

Texture2D baseMap;
SamplerState s;

PS_IN VS( VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.position = float4(input.position.xy, 0.5, 1);
	output.texCoord = input.texCoord;

	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return baseMap.Sample(s, input.texCoord);
}