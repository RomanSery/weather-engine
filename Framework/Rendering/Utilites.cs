using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace WeatherGame.Framework.Utilities
{

    public static class Utils
    {
        public static string GetRootPath()
        {
            DirectoryInfo rootDir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).Parent.Parent;
            return rootDir.FullName;
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        } 
    }

           
    public static class TextureLoader
    {
        public static Dictionary<string, Texture2D> _textures = null;
        public static Dictionary<string, Texture3D> _textures3D = null;

        public static ShaderResourceView LoadTexture(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            if (_textures == null)
                _textures = new Dictionary<string, Texture2D>();

            Texture2D tex = null;

            if (_textures.ContainsKey(fileName))
            {
                tex = _textures[fileName];
                if (tex != null) return GetTextureView(Game.Device, tex);
            }
            else
            {
                string path = Utils.GetRootPath() + "\\Content\\Textures\\" + fileName;
                tex = Texture2D.FromFile(Game.Device, path);
                _textures[fileName] = tex;
                if (tex != null) return GetTextureView(Game.Device, tex);
            }

            return null;
        }
        public static ShaderResourceView LoadCubeTexture(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            if (_textures == null)
                _textures = new Dictionary<string, Texture2D>();

            Texture2D tex = null;

            if (_textures.ContainsKey(fileName))
            {
                tex = _textures[fileName];
                if (tex != null) return GetCubeTextureView(Game.Device, tex);
            }
            else
            {
                string path = Utils.GetRootPath() + "\\Content\\Textures\\" + fileName;
                tex = Texture2D.FromFile(Game.Device, path);
                _textures[fileName] = tex;
                if (tex != null) return GetCubeTextureView(Game.Device, tex);
            }

            return null;
        }

        public static ShaderResourceView Load3DTextureFromFile(string fileName)
        {
            if (_textures3D == null) _textures3D = new Dictionary<string, Texture3D>();

            Texture3D texture = null;
            string path = Utils.GetRootPath() + "\\Content\\Textures\\" + fileName;
            ImageInformation? SrcInfo = ImageInformation.FromFile(path);
            ImageLoadInformation texLoadInfo = new ImageLoadInformation();
            texLoadInfo.Width = SrcInfo.Value.Width;
            texLoadInfo.Height = SrcInfo.Value.Height;
            texLoadInfo.Depth = SrcInfo.Value.Depth;
            texLoadInfo.FirstMipLevel = 0;
            texLoadInfo.MipLevels = SrcInfo.Value.MipLevels;
            texLoadInfo.Usage = ResourceUsage.Default;
            texLoadInfo.BindFlags = BindFlags.ShaderResource;
            texLoadInfo.CpuAccessFlags = 0;
            texLoadInfo.OptionFlags = SrcInfo.Value.OptionFlags;
            texLoadInfo.Format = SrcInfo.Value.Format;
            texLoadInfo.FilterFlags = FilterFlags.Triangle;
            texLoadInfo.MipFilterFlags = FilterFlags.Triangle;

            //if (_textures3D.ContainsKey(fileName))
              //  texture = _textures3D[fileName];
            //else
            {
                texture = Texture3D.FromFile(Game.Device, path, texLoadInfo);
                _textures3D[fileName] = texture;
            }
            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = texLoadInfo.Format;
            SRVDesc.Dimension = ShaderResourceViewDimension.Texture3D;
            SRVDesc.MostDetailedMip = 0;
            SRVDesc.MipLevels = texLoadInfo.MipLevels;
            ShaderResourceView srv = new ShaderResourceView(Game.Device, texture, SRVDesc);
            texture.Dispose();
            return srv;
        }

        public static ShaderResourceView LoadTextureArray(FileInfo[] szTextureNames)        
        {           
            Texture2DDescription textureDesc = new Texture2DDescription();
            Texture2D textureArray = null;
            Texture2D pD3D10Resource = null;

            for (int a = 0; a < szTextureNames.Length; ++a)
            {
                FileInfo f = szTextureNames[a];
                ImageInformation? SrcInfo = ImageInformation.FromFile(f.FullName);                
                ImageLoadInformation loadInfo = new ImageLoadInformation();
                loadInfo.Width = SrcInfo.Value.Width;
                loadInfo.Height = SrcInfo.Value.Height;
                loadInfo.Depth = SrcInfo.Value.Depth;
                loadInfo.FirstMipLevel = 0;
                loadInfo.MipLevels = SrcInfo.Value.MipLevels;
                loadInfo.Usage = ResourceUsage.Staging;
                loadInfo.BindFlags = 0;
                loadInfo.CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read;                
                loadInfo.Format = SrcInfo.Value.Format;
                loadInfo.FilterFlags = FilterFlags.None;
                loadInfo.MipFilterFlags = FilterFlags.None;

                pD3D10Resource = Texture2D.FromFile(Game.Device, f.FullName, loadInfo);
                if (pD3D10Resource != null)
                {                    
                    DataRectangle mappedTex2D;
                    if (textureArray == null)
                    {
                        textureDesc = pD3D10Resource.Description;
                        textureDesc.Usage = ResourceUsage.Default;
                        textureDesc.BindFlags = BindFlags.ShaderResource;
                        textureDesc.CpuAccessFlags = CpuAccessFlags.None;
                        textureDesc.ArraySize = szTextureNames.Length;
                        textureArray = new Texture2D(Game.Device, textureDesc);
                    }

                    for (int iMip = 0; iMip < textureDesc.MipLevels; ++iMip)
                    {
                        mappedTex2D = pD3D10Resource.Map(iMip, MapMode.Read, D3D10.MapFlags.None);
                        
                        DataBox db = new DataBox(mappedTex2D.Pitch, 0, mappedTex2D.Data);                                                                 
                        Game.Device.UpdateSubresource(db, textureArray, D3D10.Resource.CalculateSubresourceIndex(iMip, a, textureDesc.MipLevels));

                        pD3D10Resource.Unmap(iMip);                           
                    }
                    pD3D10Resource.Dispose();
                }

                
            }
            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = textureDesc.Format;
            SRVDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
            SRVDesc.MipLevels = textureDesc.MipLevels;
            SRVDesc.ArraySize = szTextureNames.Length;
            SRVDesc.FirstArraySlice = 0;
            SRVDesc.MostDetailedMip = 0;
            ShaderResourceView ppSRV = new ShaderResourceView(Game.Device, textureArray, SRVDesc);
            textureArray.Dispose();
            return ppSRV;            
        }


        public static void Dispose()
        {
            if (_textures != null)
            {
                foreach (KeyValuePair<string, Texture2D> e in _textures)
                {
                    if (e.Value != null && !e.Value.Disposed) e.Value.Dispose();
                }
            }
            if(_textures != null) _textures.Clear();

            if (_textures3D != null)
            {
                foreach (KeyValuePair<string, Texture3D> e in _textures3D)
                {
                    if (e.Value != null && !e.Value.Disposed) e.Value.Dispose();
                }
            }
            if (_textures3D != null) _textures3D.Clear();
        }

        public static ShaderResourceView GetTextureView(SlimDX.Direct3D10.Device device, Texture2D t)
        {
            if (t == null) return null;
            Texture2DDescription desc = t.Description;
            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = desc.Format;
            SRVDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            SRVDesc.MipLevels = desc.MipLevels;
            SRVDesc.ArraySize = desc.ArraySize;
            SRVDesc.ElementWidth = desc.Width;
            return new ShaderResourceView(device, t, SRVDesc);
        }
        public static ShaderResourceView GetCubeTextureView(SlimDX.Direct3D10.Device device, Texture2D t)
        {
            if (t == null) return null;
            Texture2DDescription desc = t.Description;
            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = desc.Format;
            SRVDesc.Dimension = ShaderResourceViewDimension.TextureCube;
            SRVDesc.MipLevels = desc.MipLevels;
            SRVDesc.ArraySize = desc.ArraySize;
            SRVDesc.ElementWidth = desc.Width;
            return new ShaderResourceView(device, t, SRVDesc);
        }
    }
    





    [StructLayout(LayoutKind.Sequential)]
    struct QuadVertex
    {
        public Vector4 pos;
        public Vector2 tex;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct QuadVertex3
    {
        public Vector3 pos;
        public Vector2 tex;
    }

    public static class QuadRenderComponent
    {
        // Private Members
        public static D3D10.Buffer quadVerts;

        // Constructor
        public static void Initialize()
        {
            QuadVertex[] svQuad = new QuadVertex[4];
            svQuad[0].pos = new Vector4(-1.0f, 1.0f, 0.5f, 1.0f);
            svQuad[0].tex = new Vector2(0.0f, 0.0f);
            svQuad[1].pos = new Vector4(1.0f, 1.0f, 0.5f, 1.0f);
            svQuad[1].tex = new Vector2(1.0f, 0.0f);
            svQuad[2].pos = new Vector4(-1.0f, -1.0f, 0.5f, 1.0f);
            svQuad[2].tex = new Vector2(0.0f, 1.0f);
            svQuad[3].pos = new Vector4(1.0f, -1.0f, 0.5f, 1.0f);
            svQuad[3].tex = new Vector2(1.0f, 1.0f);

            DataStream s = new DataStream(4 * Marshal.SizeOf(typeof(QuadVertex)), true, true);
            s.WriteRange(svQuad);


            s.Position = 0;
            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = 4 * Marshal.SizeOf(typeof(QuadVertex));
            bufferDescription.Usage = ResourceUsage.Default;

            quadVerts = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();
        }



        public static void Dispose()
        {
            if (quadVerts != null && !quadVerts.Disposed) quadVerts.Dispose();
        }
    }
    

}
