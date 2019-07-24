
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

namespace WeatherGame.Framework.Mesh3d
{
    public static class MeshInputElements10
    {
        public static int GetStride(InputElement[] ie)
        {
            int stride = 0;

            foreach (InputElement element in ie)
            {
                if (element.Format == Format.R32G32B32_Float)
                    stride += 12;
                else if (element.Format == Format.R32G32_Float)
                    stride += 8;
                else if (element.Format == Format.R32G32B32A32_Float)
                    stride += 16;
                else if (element.Format == Format.R32_UInt || element.Format == Format.R32_Float || element.Format == Format.R8_UInt)
                    stride += 4;
            }

            return stride;
        }

        public static InputElement[] PosTex = new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)				
            };

        public static InputElement[] PosTex4 = new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
			    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)				
            };

        public static InputElement[] PositionOnly4 = new InputElement[]
		{
			new InputElement("POSITION", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 0, 0)							
		};
        public static InputElement[] FullScreenQuad = new InputElement[]
        {
            new InputElement("VertexID", 0, SlimDX.DXGI.Format.R32_UInt, 0, 0)                
        };


        public static InputElement[] NormalMesh = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            new InputElement("TEXCOORD", 0, DXGI.Format.R32G32_Float, 24, 0),
            new InputElement("TANGENT", 0, DXGI.Format.R32G32B32_Float, 32, 0),
            new InputElement("BINORMAL", 0, DXGI.Format.R32G32B32_Float, 44, 0)
        };
        public struct sNormalMesh
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TEXCOORD;
            public Vector3 Tangent;
            public Vector3 Binormal;
        }
        public struct sPositionOnly4
        {
            public Vector4 Position;            
        }

        public static InputElement[] PositionOnlyInstanced = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32A32_Float, 0, 0, InputClassification.PerVertexData, 0),
            //new InputElement("mTransform", 0, DXGI.Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1)            
            new InputElement("mTransform", 0, DXGI.Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
            new InputElement("mTransform", 1, DXGI.Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
            new InputElement("mTransform", 2, DXGI.Format.R32G32B32A32_Float, 32, 1, InputClassification.PerInstanceData, 1),
            new InputElement("mTransform", 3, DXGI.Format.R32G32B32A32_Float, 48, 1, InputClassification.PerInstanceData, 1)

            //{ "mTransform", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 0, D3D10_INPUT_PER_INSTANCE_DATA, 1 },
            //{ "mTransform", 1, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 16, D3D10_INPUT_PER_INSTANCE_DATA, 1 },
            //{ "mTransform", 2, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 32, D3D10_INPUT_PER_INSTANCE_DATA, 1 },
            //{ "mTransform", 3, DXGI_FORMAT_R32G32B32A32_FLOAT, 1, 48, D3D10_INPUT_PER_INSTANCE_DATA, 1 },
        };

        public static InputElement[] PositionOnly3 = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)            
        };


        public static InputElement[] RainVertex = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            new InputElement("SEED", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            new InputElement("SPEED", 0, DXGI.Format.R32G32B32_Float, 24, 0),
            new InputElement("RAND", 0, DXGI.Format.R32_Float, 36, 0),
            new InputElement("TYPE", 0, DXGI.Format.R8_UInt, 40, 0),
            new InputElement("COLOR", 0, DXGI.Format.R32G32B32_Float, 44, 0)
        };

        public static InputElement[] SnowVertex = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            new InputElement("SEED", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            new InputElement("SPEED", 0, DXGI.Format.R32G32B32_Float, 24, 0),
            new InputElement("RAND", 0, DXGI.Format.R32G32_Float, 36, 0),
            new InputElement("COLOR", 0, DXGI.Format.R32G32B32_Float, 44, 0)
        };

        public static InputElement[] HaloVertex = new InputElement[]
        {
            new InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, DXGI.Format.R32G32B32A32_Float, 12, 0),
            new InputElement("texIndex", 0, DXGI.Format.R8_UInt, 28, 0)          
            //new InputElement("SCALE", 0, DXGI.Format.R32_Float, 28, 0)            
        };
    }

}
