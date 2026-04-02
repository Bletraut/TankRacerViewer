float4 HighlightColor;

float4 Highlight(float4 baseColor, float4 highlightColor)
{
    float grayScale = dot(baseColor.rgb, float3(0.299, 0.587, 0.114));
    float3 highlighted = lerp(highlightColor.rgb, 1, grayScale);
    float3 color = lerp(baseColor.rgb, highlighted, 1 - highlightColor.a);

    return float4(color, baseColor.a);
}