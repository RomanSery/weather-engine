#include "common.inc"

float4 color;

struct VS_IN
{
	float4 Pos : POSITION;		
};
struct PS_IN
{
	float4 Pos : SV_POSITION;			
};


PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;	
	output.Pos = mul( input.Pos, WorldViewProj );    
    return output;
}
float4 PS( PS_IN input ) : SV_Target
{	
	return color;	
}

RasterizerState RS_Grid
{
    CULLMODE = BACK;
	FILLMODE = SOLID;	
	AntialiasedLineEnable = True;		
};

BlendState BS_Grid
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = SRC_ALPHA;
    DestBlendAlpha = INV_SRC_ALPHA;
    BlendOpAlpha = ADD;
    BlendEnable[0] = TRUE;
};


technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );		
		
		SetRasterizerState( RS_Grid );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_Grid, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}
}