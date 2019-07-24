#include "common.inc" 

#include "materials.inc"


Texture2D colorMap;
Texture2D normalMap;
Texture2D positionMap;

TextureCube lightMap;
TextureCube lightMask;

float3 g_fvViewSpaceLightPosition;
float3 g_fvWorldSpaceLightPosition;

float  g_fLightMaxRange;
float4x4 matWorldViewProjection;
float4x4 matInView;
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
	return Out;
}




float4 CalcPointLight(float3 pos, float3 normal, float4 base, int materialID, bool specular)
{
	// light calcs			
	float3 fvPixelToLight = g_fvViewSpaceLightPosition - pos;				
	float3 fvPixelToLightNormalised = normalize( fvPixelToLight ); // L			
	float fNDotL = max(0,dot( normal, fvPixelToLightNormalised ));	// N.L		
	
	if( fNDotL > 0.0f )
	{	
		float3 fvPixelToLight2 = normalize(mul(g_fvViewSpaceLightPosition,matInView) - mul(pos,matInView));				
		float4 mask	= lightMask.Sample(samAniso, fvPixelToLight2);		
		float4 map	= lightMap.Sample(samAniso, fvPixelToLight2);		

		float4 fvTotalSpecular = 0;
		if(specular){			
			float3 fvPixelToViewer	= normalize( -pos );															// V
			float3 fvReflection		= normalize( 2.0f * fNDotL * normal - fvPixelToLightNormalised );			// R
			float  fRDotV			= max( 0.0f, dot( fvReflection, fvPixelToViewer ));										// R.V
			fvTotalSpecular	= pow( fRDotV, mat_SpecularPower(materialID) ) * mat_SpecularIntensity(materialID) * lightColor;				
		}			

		float fAttenuation = max( 0, 1 - ( length(fvPixelToLight) / g_fLightMaxRange ));	
		
		float4 baseColor = ((base * lightColor * map) + fvTotalSpecular) * mask;		
		return fAttenuation * (baseColor * fNDotL);									
	}		
	return 0;
}


float4 blinn_phong( in float3 normal, in float3 viewer, in float3 light, in int materialID )
{    
    // Compute the half vector
    float3 half_vector = normalize(light + viewer);
 
    // Compute the angle between the half vector and normal
    float  HdotN = max( 0.0f, dot( half_vector, normal ) );
 
    // Compute the specular colour
    float3 specular = lightColor * pow( HdotN, mat_SpecularPower(materialID) );
 
    // Compute the diffuse term
    float3 diffuse = cDiffuse * max( 0.0f, dot( normal, light ) );
 
    // Determine the final colour    
    return float4( diffuse + specular, 1.0f );
}


float4 PS(PsIn In) : SV_Target
{   	
	int3 coords = int3(int2(In.position.xy), 0);		
	
	float4 pos = positionMap.Load(coords, 0);	
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);			


	float3 viewer = normalize( -pos );	
	float3 light = normalize( g_fvViewSpaceLightPosition - pos );
	float4 c = blinn_phong(normal, viewer, light );


	return CalcPointLight(pos.xyz, normal, base, pos.a, true);	
}
float4 PS_NoSpecular(PsIn In) : SV_Target
{   	
	int3 coords = int3(int2(In.position.xy), 0);	
	
	float4 pos = positionMap.Load(coords, 0);	
	float3 normal = normalMap.Load(coords, 0).xyz;	
	float4 base	= colorMap.Load(coords, 0);	
	return CalcPointLight(pos.xyz, normal, base, pos.a, false);		
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

technique10 RenderPointLight
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
technique10 RenderPointLightNoSpecular
{
	pass p0
    {	
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_NoSpecular() ) );
        
        SetBlendState( Blending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );        
        SetRasterizerState( RS_FrontCulling );
    }  
}



