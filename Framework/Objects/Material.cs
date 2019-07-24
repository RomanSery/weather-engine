using System;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Utilities;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace WeatherGame.Framework.Objects
{
    public enum MaterialAttribute
    {
        IncludeInReflectionMap,
        UseReflectionMap,
        RecieveRainSplashes,
        RecieveSnow,
        IsGlow        
    }

    public enum LightingModel
    {
        BlinnPhong = 1,
        CookTorrance = 2,
        Strauss = 3,
        Ward = 4
    }

    [Serializable]
    public class Material : BaseGameObject
    {
        private int materialIndex;

        private int specularPower;
        private float specularIntensity;
        private float reflectivity;
        private float reflectionSmoothness;
        private float emissive;

        //CookTorrance
        private float roughness;
        private float ref_at_norm_incidence;

        //Strauss
        private float smoothness;
        private float metalness;

        //ward
        private float anisotropicRoughnessX;
        private float anisotropicRoughnessY;


        private LightingModel lModel;


        private string cubeMapTextureName;
        [NonSerialized]        
        private ShaderResourceView cubeMapSRV;

        private string detailMapTextureName;
        [NonSerialized]
        private ShaderResourceView detailMapSRV;

        private Dictionary<MaterialAttribute, bool> attributes;

        public Material() 
        {
            canHaveRefs = false;

            specularPower = 1;
            specularIntensity = 1;
            reflectivity = 0;
            reflectionSmoothness = 0.5f;
            emissive = 0;
            materialIndex = -1;

            smoothness = 0.5f;
            metalness = 0.5f;

            roughness = 0.5f;
            ref_at_norm_incidence = 1.0f;

            anisotropicRoughnessX = 0.5f;
            anisotropicRoughnessY = 0.5f;

            lModel = LightingModel.BlinnPhong;

            cubeMapTextureName = null;
            cubeMapSRV = null;

            detailMapTextureName = null;
            detailMapSRV = null;      
        }

        public int MaterialIndex
        {
            get { return materialIndex; }
            set { materialIndex = value; }
        }

        public int SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; }
        }

        public float SpecularIntensity
        {
            get { return specularIntensity; }
            set { specularIntensity = value; }
        }

        public float Reflectivity
        {
            get { return reflectivity; }
            set { reflectivity = value; }
        }

        public float ReflectionSmoothness
        {
            get { return reflectionSmoothness; }
            set { reflectionSmoothness = value; }
        }

        public float Roughness
        {
            get { return roughness; }
            set { roughness = value; }
        }
        public float RefAtNormIncidence
        {
            get { return ref_at_norm_incidence; }
            set { ref_at_norm_incidence = value; }
        }

        public LightingModel LightModel
        {
            get { return lModel; }
            set { lModel = value; }
        }


        public float Smoothness
        {
            get { return smoothness; }
            set { smoothness = value; }
        }
        public float Metalness
        {
            get { return metalness; }
            set { metalness = value; }
        }


        public float AnisotropicRoughnessX
        {
            get { return anisotropicRoughnessX; }
            set { anisotropicRoughnessX = value; }
        }
        public float AnisotropicRoughnessY
        {
            get { return anisotropicRoughnessY; }
            set { anisotropicRoughnessY = value; }
        }


        public float Emissive
        {
            get { return emissive; }
            set { emissive = value; }
        }

        public string CubeMapTextureName
        {
            get { return cubeMapTextureName; }
            set { cubeMapTextureName = value; }
        }

        public bool getAttribute(MaterialAttribute r)
        {
            initAttributes();
            return attributes.ContainsKey(r) ? attributes[r] : false;
        }
        public void setAttribute(MaterialAttribute r, bool val)
        {
            initAttributes();
            attributes[r] = val;
        }
        private void initAttributes()
        {
            if (attributes == null)
            {
                attributes = new Dictionary<MaterialAttribute, bool>();
                attributes[MaterialAttribute.IsGlow] = false;
                attributes[MaterialAttribute.RecieveRainSplashes] = true;
                attributes[MaterialAttribute.RecieveSnow] = true;                
                attributes[MaterialAttribute.UseReflectionMap] = false;
                attributes[MaterialAttribute.IncludeInReflectionMap] = false;
            }
        }

        public ShaderResourceView CubeMapSRV
        {
            get
            {
                if (cubeMapSRV == null && !string.IsNullOrEmpty(cubeMapTextureName))
                {
                    cubeMapSRV = TextureLoader.LoadCubeTexture("fx/" + cubeMapTextureName);
                }
                return cubeMapSRV;
            }
            set
            {
                cubeMapSRV = value;
            }
        }

        public string DetailMapTextureName
        {
            get { return detailMapTextureName; }
            set { detailMapTextureName = value; }
        }

        public ShaderResourceView DetailMapSRV
        {
            get
            {
                if (detailMapSRV == null && !string.IsNullOrEmpty(detailMapTextureName))
                {
                    detailMapSRV = TextureLoader.LoadTexture("detail/" + detailMapTextureName);
                }
                return detailMapSRV;
            }
            set
            {
                detailMapSRV = value;
            }
        }        

        public override void Dispose()
        {
            if (cubeMapSRV != null && !cubeMapSRV.Disposed)
                cubeMapSRV.Dispose();

            if (detailMapSRV != null && !detailMapSRV.Disposed)
                detailMapSRV.Dispose();
        }

    }
}
