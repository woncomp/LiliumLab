#include <LiliumPostprocess.hlsl>

SamplerState s;
Texture2D texLinearDepth;
Texture2D texNormalV;

float4 PS(PS_IN input) : SV_Target
{
	return texLinearDepth.Sample(s, input.texCoord);
}