using System;
using SlimDX;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Objects
{
    
    [Serializable]
    public class PointLight : Light
    {      
        public PointLight() : base() 
        {
            canHaveRefs = true;
            type = LightType.Point;
        }

        public override void RenderDebugLightVolume(GameObjectReference objRef, Cell c, Vector4 GridColor)
        {
            Matrix scale, translate, sphereMatWVP;
            translate = WorldSpace.GetRealWorldMatrix(objRef.Position, c);
            scale = Matrix.Scaling(objRef.MaxRange, objRef.MaxRange, objRef.MaxRange);
            sphereMatWVP = scale * translate;            

            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Point], sphereMatWVP, GridColor);
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Point], translate, GridColor);
        }

        public override Matrix GetLightVolumeWVP(Cell c, GameObjectReference objRef)
        {            
            return GetLightVolumeWorldMatrix(c, objRef) * Camera.ViewMatrix * Camera.ProjectionMatrix;
        }
        public Matrix GetLightVolumeWorldMatrix(Cell c, GameObjectReference objRef)
        {
            float fScale = objRef.MaxRange * 1.1f;
            Matrix scale, translate, world = Matrix.Identity;
            translate = WorldSpace.GetRealWorldMatrix(objRef.Position, c);
            scale = Matrix.Scaling(fScale, fScale, fScale);
            world = Matrix.Multiply(world, scale);
            world = Matrix.Multiply(world, translate);
            return world;
        }

        public override void Dispose()
        {
            DisposeLight();
        }
    }       

  
}
