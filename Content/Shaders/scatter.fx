#include "common.inc" 

float4x4 WorldViewProjection;

float InvOpticalDepthNLessOne = 1.0f/255.0f;
float InvOpticalDepthN = 1.0f/256.0f;
float InnerRadius = 6356.7523142;
float OuterRadius = 6356.7523142 * 1.0157313;
float PI = 3.1415159;
float NumSamples = 20;
float fScale = 1.0 / (6356.7523142 * 1.0157313 - 6356.7523142);
float2 v2dRayleighMieScaleHeight = {0.25, 0.1};

float2 InvRayleighMieNLessOne = {1.0f/255.0f, 1.0f/127.0f};
float3 v3SunDir = { 0, 1, 0 };
float KrESun = 0.0025f * 20.0f;
float KmESun = 0.0010f * 20.0f;
float Kr4PI = 0.0025f * 4.0f * 3.1415159;
float Km4PI = 0.0010f * 4.0f * 3.1415159;

float g = -0.990;
float g2 = (-0.990) * (-0.990);
float3 v3HG;
float fExposure = -2.0;

float3 InvWavelength;
float3 WavelengthMie;

float starIntensity = 0.5f;

Texture2D txRayleigh;
Texture2D StarsTex;
Texture2D txMie;

SamplerState scatterSampler
{   
    AddressU = CLAMP;
    AddressV = CLAMP;
	Filter = MIN_MAG_MIP_LINEAR;	
};


struct VS_INPUT
{
    float3 Pos	: POSITION;
    float2 Tex0 : TEXCOORD0;
};

struct PS_INPUT
{
    float4 Pos  : SV_Position;
    float2 Tex0 : TEXCOORD0;
	float3 Tex1 : TEXCOORD1;
};


float getRayleighPhase(float fCos2)
{
	return 0.75 * (1.0 + fCos2);
}

float getMiePhase(float fCos, float fCos2)
{
	v3HG.x = 1.5f * ( (1.0f - g2) / (2.0f + g2) );
	v3HG.y = 1.0f + g2;
	v3HG.z = 2.0f * g;
	return v3HG.x * (1.0 + fCos2) / pow(abs(v3HG.y - v3HG.z * fCos), 1.5);
}

float3 HDR( float3 LDR)
{
	return 1.0f - exp( fExposure * LDR );
}


PS_INPUT VS( VS_INPUT input )
{
    PS_INPUT output = (PS_INPUT)0;
	output.Pos =  mul( float4(input.Pos,1), WorldViewProjection );
	output.Pos.w = output.Pos.z;
    output.Tex0 = input.Tex0; 
    output.Tex1 = -input.Pos;
    return output;
}

float4 PS( PS_INPUT input) : SV_Target
{		
	float fCos = dot( v3SunDir, input.Tex1 ) / length( input.Tex1 );
	float fCos2 = fCos * fCos;
	
	float3 v3RayleighSamples = txRayleigh.Sample( scatterSampler, input.Tex0 );
	float3 v3MieSamples = txMie.Sample( scatterSampler, input.Tex0 );				

	float3 Color;
	Color.rgb = getRayleighPhase(fCos2) * v3RayleighSamples.rgb + getMiePhase(fCos, fCos2) * v3MieSamples.rgb;
	Color.rgb = HDR( Color.rgb );
	
	// Hack Sky Night Color
	Color.rgb += max(0,(1 - Color.rgb)) * float3( 0.05, 0.05, 0.1 ); 	 
	//Color.rgb *= 0.3f;

	return float4( Color.rgb, 1 ) + StarsTex.Sample( scatterSampler, input.Tex0 ) * starIntensity;
}

DepthStencilState DS_ScatterDepth
{
    DepthEnable = TRUE;   
    DEPTHWRITEMASK = ZERO; 
	DepthFunc = LESS_EQUAL;
};
RasterizerState RS_Reflection
{
    CULLMODE = FRONT;
	FILLMODE = SOLID;		
};

technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );				
			
		SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DS_ScatterDepth, 0 );        
		SetRasterizerState( RS_NormalSolid );
	}		
}
technique10 RenderReflection
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );				
			
		SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DS_ScatterDepth, 0 );        
		SetRasterizerState( RS_Reflection );
	}		
}



///////////////////////////////////////////////////////////////////////////////////////////

float HitOuterSphere( float3 O, float3 Dir ) 
{
	float3 L = -O;

	float B = dot( L, Dir );
	float C = dot( L, L );
	float D = C - B * B; 
	float q = sqrt( OuterRadius * OuterRadius - D );
	float t = B;
	t += q;

	return t;
}

