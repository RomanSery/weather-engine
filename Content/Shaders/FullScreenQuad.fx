#include "common.inc" 

struct VS_IN
{
	uint VertexID: SV_VertexID;		
};
struct PS_IN
{
	float4 position: SV_Position;
	float2 texCoord: TexCoord;
};

SamplerState GlowSampler
{
    Filter = ANISOTROPIC;
    AddressU = CLAMP;
    AddressV = CLAMP;
	AddressW = CLAMP;
};

PS_IN VS( VS_IN In )
{
	PS_IN output;
	
	// Produce a fullscreen triangle
	float4 position;
	position.x = (In.VertexID == 2)?  3.0 : -1.0;
	position.y = (In.VertexID == 0)? -3.0 :  1.0;
	position.zw = 1.0;

	output.position = position;
	output.texCoord = position.xy * float2(0.5, -0.5) + 0.5;
	
	
	return output;
}

float4 PS( PS_IN In ) : SV_Target0
{
	float4 diffColor = diffTexture.Sample( GlowSampler, In.texCoord );   
	diffColor.a = 1;
	return diffColor;
}

technique10 FullScreen
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );		
		
		SetRasterizerState( RS_NormalSolid );		
		SetDepthStencilState( DS_NoDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}