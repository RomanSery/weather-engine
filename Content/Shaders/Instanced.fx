#include "common.inc" 

struct VSInstIn
{
    float4 Pos : POSITION;    
    row_major float4x4 mTransform : mTransform;
};

struct PSSceneIn
{
    float4 Pos : SV_Position;    
};


//--------------------------------------------------------------------------------------
// Instancing vertex shader.  Positions the vertices based upon the matrix stored
// in the second vertex stream.
//--------------------------------------------------------------------------------------
PSSceneIn VSInstmain(VSInstIn input)
{
    PSSceneIn output;    
    
    // Transform by our Sceneance matrix    
    float4 InstancePosition = mul(input.Pos, input.mTransform);
    //float4 ViewPos = mul(InstancePosition, WorldView );    
    
    // Transform the vert to view-space        
    output.Pos = mul(InstancePosition, WorldViewProj);    
    
    //output.Pos = mul( input.Pos, WorldViewProj );
    
    return output;
}

//--------------------------------------------------------------------------------------
// PS for non-leaf or grass items.
//--------------------------------------------------------------------------------------
float4 PSScenemain(PSSceneIn input) : SV_Target
{    
    return float4(0.1,0.1,0.1,0.1);
}


//--------------------------------------------------------------------------------------
// Render instanced meshes with vertex lighting
//--------------------------------------------------------------------------------------
technique10 RenderInstanced
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSInstmain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DS_EnableDepth, 0 );
        SetRasterizerState( RS_NormalWireframe );
    }  
}


