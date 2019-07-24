#include "common.inc" 
#include "materials.inc"

Texture2D colorMap;
Texture2D normalMap;
Texture2D positionMap;

float3 g_fvViewSpaceLightDirection;
float ambientLevel;

float4 LightColor = {1.0f, 1.0f, 1.0f, 1.0f};
float FogDensity;
bool UseBlueShift;

struct VSIn {
    uint VertexID: SV_VertexID;
};
struct PsIn {
	float4 position: SV_Position;	
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

	return Out;
}
float4 PS(PsIn In) : SV_Target0
{    
	int3 coords = int3(int2(In.position.xy), 0);
	float4 pos = positionMap.Load(coords, 0);	
	int materialID = pos.a;
	float4 normalColor = normalMap.Load(coords, 0);	
	float3 normal = normalColor.xyz;	
	float alpha = normalColor.a;
	float4 base	= colorMap.Load(coords, 0);			
	
	float3 fvToLight = -normalize(g_fvViewSpaceLightDirection);	
	float NdotL = max( 0, dot( normal, fvToLight ));				

	
	float4 color = ((base * NdotL) * LightColor);				
	
	if(UseBlueShift){
		color += ((ambientLevel+LightColor) * base);		
	} else {		
		color += (ambientLevel * base);		
		if( NdotL > 0.0f ){				
			float3 fvPixelToViewer		= normalize( -pos.xyz );												// V
			float3 fvReflection			= normalize( 2.0f * NdotL * normal - fvToLight );				// R
			float fRDotV					= max( 0.0f, dot( fvReflection, fvPixelToViewer ));							// R.V	
			float4 fvTotalSpecular		= pow( fRDotV, mat_SpecularPower(materialID) ) * mat_SpecularIntensity(materialID) * LightColor * alpha;		
			color += (fvTotalSpecular * LightColor);		
		}
	}

	float4 finalColor = color + (mat_Emissive(materialID) * base);						

	if(FogDensity > 0){
		cameraPosition = mul(cameraPosition, View);	
		float fog = length(cameraPosition - pos) * FogDensity;			
		return lerp(finalColor, LightColor, saturate(fog));	
	} else {
		return finalColor;	
	}
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

BlendState AlphaBlending
{
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = DEST_ALPHA;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

technique10 RenderAmbient
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( NoDepth, 0 );        
        SetRasterizerState( RS_BackCulling );
    }  
}

