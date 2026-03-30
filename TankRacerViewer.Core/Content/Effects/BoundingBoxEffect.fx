#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#define Bias 0.001
#define Thickness 3.85

float4 Color;
matrix ModelViewProjectionMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 Uv : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 Uv : TEXCOORD0;
    float4 ClipSpacePosition : TEXCOORD1;
};

struct FragmentShaderOuput
{
    float4 Color : SV_Target0;
    float4 Depth : SV_Target1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{	
    float4 position = mul(input.Position, ModelViewProjectionMatrix);
    position.z -= Bias;
    
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = position;
    output.Uv = input.Uv;
    output.ClipSpacePosition = position;
	
    return output;
}

FragmentShaderOuput MainPS(VertexShaderOutput input)
{
    float2 uv = 1 - abs(input.Uv * 2 - 1);
    
    float2 delta = fwidth(uv);
    float2 lineThickness = 1 - step(delta * Thickness, uv);
    float maxLineThickness = max(lineThickness.x, lineThickness.y);
    
    if (maxLineThickness <= 0)
        discard;
    
    float depth = input.ClipSpacePosition.z / input.ClipSpacePosition.w;
	
    FragmentShaderOuput output;
    output.Color = Color;
    output.Depth = float4(depth, 0, 0, 0);
    
    return output;
}

technique BoundingBoxDrawing
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};