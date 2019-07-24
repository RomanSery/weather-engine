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
DepthStencilState EnableDepthTestingOnly
{
        DepthEnable = true;
        DepthWriteMask = 0x00;
        DepthFunc = Less;
    
        StencilEnable = false;
};
SamplerState samAniso
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};
RasterizerState CullNone
{
    MultiSampleEnable = False;
    CullMode=None;
};

struct VSParticleIn
{   
    float3 pos              : POSITION;         //position of the particle
    float3 seed             : SEED;
    float3 speed            : SPEED;
    float random            : RAND;
    uint   Type             : TYPE;             //particle type
	float3 color             : COLOR;
};

struct PSSceneIn
{
    float4 pos : SV_Position;
    float3 lightDir   : LIGHT;    
    float3 eyeVec     : EYE;
    float2 tex : TEXTURE0;
    uint type  : TYPE;
    float random : RAND;
	float3 color : TEXCOORD2;
};


bool cullSprite( float3 position, float SpriteSize)
{
    float4 vpos = mul(float4(position,1), g_mWorldView);
    
    
    if( (vpos.z < (g_Near - SpriteSize )) || ( vpos.z > (g_Far + SpriteSize)) ) 
    {
        return true;
    }
    else 
    {
        float4 ppos = mul( vpos, g_mProjection);
        float wext = ppos.w + SpriteSize;
        if( (ppos.x < -wext) || (ppos.x > wext) ||
            (ppos.y < -wext) || (ppos.y > wext) ) {
            return true;
        }
        else 
        {
            return false;
        }
    }
    
    return false;
}

void GenRainSpriteVertices(float3 worldPos, float3 velVec, float3 eyePos, out float3 outPos[4])
{
    float height = g_SpriteSize/2.0;
    float width = height/10.0;

    velVec = normalize(velVec);
    float3 eyeVec = eyePos - worldPos;
    float3 eyeOnVelVecPlane = eyePos - ((dot(eyeVec, velVec)) * velVec);
    float3 projectedEyeVec = eyeOnVelVecPlane - worldPos;
    float3 sideVec = normalize(cross(projectedEyeVec, velVec));
    
    outPos[0] =  worldPos - (sideVec * 0.5*width);
    outPos[1] = outPos[0] + (sideVec * width);
    outPos[2] = outPos[0] + (velVec * height);
    outPos[3] = outPos[2] + (sideVec * width );
}