float2 GetDensityRatio( float fHeight )
{
	const float fAltitude = (fHeight - InnerRadius) * fScale;
	return exp( -fAltitude / v2dRayleighMieScaleHeight.xy );
}

float2 t( float3 P, float3 Px )
{
	float2 OpticalDepth = 0;

	float3 v3Vector =  Px - P;
	float fFar = length( v3Vector );
	float3 v3Dir = v3Vector / fFar;
			
	float fSampleLength = fFar / NumSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Dir * fSampleLength;
	P += v3SampleRay * 0.5f;
			
	for(int i = 0; i < NumSamples; i++)
	{
		float fHeight = length( P );
		OpticalDepth += GetDensityRatio( fHeight );
		P += v3SampleRay;
	}		

	OpticalDepth *= fScaledLength;
	return OpticalDepth;
}


struct PS_OUTPUT_UPDATE
{
	float4 RayLeigh : SV_Target0;
	float4 Mie		: SV_Target1;    	
};
struct QuadVS_Input
{
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD0;
};

struct QuadVS_Output
{
    float4 Pos : SV_POSITION;              // Transformed position
    float2 Tex : TEXCOORD0;
};

QuadVS_Output QuadVS( QuadVS_Input Input )
{
    QuadVS_Output Output;
    Output.Pos = Input.Pos;
    Output.Tex = Input.Tex;
    return Output;
}

PS_OUTPUT_UPDATE PS_UPDATE( QuadVS_Output input)
{
	PS_OUTPUT_UPDATE output = (PS_OUTPUT_UPDATE)0;
	
	float2 Tex0 = (input.Tex);			
	 
	const float3 v3PointPv = float3( 0, InnerRadius + 1e-3, 0 );
	const float AngleY = 100.0 * Tex0.x * PI / 180.0;
	const float AngleXZ = PI * Tex0.y;
	
	float3 v3Dir;
	v3Dir.x = sin( AngleY ) * cos( AngleXZ  );
	v3Dir.y = cos( AngleY );
	v3Dir.z = sin( AngleY ) * sin( AngleXZ  );
	v3Dir = normalize( v3Dir );	

	float fFarPvPa = HitOuterSphere( v3PointPv , v3Dir );
	float3 v3Ray = v3Dir;

	float3 v3PointP = v3PointPv;
	float fSampleLength = fFarPvPa / NumSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	v3PointP += v3SampleRay * 0.5f;
				
	float3 v3RayleighSum = 0;
	float3 v3MieSum = 0;

	for( int k = 0; k < NumSamples; k++ )
	{
		float PointPHeight = length( v3PointP );

		float2 DensityRatio = GetDensityRatio( PointPHeight );
		DensityRatio *= fScaledLength;

		float2 ViewerOpticalDepth = t( v3PointP, v3PointPv );
						
		float dFarPPc = HitOuterSphere( v3PointP, v3SunDir );
		float2 SunOpticalDepth = t( v3PointP, v3PointP + v3SunDir * dFarPPc );

		float2 OpticalDepthP = SunOpticalDepth.xy + ViewerOpticalDepth.xy;
		float3 v3Attenuation = exp( - Kr4PI * InvWavelength * OpticalDepthP.x - Km4PI * OpticalDepthP.y );

		v3RayleighSum += DensityRatio.x * v3Attenuation;
		v3MieSum += DensityRatio.y * v3Attenuation;

		v3PointP += v3SampleRay;
	}

	float3 RayLeigh = v3RayleighSum * KrESun;
	float3 Mie = v3MieSum * KmESun;
	RayLeigh *= InvWavelength;
	Mie *= WavelengthMie;
	
	output.RayLeigh = float4( RayLeigh, 1 );
	output.Mie = float4( Mie, 1 );  //not working
	return output;
}


DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};


technique10 Update
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_UPDATE() ) );				
		
		SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DisableDepth, 0 );
		SetRasterizerState( RS_NormalSolid );        
	}	
}

//////////////////////////////////////////////////////////////////////////////

float alpha;

struct Moon_VS_IN
{
    float3 Pos : POSITION;
    float2 Tex : TEXCOORD0;
};

struct Moon_PS_IN
{
	float4 position : SV_POSITION;			
	float2 texCoord: TEXCOORD0;		
};

