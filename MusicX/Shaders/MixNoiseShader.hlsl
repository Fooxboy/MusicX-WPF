sampler2D iChannel0 : register(s0);
float iTime : register(C0);
float x_size : register(C1);
float y_size : register(C2);

float2x2 Rot(float a)
{
    float s = sin(a);
    float c = cos(a);
    return float2x2(c, -s, s, c);
}


// Created by inigo quilez - iq/2014
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
float2 hash( float2 p )
{
    p = float2( dot(p,float2(2127.1,81.17)), dot(p,float2(1269.5,283.37)) );
    return frac(sin(p)*43758.5453);
}

float noise( in float2 p )
{
    float2 i = floor( p );
    float2 f = frac( p );
	
    float2 u = f*f*(3.0-2.0*f);

    float n = lerp( lerp( dot( -1.0+2.0*hash( i + float2(0.0,0.0) ), f - float2(0.0,0.0) ), 
                        dot( -1.0+2.0*hash( i + float2(1.0,0.0) ), f - float2(1.0,0.0) ), u.x),
                   lerp( dot( -1.0+2.0*hash( i + float2(0.0,1.0) ), f - float2(0.0,1.0) ), 
                        dot( -1.0+2.0*hash( i + float2(1.0,1.0) ), f - float2(1.0,1.0) ), u.x), u.y);
    return 0.5 + 0.5*n;
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    const float4 tex_col = tex2D(iChannel0, uv);
    if (tex_col.a == 0.) return float4(0,0,0,0);

    float ratio = x_size / y_size;

    float2 tuv = uv;
    tuv -= .5;

    // rotate with Noise
    float degree = noise(float2(iTime*.1, tuv.x*tuv.y));

    tuv.y *= 1./ratio;
    tuv = mul(Rot(radians((degree-.5)*720.+180.)), tuv);
    tuv.y *= ratio;

    
    // Wave warp with sin
    float frequency = 5.;
    float amplitude = 30.;
    float speed = iTime * 2.;
    tuv.x += sin(tuv.y*frequency+speed)/amplitude;
    tuv.y += sin(tuv.x*frequency*1.5+speed)/(amplitude*.5);
    
    float3 color1 = float3(0.902, 0.278, 1);
    float3 color2 = float3(0.596, 0.278, 1);
    float3 color3 = float3(0.278, 0.278, 1);
    float3 color4 = float3(0.350, .71, .953);
    
    float2 rot = mul(Rot(radians(-5.)), tuv);
    float3 step = float3(smoothstep(-.3, .2, rot.x), smoothstep(-.3, .2, rot.x), smoothstep(-.3, .2, rot.x));
    float3 layer1 = lerp(color1, color2, step);
    
    float3 layer2 = lerp(color3, color4, step);
    
    float3 finalComp = lerp(layer1, layer2, smoothstep(.5, -.3, tuv.y));
    
    float3 col = finalComp;
    
    return float4(tex_col.rgb * col,tex_col.a);
}