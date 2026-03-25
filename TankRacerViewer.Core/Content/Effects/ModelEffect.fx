#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float4 HighlightColor;

sampler2D BaseColorSampler;
sampler2D OpaqueDepthSampler = sampler_state
{
    Filter = Point;
};
sampler2D TransparentDepthSampler = sampler_state
{
    Filter = Point;
};

matrix ModelMatrix;
matrix ViewProjectionMatrix;
matrix ModelViewProjectionMatrix;

float CameraYaw;
float AlphaThreshold;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
    float3 Center : TEXCOORD1;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
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
    VertexShaderOutput output = (VertexShaderOutput) 0;
	
    bool isBilliboard = input.Position.w > 0;
    float4 position = float4(input.Position.xyz, 1);
    
    float4 clipSpacePosition;
    if (isBilliboard)
    {
        float3 worldPosition = mul(position, ModelMatrix).xyz;
        float3 worldCenterPosition = mul(float4(input.Center, 1), ModelMatrix).xyz;
        float3 localPosition = worldPosition - worldCenterPosition;
        
        float cosinus = cos(-CameraYaw);
        float sinus = sin(-CameraYaw);
        float3x3 rotationMatrix = float3x3(cosinus, 0, sinus, 0, 1, 0, -sinus, 0, cosinus);
        
        float4 rotatedPosition = float4(worldCenterPosition + mul(localPosition, rotationMatrix), 1);
        
        clipSpacePosition = mul(rotatedPosition, ViewProjectionMatrix);
    }
    else
    {
        clipSpacePosition = mul(position, ModelViewProjectionMatrix);
    }
    
    output.Position = clipSpacePosition;
    output.Color = input.Color;
    output.Uv = input.Uv;
    output.ClipSpacePosition = clipSpacePosition;
	
    return output;
}

float3 BlendHardLight(float3 baseColor, float3 blendColor)
{
    float3 a = 2 * baseColor * blendColor;
    float3 b = 1 - 2 * (1 - baseColor) * (1 - blendColor);
	
    return lerp(a, b, step(0.5, blendColor));
}

float4 Highlight(float4 baseColor, float4 highlightColor)
{
    float3 color = pow(baseColor.rgb / highlightColor.rgb, highlightColor.a);
    return float4(color, baseColor.a);
}

FragmentShaderOuput OpaquePS(VertexShaderOutput input)
{
    float4 baseColor = tex2D(BaseColorSampler, input.Uv.xy);
    if (baseColor.a < AlphaThreshold)
        discard;
	
    float3 hardLightColor = BlendHardLight(baseColor.rgb, input.Color.rgb);
    float4 resultColor = float4(hardLightColor, baseColor.a);
    
    float depth = input.ClipSpacePosition.z / input.ClipSpacePosition.w;
	
    FragmentShaderOuput output;
    output.Color = Highlight(resultColor, HighlightColor);
    output.Depth = float4(depth, 0, 0, 0);
    
    return output;
}

FragmentShaderOuput TransparentPS(VertexShaderOutput input)
{
    float4 baseColor = tex2D(BaseColorSampler, input.Uv.xy);
    if (baseColor.a < AlphaThreshold)
        discard;
    
    float3 projectedPosition = input.ClipSpacePosition.xyz / input.ClipSpacePosition.w;
    float2 depthUv = projectedPosition.xy * 0.5 + 0.5;
    depthUv.y = 1 - depthUv.y;
    
    float opaqueDepth = tex2D(OpaqueDepthSampler, depthUv).r;
    if (projectedPosition.z >= opaqueDepth)
        discard;
    
    FragmentShaderOuput output;
    
    float transparentDepth = tex2D(TransparentDepthSampler, depthUv).r;
    if (projectedPosition.z <= transparentDepth)
        discard;
	
    float3 hardLightColor = BlendHardLight(baseColor.rgb, input.Color.rgb);
    float4 resultColor = float4(hardLightColor, baseColor.a);
    
    output.Color = Highlight(resultColor, HighlightColor);
    output.Depth = float4(projectedPosition.z, 0, 0, 0);
    
    return output;
}

technique SpriteDrawing
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL OpaquePS();
    }
    pass P1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL TransparentPS();
    }
};