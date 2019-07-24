#include "common.inc" 
#include "materials.inc"

#define SAMPLE_COUNT 15

Texture2D colorMap;
Texture2D positionMap;
Texture2D glowMap;

float4 SampleVals1[SAMPLE_COUNT]; //.xy = SampleOffsets .z = SampleWeights
float4 SampleVals2[SAMPLE_COUNT]; //.xy = SampleOffsets .z = SampleWeights

float BloomThreshold;
float BloomIntensity;
float BaseIntensity;
float BloomSaturation;
float BaseSaturation;

struct VSIn {
    uint VertexID: SV_VertexID;
};
struct PsIn {
	float4 position: SV_Position;	
	float2 texCoord: TEXCOORD1;	
};

SamplerState GlowSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
	AddressW = CLAMP;
};

PsIn VS(VSIn In)
{
    PsIn Out;

	// Produce a fullscreen triangle
	float4 position;
	position.x = (In.VertexID == 2)?  3.0 : -1.0;
	position.y = (In.VertexID == 0)? -3.0 :  1.0;
	position.zw = 1.0;

	Out.position = position;
	Out.texCoord = position.xy * float2(0.5, -0.5) + 0.5;
	
	return Out;
}

float4 PSBlur(PsIn In) : SV_Target0
{    		
	float4 c = 0;    
	
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		c += glowMap.Sample(GlowSampler, In.texCoord + SampleVals1[i].xy) * SampleVals1[i].z;							
	}				
	for (int x = 0; x < SAMPLE_COUNT; x++)
	{
		c += glowMap.Sample(GlowSampler, In.texCoord + SampleVals2[x].xy) * SampleVals2[x].z;							
	}			
		    
	//c.a = 1;	
	return c;		

}

float4 PSBloomExtract(PsIn In) : SV_Target0
{ 
	int3 coords = int3(int2(In.position.xy), 0);
	float4 c	=  glowMap.Sample(GlowSampler, In.texCoord);
	c.a = 1;	
	return saturate((c - BloomThreshold) / (1 - BloomThreshold));
	
}

float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}
float4 PSFinal(PsIn In) : SV_Target0
{   	
	// Look up the bloom and original base image colors.
    float4 bloom = glowMap.Sample(GlowSampler, In.texCoord);
	return bloom * BloomIntensity;
    float4 base = colorMap.Sample(GlowSampler, In.texCoord);
    
    // Adjust color saturation and intensity.
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    base *= (1 - saturate(bloom));
    
    // Combine the two images.	
    return base + bloom;

}



BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};
DepthStencilState NoDepth
{
    DepthEnable = FALSE;   
    DEPTHWRITEMASK = ZERO; 
};
RasterizerState RS_BackCulling
{
    CULLMODE = BACK;	
};
BlendState CorrectBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
}; 


technique10 RenderBloomExtract
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSBloomExtract() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( NoDepth, 0 );        
        SetRasterizerState( RS_BackCulling );
    }  
}
technique10 RenderBlur
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSBlur() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( NoDepth, 0 );        
        SetRasterizerState( RS_BackCulling );
    }  
}

technique10 RenderFinal
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSFinal() ) );
        
        SetBlendState( BS_Blending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( NoDepth, 0 );        
        SetRasterizerState( RS_BackCulling );
    }  
}



