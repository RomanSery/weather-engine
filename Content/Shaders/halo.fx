Texture2D singleTexture;
float4x4 g_mInvView;
float4x4 g_mViewProj;

Texture2DArray g_txTextures;

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


BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
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

RasterizerState CullBack
{
    MultiSampleEnable = True;
    CullMode=Back;
};

struct VSParticleIn
{   
    float3 pos  : POSITION;    	
	float4 color : COLOR;
	uint texIndex : texIndex;
};

struct PSRainIn
{
    float4 pos : SV_Position;
    float2 tex : TEXTURE0;	
	float4 color : COLOR;
	uint texIndex : texIndex;
};

VSParticleIn VSPassThroughRain(VSParticleIn input )
{		
    return input;
}

// GS for rendering rain as point sprites.  Takes a point and turns it into 2 tris.
[maxvertexcount(4)]
void GSRenderRainCheap(point VSParticleIn input[1], inout TriangleStream<PSRainIn> SpriteStream)
{
	PSRainIn output;     	
    for(int i=0; i<4; i++)
    {	
        float3 position = g_positions[i] * 10;
        position = mul( position, (float3x3)g_mInvView ) + input[0].pos;		
        output.pos = mul( float4(position,1.0), g_mViewProj );                

        output.tex = g_texcoords[i];	
		output.color = input[0].color;	
		output.texIndex = input[0].texIndex;	
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
       
}

float4 PSRenderRainCheap(PSRainIn input) : SV_Target
{   	
	float3 tex = float3(input.tex,input.texIndex);
	float4 c = g_txTextures.Sample(samAniso, tex);
	//float4 c = singleTexture.Sample(samAniso, input.tex);
	
	c.rgb *= input.color.rgb;
	//c.a = input.color.a;
	return c;    
}


technique10 RenderParticlesCheap
{
    pass p0
    {
        SetVertexShader( CompileShader(   vs_4_0, VSPassThroughRain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSRenderRainCheap() ) );
        SetPixelShader( CompileShader(    ps_4_0, PSRenderRainCheap() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepthTestingOnly, 0 );
        SetRasterizerState( CullBack );
    }  
}

