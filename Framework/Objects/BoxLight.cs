using System;
using SlimDX;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;

namespace WeatherGame.Framework.Objects
{

    [Serializable]
    public class BoxLight : Light
    {

        public BoxLight()
            : base() 
        {
            canHaveRefs = true;
            type = LightType.Box;                                 
        }


        public override void RenderDebugLightVolume(GameObjectReference objRef, Cell c, Vector4 GridColor)
        {
            Vector3 max = DeferredRenderer.lightVolume[(int)LightType.Box].Mesh3d.bb.Maximum;
            Vector3 min = DeferredRenderer.lightVolume[(int)LightType.Box].Mesh3d.bb.Minimum;   

            Matrix m = GetLightVolumeMatrix(c, objRef);

            Vector3 pos = objRef.Position;
            pos.Y += objRef.BoxHeight;
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Box], Matrix.Translation(WorldSpace.GetRealWorldPos(pos, c)), GridColor);            
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Box], m, GridColor);
            
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Box], Matrix.Scaling(2,2,2) * Matrix.Translation(WorldSpace.GetRealWorldPos(objRef.Target, c)), GridColor);            
        }
        public override Matrix GetLightVolumeWVP(Cell c, GameObjectReference objRef)
        {            
            return GetLightVolumeMatrix(c, objRef) * Camera.ViewMatrix * Camera.ProjectionMatrix;
        }
        public Matrix GetLightVolumeMatrix(Cell c, GameObjectReference objRef)
        {
            return Matrix.Scaling(objRef.BoxDepth, objRef.BoxHeight, objRef.BoxWidth) * Matrix.Translation(WorldSpace.GetRealWorldPos(objRef.Position, c));           
        }

        public override void Dispose()
        {
            DisposeLight();
        }
       

       

    }     

  
}
