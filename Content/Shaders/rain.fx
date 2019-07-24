#define PI 3.14159265

Texture2DArray rainTextureArray;

//ChangesEveryFrame

matrix g_mWorld;
matrix g_mWorldViewProj;
matrix g_mWorldView;
matrix g_mProjection;
float3 g_eyePos;   //eye in world space
float3 g_lightPos = float3(10,10,0); //the directional light in world space 
float g_FrameRate;

//changesOften
float g_ResponseDirLight = 1.0;
float dirLightIntensity = 1.0;
float3 g_TotalVel = float3(0,-0.25,0);
float g_SpriteSize = 1.0;



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
    
    //normalization factors for the rain textures, one per texture
    float g_rainfactors[370] = 
    {

        0.004535 , 0.014777 , 0.012512 , 0.130630 , 0.013893 , 0.125165 , 0.011809 , 0.244907 , 0.010722 , 0.218252,
        0.011450 , 0.016406 , 0.015855 , 0.055476 , 0.015024 , 0.067772 , 0.021120 , 0.118653 , 0.018705 , 0.142495, 
        0.004249 , 0.017267 , 0.042737 , 0.036384 , 0.043433 , 0.039413 , 0.058746 , 0.038396 , 0.065664 , 0.054761, 
        0.002484 , 0.003707 , 0.004456 , 0.006006 , 0.004805 , 0.006021 , 0.004263 , 0.007299 , 0.004665 , 0.007037, 
        0.002403 , 0.004809 , 0.004978 , 0.005211 , 0.004855 , 0.004936 , 0.006266 , 0.007787 , 0.006973 , 0.007911, 
        0.004843 , 0.007565 , 0.007675 , 0.011109 , 0.007726 , 0.012165 , 0.013179 , 0.021546 , 0.013247 , 0.012964, 
        0.105644 , 0.126661 , 0.128746 , 0.101296 , 0.123779 , 0.106198 , 0.123470 , 0.129170 , 0.116610 , 0.137528, 
        0.302834 , 0.379777 , 0.392745 , 0.339152 , 0.395508 , 0.334227 , 0.374641 , 0.503066 , 0.387906 , 0.519618, 
        0.414521 , 0.521799 , 0.521648 , 0.498219 , 0.511921 , 0.490866 , 0.523137 , 0.713744 , 0.516829 , 0.743649, 
        0.009892 , 0.013868 , 0.034567 , 0.025788 , 0.034729 , 0.036399 , 0.030606 , 0.017303 , 0.051809 , 0.030852, 
        0.018874 , 0.027152 , 0.031625 , 0.023033 , 0.038150 , 0.024483 , 0.029034 , 0.021801 , 0.037730 , 0.016639, 
        0.002868 , 0.004127 , 0.133022 , 0.013847 , 0.123368 , 0.012993 , 0.122183 , 0.015031 , 0.126043 , 0.015916, 
        0.002030 , 0.002807 , 0.065443 , 0.002752 , 0.069440 , 0.002810 , 0.081357 , 0.002721 , 0.076409 , 0.002990, 
        0.002425 , 0.003250 , 0.003180 , 0.011331 , 0.002957 , 0.011551 , 0.003387 , 0.006086 , 0.002928 , 0.005548, 
        0.003664 , 0.004258 , 0.004269 , 0.009404 , 0.003925 , 0.009233 , 0.004224 , 0.009405 , 0.004014 , 0.008435, 
        0.038058 , 0.040362 , 0.035946 , 0.072104 , 0.038315 , 0.078789 , 0.037069 , 0.077795 , 0.042554 , 0.073945, 
        0.124160 , 0.122589 , 0.121798 , 0.201886 , 0.122283 , 0.214549 , 0.118196 , 0.192104 , 0.122268 , 0.209397, 
        0.185212 , 0.181729 , 0.194527 , 0.420721 , 0.191558 , 0.437096 , 0.199995 , 0.373842 , 0.192217 , 0.386263, 
        0.003520 , 0.053502 , 0.060764 , 0.035197 , 0.055078 , 0.036764 , 0.048231 , 0.052671 , 0.050826 , 0.044863, 
        0.002254 , 0.023290 , 0.082858 , 0.043008 , 0.073780 , 0.035838 , 0.080650 , 0.071433 , 0.073493 , 0.026725, 
        0.002181 , 0.002203 , 0.112864 , 0.060140 , 0.115635 , 0.065531 , 0.093277 , 0.094123 , 0.093125 , 0.144290, 
        0.002397 , 0.002369 , 0.043241 , 0.002518 , 0.040455 , 0.002656 , 0.002540 , 0.090915 , 0.002443 , 0.101604, 
        0.002598 , 0.002547 , 0.002748 , 0.002939 , 0.002599 , 0.003395 , 0.002733 , 0.003774 , 0.002659 , 0.004583, 
        0.003277 , 0.003176 , 0.003265 , 0.004301 , 0.003160 , 0.004517 , 0.003833 , 0.008354 , 0.003140 , 0.009214, 
        0.008558 , 0.007646 , 0.007622 , 0.026437 , 0.007633 , 0.021560 , 0.007622 , 0.017570 , 0.007632 , 0.018037, 
        0.031062 , 0.028428 , 0.028428 , 0.108300 , 0.028751 , 0.111013 , 0.028428 , 0.048661 , 0.028699 , 0.061490, 
        0.051063 , 0.047597 , 0.048824 , 0.129541 , 0.045247 , 0.124975 , 0.047804 , 0.128904 , 0.045053 , 0.119087, 
        0.002197 , 0.002552 , 0.002098 , 0.200688 , 0.002073 , 0.102060 , 0.002111 , 0.163116 , 0.002125 , 0.165419, 
        0.002060 , 0.002504 , 0.002105 , 0.166820 , 0.002117 , 0.144274 , 0.005074 , 0.143881 , 0.004875 , 0.205333, 
        0.001852 , 0.002184 , 0.002167 , 0.163804 , 0.002132 , 0.212644 , 0.003431 , 0.244546 , 0.004205 , 0.315848, 
        0.002450 , 0.002360 , 0.002243 , 0.154635 , 0.002246 , 0.148259 , 0.002239 , 0.348694 , 0.002265 , 0.368426, 
        0.002321 , 0.002393 , 0.002376 , 0.074124 , 0.002439 , 0.126918 , 0.002453 , 0.439270 , 0.002416 , 0.489812, 
        0.002484 , 0.002629 , 0.002559 , 0.150246 , 0.002579 , 0.140103 , 0.002548 , 0.493103 , 0.002637 , 0.509481, 
        0.002960 , 0.002952 , 0.002880 , 0.294884 , 0.002758 , 0.332805 , 0.002727 , 0.455842 , 0.002816 , 0.431807, 
        0.003099 , 0.003028 , 0.002927 , 0.387154 , 0.002899 , 0.397946 , 0.002957 , 0.261333 , 0.002909 , 0.148548, 
        0.004887 , 0.004884 , 0.006581 , 0.414647 , 0.003735 , 0.431317 , 0.006426 , 0.148997 , 0.003736 , 0.080715, 
        0.001969 , 0.002159 , 0.002325 , 0.200211 , 0.002288 , 0.202137 , 0.002289 , 0.595331 , 0.002311 , 0.636097 

        };      
}




