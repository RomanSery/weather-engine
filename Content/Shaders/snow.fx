#define PI 3.14159265

//ChangesEveryFrame
matrix g_mWorld;
matrix g_mWorldViewProj;
matrix g_mWorldView;
matrix g_mProjection;
matrix g_mInvView;
matrix g_mViewProj;
float3 g_eyePos;   //eye in world space
float3 g_lightPos = float3(10,10,0); //the directional light in world space 
float g_FrameRate;
float fTime;		// time in the world (seconds)

//changesOften
float3 g_TotalVel = float3(0,-0.25,0);
float g_SpriteSize = 1.0;
float fTurbulence;
float4 color;


float numPointLights;
float4 g_PointLightPositions[255]; //.a = radius
float4 g_PointLightColors[255];


float numSpotLights;
float4 g_SpotLightPositions[255]; //.a = radius
float4 g_SpotLightColors[255];
float4 g_SpotLightDirections[255];
float4 g_SpotLightAngles[255];



float numAreaLights;
float4 g_AreaLightPositions1[255];
float4 g_AreaLightPositions2[255];
float4 g_AreaLightColors[255];
float4 g_AreaLightVals[255]; //.x = radius .y = LerpInc



//changesRarely
float g_heightRange = 30.0;
float g_radiusRange = 30.0;
float g_Near; 
float g_Far; 
Texture2D snowParticle;

cbuffer cbImmutable
{   
	float3 g_positions[4] =
    {
        float3( -1, 1, 0 ),
        float3( 1, 1, 0 ),
        float3( -1, -1, 0 ),
        float3( 1, -1, 0 ),
    };
    float2 g_texcoords[4] = 
    { 
        float2(0,1), 
        float2(1,1),
        float2(0,0),
        float2(1,0),
    };   
}
SamplerState samAniso
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};


//--------------------------------------------------------------------------------------------
// draw rain
//--------------------------------------------------------------------------------------------
struct PSRainIn
{
    float4 pos : SV_Position;	
    float2 tex : TEXTURE0;
	//float  fAlpha		: TEXCOORD1;	    
	float3 color : TEXCOORD2;		
};
struct VSParticleIn
{   
    float3 pos              : POSITION;         //position of the particle
    float3 seed             : SEED;
    float3 speed            : SPEED;
    float2 random            : RAND;
    float3 color             : COLOR;             //particle type
};

VSParticleIn VSPassThroughRain(VSParticleIn input )
{
    return input;
}

// GS for rendering rain as point sprites.  Takes a point and turns it into 2 tris.
[maxvertexcount(4)]
void GSRenderRainCheap(point VSParticleIn input[1], inout TriangleStream<PSRainIn> SpriteStream)
{
   
    PSRainIn output = (PSRainIn)0;			
	for(int i=0; i<4; i++)
    {			
        float3 position = mul( g_positions[i] * g_SpriteSize, (float3x3)g_mInvView ) + input[0].pos;		
        output.pos = mul( float4(position,1.0), g_mViewProj );  		
		output.tex = g_texcoords[i];
		output.color = input[0].color;					

		// fade snow particles that are far away
		//float3 dist = g_eyePos - input[0].pos;
		//float fAttenuation = max( 0, 1 - ( length(dist) / (g_radiusRange / 2.0f) ));				
		//output.fAlpha = fAttenuation;
		SpriteStream.Append(output);
    }  
        
    SpriteStream.RestartStrip();
       
}
float4 PSRenderRain(PSRainIn input) : SV_Target
{   	
	float4 c = snowParticle.Sample(samAniso, input.tex);
	//c.a *= input.fAlpha;	 
	c.xyz *= (color.xyz + input.color.xyz);	
    return c;	 		
}

BlendState CorrectBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};
BlendState Blending
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;    
    BlendEnable[0] = TRUE;
};
DepthStencilState EnableDepthTestingOnly
{
    DepthEnable = true;
    DepthWriteMask = 0x00;
    DepthFunc = Less;    
    StencilEnable = false;
};
RasterizerState CullNone
{
    MultiSampleEnable = False;
    CullMode=None;
};
technique10 RenderParticles
{
    pass p0
    {
        SetVertexShader( CompileShader(   vs_4_0, VSPassThroughRain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSRenderRainCheap() ) );
        SetPixelShader( CompileShader(    ps_4_0, PSRenderRain() ) );
        
        SetBlendState( CorrectBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepthTestingOnly, 0 );
        SetRasterizerState( CullNone );
    }  
}


