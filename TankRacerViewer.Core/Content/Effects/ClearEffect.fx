#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float DepthValue;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

struct FragmentShaderOuput
{
    float4 Color : SV_Target0;
    float4 Depth : SV_Target1;
};

FragmentShaderOuput MainPS(VertexShaderOutput input)
{
    FragmentShaderOuput output;
    output.Color = input.Color;
    output.Depth = DepthValue;
	
    return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};