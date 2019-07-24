float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProj;
float4x4 WorldView;
float4x4 ViewProj;
float4x4 InverseViewProjection;
float3 cameraPosition; 
Texture2D diffTexture;
SamplerState diffTexSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

RasterizerState RS_NormalWireframe
{
    CullMode = NONE;
	FillMode = WireFrame;	
	AntialiasedLineEnable = True;	
	DEPTHBIAS = -0.0001f;
};
RasterizerState RS_NormalSolid
{
    CULLMODE = BACK;
	FILLMODE = SOLID;		
};
RasterizerState RS_NormalSolidNoCulling
{
    CULLMODE = NONE;
	FILLMODE = SOLID;	
	MultisampleEnable = TRUE;
};
DepthStencilState DS_EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;   
};
DepthStencilState DS_NoDepth
{
    DepthEnable = FALSE;   
    DEPTHWRITEMASK = ZERO; 
};

BlendState BS_NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};


BlendState BS_Blending2
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = SRC_ALPHA;
    DestBlendAlpha = INV_SRC_ALPHA;
    BlendOpAlpha = ADD;
    BlendEnable[0] = TRUE;
};

BlendState BS_Blending
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    
    BlendEnable[0] = TRUE;
};


SamplerState BaseSampler
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};
SamplerState samAnisoMirror
{
  Filter = ANISOTROPIC;
  AddressU = Mirror;
  AddressV = Mirror;
};


float3 GetNormal( float4 A, float4 B, float4 C )
{
	return normalize( cross(A.xyz - B.xyz, C.xyz - B.xyz) );	
}
float3 GetMidPoint( float4 A, float4 B, float4 C )
{	
	return (A.xyz + B.xyz + C.xyz) / 3.0f;	
}



float3 getViewSpacePosFromDepth(float depthVal, float4 positionVS)
{	
	float3 viewRay = float3(positionVS.xy / positionVS.z, 1.0f);
	
	float FarClipDistance = 4000;
	float NearClipDistance = 1;
	float ProjectionA = FarClipDistance / (FarClipDistance - NearClipDistance);
	float ProjectionB = (-FarClipDistance * NearClipDistance) / (FarClipDistance - NearClipDistance);
	
	
	float linearDepth = ProjectionB / (depthVal - ProjectionA);
	return viewRay * linearDepth;	
}

float3 getViewSpacePosFromDepthFSQuad(float depthVal, float2 texCoord, float4x4 gMatInvProj)
{		
	// H is the viewport position at this pixel in the range -1 to 1.  
	float4 H = float4(texCoord.x * 2 - 1, (1 - texCoord.y) * 2 - 1, depthVal, 1);  
	// Transform by the view-projection inverse.  
	float4 D = mul(H, gMatInvProj);
	// Divide by w to get the view position.  
	float4 viewPos = D / D.w;  
    return viewPos.xyz;		
}




float3 getWorldSpacePosFromDepth(float depthVal, float3 viewRay, float3 eyeZAxis, float3 camPos)
{			
	float FarClipDistance = 4000;
	float NearClipDistance = 1;
	float ProjectionA = FarClipDistance / (FarClipDistance - NearClipDistance);
	float ProjectionB = (-FarClipDistance * NearClipDistance) / (FarClipDistance - NearClipDistance);
	
	// Sample the depth buffer and convert it to linear depth
	float linearDepth = ProjectionB / (depthVal - ProjectionA);

	// Project the view ray onto the camera's z-axis
	float viewZDist = dot(-eyeZAxis, viewRay);

	// Scale the view ray by the ratio of the linear z value to the projected view ray
	float3 positionWS = camPos + viewRay * (linearDepth / viewZDist);
	
	return positionWS;
}

float3 getWorldSpacePosFromDepthFSQuad(float depthVal, float2 texCoord, float4x4 gMatInvViewProj)
{
	// H is the viewport position at this pixel in the range -1 to 1.  
	float4 H = float4(texCoord.x * 2 - 1, (1 - texCoord.y) * 2 - 1, depthVal, 1);  
	// Transform by the view-projection inverse.  
	float4 D = mul(H, gMatInvViewProj);  
	// Divide by w to get the world position.  
	float4 worldPos = D / D.w;  
    return worldPos.xyz;	
}



float2 encode_normal (float3 n)
{
    float2 enc = normalize(n.xy) * (sqrt(-n.z*0.5+0.5));
    enc = enc*0.5+0.5;
    return enc;
}
float3 decode_normal (float4 enc)
{
    float4 nn = enc*float4(2,2,0,0) + float4(-1,-1,1,-1);
    float l = dot(nn.xyz,-nn.xyw);
    nn.z = l;
    nn.xy *= sqrt(l);
    return nn.xyz * 2 + float3(0,0,-1);
}