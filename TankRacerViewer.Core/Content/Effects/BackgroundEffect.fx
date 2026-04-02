#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#include "Common.fx" 

sampler2D BaseColorSampler;

matrix ModelMatrix;

float CameraYaw;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
};

struct FragmentShaderOuput
{
    float4 Color : SV_Target0;
    float4 Depth : SV_Target1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(float4(input.Position.xyz, 1), ModelMatrix);
    output.Color = input.Color;
    output.Uv = input.Uv;

	return output;
}

FragmentShaderOuput MainPS(VertexShaderOutput input)
{
    float4 baseColor = tex2D(BaseColorSampler, input.Uv.xy);
    float4 resultColor = float4(baseColor.rgb * input.Color.rgb, baseColor.a);
    
    FragmentShaderOuput output;
    output.Color = Highlight(resultColor, HighlightColor);
    output.Depth = 0;
	
    return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};