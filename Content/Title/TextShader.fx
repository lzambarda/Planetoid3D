texture text;
texture background;
float time;

sampler TextSampler = sampler_state
{
    Texture = (text);
};

sampler BackSampler = sampler_state
{
    Texture = (background);
};
 
//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point
float4 PixelShaderFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color=tex2D(TextSampler, TextureCoordinate);

	float d=distance(color,float4(1,1,1,1));
	if (d<0.8)
	{
		TextureCoordinate.x+=sin(time+TextureCoordinate.y+(time*TextureCoordinate.x))/50;
	    TextureCoordinate.y+=cos(time-TextureCoordinate.x+(time*TextureCoordinate.y))/50;

		color=lerp(color,tex2D(BackSampler, TextureCoordinate),(0.8-d)*2);
		color-=0.1;
	}

    return color;
}
 
//-------------------------- TECHNIQUES ----------------------------------------
technique Text
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}