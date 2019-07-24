using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Configuration;

using SlimDX;
using DXGI = SlimDX.DXGI;
using SlimDX.Direct3D10;
using WeatherGame.Framework;
using BuildTextures;
using System.Runtime.InteropServices;
using WeatherGame.Framework.Mesh3d;


namespace WeatherGame.BuildContent
{
    internal static class MeshHelper
    {
        internal struct SceneObject
        {
            public string nodeName;
            public string parentNodeName;
            public Assimp.Mesh pMesh;
            public Assimp.Matrix4x4 transform;           
        }

      


        internal static Mesh3D LoadFromFile(Device device, Assimp.Scene pScene, InputElement[] ieLayout)
        {          
            Assimp.Node root = pScene.RootNode;
            List<SceneObject> meshArr = new List<SceneObject>();
            CopyNodesWithMeshes(pScene, root, ref meshArr);          
            
            
            return LoadFromFileSUB(device, pScene, ieLayout, meshArr);
        }



        internal static void CopyNodesWithMeshes(Assimp.Scene pScene, Assimp.Node node, ref List<SceneObject> meshArr) 
        {                           

            // if node has meshes, create a new scene object for it   
            if( node.HasMeshes)  
            {   
                foreach(int i in node.MeshIndices)
                {
                    SceneObject so = new SceneObject();
                    so.nodeName = node.Name;
                    if (node.Parent != null) so.parentNodeName = node.Parent.Name;
                    so.pMesh = pScene.Meshes[i];
                    so.transform = node.Transform;
                    so.transform.Transpose();                    
                    
                    meshArr.Add(so);
                }            
            }
            
            // continue for all child nodes   
            if(node.HasChildren)
            {
                foreach(Assimp.Node childNode in node.Children)
                {
                    CopyNodesWithMeshes(pScene, childNode, ref meshArr); 
                }
            }           
        }