void rainResponse(PSSceneIn input, float3 lightVector, float lightIntensity, float3 lightColor, float3 eyeVector, bool fallOffFactor, inout float4 rainResponseVal)
{
    
    float opacity = 0.0;

    float fallOff;
    if(fallOffFactor)
    {  
        float distToLight = length(lightVector);
        fallOff = 1.0/( distToLight * distToLight);
        fallOff = saturate(fallOff);   
    }
    else
    {  fallOff = 1;
    }

    if(fallOff > 0.01 && lightIntensity > 0.01 )
    {
        float3 dropDir = g_TotalVel;

        #define MAX_VIDX 4
        #define MAX_HIDX 8
        // Inputs: lightVector, eyeVector, dropDir
        float3 L = normalize(lightVector);
        float3 E = normalize(eyeVector);
        float3 N = normalize(dropDir);
        
        bool is_EpLp_angle_ccw = true;
        float hangle = 0;
        float vangle = abs( (acos(dot(L,N)) * 180/PI) - 90 ); // 0 to 90
        
        {
            float3 Lp = normalize( L - dot(L,N)*N );
            float3 Ep = normalize( E - dot(E,N)*N );
            hangle = acos( dot(Ep,Lp) ) * 180/PI;  // 0 to 180
            hangle = (hangle-10)/20.0;           // -0.5 to 8.5
            is_EpLp_angle_ccw = dot( N, cross(Ep,Lp)) > 0;
        }
        
        if(vangle>=88.0)
        {
            hangle = 0;
            is_EpLp_angle_ccw = true;
        }
                
        vangle = (vangle-10.0)/20.0; // -0.5 to 4.5
        
        // Outputs:
        // verticalLightIndex[1|2] - two indices in the vertical direction
        // t - fraction at which the vangle is between these two indices (for lerp)
        int verticalLightIndex1 = floor(vangle); // 0 to 5
        int verticalLightIndex2 = min(MAX_VIDX, (verticalLightIndex1 + 1) );
        verticalLightIndex1 = max(0, verticalLightIndex1);
        float t = frac(vangle);

        // textureCoordsH[1|2] used in case we need to flip the texture horizontally
        float textureCoordsH1 = input.tex.x;
        float textureCoordsH2 = input.tex.x;
        
        // horizontalLightIndex[1|2] - two indices in the horizontal direction
        // s - fraction at which the hangle is between these two indices (for lerp)
        int horizontalLightIndex1 = 0;
        int horizontalLightIndex2 = 0;
        float s = 0;
        
        s = frac(hangle);
        horizontalLightIndex1 = floor(hangle); // 0 to 8
        horizontalLightIndex2 = horizontalLightIndex1+1;
        if( horizontalLightIndex1 < 0 )
        {
            horizontalLightIndex1 = 0;
            horizontalLightIndex2 = 0;
        }
                   
        if( is_EpLp_angle_ccw )
        {
            if( horizontalLightIndex2 > MAX_HIDX ) 
            {
                horizontalLightIndex2 = MAX_HIDX;
                textureCoordsH2 = 1.0 - textureCoordsH2;
            }
        }
        else
        {
            textureCoordsH1 = 1.0 - textureCoordsH1;
            if( horizontalLightIndex2 > MAX_HIDX ) 
            {
                horizontalLightIndex2 = MAX_HIDX;
            } else 
            {
                textureCoordsH2 = 1.0 - textureCoordsH2;
            }
        }
                
        if( verticalLightIndex1 >= MAX_VIDX )
        {
            textureCoordsH2 = input.tex.x;
            horizontalLightIndex1 = 0;
            horizontalLightIndex2 = 0;
            s = 0;
        }
        
        // Generate the final texture coordinates for each sample
        uint type = input.type;
        uint2 texIndicesV1 = uint2(verticalLightIndex1*90 + horizontalLightIndex1*10 + type,
                                     verticalLightIndex1*90 + horizontalLightIndex2*10 + type);
        float3 tex1 = float3(textureCoordsH1, input.tex.y, texIndicesV1.x);
        float3 tex2 = float3(textureCoordsH2, input.tex.y, texIndicesV1.y);
        if( (verticalLightIndex1<4) && (verticalLightIndex2>=4) ) 
        {
            s = 0;
            horizontalLightIndex1 = 0;
            horizontalLightIndex2 = 0;
            textureCoordsH1 = input.tex.x;
            textureCoordsH2 = input.tex.x;
        }
        
        uint2 texIndicesV2 = uint2(verticalLightIndex2*90 + horizontalLightIndex1*10 + type,
                                     verticalLightIndex2*90 + horizontalLightIndex2*10 + type);
        float3 tex3 = float3(textureCoordsH1, input.tex.y, texIndicesV2.x);        
        float3 tex4 = float3(textureCoordsH2, input.tex.y, texIndicesV2.y);

        // Sample opacity from the textures
        float col1 = rainTextureArray.Sample( samAniso, tex1) * g_rainfactors[texIndicesV1.x];
        float col2 = rainTextureArray.Sample( samAniso, tex2) * g_rainfactors[texIndicesV1.y];
        float col3 = rainTextureArray.Sample( samAniso, tex3) * g_rainfactors[texIndicesV2.x];
        float col4 = rainTextureArray.Sample( samAniso, tex4) * g_rainfactors[texIndicesV2.y];

        // Compute interpolated opacity using the s and t factors
        float hOpacity1 = lerp(col1,col2,s);
        float hOpacity2 = lerp(col3,col4,s);
        opacity = lerp(hOpacity1,hOpacity2,t);
        opacity = pow(opacity,0.7); // inverse gamma correction (expand dynamic range)
        opacity = 4*lightIntensity * opacity * fallOff;
    }
         
   rainResponseVal = float4(lightColor,opacity);

}