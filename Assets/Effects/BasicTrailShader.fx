sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
matrix uWorldViewProjection;
float4 uShaderSpecificData;

float pi = 3.141f;
float tau = 6.283f;
float noiseScrollRate = 2.45f;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float3 coords = input.TextureCoordinates; //x, y and z
    float2 coord = input.TextureCoordinates; //only x and y
    
    coord.y = ((coord.y - 0.5) / coords.z) + 0.5;
    
    float brightness = pow(sin(coord.y * pi), 5.6);
    float noise = tex2D(uImage1, coord * 3 - float2(uTime * noiseScrollRate, 0));
    float brightnessStreak = tex2D(uImage2, coord * float2(2, 1) - float2(uTime * ((5 * pi) / 9.7), 0)) + noise * brightness;
    
    float3 noiseColor = lerp(uColor, uSecondaryColor, noise); //xyz
    
    float4 finalColor = float4(noiseColor, 1);
    
    return (finalColor * brightness + brightnessStreak * brightness) * color.a * pow(1 - coords.x, ((5 * pi) / 9.7));
}


technique Technique1
{
    pass TrailPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}