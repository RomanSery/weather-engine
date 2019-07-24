using System;
using SlimDX;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;

namespace WeatherGame.Framework.Objects
{

    [Serializable]
    public class AreaLight : Light
    {      

        public AreaLight() : base() 
        {
            canHaveRefs = true;
            type = LightType.Area;                                 
        }


        public override void RenderDebugLightVolume(GameObjectReference objRef, Cell c, Vector4 GridColor)
        {            
            Matrix m = GetLightVolumeMatrix(c, objRef.Position, objRef.EndPoint, objRef.MaxRange);

            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Area], m, GridColor);
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Area], Matrix.Scaling(2, 2, 2) * WorldSpace.GetRealWorldMatrix(objRef.Position, c), GridColor);
            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Area], Matrix.Scaling(2, 2, 2) * WorldSpace.GetRealWorldMatrix(objRef.EndPoint, c), GridColor);
        }
        public override Matrix GetLightVolumeWVP(Cell c, GameObjectReference objRef)
        {            
            return GetLightVolumeMatrix(c, objRef.Position, objRef.EndPoint, objRef.MaxRange * 1.5f) * Camera.ViewMatrix * Camera.ProjectionMatrix;
        }
        public Matrix GetLightVolumeMatrix(Cell c, Vector3 pos, Vector3 endPoint, float maxRange)
        {
            Vector3 l = WorldSpace.GetRealWorldPos(pos, c) - WorldSpace.GetRealWorldPos(endPoint, c);
            Vector3 p1 = WorldSpace.GetRealWorldPos(pos, c), p2 = WorldSpace.GetRealWorldPos(endPoint, c);
            Vector3 midpoint = new Vector3((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);

            float f = 3.0f;

            l.X = Math.Abs(l.X);
            l.Y = Math.Abs(l.Y);
            l.Z = Math.Abs(l.Z);

            l.X += maxRange * f;
            l.Y += maxRange * f;
            l.Z += maxRange * f;

            return Matrix.Scaling(l) * Matrix.Translation(midpoint);
        }

        public override void Dispose()
        {
            DisposeLight();
        }
       

       

    }     

  
}
