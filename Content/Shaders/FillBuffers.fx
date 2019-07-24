#include "common.inc" 
#include "materials.inc"

float g_timeCycle;
float g_splashXDisplace;
float g_splashYDisplace;
float4x4 matSplashScale;
float g_KsDir = 1.4f;
bool useFaceNormals;

bool renderRainSplashes, matRainSplashes;
bool renderSnow, matSnow;
bool UseReflectionMap;
float4x4 ReflectionView;
int MaterialID;

Texture2D Base;
Texture2D DetailMap;
Texture2D NormalMap;
Texture2D reflectionMap;
Texture2D SnowTexture;
Texture2D SnowNormalTexture;
Texture3D SplashBumpTexture;
Texture3D SplashDiffuseTexture;
TextureCube EnvironmentMap;
Texture2D Depth;

bool useCubeMap;
bool useDetailMap;
float snowValue;
float wallSnowValue;    	

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
    float4 PosView	: TEXCOORD0;
	float4 PosWorld	: TEXCOORD7;
	float3 Normal   : NORMAL;
	float3 WorldNormal   : NORMALWORLD;
	float3 Tan      : TANGENT;
    float2 texCoord	: TEXCOORD1;   		
	float2 texCoord2	: TEXCOORD6;   	
	float2 texCoord3	: TEXCOORD8;   		  	
	float4 worldPos : WPOSITION;	
	float y : TEXCOORD2;		
	float sVal   : TEXCOORD3;    	
	float4 ReflectionMapSamplingPos		: TEXCOORD4;
	float reflectionAmt   : TEXCOORD5; 		
	float depth : DEPTH0;
};

PsIn VS( VsIn input )
{
	PsIn output = (PsIn)0;
	
	output.position = mul( input.position, WorldViewProj );				
	output.ReflectionMapSamplingPos = mul(input.position, WorldViewProj);
	output.PosView = mul(input.position, WorldView);	
	output.PosWorld = mul(input.position, World);	
	output.Normal   = mul( input.normal, WorldView );		
	output.WorldNormal   = mul( input.normal, World );		
	output.y = input.normal.y;	
	output.Tan   = mul( input.tangent, WorldView );			
	output.texCoord = input.texCoord;		
	output.worldPos = mul( input.position, matSplashScale );

	float3 Normal = output.WorldNormal;
    float3 upNormal = mul(float3(0,1,0), World);
    float snowAmt = abs(dot(upNormal, Normal));	 
    if(snowAmt > 0.4f)
      output.sVal = snowValue;  
    else
      output.sVal = wallSnowValue;	  
	
	output.reflectionAmt = snowAmt;	
		
	return output;
}

SamplerState EnvironmentMapSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct PsOut {
	float4 base   : SV_Target0;
	float4 normal : SV_Target1;
	float4 pos : SV_Target2;	//.a = MaterialID		
	float4 depth : SV_Target3;	
};
PsOut PS( PsIn In )
{
	PsOut Out;			
	
	float3 Tan = normalize(In.Tan);
	float3 InNormal = normalize(In.Normal);	
	float3 binorm = normalize( cross( InNormal, Tan ) );    
	float wetSurf = saturate(g_KsDir/2.0*saturate(In.y));
	float3x3 BTNMatrix =  float3x3( Tan, binorm, InNormal );		
	
	float3 N = (useFaceNormals) ? InNormal :  normalize(mul( NormalMap.Sample(BaseSampler, In.texCoord).xyz * 2.0f - 1.0f, BTNMatrix ));
	float4 baseColor = Base.Sample(BaseSampler, In.texCoord);
			
	if(useDetailMap) {
		float4 detailColor = DetailMap.Sample(BaseSampler, In.texCoord * 2.5f);
		baseColor = baseColor * detailColor * 2.0;		
	}

	float usingSnow = 1;
	if(renderRainSplashes && matRainSplashes){
		float4 splashDiffuse = 0;
		splashDiffuse = wetSurf * SplashDiffuseTexture.Sample(samAnisoMirror, float3(In.worldPos.xz, g_timeCycle));
		float4 BumpMapVal = SplashBumpTexture.Sample(samAnisoMirror, float3(In.worldPos.x/2.0 + g_splashXDisplace, In.worldPos.z/2.0 + g_splashYDisplace, g_timeCycle)) - 0.5;		
		BumpMapVal *= 2;			
		N += wetSurf * 2 * (BumpMapVal.x * Tan + BumpMapVal.y * binorm);
		N = normalize(N);   
		baseColor += splashDiffuse;
	}			
	if(renderSnow && mat_Emissive(MaterialID) == 0 && matSnow){
		float4 snowColor = SnowTexture.Sample(samAnisoMirror, In.texCoord);
		float3 snowN = normalize( mul(SnowNormalTexture.Sample(samAnisoMirror, In.texCoord).xyz * 2.0f - 1.0f, BTNMatrix) );	
		float snowAmt = abs(dot(mul(float3(0,1,0),WorldView), N));     
		if(snowAmt > In.sVal)  {
		  baseColor = snowColor;
		  N = snowN;
		  usingSnow = 0;
		} 		
	}

	if(UseReflectionMap){
		float2 ProjectedTexCoords;
		ProjectedTexCoords.x = In.ReflectionMapSamplingPos.x/In.ReflectionMapSamplingPos.w/2.0f + 0.5f;
		ProjectedTexCoords.y = -In.ReflectionMapSamplingPos.y/In.ReflectionMapSamplingPos.w/2.0f + 0.5f;	
		float4 reflectionColor = reflectionMap.Sample(samAnisoMirror, ProjectedTexCoords) * baseColor;		
		
		if(reflectionColor.a > 0)
			baseColor = lerp(baseColor, reflectionColor, mat_Reflectivity(MaterialID) * In.reflectionAmt * usingSnow);
		
	}	

	if(useCubeMap){			
		float3 viewVector = normalize(In.PosWorld - cameraPosition);				
		float3 Reflection = normalize(reflect(viewVector, useFaceNormals ? normalize(In.WorldNormal) : (normalize(In.WorldNormal) * N * 0.1f)   ));						
		
		float4 envmap = EnvironmentMap.Sample(EnvironmentMapSampler, Reflection);
		float3 Fresnel = saturate(1 + dot(normalize(viewVector), normalize(In.WorldNormal)));		
		baseColor = lerp(baseColor, envmap, float4(Fresnel,1) * mat_Reflectivity(MaterialID) * usingSnow);								
	}
	

	Out.pos = float4( In.PosView.xyz, MaterialID );
	Out.base = baseColor;		
	Out.normal = float4(N, 1);
	Out.depth = float4(length(In.PosView.xyz) / 1000, 0, 0, 1);
	return Out;
}




