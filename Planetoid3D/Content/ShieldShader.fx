struct VertexToPixel
{
    float4 Position   	: POSITION;
	float4 Color		: COLOR0;    
	float2 Distance: COLOR1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
//float3 xLightDir;
float3 sourcePosition;
float3 hitPosition;
float hitTimer;
float timer;
float presence;
//float4 color;

//------- Technique: Surface --------

VertexToPixel ShieldVS(float4 inPos : POSITION0,float3 inNormal: NORMAL0,float2 TextureCoordinate : TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;

	Output.Color=float4(0.039,0.584,0.929,0.1);
	//Output.Color=color;
	//Output.Color.a=0.1;

	Output.Distance.x=distance(hitPosition,normalize(inPos));
	Output.Distance.y=distance(sourcePosition,normalize(inPos))/10;

	float hitModifier=abs(min(Output.Distance.x*1.43-1,0)*hitTimer*cos(hitTimer*Output.Distance.x*5))/2;

	float sourceModifier=abs(sin(timer/2-Output.Distance.y*100))/20;

	inPos.xyz*=(1-hitModifier)*(1-sourceModifier);
	Output.Position =mul(inPos,mul(xWorld,mul(xView,xProjection)));

	return Output;    
}

PixelToFrame ShieldPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		

	Output.Color=PSIn.Color;

	if (PSIn.Distance.y<=0.1)
	{
			Output.Color=lerp(float4(0.11,0.56,1,0.5),PSIn.Color,PSIn.Distance.y*10);
	}

	if (PSIn.Distance.x<=0.7)
	{
			float amount=2*(0.7-PSIn.Distance.x)*hitTimer*abs(sin(hitTimer*PSIn.Distance.x*20));
			Output.Color=lerp(Output.Color,float4(1,0,0,1),amount);
			//Output.Color=lerp(PSIn.Color,float4(1-color.r,1-color.g,1-color.b,1),amount);
	}

	Output.Color.a=max(Output.Color.a*(1+sin(timer/2-PSIn.Distance.y*100)),0.1);
	Output.Color.a*=clamp(20*(0.5+abs(sin(timer/2-PSIn.Distance.y*100))/2)*(presence-PSIn.Distance.y),0,1);
	return Output;
}

technique Shield
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 ShieldVS();
		PixelShader  = compile ps_3_0 ShieldPS();

		
        // We're drawing the inside of a model
        CullMode = None;  
        // We don't want it to obscure objects with a Z < 1
        ZWriteEnable = false;
	}
}