Moon_PS_IN MoonSunVS(Moon_VS_IN In)
{
    Moon_PS_IN OUT;            	
    OUT.position = mul(float4(In.Pos,1), WorldViewProjection);	
	OUT.position.w = OUT.position.z;
    OUT.texCoord = In.Tex;    
    return OUT;    
}

float4 MoonSunPS( Moon_PS_IN In) : SV_Target
{
	float4 c = StarsTex.Sample( scatterSampler, In.texCoord );
	c.a *= alpha;
	return c;
}

BlendState BS_SkyBlending
{    
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    
    BlendEnable[0] = TRUE;
};
DepthStencilState DS_SkyDepth
{
    DepthEnable = TRUE;   
    DEPTHWRITEMASK = ZERO; 
	DepthFunc = LESS_EQUAL;
};

technique10 RenderMoon
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, MoonSunVS() ) );
		SetPixelShader( CompileShader( ps_4_0, MoonSunPS() ) );				
		
		SetBlendState( BS_SkyBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DS_SkyDepth, 0 );
		SetRasterizerState( RS_NormalSolidNoCulling );        
	}	
}

//////////////////////////////////////////////////////////////////////////////////////////////////

float  cloud1Tile;
float  cloud2Tile;
float2  scroll;
float  cloudCover;
float4 SunColor;
Texture2D clouds1Tex;
Texture2D clouds2Tex;

SamplerState cloudsSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VS_CLOUDS_IN
{
	float4 position : POSITION;	
	float3 normal : NORMAL;
	float2 texCoord: TEXCOORD0;		
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct PS_CLOUDS_IN
{
	float4 position : SV_POSITION;		
	float3 normal : TEXCOORD0;
	float2 texCoord: TEXCOORD1;	
	float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
	float2 texCoord2: TEXCOORD2;	
	float  alpha : TEXCOORD3;
};


PS_CLOUDS_IN CloudsVS(VS_CLOUDS_IN IN)
{
    PS_CLOUDS_IN OUT;
        
    OUT.position = mul(IN.position, WorldViewProj);
	OUT.position.w = OUT.position.z;
    OUT.texCoord = OUT.texCoord2 = IN.texCoord; 

    //cloud1
    OUT.texCoord *= cloud1Tile;  
    OUT.texCoord.y += scroll.x;  
    
    //cloud2
    OUT.texCoord2 *= cloud2Tile;  
    OUT.texCoord2.y -= scroll.y;  

	OUT.alpha = smoothstep(0.0f, 15.0f,IN.position.y);	
    
    return OUT;    
}

float4 CloudsPS( PS_CLOUDS_IN IN ) : SV_Target
{	
   float4 clouds = clouds1Tex.Sample( cloudsSampler, IN.texCoord ) * cloudCover;      
   float4 clouds2 = (clouds2Tex.Sample(cloudsSampler, IN.texCoord2) - 0.5f) * cloudCover;

   //clouds.rgb *= SunColor.rgb;
   //clouds2.rgb *= SunColor.rgb;

   float4 final = (clouds.a + clouds2);
   final.a *= IN.alpha;
   final.rgb *= SunColor.rgb;
   return final;
}

technique10 RenderClouds
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, CloudsVS() ) );
		SetPixelShader( CompileShader( ps_4_0, CloudsPS() ) );				
		
		SetBlendState( BS_SkyBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DS_ScatterDepth, 0 );
		SetRasterizerState( RS_NormalSolid );        
	}	
}
technique10 RenderCloudsReflection
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, CloudsVS() ) );
		SetPixelShader( CompileShader( ps_4_0, CloudsPS() ) );				
		
		SetBlendState( BS_SkyBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( DS_ScatterDepth, 0 );
		SetRasterizerState( RS_Reflection );        
	}	
}


/////////////////////////////////////////////////////////////////////////////////////////////

struct VSSceneIn
{
	float4 position : POSITION;			//position	
};

struct PSSceneIn
{
	float4 pos : SV_Position;	
};

PSSceneIn VSScenemain(VSSceneIn input)
{
	PSSceneIn output;
	output.pos = mul( float4(input.position.xyz,1.0), WorldViewProjection );			
	return output;
}
float4 PSOccluder(PSSceneIn input) : SV_Target
{	
	return float4(1,0,0,0.5);
}


BlendState OccTestBlendState
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0;
};
DepthStencilState DisableDepthWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = ZERO;		
};

RasterizerState DisableCulling
{
	CullMode = BACK;
};

technique10 RenderOccluder
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSOccluder() ) );
        
        SetBlendState( OccTestBlendState, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
        SetRasterizerState( DisableCulling );
    }  
}