using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.World;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;
using System;

namespace WeatherGame.Framework.Rendering
{
   

    public struct RenderTarget
    {
        public Texture2D texture;
        public Texture2D depthTexture;
        public ShaderResourceView srv;
        public RenderTargetView rtv;
        public DepthStencilView dsv;

        public int width, height, arraySize;
        public Format format;

        public void Dispose()
        {
            try
            {
                if (texture != null && !texture.Disposed) texture.Dispose();
                if (depthTexture != null && !depthTexture.Disposed) depthTexture.Dispose();
                if (srv != null && !srv.Disposed) srv.Dispose();
                if (rtv != null && !rtv.Disposed) rtv.Dispose();
                if (dsv != null && !dsv.Disposed) dsv.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DISPOSE: " + ex.ToString());
            }
        }
    };


    public static class RenderingFunctions
    {
        public static RenderTarget CreateRenderTarget(Format f, int width, int height)
        {
            RenderTarget rt = new RenderTarget();

            Texture2DDescription texDesc = new Texture2DDescription();
            SampleDescription sd = new SampleDescription(1, 0);
            texDesc.Width = width;
            texDesc.Height = height;
            texDesc.Format = f;
            texDesc.MipLevels = 1;
            texDesc.SampleDescription = sd;
            texDesc.Usage = ResourceUsage.Default;
            texDesc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.ArraySize = 1;
            texDesc.OptionFlags = ResourceOptionFlags.None;
            rt.texture = new Texture2D(Game.Device, texDesc);            


            ShaderResourceViewDescription d = new ShaderResourceViewDescription();
            d.ArraySize = 1;
            d.Dimension = ShaderResourceViewDimension.Texture2D;
            d.Format = texDesc.Format;
            d.MipLevels = 1;
            rt.srv = new ShaderResourceView(Game.Device, rt.texture, d);

            RenderTargetViewDescription rtDesc = new RenderTargetViewDescription();
            rtDesc.Format = texDesc.Format;
            rtDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
            rtDesc.MipSlice = 0;
            rt.rtv = new RenderTargetView(Game.Device, rt.texture, rtDesc);
            
            rt.depthTexture = null;
            rt.dsv = null;

            return rt;
        }
        public static RenderTarget CreateRenderDepthTarget(Format f, int width, int height)
        {
            RenderTarget rt = new RenderTarget();

            rt.texture = null;            
            rt.rtv = null;

            Texture2DDescription depthDesc = new Texture2DDescription();
            SampleDescription sd2 = new SampleDescription(1, 0);
            depthDesc.Width = width;
            depthDesc.Height = height;
            depthDesc.Format = f;// Format.R32_Typeless;//
            depthDesc.MipLevels = 1;
            depthDesc.SampleDescription = sd2;
            depthDesc.Usage = ResourceUsage.Default;
            depthDesc.BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource;
            depthDesc.CpuAccessFlags = CpuAccessFlags.None;
            depthDesc.ArraySize = 1;
            depthDesc.OptionFlags = ResourceOptionFlags.None;
            rt.depthTexture = new Texture2D(Game.Device, depthDesc);

            DepthStencilViewDescription dsvd = new DepthStencilViewDescription();
            dsvd.ArraySize = 1;            
            dsvd.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
            dsvd.Format = Format.D32_Float;//Format.D16_UNorm;
            rt.dsv = new DepthStencilView(Game.Device, rt.depthTexture, dsvd);

            ShaderResourceViewDescription d = new ShaderResourceViewDescription();
            //d.ArraySize = 1;            
            d.Dimension = ShaderResourceViewDimension.Texture2D;
            d.Format = Format.R32_Float;// ;R16_UNorm            
            d.MostDetailedMip = 0;
            d.MipLevels = 1; 
            rt.srv = new ShaderResourceView(Game.Device, rt.depthTexture, d);

            return rt;
        }