//--------------------------------------------------------------------------------------------
// advance snow
//--------------------------------------------------------------------------------------------
float3 GetPointLightContribution(float radius, float3 LightDir, float3 LightColor)
{	
	float fAttenuation = max( 0, 1 - ( length(LightDir) / radius ));				
	return (fAttenuation * LightColor);
}
float3 GetSpotLightContribution(float radius, float3 LightDir, float3 LightColor, float3 spotLightDir, float g_fInnerAngle, float g_fOuterAngle)
{
	float3 spotLightsContribution = 0.0f;
	float3 fvPixelToLight = LightDir;			
	float3 fvPixelToLightNormalised = normalize( fvPixelToLight );
	float fConeStrength = max( 0.0f, dot( fvPixelToLightNormalised, -spotLightDir ));		
	if( fConeStrength > g_fOuterAngle )
	{							
		float fConeAttenuation		= smoothstep( g_fOuterAngle, g_fInnerAngle, fConeStrength );			
		float fDistanceAttenuation	= max( 0, 1 - length( fvPixelToLight ) / (radius * 1.15f) );			
		spotLightsContribution = (fConeAttenuation * fDistanceAttenuation * LightColor);		
	}		
	return spotLightsContribution;
}
float3 GetAreaLightContribution(float radius, float3 LightColor, float lerpInc, float3 startPos, float3 endPos, float3 worldPos)
{
	float3 r = 0;
	float3 p = startPos;

	for(float x = 0; x <= 1; x += lerpInc){
		p = lerp(startPos, endPos, x);
		float3 fvPixelToLight = p - worldPos;						
		float fAttenuation = max( 0, 1 - ( length(fvPixelToLight) / (radius) ));			
		r += fAttenuation * LightColor;		
	}
	return r;
}
VSParticleIn VSAdvanceRain(VSParticleIn input)
{     
	float3 particleColor = 0;
	for(int i = 0; i < numPointLights; i++){
		float dist = length(g_PointLightPositions[i] - input.pos);
		if(dist <= g_PointLightPositions[i].a){
			particleColor += GetPointLightContribution(g_PointLightPositions[i].a, g_PointLightPositions[i] - input.pos, g_PointLightColors[i].xyz);
		}
	}			
	for(int i = 0; i < numSpotLights; i++){
		particleColor += GetSpotLightContribution(g_SpotLightPositions[i].a, g_SpotLightPositions[i] - input.pos,
			g_SpotLightColors[i].xyz, g_SpotLightDirections[i].xyz, g_SpotLightAngles[i].x, g_SpotLightAngles[i].y);
	}		
	for(int i = 0; i < numAreaLights; i++){
		particleColor += GetAreaLightContribution(g_AreaLightVals[i].x,g_AreaLightColors[i].xyz,g_AreaLightVals[i].y,
										g_AreaLightPositions1[i].xyz, g_AreaLightPositions2[i].xyz, input.pos);
	}	
		
	if(particleColor.x == 0 && particleColor.y == 0 && particleColor.z == 0){ particleColor = 0.2f;  }
	input.color = particleColor;


    //particle physics
    input.pos.xyz += input.speed.xyz/g_FrameRate + g_TotalVel.xyz;		
	input.pos.x += (cos(fTime - input.random.x) * fTurbulence);
	input.pos.z += (cos(fTime - input.random.y) * fTurbulence);		 
	//if the particle is outside the bounds, move it to random position near the eye      
	if(input.pos.y <=  g_eyePos.y-g_heightRange )
    {
		float x = input.seed.x + g_eyePos.x;
		float z = input.seed.z + g_eyePos.z;
		float y = input.seed.y + g_eyePos.y;
		input.pos = float3(x,y,z);
    }    

		
	

    return input;    
}

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

GeometryShader gsStreamOut = ConstructGSWithSO( CompileShader( vs_4_0, VSAdvanceRain() ), "POSITION.xyz; SEED.xyz; SPEED.xyz; RAND.xy; COLOR.xyz" );  
technique10 AdvanceParticles
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSAdvanceRain() ) );
        SetGeometryShader( gsStreamOut );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
    }  
}