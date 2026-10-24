// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightDir;

// The main texture applied to the object
//uniform const texture Texture;
Texture2D Texture;

//The blend color
float4 Color;

//The target color to replace
float4 Target=float4(1,0,0,1);

sampler Sampler  = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;//Lower anti-aliasing but at least the replace works!
    MagFilter = Linear;//Linear
    MipFilter = Linear;//Linear
    
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct BuildingVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
	float3 Normal: COLOR0;
};
struct BuildingPixelShaderInput
{
	float3 Normal : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

//Building
BuildingVertexShaderOutput BuildingVertexShader(VertexShaderInput input)
{
    BuildingVertexShaderOutput output;

    // Apply camera matrices to the input position.
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    
    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;

	output.Normal=dot(LightDir,normalize(mul(input.Normal,World)));

    
    return output;
}
float4 BuildingPixelShader(BuildingPixelShaderInput input) : SV_Target
{
	//Pick up the color
    //float4 color = tex2D(Sampler, input.TextureCoordinate);
    float4 color = Texture.Sample(Sampler, input.TextureCoordinate);

	//Replace it if it is the target one
	if (distance(color,Target)<0.7)
	{
		color=Color;
	}

	color.rgb*=clamp(1-input.Normal,0.5f,1);
	
	color.r = round(color.r*10)/10;
	color.g = round(color.g*10)/10;
	color.b = round(color.b*10)/10;

    return color;
}

technique Building
{
    pass TexturePass
    {
        VertexShader = compile vs_3_0 BuildingVertexShader();
        PixelShader = compile ps_3_0 BuildingPixelShader();
    }
}