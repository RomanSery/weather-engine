using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Weather;
using WeatherGame.Framework.World;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;
using System.Drawing;

namespace WeatherGame.Framework.Rendering
{
    

    [StructLayout(LayoutKind.Sequential)]
    public struct HaloVertex
    {
        public Vector3 pos;
        public Vector4 color;
        public int texIndex;
    };


    public static class DeferredRenderer
    {
        public static bool Disposed = false;

        public static RenderTarget baseRT, normalRT, baseRTCopy;       
        public static RenderTarget reflectionRT, reflectionDSV;        
        public static Model[] lightVolume;
        static D3D10.Buffer[] IndexBuffer, VertexBuffer;

        public static int RenderWidth;
        public static int RenderHeight;
        static ShaderResourceView whiteTex;

        public static int ScreenWidth, ScreenHeight;

        private static List<string> haloFlares;
        private static Random r;
        private static ShaderResourceView SplashBumpTexture, SplashDiffuseTexture;
        private static ShaderResourceView SnowTexture, SnowNormalTexture;
        private static ShaderResourceView WaterNormalMap;        

        public static void Initialize(int sw, int sh)
        {
            ScreenWidth = sw;
            ScreenHeight = sh;
            baseRT = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, ScreenWidth, ScreenHeight);
            baseRTCopy = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, ScreenWidth, ScreenHeight);
            normalRT = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, ScreenWidth, ScreenHeight);
           
            reflectionRT = RenderingFunctions.CreateRenderTarget(Format.R16G16B16A16_Float, ScreenWidth, ScreenHeight);
            reflectionDSV = RenderingFunctions.CreateRenderDepthTarget(Format.R32_Typeless, ScreenWidth, ScreenHeight);           

            //SpriteDrawer.AddInstance("reflectionRT", reflectionRT.srv, new Vector2(0.5f, 0.5f), new Color4(Color.White), 1, SpriteBlendMode.AlphaBlend);

            lightVolume = new Model[5];
            IndexBuffer = new D3D10.Buffer[5];
            VertexBuffer = new D3D10.Buffer[5];
            InitLightType(LightType.Point, "light_sphere");
            InitLightType(LightType.Spot, "light_cone");
            InitLightType(LightType.Area, "light_box");
            InitLightType(LightType.Box, "light_box");

            whiteTex = TextureLoader.LoadCubeTexture("fx/whiteCube.dds");
            QuadRenderComponent.Initialize();
            InitHalos();

            InitializeShaderMaterials();

            WaterNormalMap = TextureLoader.LoadTexture("fx/water_normal.dds");

            BloomEffect.SetSettings(WorldData.GetObject("defaultBloom") as BloomSetting);

            Disposed = false;
        }
        public static void InitializeShaderMaterials()
        {
            FillShaderMaterials(WorldData.GetObject("SpotLights.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("PointLights.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("AreaLights.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("AmbientLight.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("FillBuffers.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("Water.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("RenderReflection.fx") as Shader);
            FillShaderMaterials(WorldData.GetObject("Glow.fx") as Shader);

            Shader effect = WorldData.GetObject("FillBuffers.fx") as Shader;
            SplashBumpTexture = TextureLoader.Load3DTextureFromFile("rainTextures/splashes/SBumpVolume.dds");
            SplashDiffuseTexture = TextureLoader.Load3DTextureFromFile("rainTextures/splashes/SDiffuseVolume.dds");
            if (effect != null && effect.EffectObj != null)
            {
                effect.GetVar("SplashBumpTexture").AsResource().SetResource(SplashBumpTexture);
                effect.GetVar("SplashDiffuseTexture").AsResource().SetResource(SplashDiffuseTexture);
            }

            SnowTexture = TextureLoader.LoadTexture("snow/snowfloor.jpg");
            SnowNormalTexture = TextureLoader.LoadTexture("snow/snowfloor_normal.jpg");
            if (effect != null && effect.EffectObj != null)
            {
                effect.GetVar("SnowTexture").AsResource().SetResource(SnowTexture);
                effect.GetVar("SnowNormalTexture").AsResource().SetResource(SnowNormalTexture);
            }


            effect = WorldData.GetObject("Water.fx") as Shader;            
            if (effect != null && effect.EffectObj != null)
            {
                effect.GetVar("SplashBumpTexture").AsResource().SetResource(SplashBumpTexture);
                effect.GetVar("SplashDiffuseTexture").AsResource().SetResource(SplashDiffuseTexture);
            }
        }


        public static void DrawWpf(RenderTargetView rtv, DepthStencilView dsv, ShaderResourceView depthStencilSRV, int w, int h, bool renderLights)
        {
            if (Disposed) return;
            GenerateReflectionMap(dsv, rtv, null, null, true);            
            
            Game.Device.ClearRenderTargetView(rtv, new SlimDX.Color4(1.0f, 0, 0, 0));           
            Game.Device.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            FillBuffers(dsv, w, h, true, depthStencilSRV);                        
            
            Game.Device.OutputMerger.SetTargets(null, new RenderTargetView[] { rtv });
            Game.Device.Rasterizer.SetViewports(new Viewport(0, 0, w, h, 0.0f, 1.0f));

            DrawAmbientLight(true, depthStencilSRV);
            
            if (renderLights)
            {
                DrawPointLights(true, depthStencilSRV);
                DrawSpotLights(true, depthStencilSRV);
                DrawAreaLights(true, depthStencilSRV);
                DrawBoxLights(true, depthStencilSRV);                
            }

            Game.Device.OutputMerger.SetTargets(dsv, rtv);
            
            SkyDome.Draw(rtv, dsv, false);
                        
            //BloomEffect.Draw(null, null, true, false);

            //BloomEffect.Draw(null, null, true, true);            

            SpriteDrawer.Draw();
            if (renderLights) DrawHalos();
           
        }

        public static void Draw(RenderTargetView rtv, DepthStencilView dsv)
        {
            try
            {
                GenerateReflectionMap(dsv, rtv, null, null, false);
            }
            catch { }

            Game.Device.ClearRenderTargetView(rtv, new SlimDX.Color4(1.0f, 0, 0, 0));
            //Game.Device.ClearRenderTargetView(finalRT.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));
            Game.Device.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 1);

           //FillBuffers(dsv, null, null, false);            
           // Game.Device.OutputMerger.SetTargets(null, new RenderTargetView[] { finalRT.rtv }); //rtv

            DrawAmbientLight(false, null);
            //DrawPointLights(false);
           // DrawSpotLights(false);
           // DrawAreaLights(false);            

           // SkyDome.Draw(finalRT.rtv, dsv, false);

            BloomEffect.Draw(null, null, false, false);

            Game.Device.OutputMerger.SetTargets(dsv, rtv);
            //RenderingFunctions.RenderFullScreenQuad(finalRT.srv);
                        
            BloomEffect.Draw(null, null, false, true);
            SpriteDrawer.Draw();            
            DrawHalos();
        }


        private static void GenerateReflectionMap(DepthStencilView dsv, RenderTargetView rtv, int? w, int? h, bool wpf)
        {
            if (Disposed) return;
            Game.Device.ClearRenderTargetView(reflectionRT.rtv, new SlimDX.Color4(0.0f, 0, 0, 0));
            Game.Device.ClearDepthStencilView(reflectionDSV.dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 1);

            Shader effect = WorldData.GetObject("RenderReflection.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderReflection");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.NormalMesh, pass.Description.Signature);

            Game.Device.OutputMerger.SetTargets(reflectionDSV.dsv, reflectionRT.rtv);
            if (w.HasValue && h.HasValue)
                Game.Device.Rasterizer.SetViewports(new Viewport(0, 0, w.Value, h.Value, 0.0f, 1.0f));

            SkyDome.Draw(rtv, dsv, true);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetInputLayout(layout);
                        
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetObjRefsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {                                       
                    if (objRef.getAttribute(GameObjectRefAttribute.IncludeInReflectionMap) == false) continue;
                    Model model = objRef.BaseGameObject as Model;
                    D3D10.Buffer indexBuffer = model.MeshObj.GetDeviceIndexBuffer();
                    D3D10.Buffer vertextBuffer = model.MeshObj.GetDeviceVertexBuffer(0);

                    Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, model.Mesh3d.has32BitIndices ? Format.R32_UInt : Format.R16_UInt, 0);
                    Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, 56, 0));

                    for (int subset = 0; subset < model.Mesh3d.NumAttributes; subset++)
                    {                        
                       //float height = model.Mesh3d.bb.Maximum.Y - model.Mesh3d.bb.Minimum.Y;
                        //float transY = objRef.getAttribute(GameObjectRefAttribute.IsZeroPlaneReflection) ? (-(objRef.Position.Y) + height) : ((objRef.Position.Y) + height);
                        //Matrix mWorld = objRef.EntityTransform * model.Mesh3d.nodes[subset].transform;
                        //mWorld = mWorld * Matrix.Scaling(objRef.Scale.X, -objRef.Scale.Y, objRef.Scale.Z) * Matrix.Translation(0, transY, 0);                        

                        //if (objRef.AnimController != null && objRef.AnimController.IsPlaying)
                        //{
                          //  mWorld = model.Mesh3d.nodes[subset].transform * objRef.AnimController.AnimationTransform[subset] * objRef.EntityTransform * Matrix.Scaling(objRef.Scale.X, -objRef.Scale.Y, objRef.Scale.Z) * Matrix.Translation(0, transY, 0);                         
                        //}

                        //Matrix mWorldViewProj = mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix;
                        

                        Material mat = objRef.getMaterialForSubset(subset);
                        if (mat.getAttribute(MaterialAttribute.IncludeInReflectionMap) == false) continue;
                                                
                        Matrix mWorld = objRef.EntityTransform * model.Mesh3d.nodes[subset].transform;
                        if (objRef.AnimController != null && objRef.AnimController.IsPlaying)
                        {
                            mWorld = model.Mesh3d.nodes[subset].transform * objRef.AnimController.AnimationTransform[subset] * objRef.EntityTransform;
                        }
                        Matrix mWorldViewProj = mWorld * Camera.reflectionViewMatrix * Camera.ProjectionMatrix;                                               
                        
                        effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorldViewProj);
                        effect.GetVar("Base").AsResource().SetResource(model.Mesh3d.textureViews[subset]);
                        effect.GetVar("LightColor").AsVector().Set(SkyDome.SunColor);
                        effect.GetVar("ambientLevel").AsScalar().Set(SkyDome.AmbientLevel);
                        effect.GetVar("UseBlueShift").AsScalar().Set(SkyDome.UseBlueShift);

                       
                        effect.GetVar("MaterialID").AsScalar().Set(mat.MaterialIndex);                      

                        pass.Apply();
                        MeshAttributeRange r = model.Mesh3d.attrTable[subset];
                        Game.Device.DrawIndexed((r.FaceCount * 3), (r.FaceStart * 3), 0);
                    }

                    indexBuffer.Dispose();
                    vertextBuffer.Dispose();
                }
            }
        }




        private static void FillBuffers(DepthStencilView dsv, int? w, int? h, bool wpf, ShaderResourceView depthStencilSRV)
        {
            if (Disposed) return;
            Shader effect = WorldData.GetObject("FillBuffers.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderBuffers");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.NormalMesh, pass.Description.Signature);


            RenderTargetView[] rtv = { baseRT.rtv, normalRT.rtv };            
            Game.Device.OutputMerger.SetTargets(dsv, rtv);
            if (w.HasValue && h.HasValue)
                Game.Device.Rasterizer.SetViewports(new Viewport(0, 0, w.Value, h.Value, 0.0f, 1.0f));

            Game.Device.ClearRenderTargetView(baseRT.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));
            Game.Device.ClearRenderTargetView(baseRTCopy.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));            
            Game.Device.ClearRenderTargetView(normalRT.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));            
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetInputLayout(layout);


            Rain.splashTimeCycle += Rain.settings.RainSplashSpeed;
            effect.GetVar("g_timeCycle").AsScalar().Set(Rain.splashTimeCycle);
            if (Rain.splashTimeCycle >= 1)
            {
                Rain.splashTimeCycle = 0;
                effect.GetVar("g_splashXDisplace").AsScalar().Set(random() * 2);
                effect.GetVar("g_splashYDisplace").AsScalar().Set(random() * 2);
            }
            Matrix matSplashScale = Matrix.Scaling(Rain.settings.RainSplashSize, Rain.settings.RainSplashSize, Rain.settings.RainSplashSize);
            effect.GetVar("matSplashScale").AsMatrix().SetMatrix(matSplashScale);
            effect.GetVar("renderRainSplashes").AsScalar().Set(Rain.IsOn ? true : false);
            effect.GetVar("renderSnow").AsScalar().Set(Snow.settings.SnowValue == 1 && Snow.settings.WallSnowValue == 1 ? false : true);            
            effect.GetVar("snowValue").AsScalar().Set(Snow.IsOn ? Snow.settings.SnowValue : 1);
            effect.GetVar("wallSnowValue").AsScalar().Set(Snow.IsOn ? Snow.settings.WallSnowValue : 1);
            effect.GetVar("reflectionMap").AsResource().SetResource(reflectionRT.srv);
            effect.GetVar("cameraPosition").AsVector().Set(new Vector4(Camera.Position, 1));

                        
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetObjRefsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Model model = objRef.BaseGameObject as Model;

                    if (objRef.getAttribute(GameObjectRefAttribute.IsWater)) continue;
                    if (model.MeshObj.Disposed) continue;
                    D3D10.Buffer indexBuffer = model.MeshObj.GetDeviceIndexBuffer();
                    D3D10.Buffer vertextBuffer = model.MeshObj.GetDeviceVertexBuffer(0);
                                        
                    Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, model.Mesh3d.has32BitIndices ? Format.R32_UInt : Format.R16_UInt, 0);
                    Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, 56, 0));
                    effect.GetVar("View").AsMatrix().SetMatrix(Camera.ViewMatrix);
                    effect.GetVar("Projection").AsMatrix().SetMatrix(Camera.ProjectionMatrix);



                    for (int subset = 0; subset < model.Mesh3d.NumAttributes; subset++)
                    {
                        if (objRef.SelectedSubset.HasValue && objRef.SelectedSubset.Value != subset) continue;

                        effect.GetVar("Base").AsResource().SetResource(model.Mesh3d.textureViews[subset]);
                        if (model.Mesh3d.normalTextureViews[subset] != null)
                        {
                            effect.GetVar("NormalMap").AsResource().SetResource(model.Mesh3d.normalTextureViews[subset]);
                            effect.GetVar("useFaceNormals").AsScalar().Set(false);
                        }
                        else
                        {
                            effect.GetVar("NormalMap").AsResource().SetResource(SnowTexture);
                            effect.GetVar("useFaceNormals").AsScalar().Set(true);
                        }



                        Matrix mWorld = objRef.EntityTransform * model.Mesh3d.nodes[subset].transform;
                        if (objRef.AnimController != null && objRef.AnimController.IsPlaying)
                        {
                            mWorld = model.Mesh3d.nodes[subset].transform * objRef.AnimController.AnimationTransform[subset] * objRef.EntityTransform;
                        }
                    
                      
                        Matrix mWorldView = mWorld * Camera.ViewMatrix;
                        Matrix mWorldViewProj = mWorldView * Camera.ProjectionMatrix;
                        Material mat = objRef.getMaterialForSubset(subset);                       

                        effect.GetVar("World").AsMatrix().SetMatrix(mWorld);
                        effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorldViewProj);
                        effect.GetVar("WorldView").AsMatrix().SetMatrix(mWorldView);

                        effect.GetVar("UseReflectionMap").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.UseReflectionMap) && mat.getAttribute(MaterialAttribute.UseReflectionMap));                 

                        effect.GetVar("MaterialID").AsScalar().Set(mat.MaterialIndex);
                        if (mat.CubeMapSRV != null)
                        {
                            effect.GetVar("useCubeMap").AsScalar().Set(true);
                            effect.GetVar("EnvironmentMap").AsResource().SetResource(mat.CubeMapSRV);
                        }
                        else
                        {
                            effect.GetVar("useCubeMap").AsScalar().Set(false);
                            effect.GetVar("EnvironmentMap").AsResource().SetResource(whiteTex);
                        }

                        if (mat.DetailMapSRV != null)
                        {
                            effect.GetVar("useDetailMap").AsScalar().Set(true);
                            effect.GetVar("DetailMap").AsResource().SetResource(mat.DetailMapSRV);
                        }
                        else
                        {
                            effect.GetVar("useDetailMap").AsScalar().Set(false);
                            effect.GetVar("DetailMap").AsResource().SetResource(null);
                        }

                        effect.GetVar("matRainSplashes").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.RecieveRainSplashes) && mat.getAttribute(MaterialAttribute.RecieveRainSplashes));
                        effect.GetVar("matSnow").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.RecieveSnow) && mat.getAttribute(MaterialAttribute.RecieveSnow));
                            
                     
                        

                        pass.Apply();
                        MeshAttributeRange r = model.Mesh3d.attrTable[subset];
                        Game.Device.DrawIndexed((r.FaceCount * 3), (r.FaceStart * 3), 0);                        
                    }
                    

                    indexBuffer.Dispose();
                    vertextBuffer.Dispose();
                }
            }
            

            #region water
            
            Game.Device.CopyResource(baseRT.texture, baseRTCopy.texture);           


            effect = WorldData.GetObject("Water.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            tech = effect.EffectObj.GetTechniqueByName("RenderWater");
            pass = tech.GetPassByIndex(0);                       
            layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.NormalMesh, pass.Description.Signature);
                        
            effect.GetVar("g_timeCycle").AsScalar().Set(Rain.splashTimeCycle);
            if (Rain.splashTimeCycle >= 1)
            {
                Rain.splashTimeCycle = 0;
                effect.GetVar("g_splashXDisplace").AsScalar().Set(random() * 2);
                effect.GetVar("g_splashYDisplace").AsScalar().Set(random() * 2);
            }            
            effect.GetVar("matSplashScale").AsMatrix().SetMatrix(matSplashScale);
            effect.GetVar("renderRainSplashes").AsScalar().Set(Rain.IsOn ? true : false);            
            effect.GetVar("cameraPosition").AsVector().Set(new Vector4(Camera.Position, 1));                        
            effect.GetVar("reflectionMap").AsResource().SetResource(reflectionRT.srv);            
            effect.GetVar("View").AsMatrix().SetMatrix(Camera.ViewMatrix);
            effect.GetVar("Projection").AsMatrix().SetMatrix(Camera.ProjectionMatrix);
            effect.GetVar("Base").AsResource().SetResource(baseRTCopy.srv);                    

            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetObjRefsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Model model = objRef.BaseGameObject as Model;

                    if (objRef.getAttribute(GameObjectRefAttribute.IsWater) == false) continue;
                    D3D10.Buffer indexBuffer = model.MeshObj.GetDeviceIndexBuffer();
                    D3D10.Buffer vertextBuffer = model.MeshObj.GetDeviceVertexBuffer(0);

                    Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, model.Mesh3d.has32BitIndices ? Format.R32_UInt : Format.R16_UInt, 0);
                    Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, 56, 0));

                    effect.GetVar("NormalMap").AsResource().SetResource(WaterNormalMap);


                    Matrix mWorld = objRef.EntityTransform * model.Mesh3d.nodes[0].transform;                   
                    Matrix mWorldView = mWorld * Camera.ViewMatrix;
                    Matrix mWorldViewProj = mWorldView * Camera.ProjectionMatrix;                    
                    
                    effect.GetVar("World").AsMatrix().SetMatrix(mWorld);
                    effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorldViewProj);
                    effect.GetVar("WorldView").AsMatrix().SetMatrix(mWorldView);

                    Material mat = objRef.getMaterialForSubset(0);
                    effect.GetVar("MaterialID").AsScalar().Set(mat.MaterialIndex);

                    effect.GetVar("matRainSplashes").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.RecieveRainSplashes) && mat.getAttribute(MaterialAttribute.RecieveRainSplashes));

                    WaterSetting ws = objRef.WaterSettings;
                    if (ws == null) ws = WorldData.GetObject("DefaultWater") as WaterSetting;
                    if (objRef.t == null || objRef.t2 == null || objRef.t3 == null || objRef.t4 == null)
                    {
                        objRef.t = 0.0f;
                        objRef.t2 = 500.0f;
                        objRef.t3 = 750.0f;
                        objRef.t4 = 1000.0f;
                    }  
                    effect.GetVar("scroll").AsScalar().Set(objRef.t.Value);
                    effect.GetVar("scroll2").AsScalar().Set(objRef.t2.Value);
                    effect.GetVar("scroll3").AsScalar().Set(objRef.t3.Value);
                    effect.GetVar("scroll4").AsScalar().Set(objRef.t4.Value);                    

                    effect.GetVar("shoreFalloff").AsScalar().Set(ws.ShoreFalloff);
                    effect.GetVar("shoreScale").AsScalar().Set(ws.ShoreScale);
                    effect.GetVar("speed").AsScalar().Set(ws.Speed);
                    effect.GetVar("reflectionFactorOffset").AsScalar().Set(ws.ReflectionFactorOffset);                                                                          

                    pass.Apply();
                    MeshAttributeRange r = model.Mesh3d.attrTable[0];
                    Game.Device.DrawIndexed((r.FaceCount * 3), (r.FaceStart * 3), 0);                    

                    indexBuffer.Dispose();
                    vertextBuffer.Dispose();
                }
            }
            
            #endregion


        }





        private static void DrawAmbientLight(bool wpf, ShaderResourceView depthStencilSRV)
        {            
            Shader effect = WorldData.GetObject("AmbientLight.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderAmbient");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.NormalMesh, pass.Description.Signature);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetInputLayout(layout);
            effect.GetVar("colorMap").AsResource().SetResource(baseRT.srv);
            effect.GetVar("normalMap").AsResource().SetResource(normalRT.srv);            
            effect.GetVar("depthMap").AsResource().SetResource(depthStencilSRV);
            
            effect.GetVar("cameraPosition").AsVector().Set(new Vector4(Camera.Position, 1));
            effect.GetVar("View").AsMatrix().SetMatrix(Camera.ViewMatrix);
            effect.GetVar("InvProjMatrix").AsMatrix().SetMatrix(Matrix.Invert(Camera.ProjectionMatrix));
            
            
            
            Vector3 viewSpaceLightDirection = Vector3.TransformNormal(SkyDome.LightDirection, Camera.ViewMatrix);
            effect.GetVar("g_fvViewSpaceLightDirection").AsVector().Set(viewSpaceLightDirection);

            effect.GetVar("LightColor").AsVector().Set(SkyDome.SunColor);
            effect.GetVar("ambientLevel").AsScalar().Set(SkyDome.AmbientLevel);
            effect.GetVar("FogDensity").AsScalar().Set(SkyDome.UseBlueShift ? 0 : SkyDome.settings.SkySettings.FogDensity);
            effect.GetVar("UseBlueShift").AsScalar().Set(SkyDome.UseBlueShift);



            pass.Apply();
            Game.Device.Draw(3, 0);

            UnbindMRT(pass, effect.GetVar("colorMap").AsResource(), effect.GetVar("normalMap").AsResource(), effect.GetVar("depthMap").AsResource());             
        }
        
        private static void DrawBoxLights(bool wpf, ShaderResourceView depthStencilSRV)
        {
            if (lightVolume[(int)LightType.Box] == null || lightVolume[(int)LightType.Box].Mesh3d == null) return;
            Shader effect = WorldData.GetObject("BoxLights.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderBoxLight");
            EffectPass pass = tech.GetPassByIndex(0);

            InputLayout layout = ShaderHelper.ConstructInputLayout(lightVolume[(int)LightType.Box].Mesh3d.inputElements, pass.Description.Signature);            
            
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(IndexBuffer[(int)LightType.Box], Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer[(int)LightType.Box], MeshInputElements10.GetStride(lightVolume[(int)LightType.Box].Mesh3d.inputElements), 0));
            Game.Device.InputAssembler.SetInputLayout(layout);
            effect.GetVar("colorMap").AsResource().SetResource(baseRT.srv);
            effect.GetVar("normalMap").AsResource().SetResource(normalRT.srv);            
            effect.GetVar("depthMap").AsResource().SetResource(depthStencilSRV);
            effect.GetVar("cameraPosition").AsVector().Set(new Vector4(Camera.Position, 1));
            effect.GetVar("View").AsMatrix().SetMatrix(Camera.ViewMatrix);
            effect.GetVar("matInView").AsMatrix().SetMatrix(Matrix.Invert(Camera.ViewMatrix));            
            effect.GetVar("EyeZAxis").AsVector().Set(new Vector4(Camera.Forward, 1));
            

            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Light l = objRef.BaseGameObject as Light;
                    if (l is BoxLight == false || !objRef.IsOn) continue;
                    BoxLight sl = l as BoxLight;
                    
                    effect.GetVar("lightColor").AsVector().Set(objRef.LightColor);
                    effect.GetVar("lightIntensity").AsScalar().Set(objRef.Intensity);                    
                    effect.GetVar("matWorldViewProjection").AsMatrix().SetMatrix(sl.GetLightVolumeWVP(c, objRef));
                    effect.GetVar("World").AsMatrix().SetMatrix(sl.GetLightVolumeMatrix(c, objRef));
                    effect.GetVar("matWorldView").AsMatrix().SetMatrix(sl.GetLightVolumeMatrix(c, objRef) * Camera.ViewMatrix);                                      
                    effect.GetVar("useSpecular").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.UseSpecular));
                    effect.GetVar("g_fLightMaxRange").AsScalar().Set(objRef.MaxRange);

                    effect.GetVar("matRotation").AsMatrix().SetMatrix(Matrix.Invert(objRef.Rotation));                                      
                    

                    Vector3 max = DeferredRenderer.lightVolume[(int)LightType.Box].Mesh3d.bb.Maximum;
                    Vector3 min = DeferredRenderer.lightVolume[(int)LightType.Box].Mesh3d.bb.Minimum;                    

                    Vector3 pos = objRef.Position;
                    pos.Y += objRef.BoxHeight;
                    Vector3 worldSpaceLightDirection = Vector3.Normalize(WorldSpace.GetRealWorldPos(objRef.Target, c) - WorldSpace.GetRealWorldPos(pos, c));                                       
                    effect.GetVar("g_fvViewSpaceLightDirection").AsVector().Set(Vector3.TransformNormal(worldSpaceLightDirection, Camera.ViewMatrix));
                    effect.GetVar("g_fvViewSpaceLightPosition").AsVector().Set(Vector3.Transform(WorldSpace.GetRealWorldPos(pos, c), Camera.ViewMatrix));                    
                                        

                    max = Vector3.TransformCoordinate(max, sl.GetLightVolumeMatrix(c, objRef));
                    min = Vector3.TransformCoordinate(min, sl.GetLightVolumeMatrix(c, objRef));                    
                    effect.GetVar("bbMax").AsVector().Set(max);
                    effect.GetVar("bbMin").AsVector().Set(min);

                    if (sl.LightMapSRV != null)
                        effect.GetVar("lightMap").AsResource().SetResource(sl.LightMapSRV);
                    else
                        effect.GetVar("lightMap").AsResource().SetResource(whiteTex);
                    if (sl.LightMaskSRV != null)
                        effect.GetVar("lightMask").AsResource().SetResource(sl.LightMaskSRV);
                    else
                        effect.GetVar("lightMask").AsResource().SetResource(whiteTex);             


                    pass.Apply();
                    Game.Device.DrawIndexed((lightVolume[(int)LightType.Box].Mesh3d.attrTable[0].FaceCount * 3), 0, 0);                    
                }
            }

            UnbindMRT(tech, effect.GetVar("colorMap").AsResource(), effect.GetVar("normalMap").AsResource(), effect.GetVar("depthMap").AsResource());            
        }
        
        private static void DrawAreaLights(bool wpf, ShaderResourceView depthStencilSRV)
        {
            if (lightVolume[(int)LightType.Area] == null || lightVolume[(int)LightType.Area].Mesh3d == null) return;
            Shader effect = WorldData.GetObject("AreaLights.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderAreaLight");            
            EffectPass pass = tech.GetPassByIndex(0);            

            InputLayout layout = ShaderHelper.ConstructInputLayout(lightVolume[(int)LightType.Area].Mesh3d.inputElements, pass.Description.Signature);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(IndexBuffer[(int)LightType.Area], Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer[(int)LightType.Area], MeshInputElements10.GetStride(lightVolume[(int)LightType.Area].Mesh3d.inputElements), 0));
            Game.Device.InputAssembler.SetInputLayout(layout);
            effect.GetVar("colorMap").AsResource().SetResource(baseRT.srv);
            effect.GetVar("normalMap").AsResource().SetResource(normalRT.srv);            
            effect.GetVar("depthMap").AsResource().SetResource(depthStencilSRV);
                        
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Light l = objRef.BaseGameObject as Light;
                    if (l is AreaLight == false || !objRef.IsOn) continue;
                    AreaLight sl = l as AreaLight;

                    effect.GetVar("g_fvViewSpaceLightPosition").AsVector().Set(Vector3.Transform(WorldSpace.GetRealWorldPos(objRef.Position, c), Camera.ViewMatrix));
                    effect.GetVar("g_fvViewSpaceLightPosition2").AsVector().Set(Vector3.Transform(WorldSpace.GetRealWorldPos(objRef.EndPoint, c), Camera.ViewMatrix));
                    effect.GetVar("g_fLightMaxRange").AsScalar().Set(objRef.MaxRange);
                    effect.GetVar("lightColor").AsVector().Set(objRef.LightColor);
                    effect.GetVar("lightIntensity").AsScalar().Set(objRef.Intensity);
                    effect.GetVar("lerpInc").AsScalar().Set(objRef.LerpInc);
                    effect.GetVar("matWorldViewProjection").AsMatrix().SetMatrix(sl.GetLightVolumeWVP(c, objRef));
                    effect.GetVar("g_fLightMaxRange").AsScalar().Set(objRef.MaxRange);
                    effect.GetVar("useSpecular").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.UseSpecular));           
                    
                    pass.Apply();                    
                    Game.Device.DrawIndexed((lightVolume[(int)LightType.Area].Mesh3d.attrTable[0].FaceCount * 3), 0, 0);
                }
            }

            UnbindMRT(tech, effect.GetVar("colorMap").AsResource(), effect.GetVar("normalMap").AsResource(), effect.GetVar("depthMap").AsResource());            
        }
        
        private static void DrawPointLights(bool wpf, ShaderResourceView depthStencilSRV)
        {
            if (lightVolume[(int)LightType.Point] == null || lightVolume[(int)LightType.Point].Mesh3d == null) return;
            Shader effect = WorldData.GetObject("PointLights.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderPointLight");            
            EffectPass pass = tech.GetPassByIndex(0);            
            InputLayout layout = ShaderHelper.ConstructInputLayout(lightVolume[(int)LightType.Point].Mesh3d.inputElements, pass.Description.Signature);            

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(IndexBuffer[(int)LightType.Point], Format.R16_UInt, 0);            
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer[(int)LightType.Point], MeshInputElements10.GetStride(lightVolume[(int)LightType.Point].Mesh3d.inputElements), 0));
            Game.Device.InputAssembler.SetInputLayout(layout);
            effect.GetVar("colorMap").AsResource().SetResource(baseRT.srv);
            effect.GetVar("normalMap").AsResource().SetResource(normalRT.srv);                    
            effect.GetVar("depthMap").AsResource().SetResource(depthStencilSRV);
            effect.GetVar("matInView").AsMatrix().SetMatrix(Matrix.Invert(Camera.ViewMatrix));                        
                        
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Light l = objRef.BaseGameObject as Light;
                    if (l is PointLight == false || !objRef.IsOn) continue;
                    PointLight pl = l as PointLight;                                        
                    
                    
                    effect.GetVar("g_fvViewSpaceLightPosition").AsVector().Set(Vector3.TransformCoordinate(WorldSpace.GetRealWorldPos(objRef.Position, c), Camera.ViewMatrix) );
                    effect.GetVar("matWorldView").AsMatrix().SetMatrix(pl.GetLightVolumeWorldMatrix(c, objRef) * Camera.ViewMatrix); 
                    effect.GetVar("matWorldViewProjection").AsMatrix().SetMatrix(pl.GetLightVolumeWVP(c, objRef));                    
                    effect.GetVar("lightColor").AsVector().Set(objRef.LightColor);
                    effect.GetVar("lightIntensity").AsScalar().Set(objRef.Intensity);
                    
                    effect.GetVar("g_fLightMaxRange").AsScalar().Set(objRef.MaxRange);
                    effect.GetVar("useSpecular").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.UseSpecular));                           
                    

                    if (pl.LightMapSRV != null)
                        effect.GetVar("lightMap").AsResource().SetResource(pl.LightMapSRV);
                    else
                        effect.GetVar("lightMap").AsResource().SetResource(whiteTex);
                    if (pl.LightMaskSRV != null)
                        effect.GetVar("lightMask").AsResource().SetResource(pl.LightMaskSRV);
                    else
                        effect.GetVar("lightMask").AsResource().SetResource(whiteTex);
                    
                    pass.Apply();                    
                    Game.Device.DrawIndexed((lightVolume[(int)LightType.Point].Mesh3d.attrTable[0].FaceCount * 3), 0, 0);                    
                }
            }

            /*
             * This isnt the way to do it.  Maybe in ambient light??
            if (Lightning.IsOn && Lightning.RenderLight)
            {
                effect.GetVar("g_fvViewSpaceLightPosition").AsVector().Set(Vector3.TransformCoordinate(WorldSpace.GetRealWorldPos(Lightning.LightningPos, MainPlayer.CurrentCell), Camera.ViewMatrix));
                effect.GetVar("g_fvWorldSpaceLightPosition").AsVector().Set(WorldSpace.GetRealWorldPos(Lightning.LightningPos, MainPlayer.CurrentCell));
                effect.GetVar("matWorldViewProjection").AsMatrix().SetMatrix(Lightning.GetLightVolumeWVP());                
                effect.GetVar("lightColor").AsVector().Set(Lightning.LightningColor);
                effect.GetVar("g_fLightMaxRange").AsScalar().Set(Lightning.LightningRadius);

                effect.GetVar("lightMap").AsResource().SetResource(whiteTex);
                effect.GetVar("lightMask").AsResource().SetResource(whiteTex);                
                pass.Apply();                
                Game.Device.DrawIndexed((lightVolume[(int)LightType.Point].Mesh3d.attrTable[0].FaceCount * 3), 0, 0);
            }*/

            UnbindMRT(tech, effect.GetVar("colorMap").AsResource(), effect.GetVar("normalMap").AsResource(), effect.GetVar("depthMap").AsResource());
        }

        private static void DrawSpotLights(bool wpf, ShaderResourceView depthStencilSRV)
        {
            if (lightVolume[(int)LightType.Spot] == null || lightVolume[(int)LightType.Spot].Mesh3d == null) return;
            Shader effect = WorldData.GetObject("SpotLights.fx") as Shader;
            if (effect == null || effect.EffectObj == null) return;
            if (wpf) FillShaderMaterials(effect);
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderSpotLight");            
            
            EffectPass pass = tech.GetPassByIndex(0);            
            InputLayout layout = ShaderHelper.ConstructInputLayout(lightVolume[(int)LightType.Spot].Mesh3d.inputElements, pass.Description.Signature);            

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(IndexBuffer[(int)LightType.Spot], Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer[(int)LightType.Spot], MeshInputElements10.GetStride(lightVolume[(int)LightType.Spot].Mesh3d.inputElements), 0));            
            Game.Device.InputAssembler.SetInputLayout(layout);
            effect.GetVar("colorMap").AsResource().SetResource(baseRT.srv);
            effect.GetVar("normalMap").AsResource().SetResource(normalRT.srv);            
            effect.GetVar("depthMap").AsResource().SetResource(depthStencilSRV);
            effect.GetVar("matInView").AsMatrix().SetMatrix(Matrix.Invert(Camera.ViewMatrix)); 
                        
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Light l = objRef.BaseGameObject as Light;
                    if (l is SpotLight == false || !objRef.IsOn) continue;
                    SpotLight sl = l as SpotLight;              
                    

                    Vector3 worldSpaceLightDirection = Vector3.Normalize(WorldSpace.GetRealWorldPos(objRef.Target, c) - WorldSpace.GetRealWorldPos(objRef.Position, c));
                    effect.GetVar("g_fvViewSpaceLightDirection").AsVector().Set(Vector3.TransformNormal(worldSpaceLightDirection, Camera.ViewMatrix));
                    effect.GetVar("g_fvViewSpaceLightPosition").AsVector().Set(Vector3.Transform(WorldSpace.GetRealWorldPos(objRef.Position, c), Camera.ViewMatrix));                    
                    effect.GetVar("g_fLightMaxRange").AsScalar().Set(objRef.MaxRange);
                    effect.GetVar("g_fInnerAngle").AsScalar().Set((float)Math.Cos(objRef.InnerAngle * 0.5f));
                    effect.GetVar("g_fOuterAngle").AsScalar().Set((float)Math.Cos(objRef.OuterAngle * 0.5f));
                    effect.GetVar("lightColor").AsVector().Set(objRef.LightColor);
                    effect.GetVar("matWorldView").AsMatrix().SetMatrix(sl.GetLightVolumeWorldMatrix(c, objRef) * Camera.ViewMatrix); 
                    effect.GetVar("matWorldViewProjection").AsMatrix().SetMatrix(sl.GetLightVolumeWVP(c, objRef));
                    effect.GetVar("lightIntensity").AsScalar().Set(objRef.Intensity);
                    effect.GetVar("useSpecular").AsScalar().Set(objRef.getAttribute(GameObjectRefAttribute.UseSpecular));

                    if (sl.LightMapSRV != null)
                        effect.GetVar("lightMap").AsResource().SetResource(sl.LightMapSRV);
                    else
                        effect.GetVar("lightMap").AsResource().SetResource(whiteTex);
                    if (sl.LightMaskSRV != null)
                        effect.GetVar("lightMask").AsResource().SetResource(sl.LightMaskSRV);
                    else
                        effect.GetVar("lightMask").AsResource().SetResource(whiteTex);


                    pass.Apply();                    
                    Game.Device.DrawIndexed((lightVolume[(int)LightType.Spot].Mesh3d.attrTable[0].FaceCount * 3), 0, 0);
                }
            }

            UnbindMRT(tech, effect.GetVar("colorMap").AsResource(), effect.GetVar("normalMap").AsResource(), effect.GetVar("depthMap").AsResource());             
        }

      


        #region light Halo
        private static void InitHalos()
        {
            DirectoryInfo di = new DirectoryInfo(Utils.GetRootPath() + "\\Content\\Textures\\flares");
            FileInfo[] haloFiles = di.GetFiles("light*");
            haloFlares = new List<string>(haloFiles.Length);
            foreach (FileInfo f in haloFiles) haloFlares.Add(f.Name);

            ShaderResourceView ppSRV = TextureLoader.LoadTextureArray(haloFiles);
            Shader haloEff = WorldData.GetObject("halo.fx") as Shader;
            if(haloEff != null)
                haloEff.GetVar("g_txTextures").AsResource().SetResource(ppSRV);
            ppSRV.Dispose();
        }

        private static void DrawHalos()
        {
            List<HaloVertex> vertices = new List<HaloVertex>();            
            List<Cell> cells = MainPlayer.CurrentWorldSpace == null ? new List<Cell>() : MainPlayer.CurrentWorldSpace.CellsToDraw;
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> objRefs = MainPlayer.CurrentWorldSpace.GetLightsToDraw(c);
                foreach (GameObjectReference objRef in objRefs.Values)
                {
                    Light l = objRef.BaseGameObject as Light;
                    if (l is AreaLight || !objRef.IsOn || l.HaloSRV == null) continue;

                    HaloVertex hv = new HaloVertex();
                    hv.pos = WorldSpace.GetRealWorldPos(objRef.Position, c);
                    hv.color = new Vector4(objRef.LightColor, 1);
                    hv.texIndex = haloFlares.IndexOf(l.HaloTextureName);

                    vertices.Add(hv);
                }
            }

            if (vertices.Count == 0) return;

            //create vertex buffers for the rain, two will be used to pingpong between during animation            
            BufferDescription bd = new BufferDescription();
            bd.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(vertices[0]) * vertices.Count;
            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write;
            bd.BindFlags = BindFlags.VertexBuffer;

            using (D3D10.Buffer g_pParticleDrawFrom = new D3D10.Buffer(Game.Device, bd))
            {
                using (DataStream ds = g_pParticleDrawFrom.Map(MapMode.WriteDiscard, SlimDX.Direct3D10.MapFlags.None))
                {
                    ds.Position = 0;
                    ds.WriteRange(vertices.ToArray());
                    g_pParticleDrawFrom.Unmap();

                    Shader haloEff = WorldData.GetObject("halo.fx") as Shader;
                    EffectTechnique RenderParticlesCheap = haloEff.EffectObj.GetTechniqueByName("RenderParticlesCheap");

                    Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.PointList);
                    InputLayout g_pVertexLayoutRainVertex = ShaderHelper.ConstructInputLayout(MeshInputElements10.HaloVertex, RenderParticlesCheap.GetPassByIndex(0).Description.Signature);
                    Game.Device.InputAssembler.SetInputLayout(g_pVertexLayoutRainVertex);
                    Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(g_pParticleDrawFrom, MeshInputElements10.GetStride(MeshInputElements10.HaloVertex), 0));

                    haloEff.GetVar("g_mInvView").AsMatrix().SetMatrix(Matrix.Invert(Camera.ViewMatrix));
                    haloEff.GetVar("g_mViewProj").AsMatrix().SetMatrix(Camera.ViewMatrix * Camera.ProjectionMatrix);

                    RenderParticlesCheap.GetPassByIndex(0).Apply();
                    Game.Device.Draw(vertices.Count, 0);

                    Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
                    RenderParticlesCheap.GetPassByIndex(0).Apply();
                }
            }


            //Vector3 eyeToLight = l.Position - Camera.Position;
            //eyeToLight.Normalize();

            //double n = Vector3.Dot(eyeToLight, Camera.Forward);
            //double alpha = Math.Pow(n, 3.0);


            //float n2 = Vector3.Dot(eyeToLight, dirVector);
            //if (n2 > 0)
            //  alpha -= n2 * 2;


            //scaleValue = Math.Pow(n, 3) * maxScale;
            //if (scaleValue < minScale) scaleValue = minScale;
        }

        #endregion


        #region Helpers
        public static void FillShaderMaterials(Shader effect)
        {
            if (effect == null || effect.EffectObj == null) return;
                        
            List<BaseGameObject> materials = WorldData.GetObjectsByType(typeof(Material));
            Material[] matsArr = new Material[materials.Count];
            foreach (Material mat in materials)
            {
                matsArr[mat.MaterialIndex] = mat;                
            }

            
            List<Vector4> mat_Properties1 = new List<Vector4>();
            List<Vector4> mat_Properties2 = new List<Vector4>();
            List<Vector4> mat_Properties3 = new List<Vector4>();
            List<Vector4> mat_Properties4 = new List<Vector4>();

            foreach (Material mat in matsArr)
            {
                if (mat == null)
                {
                    Material m = WorldData.GetObject("DefaultMaterial") as Material;
                    mat_Properties1.Add(new Vector4(m.SpecularPower, m.SpecularIntensity, m.Reflectivity, m.Emissive));
                    mat_Properties2.Add(new Vector4(m.getAttribute(MaterialAttribute.IsGlow) ? 1 : 0, m.ReflectionSmoothness, (int)m.LightModel, 1));
                    mat_Properties3.Add(new Vector4(m.Roughness, m.RefAtNormIncidence, m.Smoothness, m.Metalness));
                    mat_Properties4.Add(new Vector4(m.AnisotropicRoughnessX, m.AnisotropicRoughnessY, 1, 1));
                }
                else
                {
                    mat_Properties1.Add(new Vector4(mat.SpecularPower, mat.SpecularIntensity, mat.Reflectivity, mat.Emissive));
                    mat_Properties2.Add(new Vector4(mat.getAttribute(MaterialAttribute.IsGlow) ? 1 : 0, mat.ReflectionSmoothness, (int)mat.LightModel, 1));
                    mat_Properties3.Add(new Vector4(mat.Roughness, mat.RefAtNormIncidence, mat.Smoothness, mat.Metalness));
                    mat_Properties4.Add(new Vector4(mat.AnisotropicRoughnessX, mat.AnisotropicRoughnessY, 1, 1));
                }
            }

            if (mat_Properties1 != null && mat_Properties1.Count > 0) effect.GetVar("mat_Properties1").AsVector().Set(mat_Properties1.ToArray());
            if (mat_Properties2 != null && mat_Properties2.Count > 0) effect.GetVar("mat_Properties2").AsVector().Set(mat_Properties2.ToArray());
            if (mat_Properties3 != null && mat_Properties3.Count > 0) effect.GetVar("mat_Properties3").AsVector().Set(mat_Properties3.ToArray());
            if (mat_Properties4 != null && mat_Properties4.Count > 0) effect.GetVar("mat_Properties4").AsVector().Set(mat_Properties4.ToArray());
             
        }


        public static void Dispose()
        {
            Disposed = true;
            baseRT.Dispose();            
            normalRT.Dispose();            
            reflectionRT.Dispose();
            reflectionDSV.Dispose();
            baseRTCopy.Dispose();            

            if (whiteTex != null && !whiteTex.Disposed) whiteTex.Dispose();
            if (SplashBumpTexture != null && !SplashBumpTexture.Disposed) SplashBumpTexture.Dispose();
            if (SplashDiffuseTexture != null && !SplashDiffuseTexture.Disposed) SplashDiffuseTexture.Dispose();
            if (SnowTexture != null && !SnowTexture.Disposed) SnowTexture.Dispose();
            if (SnowNormalTexture != null && !SnowNormalTexture.Disposed) SnowNormalTexture.Dispose();
            if (WaterNormalMap != null && !WaterNormalMap.Disposed) WaterNormalMap.Dispose();
            

            DisposeLightType(LightType.Point);
            DisposeLightType(LightType.Spot);
            DisposeLightType(LightType.Ambient);
            DisposeLightType(LightType.Area);
            DisposeLightType(LightType.Box);

            QuadRenderComponent.Dispose();            
        }
        public static void Update()
        {
            
        }

       
        private static void InitLightType(LightType lt, string objId)
        {                       
            lightVolume[(int)lt] = WorldData.GetObject(objId) as Model;

            if (lightVolume[(int)lt] != null)
            {
                Mesh m = lightVolume[(int)lt].MeshObj;
                if (m != null)
                {
                    IndexBuffer[(int)lt] = m.GetDeviceIndexBuffer();
                    VertexBuffer[(int)lt] = m.GetDeviceVertexBuffer(0);
                }
            }
        }
        private static void DisposeLightType(LightType lt)
        {
            if (lightVolume == null || lightVolume[(int)lt] == null) return;

            Model gm = lightVolume[(int)lt];
            D3D10.Buffer b1 = IndexBuffer[(int)lt];
            D3D10.Buffer b2 = VertexBuffer[(int)lt];

            if (gm != null) gm.Dispose();
            if (b1 != null && !b1.Disposed) b1.Dispose();
            if (b2 != null && !b2.Disposed) b2.Dispose();
        }
        internal static void UnbindMRT(EffectTechnique tech, params EffectResourceVariable[] vars)
        {
            UnbindMRT(tech.GetPassByIndex(0), vars);
        }
        internal static void UnbindMRT(EffectPass p, params EffectResourceVariable[] vars)
        {
            foreach (EffectResourceVariable v in vars)
                v.SetResource(null);
            p.Apply();
        }
        private static float random()
        {
            if (r == null) r = new Random();
            double n = r.NextDouble();
            return (float)n;

        }
        #endregion

    }
}
