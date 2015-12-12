#include <LiliumPostprocess.hlsl>

SamplerState s;
Texture2D texNormal;
Texture2D texColor;
TextureCube texCube;

float4 PS(PS_IN input) : SV_Target
{
	float4 blob = texNormal.Sample(s, input.texCoord);
	float4 color = texColor.Sample(s, input.texCoord);

	if(color.w > 0.015)
	{
		color /= blob.w;
		float3 normal = float3(blob.x / blob.w, blob.y / blob.w, blob.w - 0.015);
		normal = normalize(normal);
		float3 lightDir = normalize(float3(1, 1, 1));
		color *= saturate(dot(normal, lightDir));
		//color = texCube.Sample(s, normal);
	}
	else if(color.w > 0.01)
	{
		color = float4(0.3, 0.3, 1, 1);
	}
	else
	{
		discard;
	}

	return color;
}