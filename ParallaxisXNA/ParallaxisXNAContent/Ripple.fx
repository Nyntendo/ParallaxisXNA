uniform extern texture ScreenTexture;	

texture RippleTexture;

sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;
	AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler RippleS = sampler_state
{
	Texture = <RippleTexture>;
	AddressU  = CLAMP;
    AddressV  = CLAMP;
};


float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR
{
	float4 rippleColor = tex2D(RippleS, texCoord);

	float offset = rippleColor.r;
	
	float4 color = tex2D(ScreenS, texCoord+offset);	
			
	return color;
}

technique Technique1  
{  
    pass Pass1  
    {  
        PixelShader = compile ps_2_0 PixelShaderFunction();  
    }  
} 