        internal static Mesh3D LoadFromFileSUB(Device device, Assimp.Scene pScene, InputElement[] ieLayout, List<SceneObject> meshArr)
        {
            string sourceTextures = ConfigurationSettings.AppSettings["SourceTextures"];                                   
           
            int iVBufferSize = 0;
            int iIBufferSize = 0;           
            foreach (SceneObject so in meshArr)
            {                               
                iVBufferSize += (int)so.pMesh.VertexCount;
                iIBufferSize += (int)so.pMesh.FaceCount * 3;               
            }
            
                        
            MeshInputElements10.sNormalMesh[] tVertex = new MeshInputElements10.sNormalMesh[iVBufferSize];
            //short[] tIndex = new short[iIBufferSize];
            uint[] tIndex = new uint[iIBufferSize];
            MeshAttributeRange[] tAttibutes = new MeshAttributeRange[meshArr.Count];
            MeshNode[] nodes = new MeshNode[meshArr.Count];
            string[] tTextures = new string[meshArr.Count];           

            // Monitor global poisition in the vertex, index and attribute buffers.
            int iAttribute = 0;
            int iBVertex = 0;
            int iBIndex = 0;          
            int prevVertCount = 0;            
            foreach (SceneObject so in meshArr)
            {
                MeshNode n = new MeshNode();
                n.attributeId = iAttribute;
                n.nodeName = so.nodeName;
                n.parentNodeName = so.parentNodeName;
                n.transform = toDXMat(so.transform);
                nodes[iAttribute] = n;


                MeshAttributeRange pAttrib = new MeshAttributeRange();                
                pAttrib.Id = iAttribute;
                pAttrib.VertexStart = iBVertex;
                pAttrib.FaceStart = iBIndex / 3;

                Assimp.Material mat = pScene.Materials[so.pMesh.MaterialIndex];
                if (mat.GetTextureCount(Assimp.TextureType.Diffuse) > 0)
                {
                    string inputFilePath = mat.GetTexture(Assimp.TextureType.Diffuse, 0).FilePath.Replace("\\\\", "\\");
                    string inputFileName = Path.GetFileName(inputFilePath);
                    string ext = Path.GetExtension(inputFileName);
                    string subFolder = inputFilePath.Replace(sourceTextures, "").Split('\\')[1];
                    string diffOutFile = subFolder + "\\" + inputFileName.Replace(ext, ".dds");
                    tTextures[iAttribute] = diffOutFile;
                }                               
               
                // Copy verticies.
                int iMeshVerts = (int)so.pMesh.VertexCount;
                for (int iVertex = 0; iVertex < iMeshVerts; ++iVertex) { tVertex[iVertex + iBVertex] = getVert(so, iVertex); }

                // Increment the vertex count by the number of verticies we just looped over.                
                iBVertex += iMeshVerts;

                // Copy indicies.
                int iMeshFaces = (int)so.pMesh.FaceCount;                    
                for (int iFace = 0; iFace < iMeshFaces; ++iFace)
                {
                    uint[] tIndices = so.pMesh.Faces[iFace].Indices;
                    //tIndex[iBIndex++] = Convert.ToInt16(tIndices[0] + (iAttribute == 0 ? 0 : prevVertCount));
                    //tIndex[iBIndex++] = Convert.ToInt16(tIndices[1] + (iAttribute == 0 ? 0 : prevVertCount));
                    //tIndex[iBIndex++] = Convert.ToInt16(tIndices[2] + (iAttribute == 0 ? 0 : prevVertCount));                        
                    tIndex[iBIndex++] = Convert.ToUInt32(tIndices[0] + (iAttribute == 0 ? 0 : prevVertCount));
                    tIndex[iBIndex++] = Convert.ToUInt32(tIndices[1] + (iAttribute == 0 ? 0 : prevVertCount));
                    tIndex[iBIndex++] = Convert.ToUInt32(tIndices[2] + (iAttribute == 0 ? 0 : prevVertCount));                        
                }

                // Increment the face count by the number of faces we just looped over.                
                prevVertCount += iMeshVerts;               

                
                pAttrib.FaceCount = iMeshFaces;
                pAttrib.VertexCount = iMeshVerts;
                tAttibutes[iAttribute] = pAttrib;               
                iAttribute++;
            }

            Mesh3D gm = new Mesh3D();
            Mesh output = new Mesh(device, ieLayout, ieLayout[0].SemanticName, iVBufferSize, iIBufferSize / 3, MeshFlags.Has32BitIndices);
            output.SetAttributeTable(tAttibutes);

            DataStream verts = new DataStream(iVBufferSize * Marshal.SizeOf(typeof(MeshInputElements10.sNormalMesh)), true, true);
            //DataStream indicies = new DataStream(iIBufferSize * Marshal.SizeOf(typeof(short)), true, true);            
            DataStream indicies = new DataStream(iIBufferSize * Marshal.SizeOf(typeof(uint)), true, true);            
            verts.WriteRange(tVertex);
            indicies.WriteRange(tIndex);


            verts.Position = 0;
            indicies.Position = 0;

            output.SetVertexData(0, verts);
            output.SetIndexData(indicies, iIBufferSize);
            output.Commit();
            verts.Dispose();
            indicies.Dispose();            

            gm.attrTable = tAttibutes;
            gm.meshObj = output;
            gm.NumAttributes = meshArr.Count;
            gm.materials = tTextures;
            gm.nodes = nodes;

            if (pScene.HasAnimations)
            {
                gm.animations = new List<Mesh3DAnimation>();
                foreach (Assimp.Animation a in pScene.Animations)
                {
                    Mesh3DAnimation anim = new Mesh3DAnimation();
                    anim.animName = a.Name;
                    anim.duration = a.DurationInTicks;
                    anim.channels = new AnimationChannel[a.NodeAnimationChannelCount];

                    int x = 0;
                    foreach (Assimp.NodeAnimationChannel c in a.NodeAnimationChannels)
                    {
                        if (c == null) continue;

                        AnimationChannel ac = new AnimationChannel();
                        ac.nodeName = c.NodeName;
                        ac.animKeys = new AnimationKey[c.RotationKeyCount];
                        for (int i = 0; i < c.PositionKeys.Length; i++)
                        {
                            double time = 0;
                            SceneObject sObj = meshArr[0];
                            foreach (SceneObject so in meshArr)
                            {
                                if (so.nodeName == c.NodeName)
                                {
                                    sObj = so;
                                    break;
                                }
                            }

                            time = c.PositionKeys[i].Time;
                            Matrix mat = toDXMat(sObj.transform);                                                               
                            Vector3 rotationCenter = new Vector3(mat.M41, mat.M42, mat.M43);                                                        
                            mat.Invert();  
                            Vector3 pos = Vector3.TransformCoordinate(toDX(c.PositionKeys[i].Value), mat);                                                                                                                                              

                            
                            Matrix matAnim = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, new Vector3(1, 1, 1), rotationCenter, toDX(c.RotationKeys[i].Value), pos);
                           
                            
                            AnimationKey ak = new AnimationKey();
                            ak.animMat = matAnim;
                            ak.time = time;
                            ac.animKeys[i] = ak;
                        }

                        anim.channels[x++] = ac;
                    }

                    gm.animations.Add(anim);
                }
            }
            
           
           

