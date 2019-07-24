cbuffer cbMaterials
{	
	float4 mat_Properties1[50];	
	float4 mat_Properties2[50];
	float4 mat_Properties3[50];
	float4 mat_Properties4[50];
}
#define mat_SpecularPower(id) (mat_Properties1[id].x)
#define mat_SpecularIntensity(id) (mat_Properties1[id].y)
#define mat_Reflectivity(id) (mat_Properties1[id].z)
#define mat_Emissive(id) (mat_Properties1[id].w)

#define mat_Glow(id) (mat_Properties2[id].x)
#define mat_ReflectionSmoothness(id) (mat_Properties2[id].y)
#define mat_LightModel(id) (mat_Properties2[id].z)


#define mat_Roughness(id) (mat_Properties3[id].x)
#define mat_RefAtNormIncidence(id) (mat_Properties3[id].y)
#define mat_Smoothness(id) (mat_Properties3[id].z)
#define mat_Metalness(id) (mat_Properties3[id].w)

#define mat_AnisotropicRoughnessX(id) (mat_Properties4[id].x)
#define mat_AnisotropicRoughnessY(id) (mat_Properties4[id].y)


#define BlinnPhong 1
#define CookTorrance 2
#define Strauss 3
#define Ward 4