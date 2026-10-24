float4x4 World;
float4x4 View;
float4x4 Projection;

// Size of a pixel in clip space: (2 / viewport width, 2 / viewport height)
float2 PixelSize;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color: COLOR0;
	// Corner of the star quad (-1 or +1 on each axis) multiplied by the star radius in pixels
	float2 Corner : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color: COLOR0;
	// xy: position inside the star (-1 to +1), z: star radius in pixels
	float3 Disc : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    // Expand the quad in screen space, so that stars keep the same size in pixels
    // instead of shrinking below a pixel (and flickering) because of their distance
    output.Position.xy += input.Corner * PixelSize * output.Position.w;
	output.Color = input.Color;
	output.Disc = float3(sign(input.Corner), abs(input.Corner.x));

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	// Round star with a one pixel soft edge, so that it is antialiased
	float coverage = saturate((1 - length(input.Disc.xy)) * input.Disc.z);
    return input.Color * coverage;
}

technique StarField
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