            return gm;
        }

      


     


        #region Helpers      
        private static MeshInputElements10.sNormalMesh getVert(SceneObject so, int i)
        {            
            MeshInputElements10.sNormalMesh pVertex = new MeshInputElements10.sNormalMesh();

            pVertex.Position = toDX(so.pMesh.Vertices[i]); //toDx3 for cone.mesh
            if (so.pMesh.Normals != null) pVertex.Normal = toDX(so.pMesh.Normals[i]);
            if (so.pMesh.Tangents != null) pVertex.Tangent = toDX(so.pMesh.Tangents[i]);

            if (so.pMesh.BiTangents != null) pVertex.Binormal = toDX(so.pMesh.BiTangents[i]);
            Assimp.Vector3D[] texCoord = so.pMesh.GetTextureCoords(0);
            if (texCoord != null) pVertex.TEXCOORD = toDX2(texCoord[i]);

            return pVertex;
        }


        private static Matrix toDXMat(Assimp.Matrix4x4 mat)
        {            
            //scaling is
            //M11, M22, M33

            //translation is 
            //M41, M42, M43


            Matrix m = new Matrix();
            m.M11 = mat.A1;
            m.M12 = mat.A2;
            m.M13 = mat.A3;
            m.M14 = mat.A4;

            m.M21 = mat.B1;
            m.M22 = mat.B2;
            m.M23 = mat.B3;
            m.M24 = mat.B4;

            m.M31 = mat.C1;
            m.M32 = mat.C2;
            m.M33 = mat.C3;
            m.M34 = mat.C4;

            m.M41 = mat.D1;
            m.M42 = mat.D2;
            m.M43 = mat.D3;
            m.M44 = mat.D4;

            if (m.M11 == 0) m.M11 = 1;
            if (m.M22 == 0) m.M22 = 1;
            if (m.M33 == 0) m.M33 = 1;

            return m;

        }

        private static Matrix toDXMat(Assimp.Matrix3x3 mat)
        {
            //scaling is
            //M11, M22, M33

            //translation is 
            //M41, M42, M43


            Matrix m = new Matrix();
            m.M11 = mat.A1;
            m.M12 = mat.A2;
            m.M13 = mat.A3;
            //m.M14 = 1;// mat.A4;

            m.M21 = mat.B1;
            m.M22 = mat.B2;
            m.M23 = mat.B3;
           // m.M24 = 1;// mat.B4;

            m.M31 = mat.C1;
            m.M32 = mat.C2;
            m.M33 = mat.C3;
           // m.M34 = 1;// mat.C4;

           /// m.M41 = 1;// mat.D1;
            //m.M42 = 1;//mat.D2;
           // m.M43 = 1;//mat.D3;
           // m.M44 = 1;//mat.D4;

            if (m.M11 == 0) m.M11 = 1;
            if (m.M22 == 0) m.M22 = 1;
            if (m.M33 == 0) m.M33 = 1;

            return m;

        }

