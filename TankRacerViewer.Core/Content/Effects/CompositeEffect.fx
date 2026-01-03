#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D DepthTexture;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : SV_Target
{
    float depth = tex2D(DepthTexture, input.Uv).r;
	if (depth <= 0)
        discard;
	
    return float4(depth, 0, 0, 0);
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};