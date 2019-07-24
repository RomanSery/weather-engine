#include "common.inc" 
#include "materials.inc"

Texture2D Base;
Texture2D NormalMap;	

float ambientLevel;
float4 LightColor = {1.0f, 1.0f, 1.0f, 1.0f};
bool UseBlueShift;
int MaterialID;

struct VsIn
{
	float4 position : POSITION;	
	float3 normal : NORMAL;
	float2 texCoord: TEXCOORD0;		
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct PsIn
{
	float4 position : SV_Position;	    
    float2 texCoord	: TEXCOORD0;   			
};

PsIn VS( VsIn input )
{
	PsIn output = (PsIn)0;
	
	output.position = mul( input.position, WorldViewProj );					
	output.texCoord = input.texCoord;			
	
	return output;
}


float4 PS( PsIn In ) : SV_Target0
{	
	float4 base = Base.Sample(BaseSampler, In.texCoord);
	
	float4 color = base;
	
	color.a = 1;
	if(mat_Emissive(MaterialID) > 0)
		return color + (mat_Emissive(MaterialID) * 5 * base);
	else {
		color.rgb *= 0.7f;	
		return color;
	}

	
}

RasterizerState RS_Reflection
{
    CULLMODE = FRONT;
	FILLMODE = SOLID;		
};
DepthStencilState EnableDepthTestingOnly
{
        DepthEnable = true;
        DepthWriteMask = 0x00;
        DepthFunc = Less;
    
        StencilEnable = false;
};
DepthStencilState DisableDepth
{
        DepthEnable = false;
        DepthWriteMask = ZERO;
        DepthFunc = Less;
    
        //Stencil
        StencilEnable = false;
        StencilReadMask = 0xFF;
        StencilWriteMask = 0x00;
};
technique10 RenderReflection
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );		
		
		
		SetRasterizerState( RS_Reflection );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}