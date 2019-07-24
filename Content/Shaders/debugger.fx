#include "common.inc" 

struct VS_IN
{
	float4 Pos : POSITION;	
	float3 Norm : NORMAL;	
};

struct PS_INPUT
{
	float4 Pos : SV_POSITION;			
	float3 Norm : TEXCOORD1;	
};


PS_INPUT VS_WIREFRAME( VS_IN input )
{
	PS_INPUT output = (PS_INPUT)0;	
	output.Pos = mul( input.Pos, WorldViewProj );		
	return output;
}

float4 PS_WIREFRAME( PS_INPUT pixelShaderIn ) : SV_Target
{    
	return float4(1,1,1,1);	
}




PS_INPUT VS_NORMALS( VS_IN input )
{
	PS_INPUT output = (PS_INPUT)0;		
	output.Pos = input.Pos;		
	//output.Pos = mul( input.Pos, WorldViewProj );	
	output.Norm = normalize(input.Norm);	
	return output;
}
float4 PS_NORMALS( PS_INPUT pixelShaderIn ) : SV_Target
{    
	return float4(0,1,0,1);	
}

[maxvertexcount(2)]
void GS_NORMALS( triangle PS_INPUT input[3], inout LineStream<PS_INPUT> OutputStream )
{        

	float3 TriNormal = GetNormal( input[0].Pos, input[1].Pos, input[2].Pos );
	float3 midPoint = GetMidPoint(input[0].Pos, input[1].Pos, input[2].Pos);	

    PS_INPUT output = (PS_INPUT)0;        
    output.Pos = mul( float4(midPoint,1), WorldViewProj );
    OutputStream.Append(output);    
    
    float3 newPos = float4(midPoint,1) - (TriNormal * 5.0f);        
    output.Pos = mul( float4(newPos,1), WorldViewProj );
    OutputStream.Append(output);       
    
    OutputStream.RestartStrip();
}


technique10 RenderNormals
{
	pass P0
	{
		SetGeometryShader( CompileShader( gs_4_0, GS_NORMALS() ) );
		SetVertexShader( CompileShader( vs_4_0, VS_NORMALS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_NORMALS() ) );		
		
		SetRasterizerState( RS_NormalWireframe );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}



technique10 RenderWireframe
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_WIREFRAME() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_WIREFRAME() ) );		
		
		SetRasterizerState( RS_NormalWireframe );		
		SetDepthStencilState( DS_EnableDepth, 0 );
        SetBlendState( BS_NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );		
	}	
}


