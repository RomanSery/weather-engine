using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Utilities;
using System.Xml.Serialization;


namespace WeatherGame.Framework.Mesh3d
{
    [Serializable]
    public struct MeshNode
    {
        public string nodeName;
        public string parentNodeName;
        public int attributeId;
        public Matrix transform;
    }


    [Serializable]
    public struct AnimationKey
    {
        public Matrix animMat;
        public double time;
    }

    [Serializable]
    public class AnimationChannel
    {
        public string nodeName = null;        
        public AnimationKey[] animKeys;
    }

    [Serializable]
    public class Mesh3DAnimation
    {
        public string animName = null;
        public double duration;
        public AnimationChannel[] channels;
    }

    [Serializable]
    public class Mesh3D : ISerializable
    {
        public BoundingBox bb;

        #region Internal        
        public Texture2D[] textures;        
        public Texture2D[] normalTextures;
        public MeshAttributeRange[] attrTable;        
        public MeshNode[] nodes;
        public string[] materials;        
        public int NumAttributes = 1;
        public bool has32BitIndices = true;
        #endregion

        public string meshFileName;        
        public ShaderResourceView[] textureViews;        
        public ShaderResourceView[] normalTextureViews;        
        public Mesh meshObj;
        public InputElement[] inputElements;        
        public List<Mesh3DAnimation> animations = null;



