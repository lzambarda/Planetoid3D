//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightColor = 1.25;
float3 MaterialColor = 0.25;


Texture2D Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};


struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // Compute lighting, using a simple Lambert model.
    float3 worldNormal = mul(input.Normal, instanceTransform);
  
    float diffuseAmount = max(-dot(worldNormal, normalize(worldPosition)),0)/(length(worldPosition)/500);
    
    float3 lightingResult = saturate(diffuseAmount * LightColor + MaterialColor);
    
    output.Color = float4(lightingResult, 1);

    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
    return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}

// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	float4 color=Texture.Sample(Sampler, input.TextureCoordinate) * input.Color;

	color.r = round(color.r*6)/6;
	color.g = round(color.g*6)/6;
	color.b = round(color.b*6)/6;
    return color;
}


// Hardware instancing technique.
technique HardwareInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
