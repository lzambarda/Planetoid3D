// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightDir;

//The texture for the change
Texture2D ChangeTexture;
float ChangeValue;

// The main texture applied to the object
Texture2D Texture;

//The normal texture
//texture NormalTex;

//The blend color
float4 Color;

bool Sun;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler SamplerB = sampler_state
{
    Texture = (ChangeTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Wrap;
    AddressV = Wrap;
};

/*sampler SamplerC = sampler_state
{
    Texture = (NormalTex);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};*/


// Vertex shader input structure.
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TextureCoordinate : TEXCOORD0;
};

//Structures for the planet pass
struct PlanetVertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
	float3 SurfNorm : TEXCOORD1;
};
struct PlanetPixelShaderInput
{
	float3 Normal : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
	float3 SurfNorm : TEXCOORD1;
};

//PLANET
PlanetVertexShaderOutput PlanetVertexShader(VertexShaderInput input)
{
    PlanetVertexShaderOutput output;
	/// Apply camera matrices to the input position.
    output.Position = mul(mul(mul(input.Position/*+normalize(input.Position)*/, World), View), Projection);
	output.Normal=dot(LightDir,normalize(mul(input.Normal,World)));
	output.SurfNorm = input.Normal;
    // Copy across the input texture coordinate.
    output.TextureCoordinate = input.TextureCoordinate;    
    return output;
}

float4 PlanetPixelShader(PlanetPixelShaderInput input) : SV_Target
{
	//The resulting texture will be a mix of the textures
	float4 	color = lerp(
			Texture.Sample(Sampler, input.TextureCoordinate),
			ChangeTexture.Sample(SamplerB, input.TextureCoordinate),
			ChangeValue);
			
	color*=lerp(Color,1,ChangeValue);

	/*color.r = round(color.r*12)/12;
	color.g = round(color.g*12)/12;
	color.b = round(color.b*12)/12;*/


	//Add shadow
	if (!Sun)
	{
		//float3 N = (2.0 * (tex2D(SamplerC, input.TextureCoordinate))) - 1.0;
		//color.rgb *= clamp(dot(N,input.SurfNorm),0.5f,1);
		color.rgb*=clamp(1-input.Normal,0.3f,1);
	}

    return color;
}

technique Planet
{
    pass TexturePass
    {
        VertexShader = compile vs_3_0 PlanetVertexShader();
        PixelShader = compile ps_3_0 PlanetPixelShader();
    }
}

