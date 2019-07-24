using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.World;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;

namespace WeatherGame.Framework.Weather
{      

    public static class SkyDome
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector3 position;
            public Vector2 texCoord;
        }

        public static WeatherSetting settings = new WeatherSetting();  

        private static RenderTarget mieRT, rayleighRT;
        public static ShaderResourceView moonTex, glowTex, starsTex, skyClouds1, skyClouds2;        
        private static D3D10.Buffer domeVerts, domeIndices;
        private static D3D10.Buffer moonVerts, moonIndices;
        private static Model skyBoxCloudsMesh;
        private static int DomeN, DVSize, DISize;        
        private static float Theta, fPhi, previousTheta, previousPhi;
        private static Viewport vp;
        
        private static Vector3 InvWaveLengths;
        private static Vector3 WaveLengthsMie;

        public static float AMBIENT_LEVEL_MAX = 0.4f;
        public static float AMBIENT_LEVEL_MIN = 0.1f;
        
        public static Vector4 SunColor;        
        public static Vector3 LightDirection;        
                      
        
        
        public static Vector2 t;
        public static float greyValue;
        public static float AmbientLevel = 0.4f;
        public static bool UseBlueShift = false;
        public static bool UseBloom = false;
        public static LensFlare lensFlare = null;


        static SkyDome()
        {
            Theta = 0.0f;
            fPhi = 0.0f;
            Theta = 2.4f;            

            LightDirection = new Vector3(100.0f, 100.0f, 100.0f);           
            setLengths();
            vp = new Viewport(0, 0, 128, 64, 0.0f, 1.0f);            

            DomeN = 32;
        }

        public static void SetSettings(WeatherSetting s)
        {
            if (s == null) s = new WeatherSetting();
            settings = s;

            Rain.IsOn = settings.IsRainOn;
            Snow.IsOn = settings.IsSnowOn;
            Lightning.IsOn = settings.IsLightningOn;

            Rain.SetSettings(settings.RainSettings);
            Snow.SetSettings(settings.SnowSettings);
            setLengths();
        }


        public static void Initialize()
        {
            
            mieRT = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, 128, 64);
            rayleighRT = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, 128, 64);            
            // SpriteDrawer.AddInstance(rayleighRT.srv, 0.0f, 0.0f);           
                 

            moonTex = TextureLoader.LoadTexture("sky/moon.png");
            glowTex = TextureLoader.LoadTexture("sky/moonglow.png");
            starsTex = TextureLoader.LoadTexture("sky/starfield.png");
            skyClouds1 = TextureLoader.LoadTexture("sky/clouds1.png");
            skyClouds2 = TextureLoader.LoadTexture("sky/clouds2.png");            
            skyBoxCloudsMesh = WorldData.GetObject("skyDomeClouds") as Model;

            GenerateDome();
            GenerateMoon();



            lensFlare = new LensFlare();
            lensFlare.Initialize();

            SetSettings(WorldData.GetObject("defaultWeather") as WeatherSetting);
            Rain.Initialize();            
            Snow.Initialize();            
            Wind.Initalize();
            Lightning.Initalize();
        }


        public static void Update()
        {
            int minutos = WorldTime.time.Hour * 60 + WorldTime.time.Minute;
            Theta = (float)minutos * (float)(Math.PI) / 12.0f / 60.0f;

            LightDirection = GetDirection();

            if (settings != null && settings.SkySettings != null)
            {
                if (t.X < 10.0f)
                    t.X += settings.SkySettings.Scroll.X;
                else
                    t.X = 0;

                if (t.Y < 10.0f)
                    t.Y += settings.SkySettings.Scroll.Y;
                else
                    t.Y = 0;
            }
            
            WorldTime.Update();
            Wind.Update();
            Lightning.Update();
        }

        public static void Dispose()
        {
            mieRT.Dispose();
            rayleighRT.Dispose();
            if(skyBoxCloudsMesh != null) skyBoxCloudsMesh.Dispose();

            if (moonTex != null && !moonTex.Disposed) moonTex.Dispose();
            if (glowTex != null && !glowTex.Disposed) glowTex.Dispose();
            if (starsTex != null && !starsTex.Disposed) starsTex.Dispose();
            if (skyClouds1 != null && !skyClouds1.Disposed) skyClouds1.Dispose();
            if (skyClouds2 != null && !skyClouds2.Disposed) skyClouds2.Dispose();


            if (domeVerts != null && !domeVerts.Disposed) domeVerts.Dispose();
            if (domeIndices != null && !domeIndices.Disposed) domeIndices.Dispose();
            if (moonVerts != null && !moonVerts.Disposed) moonVerts.Dispose();
            if (moonIndices != null && !moonIndices.Disposed) moonIndices.Dispose();

            if(lensFlare != null) lensFlare.Dispose();
            Rain.Dispose();
            Snow.Dispose();
        }


        #region Render
        public static void Draw(RenderTargetView rtv, DepthStencilView dsv, bool renderReflection)
        {            
            //if (previousTheta != Theta || previousPhi != fPhi)
            if(!renderReflection)
                UpdateMieRayleighTextures(rtv, dsv);

            SunColor = GetSunColor(-Theta, 2);

            Shader scatterEffect = WorldData.GetObject("scatter.fx") as Shader;
            if (scatterEffect == null) return;
            EffectTechnique tech = scatterEffect.EffectObj.GetTechniqueByName("Render");
            if (renderReflection) tech = scatterEffect.EffectObj.GetTechniqueByName("RenderReflection");

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(domeIndices, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(domeVerts, 20, 0));
            EffectPass p1 = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.PosTex, p1.Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(layout);

            scatterEffect.GetVar("cameraPosition").AsVector().Set(new Vector4(Camera.Position, 1));
            scatterEffect.GetVar("txMie").AsResource().SetResource(mieRT.srv);
            scatterEffect.GetVar("txRayleigh").AsResource().SetResource(rayleighRT.srv);
            if(renderReflection)
                scatterEffect.GetVar("WorldViewProjection").AsMatrix().SetMatrix(Matrix.Scaling(1, -1, 1) * Matrix.Translation(Camera.Position) * Camera.ViewMatrix * Camera.ProjectionMatrix);
            else
                scatterEffect.GetVar("WorldViewProjection").AsMatrix().SetMatrix(Matrix.Translation(Camera.Position) * Camera.ViewMatrix * Camera.ProjectionMatrix);
            scatterEffect.GetVar("v3SunDir").AsVector().Set(-LightDirection);

            if (settings != null && settings.SkySettings != null)
            {
                scatterEffect.GetVar("NumSamples").AsScalar().Set(settings.SkySettings.NumSamples);
                scatterEffect.GetVar("fExposure").AsScalar().Set(settings.SkySettings.Exposure);
            }
            scatterEffect.GetVar("StarsTex").AsResource().SetResource(starsTex);
            if (Theta < Math.PI / 2.0f || Theta > 3.0f * Math.PI / 2.0f)
                scatterEffect.GetVar("starIntensity").AsScalar().Set((float)Math.Abs(Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                scatterEffect.GetVar("starIntensity").AsScalar().Set(0.0f);

            p1.Apply();
            Game.Device.DrawIndexed(DISize * 3, 0, 0);

            UnbindMRT(tech, scatterEffect.GetVar("txMie").AsResource(), scatterEffect.GetVar("txRayleigh").AsResource());


            if (!renderReflection) DrawMoon();
            if (!renderReflection) DrawClouds(renderReflection);
            if (!renderReflection) lensFlare.Draw();
            

            previousTheta = Theta;
            previousPhi = fPhi;

            if (!renderReflection)
            {
                Rain.RenderParticles();
                Snow.RenderParticles();
            }

        }
        private static void DrawMoon()
        {
            Shader scatterEffect = WorldData.GetObject("scatter.fx") as Shader;
            EffectTechnique tech = scatterEffect.EffectObj.GetTechniqueByName("RenderMoon");            
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(moonIndices, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(moonVerts, 20, 0));
            EffectPass sunMoonPass = tech.GetPassByIndex(0);
            InputLayout l = ShaderHelper.ConstructInputLayout(MeshInputElements10.PosTex, sunMoonPass.Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(l);
            
            int scale = 15;
            Matrix m = Matrix.Scaling(1, 1, 1) * Matrix.RotationX(Theta + (float)Math.PI / 2.0f) *
                 Matrix.RotationY(-fPhi + (float)Math.PI / 2.0f) *
                 Matrix.Translation(LightDirection * scale) *
                 Matrix.Translation(Camera.Position);            

            scatterEffect.GetVar("WorldViewProjection").AsMatrix().SetMatrix(m * Camera.ViewMatrix * Camera.ProjectionMatrix);
            scatterEffect.GetVar("StarsTex").AsResource().SetResource(moonTex);
            
            if (Theta < Math.PI / 2.0f || Theta > 3.0f * Math.PI / 2.0f)
                scatterEffect.GetVar("alpha").AsScalar().Set((float)Math.Abs(Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                scatterEffect.GetVar("alpha").AsScalar().Set(0.0f);

            sunMoonPass.Apply();
            Game.Device.DrawIndexed(6, 0, 0);
        }
        private static void DrawClouds(bool renderReflection)
        {
            if (skyBoxCloudsMesh == null || skyBoxCloudsMesh.MeshObj == null) return;
            Shader scatterEffect = WorldData.GetObject("scatter.fx") as Shader;
            EffectTechnique tech = scatterEffect.EffectObj.GetTechniqueByName("RenderClouds");
            if (renderReflection) tech = scatterEffect.EffectObj.GetTechniqueByName("RenderCloudsReflection");

            D3D10.Buffer iBuffer = skyBoxCloudsMesh.MeshObj.GetDeviceIndexBuffer();
            D3D10.Buffer vBuffer = skyBoxCloudsMesh.MeshObj.GetDeviceVertexBuffer(0);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(iBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vBuffer, 56, 0));
            EffectPass p1 = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.NormalMesh, p1.Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(layout);

            Matrix mWorld = Matrix.Translation(Camera.Position);
            if (renderReflection) mWorld = Matrix.Scaling(1, -1, 1) * Matrix.Translation(Camera.Position);
            scatterEffect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix);
            if (settings != null && settings.SkySettings != null)
            {
                scatterEffect.GetVar("cloud1Tile").AsScalar().Set(settings.SkySettings.CloudTile.X);
                scatterEffect.GetVar("cloud2Tile").AsScalar().Set(settings.SkySettings.CloudTile.Y);
                scatterEffect.GetVar("cloudCover").AsScalar().Set(settings.SkySettings.CloudCover);
            }
            scatterEffect.GetVar("scroll").AsVector().Set(t);            
            scatterEffect.GetVar("clouds1Tex").AsResource().SetResource(skyClouds1);
            scatterEffect.GetVar("clouds2Tex").AsResource().SetResource(skyClouds2);
            scatterEffect.GetVar("SunColor").AsVector().Set(SunColor);

            p1.Apply();
            Game.Device.DrawIndexed((skyBoxCloudsMesh.Mesh3d.attrTable[0].FaceCount * 3), 0, 0);

            iBuffer.Dispose();
            vBuffer.Dispose();            
        }    
        #endregion



        #region Helpers       
        public static Vector4 GetSunColor(float fTheta, int nTurbidity)
        {
            float fBeta = 0.04608365822050f * nTurbidity - 0.04586025928522f;
            float fTauR, fTauA;
            float[] fTau = new float[3];

            float coseno = (float)Math.Cos((double)fTheta + Math.PI);
            double factor = (double)fTheta / Math.PI * 180.0;
            double jarl = Math.Pow(93.885 - factor, -1.253);
            float potencia = (float)jarl;
            float m = 1.0f / (coseno + 0.15f * potencia);

            int i;
            float[] fLambda = new float[3];
            if (settings != null && settings.SkySettings != null)
            {
                fLambda[0] = settings.SkySettings.WaveLengths.X;
                fLambda[1] = settings.SkySettings.WaveLengths.Y;
                fLambda[2] = settings.SkySettings.WaveLengths.Z;
            }


            for (i = 0; i < 3; i++)
            {
                potencia = (float)Math.Pow((double)fLambda[i], 4.0);
                fTauR = (float)Math.Exp((double)(-m * 0.008735f * potencia));

                const float fAlpha = 1.3f;
                potencia = (float)Math.Pow((double)fLambda[i], (double)-fAlpha);
                if (m < 0.0f)
                    fTau[i] = 0.0f;
                else
                {
                    fTauA = (float)Math.Exp((double)(-m * fBeta * potencia));
                    fTau[i] = fTauR * fTauA;
                }

            }

            Vector4 vAttenuation = new Vector4(fTau[0], fTau[1], fTau[2], 1.0f);
            UseBlueShift = false;
            if (vAttenuation.X == 0 && vAttenuation.Y == 0 && vAttenuation.Z == 0)
            {
                vAttenuation = new Vector4(0, 0, 0.07f, 1);
                AmbientLevel = AMBIENT_LEVEL_MIN;
                UseBlueShift = true;
            }
           
            float h;
            if (WorldTime.isTimeBetween(WorldTime.Sunrise, 12)) //Sunrise - 12pm
            {
                int numSeconds = (12 - WorldTime.Sunrise) * 60 * 60;
                h = (numSeconds - (float)WorldTime.getNumSecondsTillNoon()) / numSeconds;
                AmbientLevel = h / 2.0f;
                if(AmbientLevel > AMBIENT_LEVEL_MAX) AmbientLevel = AMBIENT_LEVEL_MAX;
                if (AmbientLevel < AMBIENT_LEVEL_MIN) AmbientLevel = AMBIENT_LEVEL_MIN;
            }
            else if (WorldTime.isTimeBetween(12, 17)) //12pm - 5pm
            {
                AmbientLevel = AMBIENT_LEVEL_MAX;
            }
            else if (WorldTime.isTimeBetween(17, WorldTime.Sunset)) //5pm - sunset
            {
                int numSeconds = (WorldTime.Sunset - 17) * 60 * 60;
                h = (numSeconds - (float)WorldTime.getNumSecondsTill(WorldTime.Sunset)) / numSeconds;
                AmbientLevel = (AMBIENT_LEVEL_MAX - (h / 2.0f));
                if (AmbientLevel < AMBIENT_LEVEL_MIN) AmbientLevel = AMBIENT_LEVEL_MIN;
                if (AmbientLevel > AMBIENT_LEVEL_MAX) AmbientLevel = AMBIENT_LEVEL_MAX;
            }

            if (!WorldTime.isTimeBetween(5, 19))
                UseBloom = true;
            else
                UseBloom = false;
            
            

            return vAttenuation;
        }
        static Vector3 GetDirection()
        {
            //Get Light Direction
            float y = (float)Math.Cos((double)Theta);
            float x = (float)(Math.Sin((double)Theta) * Math.Cos(fPhi));
            float z = (float)(Math.Sin((double)Theta) * Math.Sin(fPhi));            

            Vector3 v = new Vector3(x, y, z);
            //v.Normalize();                 
            return v;
        }

        static private void GenerateMoon()
        {
            QuadVertex3[] svQuad = new QuadVertex3[4];
            svQuad[0].pos = new Vector3(1, -1, 0);
            svQuad[0].tex = new Vector2(1,1);
            svQuad[1].pos = new Vector3(-1,-1,0);
            svQuad[1].tex = new Vector2(0,1);
            svQuad[2].pos = new Vector3(-1, 1, 0);
            svQuad[2].tex = new Vector2(0,0);
            svQuad[3].pos = new Vector3(1,1,0);
            svQuad[3].tex = new Vector2(1,0);

            DataStream s = new DataStream(4 * Marshal.SizeOf(typeof(QuadVertex3)), true, true);
            s.WriteRange(svQuad);

            s.Position = 0;
            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = 4 * Marshal.SizeOf(typeof(QuadVertex3));
            bufferDescription.Usage = ResourceUsage.Default;

            moonVerts = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();  

            short[] quadIb = new short[] { 0, 1, 2, 2, 3, 0 };
            s = new DataStream(6 * Marshal.SizeOf(typeof(short)), true, true);
            s.WriteRange(quadIb);

            s.Position = 0;
            bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.IndexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = 6 * Marshal.SizeOf(typeof(short));
            bufferDescription.Usage = ResourceUsage.Default;

            moonIndices = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close(); 
        }
        static private void GenerateDome()
        {
            int Latitude = DomeN / 2;
            int Longitude = DomeN;
            DVSize = Longitude * Latitude;
            DISize = (Longitude - 1) * (Latitude - 1) * 2;
            DVSize *= 2;
            DISize *= 2;

            DataStream s = new DataStream(DVSize * Marshal.SizeOf(typeof(Vertex)), true, true);

            // Fill Vertex Buffer
            int DomeIndex = 0;
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)Longitude - 1.0f)) * Math.PI / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = Math.PI * j / (Latitude - 1);

                    Vertex v = new Vertex();
                    v.position = new Vector3((float)(Math.Sin(MoveXZ) * Math.Cos(MoveY)), (float)Math.Cos(MoveXZ), (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY)));
                    v.position *= 10.0f;
                    v.texCoord = new Vector2();
                    v.texCoord.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    v.texCoord.Y = 0.5f / (float)Latitude + j / (float)Latitude;
                    s.Write(v);

                    DomeIndex++;
                }
            }
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(Longitude - 1)) * Math.PI / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = (Math.PI * 2.0) - (Math.PI * j / (Latitude - 1));

                    Vertex v = new Vertex();
                    v.position = new Vector3((float)(Math.Sin(MoveXZ) * Math.Cos(MoveY)), (float)Math.Cos(MoveXZ), (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY)));
                    v.position *= 10.0f;
                    v.texCoord = new Vector2();
                    v.texCoord.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    v.texCoord.Y = 0.5f / (float)Latitude + j / (float)Latitude;
                    s.Write(v);

                    DomeIndex++;
                }
            }

            s.Position = 0;
            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = DVSize * Marshal.SizeOf(typeof(Vertex));
            bufferDescription.Usage = ResourceUsage.Default;

            domeVerts = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();


            // Fill index buffer
            s = new DataStream((DISize * 3) * sizeof(short), true, true);
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    s.Write((short)(i * Latitude + j));
                    s.Write((short)((i + 1) * Latitude + j));
                    s.Write((short)((i + 1) * Latitude + j + 1));

                    s.Write((short)((i + 1) * Latitude + j + 1));
                    s.Write((short)(i * Latitude + j + 1));
                    s.Write((short)(i * Latitude + j));
                }
            }
            short Offset = (short)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    s.Write((short)(Offset + i * Latitude + j));
                    s.Write((short)(Offset + (i + 1) * Latitude + j + 1));
                    s.Write((short)(Offset + (i + 1) * Latitude + j));

                    s.Write((short)(Offset + i * Latitude + j + 1));
                    s.Write((short)(Offset + (i + 1) * Latitude + j + 1));
                    s.Write((short)(Offset + i * Latitude + j));
                }
            }

            s.Position = 0;
            bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.IndexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = (DISize * 3) * sizeof(short);
            bufferDescription.Usage = ResourceUsage.Default;

            domeIndices = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();
        }

        static void UpdateMieRayleighTextures(RenderTargetView rtv, DepthStencilView dsv)
        {
            // Save the Old viewport and set new one
            Viewport[] oldViews =  Game.Device.Rasterizer.GetViewports();            
            Game.Device.Rasterizer.SetViewports(vp);

            Game.Device.OutputMerger.SetTargets(new RenderTargetView[] { rayleighRT.rtv, mieRT.rtv });            
            Game.Device.ClearRenderTargetView(rayleighRT.rtv, Color.CornflowerBlue);
            Game.Device.ClearRenderTargetView(mieRT.rtv, Color.CornflowerBlue);

            Shader scatterEffect = WorldData.GetObject("scatter.fx") as Shader;
            if (scatterEffect == null) return;
            EffectTechnique tech = scatterEffect.EffectObj.GetTechniqueByName("Update");
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
            InputLayout l = ShaderHelper.ConstructInputLayout(MeshInputElements10.PosTex4, tech.GetPassByIndex(0).Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(l);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(QuadRenderComponent.quadVerts, 24, 0));

            if (settings != null && settings.SkySettings != null)
            {
                scatterEffect.GetVar("NumSamples").AsScalar().Set(settings.SkySettings.NumSamples);
            }
            scatterEffect.GetVar("InvWavelength").AsVector().Set(InvWaveLengths);
            scatterEffect.GetVar("WavelengthMie").AsVector().Set(WaveLengthsMie);
            scatterEffect.GetVar("v3SunDir").AsVector().Set(-LightDirection);

            tech.GetPassByIndex(0).Apply();
            Game.Device.Draw(4, 0);

            Game.Device.OutputMerger.SetTargets(dsv, rtv);
            Game.Device.Rasterizer.SetViewports(oldViews);
        }

        static private void UnbindMRT(EffectTechnique tech, params EffectResourceVariable[] vars)
        {
            UnbindMRT(tech.GetPassByIndex(0), vars);
        }
        static private void UnbindMRT(EffectPass p, params EffectResourceVariable[] vars)
        {
            foreach (EffectResourceVariable v in vars)
                v.SetResource(null);
            p.Apply();
        }
        private static void setLengths()
        {
            if (settings != null && settings.SkySettings != null)
            {
                InvWaveLengths.X = 1.0f / (float)Math.Pow((double)settings.SkySettings.WaveLengths.X, 4.0);
                InvWaveLengths.Y = 1.0f / (float)Math.Pow((double)settings.SkySettings.WaveLengths.Y, 4.0);
                InvWaveLengths.Z = 1.0f / (float)Math.Pow((double)settings.SkySettings.WaveLengths.Z, 4.0);

                WaveLengthsMie.X = (float)Math.Pow((double)settings.SkySettings.WaveLengths.X, -0.84);
                WaveLengthsMie.Y = (float)Math.Pow((double)settings.SkySettings.WaveLengths.Y, -0.84);
                WaveLengthsMie.Z = (float)Math.Pow((double)settings.SkySettings.WaveLengths.Z, -0.84);
            }
        }
        #endregion  

    }



   
}
