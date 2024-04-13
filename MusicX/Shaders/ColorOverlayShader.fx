sampler2D input : register(s0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);
    
    if (color.a > 0.0)
        color.rgb = (1.*color.a) - color.rgb;
        
    return color;
}
