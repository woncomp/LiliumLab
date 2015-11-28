#include <LiliumPostprocess.hlsl>

static const int IT_COUNT = 4;
static const float Offsets[4] = { -1.5, -0.5, 0.5, 1.5 };

SamplerState s;
Texture2D tex;

float4 PS(PS_IN input) : SV_Target
{
    float4 color = 0;

    for (int i = 0 ; i < IT_COUNT ; i++) {
        for (int j = 0 ; j < IT_COUNT ; j++) {
            float2 texCoord = input.texCoord;
            texCoord.x = texCoord.x + Offsets[j] / renderTargetWidth;
            texCoord.y = texCoord.y + Offsets[i] / renderTargetHeight;
            color += tex.Sample(s, texCoord);
        }
    }

    color = color / IT_COUNT / IT_COUNT;
    color.a = 1;
	return color;
}