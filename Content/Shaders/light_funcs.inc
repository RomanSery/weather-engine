float4 blinn_phong( in float3 normal, in float3 viewer, in float3 light, in int materialID, in float4 cDiffuse, 
	in float atten, in bool spec, in float4 lColor, in float lIntensity,
	in float4 mask, in float4 map	)
{        
 	float3 specular = 0.0f;
 	if(spec) {
 		// Compute the half vector
    	float3 half_vector = normalize(light + viewer);
    
	    // Compute the angle between the half vector and normal
	    float  HdotN = max( 0.0f, dot( half_vector, normal ) );
	 
	    // Compute the specular colour
	    specular = mat_SpecularIntensity(materialID) * lColor * pow( HdotN, mat_SpecularPower(materialID) );
	}
 
    // Compute the diffuse term
    float3 diffuse = (cDiffuse * lColor * lIntensity * map) * max( 0.0f, dot( normal, light ) );
 
    // Determine the final colour    
    return (float4( diffuse + (specular * mask), 1.0f ) * atten) * mask;
}


float4 cook_torrance( in float3 normal, in float3 viewer, in float3 light, in int materialID, in float4 cDiffuse, 
	in float atten, in bool spec, in float4 lColor, in float lIntensity,
	in float4 mask, in float4 map	)
{    
	float roughness_value = mat_Roughness(materialID);
	float ref_at_norm_incidence = mat_RefAtNormIncidence(materialID);

    // Compute any aliases and intermediary values    
    float3 half_vector = normalize( light + viewer );
    float NdotL        = saturate( dot( normal, light ) );
    float NdotH        = saturate( dot( normal, half_vector ) );
    float NdotV        = saturate( dot( normal, viewer ) );
    float VdotH        = saturate( dot( viewer, half_vector ) );
    float r_sq         = roughness_value * roughness_value; 
 
    // Evaluate the geometric term    
    float geo_numerator   = 2.0f * NdotH;
    float geo_denominator = VdotH;
 
    float geo_b = (geo_numerator * NdotV ) / geo_denominator;
    float geo_c = (geo_numerator * NdotL ) / geo_denominator;
    float geo   = min( 1.0f, min( geo_b, geo_c ) ); 
 
    // Now evaluate the roughness term      
    float roughness_a = 1.0f / ( 4.0f * r_sq * pow( NdotH, 4 ) );
    float roughness_b = NdotH * NdotH - 1.0f;
    float roughness_c = r_sq * NdotH * NdotH; 
    float roughness = roughness_a * exp( roughness_b / roughness_c );  
 
    // Next evaluate the Fresnel value    
    float fresnel = pow( 1.0f - VdotH, 5.0f );
    fresnel *= ( 1.0f - ref_at_norm_incidence );
    fresnel += ref_at_norm_incidence; 
 
    // Put all the terms together to compute the specular term in the equation   
    float3 Rs = 1;
	if(spec) {    
	    float3 Rs_numerator   = ( fresnel * geo * roughness );
	    float Rs_denominator  = NdotV * NdotL;
	    Rs             = Rs_numerator/ Rs_denominator; 
    }
 
    // Put all the parts together to generate the final colour    
    float3 final = mask * atten * max(0.0f, NdotL) * (lColor * lIntensity * map * (Rs * mask) + cDiffuse);     
    return float4( final, 1.0f );
}


float strauss_fresnel(float x)
{
	const float Kf = 2.12f;
	float x_minus_kf = x - Kf;
	float one_minus_kf = 1 - Kf;
	float kf_sq = Kf * Kf;

	float fres_top_a = 1.0f / (x_minus_kf * x_minus_kf);
	float fres_top_b = 1.0f / kf_sq;

	float fres_bottom_a = 1.0f / (one_minus_kf * one_minus_kf);
	float fres_bottom_b = 1.0f / kf_sq;
	
	return (fres_top_a - fres_top_b) / (fres_bottom_a - fres_bottom_b);
}

float strauss_shadow(float x)
{	
	const float Ks = 1.01f;
	float x_minus_ks = x - Ks;
	float one_minus_ks = 1 - Ks;
	float ks_sq = Ks * Ks;


	float shadow_top_a = 1.0f / (one_minus_ks * one_minus_ks);
	float shadow_top_b = 1.0f / (x_minus_ks * x_minus_ks);

	float shadow_bottom_a = 1.0f / (one_minus_ks * one_minus_ks);
	float shadow_bottom_b = 1.0f / ks_sq;
	
	return (shadow_top_a - shadow_top_b) / (shadow_bottom_a - shadow_bottom_b);
}

