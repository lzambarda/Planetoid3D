struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float4 xFilter;
float xTime;

//------- Texture Samplers --------

Texture2D xTexture;
sampler TextureSampler = sampler_state 
{
	texture = <xTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter= LINEAR;
	//AddressU = clamp;
	//AddressV = clamp;
};

//------- Technique: Atmosphere --------

VertexToPixel AtmosphereVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame AtmospherePS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	return Output;
}

technique Atmosphere
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 AtmosphereVS();
		PixelShader  = compile ps_3_0 AtmospherePS();
	}
}

//------- Technique: TexturedNoShading --------

VertexToPixel TexturedNoShadingVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;    
	Output.Position = mul(inPos, mul (xWorld, mul (xView, xProjection)));	
	Output.TextureCoords = inTexCoords;
    
	return Output;    
}

PixelToFrame TexturedNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = Texture.Sample(TextureSampler, PSIn.TextureCoords);

	return Output;
}

technique TexturedNoShading
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedNoShadingVS();
		PixelShader  = compile ps_3_0 TexturedNoShadingPS();
	}
}

//------- Technique: Ray --------

VertexToPixel RayVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;    
	Output.Position = mul(inPos, mul (xWorld, mul (xView, xProjection)));	
	Output.TextureCoords = inTexCoords;
    
	return Output;    
}

PixelToFrame RayPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords + float2(xTime,0) );

	Output.Color = saturate(Output.Color*xFilter);
	//Output.Color.a=0.5;
	return Output;
}

technique Ray
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 RayVS();
		PixelShader  = compile ps_3_0 RayPS();

        //CullMode = None;  

		//ZEnable = false;  
        //ZWriteEnable = false;  
        //AlphaBlendEnable = true;  
	}
}