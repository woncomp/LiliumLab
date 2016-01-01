#include <LiliumPostprocess.hlsl>

cbuffer _data
{
	float Threshold = 0.5;
	float UVScale = 1;
	float RimPower = 1;
	float RimIntensity = 1;
};

SamplerState s;
Texture2D texMeta;
TextureCube texCube;

float4 PS(PS_IN input) : SV_Target
{
	float4 meta = texMeta.Sample(s, input.texCoord);

	if(meta.a < Threshold) discard;

	meta /= meta.a;

	float2 uv = meta.xy;
	meta.z  = sqrt(saturate(1.0 - dot(uv, uv)));
	float3 normal = normalize(meta.xyz);

	float4 color = texCube.Sample(s, reflect(float3(0,0,1), normal));

	float fresnel = 1 - saturate(dot(float3(0,0,1), normal));
	float4 rim = float4(0.2, 0.2, 1, 1) * pow(fresnel, RimPower) * RimIntensity;
	
	color.xyz += rim.xyz;

	return color;
}