        public void ComputeBoundingBox()
        {
            try
            {
                using (MeshBuffer b = meshObj.GetVertexBuffer(0))
                {
                    using (DataStream v = b.Map())
                    {
                        v.Position = 0;
                        bb = BoundingBox.FromPoints(v, meshObj.VertexCount, MeshInputElements10.GetStride(inputElements));
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public Mesh3D(){ }

        public void Dispose()
        {            
            if (textures != null)
            {
                foreach (Texture2D t in textures)
                {
                    if (t != null && !t.Disposed) t.Dispose();
                }
            }
            

            if (normalTextures != null)
            {
                foreach (Texture2D t in normalTextures)
                {
                    if (t != null && !t.Disposed) t.Dispose();
                }
            }

            if (textureViews != null)
            {
                foreach (ShaderResourceView v in textureViews)
                {
                    if (v != null && !v.Disposed) v.Dispose();
                }
            }

            if (normalTextureViews != null)
            {
                foreach (ShaderResourceView v in normalTextureViews)
                {
                    if (v != null && !v.Disposed) v.Dispose();
                }
            }

            if (meshObj != null && !meshObj.Disposed)
                meshObj.Dispose();                      
        }

        public Vector3[] GetMeshVertices()
        {
            MeshInputElements10.sNormalMesh[] meshData;
            using (MeshBuffer b = meshObj.GetVertexBuffer(0))
            {
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    meshData = v.ReadRange<MeshInputElements10.sNormalMesh>(meshObj.VertexCount);
                }
            }

            List<Vector3> verts = new List<Vector3>(meshObj.VertexCount);
            foreach (MeshInputElements10.sNormalMesh n in meshData)
                verts.Add(n.Position);
            return verts.ToArray();                      
        }

        public int[] GetMeshIndices()
        {        
            byte[] indices = null;
            int ibSizeInBytes;            
            using (MeshBuffer b = meshObj.GetIndexBuffer())
            {
                ibSizeInBytes = b.SizeInBytes;
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    indices = new byte[b.SizeInBytes];
                    v.Read(indices, 0, b.SizeInBytes);
                }
            }

            if (has32BitIndices)
            {
                int[] realIndices = new int[ibSizeInBytes / 4];
                for (int i = 0, j = 0; i < indices.Length; i += 4, j++)
                {
                    byte[] bytes = { indices[i], indices[i + 1], indices[i + 2], indices[i + 3] };

                    int n = BitConverter.ToInt32(bytes, 0);

                    realIndices[j] = n;
                }
                return realIndices;  
            }
            else
            {
                int[] realIndices = new int[ibSizeInBytes / 2];
                for (int i = 0, j = 0; i < indices.Length; i += 2, j++)
                {
                    byte[] bytes = { indices[i], indices[i + 1] };

                    short n = BitConverter.ToInt16(bytes, 0);

                    realIndices[j] = (short)n;
                }
                return realIndices;  
            }
                                  
        }       



        protected Mesh3D(SerializationInfo info, StreamingContext context)
        {
            SlimDX.Direct3D10.Device d = (SlimDX.Direct3D10.Device)((object[])context.Context)[0];
            InputElement[] inputElements = (InputElement[])((object[])context.Context)[1];
            int vertexCount = info.GetInt32("VertexCount");
            int faceCount = info.GetInt32("FaceCount");
            int indexCount = faceCount * 3;
            int vertexBufferCount = info.GetInt32("VertexBufferCount");
            NumAttributes = info.GetInt32("NumAttributes");


            try
            {
                animations = (List<Mesh3DAnimation>)info.GetValue("Animations", typeof(List<Mesh3DAnimation>));
            }
            catch { }
            
                

            int IndexBufferSizeInBytes = info.GetInt32("IndexBufferSizeInBytes");
            int AttributeBufferSizeInBytes = info.GetInt32("AttributeBufferSizeInBytes");

            MeshFlags mf = MeshFlags.Has32BitIndices;
            if (IndexBufferSizeInBytes / indexCount == 2)
            {
                mf = MeshFlags.None;
                has32BitIndices = false;
            }
            

            meshObj = new Mesh(d, inputElements, inputElements[0].SemanticName, vertexCount, faceCount, mf);

            try
            {
                for (int i = 0; i < vertexBufferCount; i++)
                {                    
                    byte[] vertices = (byte[])info.GetValue("VertexBuffer_" + i, typeof(byte[]));
                    int VertexBufferSizeInBytes = info.GetInt32("VertexBufferSizeInBytes_" + i);
                    using (DataStream ds = new DataStream(VertexBufferSizeInBytes, true, true))
                    {
                        ds.Write(vertices, 0, VertexBufferSizeInBytes);
                        ds.Position = 0;
                        meshObj.SetVertexData(i, ds);
                    }
                }

                byte[] indicies = (byte[])info.GetValue("IndexBuffer", typeof(byte[]));
                using (DataStream ds = new DataStream(IndexBufferSizeInBytes, true, true))
                {
                    ds.Write(indicies, 0, IndexBufferSizeInBytes);
                    ds.Position = 0;
                    meshObj.SetIndexData(ds, faceCount * 3);
                }

                byte[] attr = (byte[])info.GetValue("AttributeBuffer", typeof(byte[]));
                using (DataStream ds = new DataStream(AttributeBufferSizeInBytes, true, true))
                {
                    ds.Write(attr, 0, AttributeBufferSizeInBytes);
                    ds.Position = 0;
                    meshObj.SetAttributeData(ds);
                }


                attrTable = new MeshAttributeRange[NumAttributes];
                for (int i = 0; i < NumAttributes; i++)
                {
                    attrTable[i].FaceCount = info.GetInt32("MeshAttribute_FaceCount_" + i);
                    attrTable[i].FaceStart = info.GetInt32("MeshAttribute_FaceStart_" + i);
                    attrTable[i].Id = info.GetInt32("MeshAttribute_Id_" + i);
                    attrTable[i].VertexCount = info.GetInt32("MeshAttribute_VertexCount_" + i);
                    attrTable[i].VertexStart = info.GetInt32("MeshAttribute_VertexStart_" + i);                    
                }
                meshObj.SetAttributeTable(attrTable);

                nodes = new MeshNode[NumAttributes];
                for (int i = 0; i < NumAttributes; i++)
                {
                    try
                    {
                        nodes[i] = (MeshNode) info.GetValue("MeshNodes_" + i, typeof(MeshNode));
                    }
                    catch { }
                }

                materials = new string[NumAttributes];
                textures = new Texture2D[NumAttributes];
                textureViews = new ShaderResourceView[NumAttributes];
                normalTextures = new Texture2D[NumAttributes];
                normalTextureViews = new ShaderResourceView[NumAttributes];
                for (int i = 0; i < NumAttributes; i++)
                {
                    try
                    {
                        if (info.GetString("TextureFileName_" + i) == null) continue;

                        string diffTexturesPath = Utils.GetRootPath() + "\\Content\\export\\DiffuseTextures\\";
                        string normalTexturesPath = Utils.GetRootPath() + "\\Content\\export\\NormalMaps\\";

                        string[] found = Directory.GetFiles(diffTexturesPath, info.GetString("TextureFileName_" + i), SearchOption.AllDirectories);
                        textures[i] = Texture2D.FromFile(d, found[0]);
                        textureViews[i] = TextureLoader.GetTextureView(d, textures[i]);
                        materials[i] = System.IO.Path.GetFileName(found[0]);


                        string[] normalFound = null;
                        try
                        {
                            normalFound = Directory.GetFiles(normalTexturesPath, info.GetString("TextureFileName_" + i), SearchOption.AllDirectories);
                        }
                        catch { }                        
                        if (normalFound != null && normalFound.Length > 0)
                        {
                            normalTextures[i] = Texture2D.FromFile(d, normalFound[0]);
                            normalTextureViews[i] = TextureLoader.GetTextureView(d, normalTextures[i]);
                        }
                        else
                        {
                            normalTextures[i] = null;
                            normalTextureViews[i] = null;
                        }
                    }
                    catch { }
                }            

                meshObj.GenerateAdjacencyAndPointRepresentation(0.001f);
                meshObj.Optimize(MeshOptimizeFlags.AttributeSort | MeshOptimizeFlags.VertexCache);
                //meshObj.GenerateGeometryShaderAdjacency();                                
                meshObj.Commit();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("VertexCount", meshObj.VertexCount);
            info.AddValue("FaceCount", meshObj.FaceCount);            
            info.AddValue("NumAttributes", NumAttributes);
            info.AddValue("VertexBufferCount", meshObj.VertexBufferCount);            

            for (int i = 0; i < meshObj.VertexBufferCount; i++)
            {
                using (MeshBuffer b = meshObj.GetVertexBuffer(i))
                {
                    using (DataStream v = b.Map())
                    {
                        v.Position = 0;
                        byte[] vertices = new byte[b.SizeInBytes];
                        v.Read(vertices, 0, b.SizeInBytes);
                        info.AddValue("VertexBuffer_" + i, vertices, typeof(byte[]));                        
                        info.AddValue("VertexBufferSizeInBytes_" + i, b.SizeInBytes);                       
                    }
                }
            }

            using (MeshBuffer b = meshObj.GetIndexBuffer())
            {
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    byte[] indicies = new byte[b.SizeInBytes];
                    v.Read(indicies, 0, b.SizeInBytes);
                    info.AddValue("IndexBuffer", indicies, typeof(byte[]));
                    info.AddValue("IndexBufferSizeInBytes", b.SizeInBytes);
                }
            }

            using (MeshBuffer b = meshObj.GetAttributeBuffer())
            {
                using (DataStream v = b.Map())
                {
                    v.Position = 0;
                    byte[] attr = new byte[b.SizeInBytes];
                    v.Read(attr, 0, b.SizeInBytes);
                    info.AddValue("AttributeBuffer", attr, typeof(byte[]));
                    info.AddValue("AttributeBufferSizeInBytes", b.SizeInBytes);
                }
            }
           
                        
            for (int i = 0; i < attrTable.Length; i++)
            {
                info.AddValue("MeshAttribute_FaceCount_" + i, attrTable[i].FaceCount);
                info.AddValue("MeshAttribute_FaceStart_" + i, attrTable[i].FaceStart);
                info.AddValue("MeshAttribute_Id_" + i, attrTable[i].Id);
                info.AddValue("MeshAttribute_VertexCount_" + i, attrTable[i].VertexCount);
                info.AddValue("MeshAttribute_VertexStart_" + i, attrTable[i].VertexStart);
            }

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    info.AddValue("MeshNodes_" + i, nodes[i]);
                }
            }

            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    info.AddValue("TextureFileName_" + i, materials[i]);
                }
            }

            if (animations != null && animations.Count > 0)
            {
                info.AddValue("Animations", animations);
            }

        }
    }
}
