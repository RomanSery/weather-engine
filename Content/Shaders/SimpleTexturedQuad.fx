#include "common.inc" 



struct VS_IN
{
	float4 position : POSITION;	
	float3 normal : NORMAL;
	float2 texCoord: TEXCOORD0;		
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct GSPS_INPUT
{
	float4 position : SV_POSITION;		
	float3 normal : TEXCOORD0;
	float2 texCoord: TEXCOORD1;	
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};




GSPS_INPUT VS( VS_IN input )
{
	GSPS_INPUT output = (GSPS_INPUT)0;
	
	output.position = mul( input.position, WorldViewProj );    
	
	output.texCoord = input.texCoord;
	output.normal = mul( input.normal, (float3x3)World );
	output.tangent = input.tangent;
	output.binormal = input.binormal;
	
	return output;
}

float4 PS( GSPS_INPUT pixelShaderIn ) : SV_Target
{
    float4 finalColor = diffTexture.Sample( diffTexSampler, pixelShaderIn.texCoord );        
	return finalColor;
}
float4 PS2( GSPS_INPUT pixelShaderIn ) : SV_Target
{    
	return 1;//float4(0.3,0.3,0.3,0.3);
}






technique10 RenderDiffOnly
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );		
		
		SetRasterizerState( RS_NormalSolid );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}




technique10 RenderWireframe
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS2() ) );		
		
		SetRasterizerState( RS_NormalWireframe );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}


RasterizerState Raster
{
    CULLMODE = NONE;		
};
DepthStencilState Stencil
{
    DepthEnable = true;
    DepthWriteMask = ALL;    	
};

technique10 RenderDepthOnly
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( 0 );		
		
		SetRasterizerState( Raster );		
		SetDepthStencilState( Stencil, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}


/////////////////////////////////////////////////////////
float4 color;

struct VS_IN_POS
{
	float4 position : Position;
};
struct GSPS_INPUT_POS
{
	float4 position : SV_POSITION;			
};
GSPS_INPUT_POS VS_RenderLightVolume( VS_IN_POS input )
{
	GSPS_INPUT_POS output = (GSPS_INPUT_POS)0;	
	output.position = mul( float4(input.position.xyz,1), WorldViewProj );
	return output;
}
float4 PS_RenderLightVolume( GSPS_INPUT_POS pixelShaderIn ) : SV_Target
{    
	return 1;//color;
}

DepthStencilState DS_ScatterDepth
{
    DepthEnable = TRUE;   
    DEPTHWRITEMASK = ZERO; 
	DepthFunc = LESS_EQUAL;
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

technique10 RenderLightVolume
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_RenderLightVolume() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_RenderLightVolume() ) );		
		
		SetRasterizerState( RS_NormalWireframe );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_Grid, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}