#include "rainFuncs.fx" 

//--------------------------------------------------------------------------------------------
// draw rain
//--------------------------------------------------------------------------------------------


VSParticleIn VSPassThroughRain(VSParticleIn input )
{
    return input;
}

// GS for rendering rain as point sprites.  Takes a point and turns it into 2 tris.
[maxvertexcount(4)]
void GSRenderRain(point VSParticleIn input[1], inout TriangleStream<PSSceneIn> SpriteStream)
{
    float totalIntensity = dirLightIntensity*g_ResponseDirLight;
    if(!cullSprite(input[0].pos,2*g_SpriteSize) && totalIntensity > 0)
    {    
        PSSceneIn output = (PSSceneIn)0;
        output.type = input[0].Type;
        output.random = input[0].random;
       
        float3 pos[4];
        GenRainSpriteVertices(input[0].pos.xyz, input[0].speed.xyz/g_FrameRate + g_TotalVel, g_eyePos, pos);         
		
        
        output.pos = mul( float4(pos[0],1.0), g_mWorldViewProj );
        output.lightDir = g_lightPos - pos[0];        
        output.eyeVec = g_eyePos - pos[0];
        output.tex = g_texcoords[0];
		output.color = input[0].color;
        SpriteStream.Append(output);
                
        output.pos = mul( float4(pos[1],1.0), g_mWorldViewProj );
        output.lightDir = g_lightPos - pos[1];        
        output.eyeVec = g_eyePos - pos[1];
        output.tex = g_texcoords[1];
		output.color = input[0].color;
        SpriteStream.Append(output);
        
        output.pos = mul( float4(pos[2],1.0), g_mWorldViewProj );
        output.lightDir = g_lightPos - pos[2];        
        output.eyeVec = g_eyePos - pos[2];
        output.tex = g_texcoords[2];
		output.color = input[0].color;
        SpriteStream.Append(output);
                
        output.pos = mul( float4(pos[3],1.0), g_mWorldViewProj );
        output.lightDir = g_lightPos - pos[3];        
        output.eyeVec = g_eyePos - pos[3];
        output.tex = g_texcoords[3];
		output.color = input[0].color;
        SpriteStream.Append(output);
        
        SpriteStream.RestartStrip();
    }   
}


float4 PSRenderRain(PSSceneIn input) : SV_Target
{               		
      //directional lighting---------------------------------------------------------------------------------
      float4 directionalLight = float4(0,0,0,0);
      rainResponse(input, input.lightDir, 2.0*dirLightIntensity*g_ResponseDirLight*input.random, float3(1.0,1.0,1.0), input.eyeVec, false, directionalLight);      
	  	  
      
	  if(input.color.x == 0 && input.color.y == 0 && input.color.z == 0) {
		 float totalOpacity = directionalLight.a;   
		 return float4( float3(directionalLight.rgb*directionalLight.a/totalOpacity), totalOpacity);   
	  } else {
		 float4 pointLight = float4(input.color,directionalLight.a * 2);
		 float totalOpacity = pointLight.a+directionalLight.a;   
		 return float4( float3(pointLight.rgb*pointLight.a/totalOpacity + directionalLight.rgb*directionalLight.a/totalOpacity), totalOpacity);   
	  }              
}


technique10 RenderParticles
{
    pass p0
    {
        SetVertexShader( CompileShader(   vs_4_0, VSPassThroughRain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSRenderRain() ) );
        SetPixelShader( CompileShader(    ps_4_0, PSRenderRain() ) );
        
        SetBlendState( CorrectBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepthTestingOnly, 0 );
        SetRasterizerState( CullNone );
    }  
}


//--------------------------------------------------------------------------------------------
// advance rain
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
	
	input.color = particleColor;

    //particle physics
    input.pos.xyz += input.speed.xyz/g_FrameRate + g_TotalVel.xyz;		 		 	 		 		 	        		 
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

GeometryShader gsStreamOut = ConstructGSWithSO( CompileShader( vs_4_0, VSAdvanceRain() ), "POSITION.xyz; SEED.xyz; SPEED.xyz; RAND.x; TYPE.x; COLOR.xyz" );  
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