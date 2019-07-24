#include "common.inc" 

#include "materials.inc"

Texture2D colorMap;
Texture2D normalMap;
Texture2D positionMap;

TextureCube lightMap;
TextureCube lightMask;


float4x4 matWorldViewProjection;
float4x4 matInView;
float3 g_fvViewSpaceLightPosition;
float3 g_fvWorldSpaceLightPosition;
float3 g_fvViewSpaceLightDirection;
float  g_fLightMaxRange;
float g_fInnerAngle;
float g_fOuterAngle;
float4 lightColor;

struct VsIn {
	float4 position : Position;
	float2 texCoord : TEXCOORD;	
};
struct PsIn {
	float4 position : SV_Position;	
	float2 texCoord : TEXCOORD;	
};
SamplerState samAniso
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

PsIn VS(VsIn In)
{
    PsIn Out;
	Out.position = mul( In.position, matWorldViewProjection );	
	Out.texCoord = In.texCoord;
	return Out;
}

float4 CalcSpotLight(float3 pos, float3 normal, float4 base, int materialID, bool specular)
{
	// need these two vectors for lighting calcs
	float3 fvPixelToLight = g_fvViewSpaceLightPosition - pos;				
	float3 fvPixelToLightNormalised = normalize( fvPixelToLight ); // L			
	float fNDotL = max(0,dot( normal, fvPixelToLightNormalised ));	// N.L							
	
	if( fNDotL > 0.0f )
	{			
		float fConeStrength = max( 0.0f, dot( fvPixelToLightNormalised, -g_fvViewSpaceLightDirection ));
		
		if( fConeStrength > g_fOuterAngle )
		{						
			float fConeAttenuation		= smoothstep( g_fOuterAngle, g_fInnerAngle, fConeStrength );			
			float fDistanceAttenuation	= max( 0, 1 - length( fvPixelToLight ) / (g_fLightMaxRange * 1.15f) );
				
			float4 fvTotalSpecular = 0;	
			if(specular)
			{					
				float3 fvPixelToViewer	= normalize( -pos );															// V
				float3 fvReflection		= normalize( 2.0f * fNDotL * normal - fvPixelToLightNormalised );			// R
				float  fRDotV			= max( 0.0f, dot( fvReflection, fvPixelToViewer ));										// R.V
				fvTotalSpecular	= pow( fRDotV, mat_SpecularPower(materialID) ) * mat_SpecularIntensity(materialID) * lightColor;					
			}
			

			float3 fvPixelToLight2 = normalize(mul(g_fvViewSpaceLightPosition,matInView) - mul(pos,matInView));			
			float4 mask	= lightMask.Sample(samAniso, fvPixelToLight2);		
			float4 map	= lightMap.Sample(samAniso, fvPixelToLight2);		

			float4 baseColor = ((base * lightColor * map) + fvTotalSpecular) * mask;		
			return fConeAttenuation * fDistanceAttenuation * (baseColor * fNDotL);
		}						
	}	
	return 0;
}

float4 PS(PsIn In) : SV_Target
{   	
	int3 coords = int3(int2(In.position.xy), 0);
	
	float4 pos = positionMap.Load(coords, 0);	
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);			
	return CalcSpotLight(pos.xyz, normal, base, pos.a, true);	
}
float4 PSNoSpecular(PsIn In) : SV_Target
{   	
	int3 coords = int3(int2(In.position.xy), 0);
	
	float4 pos = positionMap.Load(coords, 0);	
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);		
	return CalcSpotLight(pos.xyz, normal, base, pos.a, false);			
}


BlendState Blending
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;    
    BlendEnable[0] = TRUE;
};
DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};
RasterizerState RS_FrontCulling
{
    CULLMODE = FRONT;	
};

technique10 RenderSpotLight
{
	pass p0
    {	
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
        
        SetBlendState( Blending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );        
        SetRasterizerState( RS_FrontCulling );
    }  
}
technique10 RenderSpotLightNoSpecular
{
	pass p0
    {	
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSNoSpecular() ) );
        
        SetBlendState( Blending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );        
        SetRasterizerState( RS_FrontCulling );
    }  
}



