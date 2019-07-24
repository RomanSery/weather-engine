#include "common.inc" 

#include "materials.inc"


Texture2D colorMap;
Texture2D normalMap;
Texture2D positionMap;

float3 g_fvViewSpaceLightPosition;
float3 g_fvViewSpaceLightPosition2;
float4 lightColor;
float  g_fLightMaxRange;
float4x4 matWorldViewProjection;
float g_fSpecularPower = 150.0f;
float lerpInc = 0.0f;

struct VsIn {
	float4 position : Position;
};
struct PsIn {
	float4 position : SV_Position;	
};


PsIn VS(VsIn In)
{
	PsIn Out;
	Out.position = mul( In.position, matWorldViewProjection );
	return Out;
}


float4 CalcAreaLight(float3 pos, float3 normal, float4 base, int materialID, bool specular)
{
	float4 r = 0;
	float3 lightPos = g_fvViewSpaceLightPosition;
	float i = 0;
	
	for(i = 0; i <= 1; i += lerpInc){
		lightPos = lerp(g_fvViewSpaceLightPosition, g_fvViewSpaceLightPosition2, i);		
						
		float3 fvPixelToLight = lightPos - pos;				
		float3 fvPixelToLightNormalised = normalize( fvPixelToLight );		
		float fNDotL = max(0,dot( normal, fvPixelToLightNormalised ));
	
		if( fNDotL > 0.0f )
		{	
			float4 fvTotalSpecular = 0;			
			if(specular){
				float3 fvPixelToViewer	= normalize( -pos );															// V
				float3 fvReflection		= normalize( 2.0f * fNDotL * normal - fvPixelToLightNormalised );			// R
				float  fRDotV			= max( 0.0f, dot( fvReflection, fvPixelToViewer ));										// R.V
				fvTotalSpecular	= pow( fRDotV, mat_SpecularPower(materialID) ) * mat_SpecularIntensity(materialID) * lightColor;				
			}

			float fAttenuation = max( 0, 1 - ( length(fvPixelToLight) / (g_fLightMaxRange) ));			
			r += fAttenuation * ((base * fNDotL * lightColor) + fvTotalSpecular );			
		}		
		
	}
	
	return r;
}


float4 PS(PsIn In) : SV_Target
{		
	int3 coords = int3(int2(In.position.xy), 0);
	
	float4 pos = positionMap.Load(coords, 0);
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);	
	return CalcAreaLight(pos.xyz, normal, base, pos.a, true);			
}

float4 PSNoSpecular(PsIn In) : SV_Target
{		
	int3 coords = int3(int2(In.position.xy), 0);

	float4 pos = positionMap.Load(coords, 0);
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);	
	return CalcAreaLight(pos.xyz, normal, base, pos.a, false);				
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


technique10 RenderAreaLight
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

technique10 RenderAreaLightNoSpecular
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




