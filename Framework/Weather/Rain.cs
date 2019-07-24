using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Utilities;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Weather
{

    struct RainVertex
    {
        public Vector3 pos;
        public Vector3 seed;
        public Vector3 speed;
        public float random;
        public byte Type;
        public Vector3 color;
    };


    

    public static class Rain
    {
        public static bool IsOn = false;

        static int g_numRainVertices = 800000;
        static float g_radiusRange = 250.0f;
        static float g_heightRange = 40.0f;
        static float timeCycle = 0;
        static Random r;       
        static D3D10.Buffer g_pParticleStart = null, g_pParticleStreamTo = null, g_pParticleDrawFrom = null;
        static bool firstFrame;
        static Shader rainEff;
        public static float splashTimeCycle = 0;

        //settings
        public static RainSetting settings = new RainSetting();

        public static void SetSettings(RainSetting s)
        {
            if (s == null) s = new RainSetting();
            settings = s;            
        }

        public static void Initialize()
        {            
            rainEff = WorldData.GetObject("rain.fx") as Shader;
            
            r = new Random();
            RainVertex[] vertices = new RainVertex[g_numRainVertices];

            for(int i = 0; i < g_numRainVertices; i++)
            {
                RainVertex raindrop;
                //use rejection sampling to generate random points inside a circle of radius 1 centered at 0,0
                float SeedX = 0;
                float SeedZ = 0;
                bool pointIsInside = false;
                while(!pointIsInside)
                { 
                   SeedX = random() - 0.5f;
                   SeedZ = random() - 0.5f;
                   if( Math.Sqrt( SeedX*SeedX + SeedZ*SeedZ ) <= 0.5f )
                       pointIsInside = true;
                }
                //save these random locations for reinitializing rain particles that have fallen out of bounds
                SeedX *= g_radiusRange;
                SeedZ *= g_radiusRange;
                float SeedY = random()*g_heightRange;
                raindrop.seed = new Vector3(SeedX,SeedY,SeedZ); 
        
                //add some random speed to the particles, to prevent all the particles from following exactly the same trajectory
                //additionally, random speeds in the vertical direction ensure that temporal aliasing is minimized
                float SpeedX = 40.0f*(random()/20.0f);
                float SpeedZ = 40.0f*(random()/20.0f);
                float SpeedY = 40.0f*(random()/10.0f); 
                raindrop.speed = new Vector3(SpeedX,SpeedY,SpeedZ);

                //move the rain particles to a random positions in a cylinder above the camera
                float x = SeedX + Camera.Position.X;
                float z = SeedZ + Camera.Position.Z;
                float y = SeedY + Camera.Position.Y;
                raindrop.pos = new Vector3(x,y,z); 

                //get an integer between 1 and 8 inclusive to decide which of the 8 types of rain textures the particle will use
                raindrop.Type = (byte)Math.Floor(  random()*8 + 1 );

                raindrop.color = new Vector3(1, 1, 1);

                //this number is used to randomly increase the brightness of some rain particles
                raindrop.random = 1;
                float randomIncrease = random();
                if( randomIncrease > 0.8)
                    raindrop.random += randomIncrease;

                vertices[i] = raindrop;
            }


            //create vertex buffers for the rain, two will be used to pingpong between during animation            
            BufferDescription bd = new BufferDescription();
            bd.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(typeof(RainVertex)) * g_numRainVertices;
            bd.Usage = ResourceUsage.Default;
            bd.BindFlags = BindFlags.VertexBuffer;
            bd.CpuAccessFlags = CpuAccessFlags.None;
            bd.OptionFlags = ResourceOptionFlags.None;

            DataStream s = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(typeof(RainVertex)) * vertices.Length, true, true);
            s.WriteRange(vertices);
            s.Position = 0;
            g_pParticleStart = new D3D10.Buffer(Game.Device, s, bd);

            
            bd.BindFlags |= BindFlags.StreamOutput;
            g_pParticleDrawFrom = new D3D10.Buffer(Game.Device, bd);
            g_pParticleStreamTo = new D3D10.Buffer(Game.Device, bd);

        
            DirectoryInfo di = new DirectoryInfo(Utils.GetRootPath() + "\\Content\\Textures\\rainTextures");
            FileInfo[] rainFiles = di.GetFiles("cv0_vPositive_*");            

            ShaderResourceView ppSRV = TextureLoader.LoadTextureArray(rainFiles);            
            if(rainEff != null) rainEff.GetVar("rainTextureArray").AsResource().SetResource(ppSRV);
            ppSRV.Dispose();            

            firstFrame = true;

            if (rainEff != null)
            {
                rainEff.GetVar("g_heightRange").AsScalar().Set(g_heightRange);
                rainEff.GetVar("g_radiusRange").AsScalar().Set(g_radiusRange);
                rainEff.GetVar("g_Near").AsScalar().Set(CameraConstants.DEFAULT_ZNEAR);
                rainEff.GetVar("g_Far").AsScalar().Set(CameraConstants.DEFAULT_ZFAR);
                rainEff.GetVar("g_SpriteSize").AsScalar().Set(0.8f);
            }
        }

        public static void Dispose()
        {            
            if (g_pParticleStart != null && !g_pParticleStart.Disposed) g_pParticleStart.Dispose();
            if (g_pParticleStreamTo != null && !g_pParticleStreamTo.Disposed) g_pParticleStreamTo.Dispose();
            if (g_pParticleDrawFrom != null && !g_pParticleDrawFrom.Disposed) g_pParticleDrawFrom.Dispose();
        }

        public static void RenderParticles()
        {
            if (!IsOn) return;

            //set effect vars            
            Matrix g_WorldMatrix = Matrix.Identity;
            Matrix WorldViewMatrix = g_WorldMatrix * Camera.ViewMatrix;
            Matrix WorldViewProjMatrix = WorldViewMatrix * Camera.ProjectionMatrix;

            if (Game.Time.TotalGameTime < 2)
                rainEff.GetVar("g_FrameRate").AsScalar().Set(40);
            else
                rainEff.GetVar("g_FrameRate").AsScalar().Set(Game.Time.FramesPerSecond);

            rainEff.GetVar("g_TotalVel").AsVector().Set(Wind.interpolatedWind * settings.RainSpeed);                        
            rainEff.GetVar("g_eyePos").AsVector().Set(Camera.Position);                                
            rainEff.GetVar("g_mWorld").AsMatrix().SetMatrix(g_WorldMatrix);
            rainEff.GetVar("g_mWorldView").AsMatrix().SetMatrix(WorldViewMatrix);
            rainEff.GetVar("g_mWorldViewProj").AsMatrix().SetMatrix(WorldViewProjMatrix);         
            rainEff.GetVar("g_lightPos").AsVector().Set(Camera.Forward * 5000);
            rainEff.GetVar("g_mProjection").AsMatrix().SetMatrix(Camera.ProjectionMatrix);
            rainEff.GetVar("dirLightIntensity").AsScalar().Set(settings.DirLightIntensity);
            rainEff.GetVar("g_ResponseDirLight").AsScalar().Set(settings.ResponseDirLight);           


            List<Vector4> pointLightPositions = new List<Vector4>();
            List<Vector4> pointLightColors = new List<Vector4>();
            List<Vector4> spotLightPositions = new List<Vector4>();
            List<Vector4> spotLightColors = new List<Vector4>();
            List<Vector4> spotLightDirections = new List<Vector4>();
            List<Vector4> spotLightAngles = new List<Vector4>();

            List<Vector4> areaLightPositions1 = new List<Vector4>();
            List<Vector4> areaLightPositions2 = new List<Vector4>();
            List<Vector4> areaLightColors = new List<Vector4>();
            List<Vector4> areaLightVals = new List<Vector4>();
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> lights = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in lights.Values)
                {
                    if (!objRef.IsOn) continue;
                    float intensity = 1;
                    Light l = objRef.BaseGameObject as Light;
                    if (objRef.Intensity < 1) intensity = objRef.Intensity;
                    if (l is PointLight)
                    {
                        pointLightPositions.Add(new Vector4(WorldSpace.GetRealWorldPos(objRef.Position, c), objRef.MaxRange));
                        pointLightColors.Add(new Vector4(objRef.LightColor * intensity, 1));
                    }
                    else if (l is SpotLight)
                    {
                        spotLightPositions.Add(new Vector4(WorldSpace.GetRealWorldPos(objRef.Position, c), objRef.MaxRange));
                        spotLightColors.Add(new Vector4(objRef.LightColor * intensity, 1));
                        Vector3 worldSpaceLightDirection = Vector3.Normalize(WorldSpace.GetRealWorldPos(objRef.Target, c) - WorldSpace.GetRealWorldPos(objRef.Position, c));
                        spotLightDirections.Add(new Vector4(worldSpaceLightDirection, 1));
                        spotLightAngles.Add(new Vector4((float)Math.Cos(objRef.InnerAngle * 0.5f), (float)Math.Cos(objRef.OuterAngle * 0.5f), 1, 1));
                    }
                    else if (l is AreaLight)
                    {
                        areaLightPositions1.Add(new Vector4(WorldSpace.GetRealWorldPos(objRef.Position, c), 1));
                        areaLightPositions2.Add(new Vector4(WorldSpace.GetRealWorldPos(objRef.EndPoint, c), 1));
                        areaLightColors.Add(new Vector4(objRef.LightColor * intensity, 1));
                        areaLightVals.Add(new Vector4(objRef.MaxRange, objRef.LerpInc, 1, 1));
                    }
                }
            }
            if (pointLightPositions.Count > 0) rainEff.GetVar("g_PointLightPositions").AsVector().Set(pointLightPositions.ToArray());
            if (pointLightColors.Count > 0) rainEff.GetVar("g_PointLightColors").AsVector().Set(pointLightColors.ToArray());
            rainEff.GetVar("numPointLights").AsScalar().Set(pointLightPositions.Count);

            if (spotLightPositions.Count > 0) rainEff.GetVar("g_SpotLightPositions").AsVector().Set(spotLightPositions.ToArray());
            if (spotLightColors.Count > 0) rainEff.GetVar("g_SpotLightColors").AsVector().Set(spotLightColors.ToArray());
            if (spotLightDirections.Count > 0) rainEff.GetVar("g_SpotLightDirections").AsVector().Set(spotLightDirections.ToArray());
            if (spotLightAngles.Count > 0) rainEff.GetVar("g_SpotLightAngles").AsVector().Set(spotLightAngles.ToArray());
            rainEff.GetVar("numSpotLights").AsScalar().Set(spotLightPositions.Count);

            if (areaLightPositions1.Count > 0) rainEff.GetVar("g_AreaLightPositions1").AsVector().Set(areaLightPositions1.ToArray());
            if (areaLightPositions2.Count > 0) rainEff.GetVar("g_AreaLightPositions2").AsVector().Set(areaLightPositions2.ToArray());
            if (areaLightColors.Count > 0) rainEff.GetVar("g_AreaLightColors").AsVector().Set(areaLightColors.ToArray());
            if (areaLightVals.Count > 0) rainEff.GetVar("g_AreaLightVals").AsVector().Set(areaLightVals.ToArray());
            rainEff.GetVar("numAreaLights").AsScalar().Set(areaLightPositions1.Count);


            if (Game.Time.TotalGameTime > 1)
            {
                EffectTechnique AdvanceParticles = rainEff.EffectObj.GetTechniqueByName("AdvanceParticles");
                InputLayout g_pVertexLayoutRainVertex = ShaderHelper.ConstructInputLayout(MeshInputElements10.RainVertex, AdvanceParticles.GetPassByIndex(0).Description.Signature);
                Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.PointList);
                Game.Device.InputAssembler.SetInputLayout(g_pVertexLayoutRainVertex);
                D3D10.Buffer pBuffers = firstFrame ? g_pParticleStart : g_pParticleDrawFrom;
                Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(pBuffers, MeshInputElements10.GetStride(MeshInputElements10.RainVertex), 0));

                // Point to the correct output buffer
                pBuffers = g_pParticleStreamTo;
                Game.Device.StreamOutput.SetTargets(new StreamOutputBufferBinding(pBuffers, 0));

                // draw            
                AdvanceParticles.GetPassByIndex(0).Apply();
                Game.Device.Draw(g_numRainVertices, 0);

                // Get back to normal
                pBuffers = null;
                Game.Device.StreamOutput.SetTargets(null);

                // Swap buffers
                D3D10.Buffer pTemp = g_pParticleDrawFrom;
                g_pParticleDrawFrom = g_pParticleStreamTo;
                g_pParticleStreamTo = pTemp;

                firstFrame = false;
            }


            //draw rain particles
            EffectTechnique RenderParticlesCheap = rainEff.EffectObj.GetTechniqueByName("RenderParticles");
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.PointList);
            InputLayout g_pVertexLayoutRainVertex2 = ShaderHelper.ConstructInputLayout(MeshInputElements10.RainVertex, RenderParticlesCheap.GetPassByIndex(0).Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(g_pVertexLayoutRainVertex2);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(g_pParticleDrawFrom, MeshInputElements10.GetStride(MeshInputElements10.RainVertex), 0));
            RenderParticlesCheap.GetPassByIndex(0).Apply();
            Game.Device.Draw((int)(settings.PercentDrawParticles * g_numRainVertices), 0);
        }  
     
        private static float random()
        {
            double n = r.NextDouble();
            return (float)n;          

        }
    }


}