float4 strauss_shading( in float3 normal, in float3 viewer, in float3 light, in int materialID, in float4 cDiffuse, 
	in float atten, in bool spec, in float4 lColor, in float lIntensity,
	in float4 mask, in float4 map	)
{  
	float fTransparency = 0.0f; //completley opaque
	float fSmoothness = mat_Smoothness(materialID);
	float fMetalness = mat_Metalness(materialID);
	
    // Make sure the interpolated inputs and
    // constant parameters are normalized
    float3 n = normalize( normal );
    float3 l = normalize( light );
    float3 v = normalize( viewer );
    float3 h = reflect( l, n );
 
    // Declare any aliases:
    float NdotL   = dot( n, l );
    float NdotV   = dot( n, v );
    float HdotV   = dot( h, v );
    float fNdotL  = strauss_fresnel( NdotL );
    float s_cubed = fSmoothness * fSmoothness * fSmoothness;
 
    // Evaluate the diffuse term
    float d  = ( 1.0f - fMetalness * fSmoothness );
    float Rd = ( 1.0f - s_cubed ) * ( 1.0f - fTransparency );
    float3 diffuse = NdotL * d * Rd * cDiffuse * map;
 
    // Compute the inputs into the specular term
    float r = ( 1.0f - fTransparency ) - Rd;
 
    float j = fNdotL * strauss_shadow( NdotL ) * strauss_shadow( NdotV );
 
    // 'k' is used to provide small off-specular peak for very rough surfaces. Can be changed to suit desired results...
    const float k = 0.1f;
    float reflect = min( 1.0f, r + j * ( r + k ) );
 
    float3 C1 = float3( 1.0f, 1.0f, 1.0f );
    float3 Cs = C1 + fMetalness * (1.0f - fNdotL) * (cDiffuse - C1);
 
    // Evaluate the specular term
    float3 specular = 0;
    if(spec) {
    	specular = Cs * reflect;
    	specular *= pow( -HdotV, 3.0f / (1.0f - fSmoothness) );
    }
 
    // Composite the final result, ensuring the values are >= 0.0f yields better results. Some combinations of inputs generate negative values which looks wrong when rendered...
    diffuse  = max( 0.0f, diffuse );
    specular = max( 0.0f, specular );       
    return mask * atten * float4( lColor * lIntensity * map * (diffuse + (specular * mask)), 1.0f );
}


float4 ward_anisotropic( in float3 normal, in float3 viewer, in float3 light, in int materialID, in float4 cDiffuse, 
	in float atten, in bool spec, in float4 lColor, in float lIntensity,
	in float4 mask, in float4 map	)
{	
	float2 fAnisotropicRoughness = float2(mat_AnisotropicRoughnessX(materialID), mat_AnisotropicRoughnessY(materialID));

    // Make sure the interpolated inputs and
    // constant parameters are normalized
    float3 n = normalize( normal );
    float3 l = normalize( viewer );
    float3 v = normalize( light );
    float3 h = normalize( l + v );
 
    // Apply a small bias to the roughness
    // coefficients to avoid divide-by-zero
    fAnisotropicRoughness += float2( 1e-5f, 1e-5f );
 
    // Define the coordinate frame
    float3 epsilon   = float3( 1.0f, 0.0f, 0.0f );
    float3 tangent   = normalize( cross( n, epsilon ) );
    float3 bitangent = normalize( cross( n, tangent ) );
 
    // Define material properties
    float3 Ps   = float3( 1.0f, 1.0f, 1.0f );
 
    // Generate any useful aliases
    float VdotN = dot( v, n );
    float LdotN = dot( l, n );
    float HdotN = dot( h, n );
    float HdotT = dot( h, tangent );
    float HdotB = dot( h, bitangent );
 
    // Evaluate the specular exponent
    float beta_a  = HdotT / fAnisotropicRoughness.x;
    beta_a       *= beta_a;
 
    float beta_b  = HdotB / fAnisotropicRoughness.y;
    beta_b       *= beta_b;
 
    float beta = -2.0f * ( ( beta_a + beta_b ) / ( 1.0f + HdotN ) );
 
    // Evaluate the specular denominator
    float s_den  = 4.0f * 3.14159f; 
    s_den       *= fAnisotropicRoughness.x;
    s_den       *= fAnisotropicRoughness.y;
    s_den       *= sqrt( LdotN * VdotN );
 
    // Compute the final specular term
    float3 Specular = 0;
 	if(spec) {
 		Specular = Ps * ( exp( beta ) / s_den );
 	}
 	
    // Composite the final value:
    float3 color = lColor * lIntensity * map * (cDiffuse + (Specular * mask) );
    return mask * atten * float4( dot( n, l ) * color, 1.0f );
}