        public static void RenderMesh2(Model m, Vector3 position, string effectName, string technique, Matrix mWorld)
        {
            RenderMesh2(m, position, effectName, technique, mWorld, 56);
        }
        public static void RenderMesh2(Model m, Vector3 position, string effectName, string technique, Matrix mWorld, int stride)
        {
            if (m == null || m.MeshObj == null) return;
            Shader e = WorldData.GetObject(effectName) as Shader;
            Matrix world = Matrix.Identity + Matrix.Translation(position);
            if (mWorld != null) world = mWorld;
            ShaderHelper.UpdateCommonEffectVars(e, world);

            EffectTechnique t = e.EffectObj.GetTechniqueByName(technique);
            D3D10.Buffer indexBuffer = m.MeshObj.GetDeviceIndexBuffer();
            D3D10.Buffer vertextBuffer = m.MeshObj.GetDeviceVertexBuffer(0);            

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, stride, 0));            
            
            for (int p = 0; p < t.Description.PassCount; p++)
            {
                EffectPass pass = t.GetPassByIndex(p);
                InputLayout l = ShaderHelper.ConstructInputLayout(m.Mesh3d.inputElements, pass.Description.Signature);
                Game.Device.InputAssembler.SetInputLayout(l);

                for (int subset = 0; subset < m.Mesh3d.NumAttributes; subset++)
                {
                    EffectResourceVariable diffTex = e.GetVar(ShaderHelper.DiffTex).AsResource();
                    if (diffTex != null) diffTex.SetResource(m.Mesh3d.textureViews[subset]);
                    pass.Apply();
                    MeshAttributeRange r = m.Mesh3d.attrTable[subset];
                    // * 2 cause adj data is twice as much data
                    //Game.Device.DrawIndexed((r.FaceCount * 3) * 2, (r.FaceStart * 3) * 2, 0);

                    Game.Device.DrawIndexed((r.FaceCount * 3), (r.FaceStart * 3), 0);
                }
            }

