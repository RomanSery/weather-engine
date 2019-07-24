using System;
using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Scripting;
using System.Xml.Serialization;

namespace WeatherGame.Framework.Objects
{
    public enum LightType
    {
        Point,
        Spot,
        Ambient,
        Area,
        Box
    }

    [Serializable]
    public abstract class Light : BaseGameObject
    {
        protected LightType type;        
        

        [NonSerialized]        
        protected ShaderResourceView haloSRV = null;
        protected string haloTextureName;

        [NonSerialized]        
        protected ShaderResourceView lightMap = null, lightMask = null;
        protected string lightMapTextureName, lightMaskTextureName;

        public Light()
        {
            canHaveRefs = true;            
                              
        }


        public ShaderResourceView HaloSRV
        {
            get
            {
                if (haloSRV == null && !string.IsNullOrEmpty(haloTextureName))
                {
                    haloSRV = TextureLoader.LoadTexture("flares/" + haloTextureName);
                }
                return haloSRV;
            }
            set
            {
                haloSRV = value;
            }
        }

        public ShaderResourceView LightMapSRV
        {
            get
            {
                if (lightMap == null && !string.IsNullOrEmpty(lightMapTextureName))
                {
                    lightMap = TextureLoader.LoadCubeTexture("fx/" + lightMapTextureName);
                }
                return lightMap;
            }
            set
            {
                lightMap = value;
            }
        }

        public ShaderResourceView LightMaskSRV
        {
            get
            {
                if (lightMask == null && !string.IsNullOrEmpty(lightMaskTextureName))
                {
                    lightMask = TextureLoader.LoadCubeTexture("fx/" + lightMaskTextureName);
                }
                return lightMask;
            }
            set
            {
                lightMask = value;
            }
        }



        public LightType Type
        {
            get { return type; }            
        }

        public string HaloTextureName
        {
            get { return haloTextureName; }
            set { haloTextureName = value; }
        }
        public string LightMapTextureName
        {
            get { return lightMapTextureName; }
            set { lightMapTextureName = value; }
        }
        public string LightMaskTextureName
        {
            get { return lightMaskTextureName; }
            set { lightMaskTextureName = value; }
        }

           

        public override void Update()
        {
           
        }

        public void DisposeLight()
        {
            if (lightMap != null && !lightMap.Disposed) lightMap.Dispose();
            if (lightMask != null && !lightMask.Disposed) lightMask.Dispose();
            if (haloSRV != null && !haloSRV.Disposed) haloSRV.Dispose();
        }

        public abstract void RenderDebugLightVolume(GameObjectReference objRef, Cell c, Vector4 GridColor);        
        public abstract Matrix GetLightVolumeWVP(Cell c, GameObjectReference objRef);

       
  

    }   

  
}