        private static Quaternion toDX(Assimp.Quaternion q)
        {            
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        private static Vector3 toDX(Assimp.Vector3D vValue)
        {
            return new Vector3(vValue.X, vValue.Y, vValue.Z);                        
        }
        private static Vector3 toDX3(Assimp.Vector3D vValue)
        {
            //return new Vector3(vValue.X, vValue.Y, vValue.Z);     
            float y = vValue.Y - 1.0f;
            return new Vector3(vValue.X, y, -vValue.Z);
        }
        private static Vector2 toDX2(Assimp.Vector3D vValue)
        {
            return new Vector2(vValue.X, vValue.Y);
        }
        private static List<Assimp.Mesh> getMeshesByMatIndex(Assimp.Scene pScene, int matIndex)
        {
            List<Assimp.Mesh> meshes = new List<Assimp.Mesh>();
            foreach (Assimp.Mesh m in pScene.Meshes)
            {
                if (m.PrimitiveType != Assimp.PrimitiveType.Triangle) continue;
                if (m.MaterialIndex == matIndex) meshes.Add(m);
            }
            return meshes;
        }

        private static ShaderResourceView GetTextureView(Device device, Texture2D t)
        {
            Texture2DDescription desc = t.Description;            
            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = desc.Format;            
            SRVDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            SRVDesc.MipLevels = desc.MipLevels;
            SRVDesc.ArraySize = desc.ArraySize;
            SRVDesc.ElementWidth = desc.Width;            
            return new ShaderResourceView(device, t, SRVDesc);
        }       
        #endregion




        internal static void BuildMeshTextures(Device device, Assimp.Scene model)
        {
            string sourceTextures = ConfigurationSettings.AppSettings["SourceTextures"];

            if (model.HasMaterials)
            {
                foreach (Assimp.Material mat in model.Materials)
                {
                    if (mat.GetTextureCount(Assimp.TextureType.Diffuse) > 0)
                    {
                        string texFilePath = mat.GetTexture(Assimp.TextureType.Diffuse, 0).FilePath;
                        if (!string.IsNullOrEmpty(texFilePath))
                        {
                            string inputFilePath = texFilePath.Replace("\\\\", "\\");
                            string inputFileName = Path.GetFileName(inputFilePath);
                            string ext = Path.GetExtension(inputFileName);
                            string subFolder = inputFilePath.Replace(sourceTextures, "").Split('\\')[1];

                            string diffOutFile = ConfigurationSettings.AppSettings["ProcessedTextures"] + "DiffuseTextures\\" + subFolder + "\\" + inputFileName.Replace(ext, ".dds");
                            string normalMapOutFile = ConfigurationSettings.AppSettings["ProcessedTextures"] + "NormalMaps\\" + subFolder + "\\" + inputFileName.Replace(ext, ".dds");



                            if (!Directory.Exists(Path.GetDirectoryName(diffOutFile)))
                                Directory.CreateDirectory(Path.GetDirectoryName(diffOutFile));

                            if (!Directory.Exists(Path.GetDirectoryName(normalMapOutFile)))
                                Directory.CreateDirectory(Path.GetDirectoryName(normalMapOutFile));

                            if (File.Exists(normalMapOutFile)) normalMapOutFile = null;
                            if (File.Exists(diffOutFile)) diffOutFile = null;
                            BuildTextures.TextureHelper.ProcessTexture(inputFilePath, diffOutFile, normalMapOutFile);
                        }

                    }
                }
            }
        }
      

    }
}
