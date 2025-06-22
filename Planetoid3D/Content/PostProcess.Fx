//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
Texture2D ScreenTexture;

float Amount=0;
float4 Color=0;
bool Fade;

float CinemaAmount;

float cameraSpeed;
float2 cameraTarget;
 
// Our sampler for the texture, which is just going to be pretty simple
sampler TextureSampler = sampler_state
{
    Texture = (ScreenTexture);
};

float radius;

float4 BlurFadeCinematic(float2 TextureCoordinate : TEXCOORD0) : SV_Target
{
	float4 color=ScreenTexture.Sample(TextureSampler, TextureCoordinate);
	if (cameraSpeed>0)
	{
		float BlurStart = 1.0;
		float BlurWidth = -cameraSpeed/300;
		float samples   = 10;
   
		for(int i = 0; i < samples; i++)
		{
			float scale = BlurStart + BlurWidth * (i / (samples - 1));
			color += tex2D(TextureSampler, (TextureCoordinate -cameraTarget) * scale + cameraTarget);
		}
			color /= samples;
	}

	if (Fade)
	{
		color=lerp(color,Color,min(1,Amount));
	}

	if (CinemaAmount>0)
	{
		color=lerp(color,float4(0,0,0,1),abs(0.5-TextureCoordinate.y)*(4*CinemaAmount));
	}

	
    return color;
}
 
//-------------------------- TECHNIQUES ----------------------------------------
technique Post
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 BlurFadeCinematic();
    }
}