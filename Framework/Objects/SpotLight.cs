using System;
using SlimDX;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Objects
{

    [Serializable]
    public class SpotLight : Light
    {

        public SpotLight() : base() 
        {
            canHaveRefs = true;
            type = LightType.Spot;
            
            //innerAngle = CameraHelper.ToRadians(innerAngle);
            //outerAngle = CameraHelper.ToRadians(outerAngle);
        }


        public override void RenderDebugLightVolume(GameObjectReference objRef, Cell c, Vector4 GridColor)
        {
            //if (targetMesh == null)
              //  targetMesh = new GameObjectRef(@"system/box.mesh", "box.mesh", MeshInputElements10.PositionOnly4);

            
            Model targetMesh = WorldData.GetObject("light_target") as Model;

            Matrix scale, rotate, translate, matWVP;
            float fRadius = (float)Math.Tan(objRef.OuterAngle * 0.5f) * objRef.MaxRange;

            scale = Matrix.Scaling(fRadius, objRef.MaxRange, fRadius);
            rotate = RotateToFace(WorldSpace.GetRealWorldPos(objRef.Position, c), WorldSpace.GetRealWorldPos(objRef.Target, c), Vector3.UnitY);

            translate = WorldSpace.GetRealWorldMatrix(objRef.Position, c);
            matWVP = scale * rotate * translate;

            RenderingFunctions.DrawLightVolume(DeferredRenderer.lightVolume[(int)LightType.Spot], matWVP, GridColor);
            RenderingFunctions.DrawLightVolume(targetMesh, Matrix.Scaling(1, 1, 1) * WorldSpace.GetRealWorldMatrix(objRef.Target, c), GridColor);
        }

        public override Matrix GetLightVolumeWVP(Cell c, GameObjectReference objRef)
        {
            return GetLightVolumeWorldMatrix(c, objRef) * Camera.ViewMatrix * Camera.ProjectionMatrix;            
        }

        public Matrix GetLightVolumeWorldMatrix(Cell c, GameObjectReference objRef)
        {
            Matrix scale, rotate, translate;
            float fRadius = (float)Math.Tan(objRef.OuterAngle * 0.5f) * objRef.MaxRange;
            scale = Matrix.Scaling(fRadius * 2.0f, objRef.MaxRange * 2.0f, fRadius * 2.0f);
            rotate = RotateToFace(WorldSpace.GetRealWorldPos(objRef.Position, c), WorldSpace.GetRealWorldPos(objRef.Target, c), Vector3.UnitY);
            translate = WorldSpace.GetRealWorldMatrix(objRef.Position, c);
            return scale * rotate * translate;
        }

       
        


        // O is your object's position
        // P is the position of the object to face
        // U is the nominal "up" vector (typically Vector3.Y)
        // Note: this does not work when O is straight below or straight above P
        private Matrix RotateToFace(Vector3 O, Vector3 P, Vector3 U)
        {
            Vector3 D = (O - P);

            U = Vector3.Normalize(D);

            Vector3 Right = Vector3.Cross(U, D);
            if (Right.X == 0 && Right.Y == 0 && Right.Z == 0) Right = Vector3.Cross(O, P);
            Vector3.Normalize(ref Right, out Right);
            Vector3 Backwards = Vector3.Cross(Right, U);
            Vector3.Normalize(ref Backwards, out Backwards);
            Vector3 Up = Vector3.Cross(Backwards, Right);
            Matrix rot = new Matrix();
            //Right.X, Right.Y, Right.Z, 0
            //Up.X, Up.Y, Up.Z, 0
            //Backwards.X, Backwards.Y, Backwards.Z, 0
            //0, 0, 0, 1
            rot[0, 0] = Right.X; rot[0, 1] = Right.Y; rot[0, 2] = Right.Z; rot[0, 3] = 0;
            rot[1, 0] = Up.X; rot[1, 1] = Up.Y; rot[1, 2] = Up.Z; rot[1, 3] = 0;
            rot[2, 0] = Backwards.X; rot[2, 1] = Backwards.Y; rot[2, 2] = Backwards.Z; rot[2, 3] = 0;
            rot[3, 0] = 0; rot[3, 1] = 0; rot[3, 2] = 0; rot[3, 3] = 1;

            return rot;
        }

        public override void Dispose()
        {
            DisposeLight();
        }
    }      

  
}