            indexBuffer.Dispose();
            vertextBuffer.Dispose();
        }

        public static void DrawLightVolume(Model gm, Matrix mWorld, Vector4 GridColor)
        {
            if (gm == null || gm.Mesh3d == null) return;
            Shader effect = WorldData.GetObject("SimpleTexturedQuad.fx") as Shader;
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderLightVolume");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(gm.Mesh3d.inputElements, pass.Description.Signature);

            SlimDX.Direct3D10.Buffer indexBuffer = gm.MeshObj.GetDeviceIndexBuffer();
            SlimDX.Direct3D10.Buffer vertexBuffer = gm.MeshObj.GetDeviceVertexBuffer(0);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);            
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, MeshInputElements10.GetStride(gm.Mesh3d.inputElements), 0));
            Game.Device.InputAssembler.SetInputLayout(layout);

            effect.GetVar("color").AsVector().Set(GridColor);            
            effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix);
            pass.Apply();
            Game.Device.DrawIndexed((gm.Mesh3d.attrTable[0].FaceCount * 3), 0, 0);
        }

        
        public static void DrawInstancedSpheres(GraphicsDeviceManager gdm,  List<Matrix> instances)
        {            
            Model sphere = WorldData.GetObject("sphereInstanced.mesh") as Model;
            D3D10.Buffer indexBuffer = sphere.MeshObj.GetDeviceIndexBuffer();
            D3D10.Buffer vertextBuffer = sphere.MeshObj.GetDeviceVertexBuffer(0);
            Shader e = WorldData.GetObject("Instanced.fx") as Shader;
            EffectTechnique t = e.EffectObj.GetTechniqueByName("RenderInstanced");

            InputLayout l = ShaderHelper.ConstructInputLayout(MeshInputElements10.PositionOnlyInstanced, t.GetPassByIndex(0).Description.Signature);



            BufferDescription bd = new BufferDescription();
            bd.SizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(instances[0]) * instances.Count;
            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write;
            bd.BindFlags = BindFlags.VertexBuffer;

            D3D10.Buffer instanceData = new D3D10.Buffer(gdm.Direct3D10.Device, bd);

            DataStream ds = instanceData.Map(MapMode.WriteDiscard, SlimDX.Direct3D10.MapFlags.None);
            ds.Position = 0;
            ds.WriteRange(instances.ToArray());
            instanceData.Unmap();




            Game.Device.InputAssembler.SetInputLayout(l);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, 16, 0), new VertexBufferBinding(instanceData, System.Runtime.InteropServices.Marshal.SizeOf(instances[0]), 0));            
            Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

            e.GetVar("WorldViewProj").AsMatrix().SetMatrix(Camera.ViewMatrix * Camera.ProjectionMatrix);

            t.GetPassByIndex(0).Apply();
            Game.Device.DrawIndexedInstanced((sphere.Mesh3d.attrTable[0].FaceCount * 3), instances.Count, 0, 0, 0);
            //Game.Device.DrawIndexed((sphere.attrTable[0].FaceCount * 3) * 2, 0, 0);
            
            indexBuffer.Dispose();
            vertextBuffer.Dispose();
            sphere.Dispose();           
           
        }


        public static void RenderVertices(VertexBufferBinding vbb, int numVertices, Matrix world, InputElement[] inputElements, string effectName, string technique)
        {
            Shader e = WorldData.GetObject(effectName) as Shader;
            ShaderHelper.UpdateCommonEffectVars(e, world);
            EffectTechnique t = e.EffectObj.GetTechniqueByName(technique);
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineList);
            Game.Device.InputAssembler.SetVertexBuffers(0, vbb);

            for (int p = 0; p < t.Description.PassCount; p++)
            {
                EffectPass pass = t.GetPassByIndex(p);
                InputLayout l = ShaderHelper.ConstructInputLayout(inputElements, pass.Description.Signature);
                Game.Device.InputAssembler.SetInputLayout(l);                
                
                pass.Apply();
                Game.Device.Draw(numVertices, 0);
            }
        }      


        public static void RenderFullScreenQuad(ShaderResourceView srv)
        {
            Shader e = WorldData.GetObject("FullScreenQuad.fx") as Shader;
            if (e == null) return;
            ShaderHelper.UpdateCommonEffectVars(e, Matrix.Identity);
            EffectTechnique t = e.EffectObj.GetTechniqueByName("FullScreen");
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);            

            for (int p = 0; p < t.Description.PassCount; p++)
            {
                EffectPass pass = t.GetPassByIndex(p);
                InputLayout l = ShaderHelper.ConstructInputLayout(MeshInputElements10.FullScreenQuad, pass.Description.Signature);
                Game.Device.InputAssembler.SetInputLayout(l);

                EffectResourceVariable diffTex = e.GetVar(ShaderHelper.DiffTex).AsResource();                
                if (diffTex != null) diffTex.SetResource(srv);
                pass.Apply();
                Game.Device.Draw(3, 0);
            }            
        }


        public static void RenderBoundingBox(Model gm, Matrix mWorld)
        {
            RenderBoundingBox(gm.Mesh3d.bb, mWorld);
        }
        public static void RenderBoundingBox(BoundingBox bb, Matrix mWorld)
        {
            Vector3[] corners = bb.GetCorners();

            DataStream s = new DataStream(corners.Length * Marshal.SizeOf(typeof(Vector3)), true, true);
            s.WriteRange(corners);            
            s.Position = 0;

            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = corners.Length * Marshal.SizeOf(typeof(Vector3));
            bufferDescription.Usage = ResourceUsage.Default;

            D3D10.Buffer vertices = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();


            short[] IB = new short[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,
                0, 4,
                1, 5,
                2, 6,
                3, 7,
                4, 5,
                5, 6,
                6, 7,
                7, 4,
            };
            s = new DataStream(IB.Length * Marshal.SizeOf(typeof(short)), true, true);
            s.WriteRange(IB);
            s.Position = 0;
            bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.IndexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = IB.Length * Marshal.SizeOf(typeof(short));
            bufferDescription.Usage = ResourceUsage.Default;

            D3D10.Buffer indices = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();



            Shader effect = WorldData.GetObject("SimpleTexturedQuad.fx") as Shader;
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderLightVolume");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.PositionOnly3, pass.Description.Signature);            

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineList);
            Game.Device.InputAssembler.SetIndexBuffer(indices, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, MeshInputElements10.GetStride(MeshInputElements10.PositionOnly3), 0));
            Game.Device.InputAssembler.SetInputLayout(layout);

            effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix);
            pass.Apply();
            Game.Device.DrawIndexed(IB.Length, 0, 0);

        }


        public static void RenderIndexedTriangles(Model gm, Matrix mWorld)
        {
            byte[] vertices = null;
            byte[] indices = null;
            int vbSizeInBytes, ibSizeInBytes;

            using (MeshBuffer b = gm.MeshObj.GetVertexBuffer(0))
            {
                vbSizeInBytes = b.SizeInBytes;
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    vertices = new byte[b.SizeInBytes];
                    v.Read(vertices, 0, b.SizeInBytes);
                }
            }
            using (MeshBuffer b = gm.MeshObj.GetIndexBuffer())
            {
                ibSizeInBytes = b.SizeInBytes;
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    indices = new byte[b.SizeInBytes];
                    v.Read(indices, 0, b.SizeInBytes);
                }
            }

            
            DataStream s = new DataStream(vbSizeInBytes, true, true);
            s.WriteRange(vertices);            
            s.Position = 0;
            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = vbSizeInBytes;
            bufferDescription.Usage = ResourceUsage.Default;
            D3D10.Buffer verticesBuffer = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();            
           
            s = new DataStream(ibSizeInBytes, true, true);
            s.WriteRange(indices);
            s.Position = 0;
            bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.IndexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = ibSizeInBytes;
            bufferDescription.Usage = ResourceUsage.Default;
            D3D10.Buffer indicesBuffer = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close(); 

            
            Shader effect = WorldData.GetObject("SimpleTexturedQuad.fx") as Shader;
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderWireframe");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(gm.Mesh3d.inputElements, pass.Description.Signature);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(indicesBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(verticesBuffer, MeshInputElements10.GetStride(gm.Mesh3d.inputElements), 0));            

            Game.Device.InputAssembler.SetInputLayout(layout);

            effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix);
            pass.Apply();
            Game.Device.DrawIndexed(gm.MeshObj.FaceCount * 3, 0, 0);            

        }




        public static void RenderNormals(Model gm, Matrix mWorld)
        {
            byte[] vertices = null;
            byte[] indices = null;
            int vbSizeInBytes, ibSizeInBytes;

            using (MeshBuffer b = gm.MeshObj.GetVertexBuffer(0))
            {
                vbSizeInBytes = b.SizeInBytes;
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    vertices = new byte[b.SizeInBytes];
                    v.Read(vertices, 0, b.SizeInBytes);
                }
            }
            using (MeshBuffer b = gm.MeshObj.GetIndexBuffer())
            {
                ibSizeInBytes = b.SizeInBytes;
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    indices = new byte[b.SizeInBytes];
                    v.Read(indices, 0, b.SizeInBytes);
                }
            }


            DataStream s = new DataStream(vbSizeInBytes, true, true);
            s.WriteRange(vertices);
            s.Position = 0;
            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = vbSizeInBytes;
            bufferDescription.Usage = ResourceUsage.Default;
            D3D10.Buffer verticesBuffer = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();

            s = new DataStream(ibSizeInBytes, true, true);
            s.WriteRange(indices);
            s.Position = 0;
            bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.IndexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = ibSizeInBytes;
            bufferDescription.Usage = ResourceUsage.Default;
            D3D10.Buffer indicesBuffer = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();

            Shader effect = WorldData.GetObject("debugger.fx") as Shader;
            EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderNormals");
            EffectPass pass = tech.GetPassByIndex(0);
            InputLayout layout = ShaderHelper.ConstructInputLayout(gm.Mesh3d.inputElements, pass.Description.Signature);

            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(indicesBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(verticesBuffer, MeshInputElements10.GetStride(gm.Mesh3d.inputElements), 0));

            Game.Device.InputAssembler.SetInputLayout(layout);

            effect.GetVar("WorldViewProj").AsMatrix().SetMatrix(mWorld * Camera.ViewMatrix * Camera.ProjectionMatrix);
            pass.Apply();
            Game.Device.DrawIndexed(gm.MeshObj.FaceCount * 3, 0, 0);

        }
    }
}