technique10 RenderBuffers
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


/////////////////////////////////////////////////// WATER ///////////////////////////////////////////////////////////////////



float scroll;
float scroll2;
float scroll3;
float scroll4;
float shoreFalloff;
float shoreScale;     
float speed;
float reflectionFactorOffset;

SamplerState samWater
{
  Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
	AddressW = Clamp;
};

PsIn VS_Water( VsIn input )
{
	PsIn output = (PsIn)0;
	
	output.position = mul( input.position, WorldViewProj );				
	output.ReflectionMapSamplingPos = mul(input.position, WorldViewProj);
	output.PosView = mul(input.position, WorldView);	
	output.PosWorld = mul(input.position, World);	
	output.Normal   = mul( input.normal, WorldView );		
	output.WorldNormal   = mul( input.normal, World );		
	output.y = input.normal.y;	
	output.Tan   = mul( input.tangent, WorldView );			
	output.texCoord = input.texCoord;
	output.texCoord2 = input.texCoord;
	output.texCoord3 = input.texCoord;	
	output.worldPos = mul( input.position, matSplashScale );
	
	
	output.texCoord.y -= (scroll2) * (speed);            
    output.texCoord.x -= (scroll) * (speed);     
	
	output.texCoord2.y += (scroll) * (speed);            
    output.texCoord2.x += (scroll2) * (speed);         

	output.texCoord3.y += (scroll3) * (speed);            
    output.texCoord3.x -= (scroll4) * (speed);         
	
	return output;
}

float FresnelApproximation(float3 lightDir, float3 normal)
{
    float3 reflectedViewDir = -reflect(lightDir, normal);
 
    float viewDotNorm = abs(dot(lightDir, normal));
    float fresnelFactor = 1 - pow(viewDotNorm, 0.5);
 
    return saturate(fresnelFactor + reflectionFactorOffset);
}

PsOut PS_Water( PsIn In )
{
	PsOut Out;	
	
	int3 coords = int3(int2(In.position.xy), 0);				
	float sceneViewZ = Depth.Load(coords, 0).r;	
	float waterViewZ = length(In.PosView) / 1000;
	float depthRange = (sceneViewZ - waterViewZ);
    float alpha = saturate( max(pow(depthRange , shoreFalloff) * 1000 * shoreScale, 0) );

	float3 N = NormalMap.Sample(BaseSampler, In.texCoord).xyz * 2.0f - 1.0f;
	N *= NormalMap.Sample(BaseSampler, In.texCoord2).xyz * 2.0f - 1.0f;	
	N *= NormalMap.Sample(BaseSampler, In.texCoord3).xyz * 2.0f - 1.0f;		
	
	coords = int3(int2(In.position.xy + (N * 40)), 0);			
	float4 baseColor = Base.Load(coords, 0);		
	
	In.ReflectionMapSamplingPos.xy += (N * 10); 

	float3 Tan = normalize(In.Tan);
	float3 InNormal = normalize(In.Normal);	
	float3 binorm = normalize( cross( InNormal, Tan ) );   
	float wetSurf = saturate(g_KsDir/2.0*saturate(In.y));
	float3x3 BTNMatrix =  float3x3( Tan, binorm, InNormal );		
	N = normalize(mul(N, BTNMatrix ));	
	

	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = In.ReflectionMapSamplingPos.x/In.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ProjectedTexCoords.y = -In.ReflectionMapSamplingPos.y/In.ReflectionMapSamplingPos.w/2.0f + 0.5f;	
	float3 reflectionColor = reflectionMap.Sample(samWater, ProjectedTexCoords).rgb;				
	
	float3 viewVector = normalize(In.PosWorld - cameraPosition);
	float reflectionFactor = FresnelApproximation(viewVector, normalize(In.WorldNormal));
	float3 finalColor = lerp(baseColor, reflectionColor, reflectionFactor );					

	Out.pos = float4( In.PosView.xyz, MaterialID );
	Out.base = float4(finalColor, alpha);
	Out.normal = float4(N,alpha);
	return Out;
}




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


technique10 RenderWater
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_Water() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Water() ) );		
		
		
		SetRasterizerState( RS_NormalSolid );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}