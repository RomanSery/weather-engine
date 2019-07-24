using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3D10 = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Interfaces;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Player;
using WeatherGame;
using WeatherGame.Framework;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Objects;

namespace WorldEditor.Rendering
{
    public static class Grid
    {        
        static  D3D10.Buffer vertices;        
        public static int MetersPerSquare = 100;
        private static int NumVerts;
        public static Vector3 Position = new Vector3(0, 0, 0);

        public static void Initialize()
        {
            Dispose();

            int NumSquares = (WorldSpace.cellSize / MetersPerSquare) * (WorldSpace.cellSize / MetersPerSquare);            

            List<Vector4> verts = new List<Vector4>();
            for (int x = 0; x < WorldSpace.cellSize + 1; x += MetersPerSquare)
            {
                verts.Add(new Vector4(x, 0, 0, 1));
                verts.Add(new Vector4(x, 0, WorldSpace.cellSize, 1));
            }
            for (int z = 0; z < WorldSpace.cellSize + 1; z += MetersPerSquare)
            {
                verts.Add(new Vector4(0, 0, z, 1));
                verts.Add(new Vector4(WorldSpace.cellSize, 0, z, 1));
            }            

            DataStream s = new DataStream(verts.Count * Marshal.SizeOf(typeof(Vector4)), true, true);
            s.WriteRange(verts.ToArray());           
            s.Position = 0;

            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.BindFlags = BindFlags.VertexBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.None;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = verts.Count * Marshal.SizeOf(typeof(Vector4));
            bufferDescription.Usage = ResourceUsage.Default;
            NumVerts = verts.Count;

            vertices = new D3D10.Buffer(Game.Device, s, bufferDescription);
            s.Close();
            s.Dispose();       
        }

        public static void Draw(Matrix mWorld)
        {
            Matrix w = Matrix.Translation(Position) * mWorld;
            w = w + Matrix.Translation(-WorldSpace.cellSize, 0, -WorldSpace.cellSize);
            VertexBufferBinding vbb = new VertexBufferBinding(vertices, Marshal.SizeOf(typeof(Vector4)), 0);
                        
            Shader e = WorldData.GetObject("grid.fx") as Shader;
            if (e == null) return;
            e.GetVar("color").AsVector().Set(Global.GlobalSettings.GridColor);
            RenderingFunctions.RenderVertices(vbb, NumVerts, w, MeshInputElements10.PositionOnly4, "grid.fx", "Render");            
        }       


        public static void Dispose()
        {
            if (vertices != null && !vertices.Disposed)
            {
                vertices.Dispose();
                vertices = null;
            }
        }     
     
    }
}
