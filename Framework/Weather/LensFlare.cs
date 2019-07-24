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
    public class LensFlare
    {
        #region Fields

        // The lensflare effect is made up from several individual flare graphics,
        // which move across the screen depending on the position of the sun. This
        // helper class keeps track of the position, size, and color for each flare.
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName, string s)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
                id = s;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public ShaderResourceView Texture;
            public string id;
        }

        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, Color.FromArgb( 50,  25,  50), "flares/flare1.png", "sunflare_1"),
            new Flare( 0.3f, 0.4f, Color.FromArgb(100, 255, 200), "flares/flare1.png", "sunflare_2"),
            new Flare( 1.2f, 1.0f, Color.FromArgb(100,  50,  50), "flares/flare1.png", "sunflare_3"),
            new Flare( 1.5f, 1.5f, Color.FromArgb( 50, 100,  50), "flares/flare1.png", "sunflare_4"),

            new Flare(-0.3f, 0.7f, Color.FromArgb(200,  50,  50), "flares/flare2.png", "sunflare_5"),
            new Flare( 0.6f, 0.9f, Color.FromArgb( 50, 100,  50), "flares/flare2.png", "sunflare_6"),
            new Flare( 0.7f, 0.4f, Color.FromArgb( 50, 200, 200), "flares/flare2.png", "sunflare_7"),

            new Flare(-0.7f, 0.7f, Color.FromArgb( 50, 100,  25), "flares/flare3.png", "sunflare_8"),
            new Flare( 0.0f, 0.6f, Color.FromArgb( 25,  25,  25), "flares/flare3.png", "sunflare_9"),
            new Flare( 2.0f, 1.4f, Color.FromArgb( 25,  50, 100), "flares/flare3.png", "sunflare_10")
        };
        #endregion

        const float querySize = 100;
        bool occlusionQueryActive = false;
        float occlusionAlpha = 0;
        Model gm = null;
        Query occQuery = null;

        public void Initialize()
        {
            foreach (Flare flare in flares)
            {
                flare.Texture = TextureLoader.LoadTexture(flare.TextureName);
            }
            gm = WorldData.GetObject("sphere") as Model;

            QueryDescription qd = new QueryDescription();
            qd.Type = QueryType.Occlusion;
            occQuery = new Query(Game.Device, qd);
        }
        public void Dispose()
        {
            if (flares == null || flares.Length == 0) return;
            foreach (Flare flare in flares)
            {
                if (flare.Texture != null && !flare.Texture.Disposed)
                    flare.Texture.Dispose();
            }

            if (gm != null) gm.Dispose();
            if (occQuery != null && !occQuery.Disposed) occQuery.Dispose();
        }

        #region Draw
        public void Draw()
        {
            float dot = Vector3.Dot(SkyDome.LightDirection, Camera.Forward);
            if (dot < 0.5f)
            {
                foreach (Flare flare in flares) SpriteDrawer.SetVisible(flare.id, false);
                return;
            }

            Matrix infiniteView = Camera.ViewMatrix;
            infiniteView[3, 0] = infiniteView[3, 1] = infiniteView[3, 2] = 0;

            // Project the light position into 2D screen space.
            Viewport vp = Game.Device.Rasterizer.GetViewports()[0];
            Matrix proj = Matrix.PerspectiveFovLH(Camera.Fovx, Camera.AspectRatio, 1.0f, 1000000.0f);
            Vector3 projectedPosition = Vector3.Project(-SkyDome.LightDirection, vp.X, vp.Y, vp.Width, vp.Height, vp.MinZ, vp.MaxZ, Matrix.Identity * infiniteView * proj);

            projectedPosition.X /= WeatherGame.Framework.Rendering.DeferredRenderer.RenderWidth;
            projectedPosition.Y /= WeatherGame.Framework.Rendering.DeferredRenderer.RenderHeight;

            Vector2 lightPosition = new Vector2(projectedPosition.X * 2.0f - 1.0f, (projectedPosition.Y * 2.0f - 1.0f) * -1);

            UpdateOcclusion();

            DrawFlares(lightPosition);
        }


        void UpdateOcclusion()
        {
            if (gm == null) return;
            if (occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!occQuery.IsDataAvailable) return;

                UInt64 pixelCount = occQuery.GetData().Read<UInt64>();
                occlusionAlpha = Math.Min(pixelCount / 25000.0f, 1);
            }


            occQuery.Begin();

            Matrix m = Matrix.Scaling(3000, 3000, 3000) * Matrix.Translation(-SkyDome.LightDirection * 25000);
            Shader e = WorldData.GetObject("scatter.fx") as Shader;
            EffectTechnique t = e.EffectObj.GetTechniqueByName("RenderOccluder");
            D3D10.Buffer indexBuffer = gm.MeshObj.GetDeviceIndexBuffer();
            D3D10.Buffer vertextBuffer = gm.MeshObj.GetDeviceVertexBuffer(0);
            Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            Game.Device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            Game.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertextBuffer, 16, 0));
            EffectPass pass = t.GetPassByIndex(0);
            InputLayout l = ShaderHelper.ConstructInputLayout(gm.Mesh3d.inputElements, pass.Description.Signature);
            Game.Device.InputAssembler.SetInputLayout(l);
            Matrix proj = Matrix.PerspectiveFovLH(Camera.Fovx, Camera.AspectRatio, 1.0f, 1000000.0f);
            e.GetVar("WorldViewProjection").AsMatrix().SetMatrix(m * Camera.ViewMatrix * proj);
            pass.Apply();
            Game.Device.DrawIndexed((gm.Mesh3d.attrTable[0].FaceCount * 3), (gm.Mesh3d.attrTable[0].FaceStart * 3), 0);
            indexBuffer.Dispose();
            vertextBuffer.Dispose();

            occQuery.End();

            occlusionQueryActive = true;
        }


        void DrawFlares(Vector2 lightPosition)
        {
            // Lensflare sprites are positioned at intervals along a line that
            // runs from the 2D light position toward the center of the screen.
            Vector2 screenCenter = Vector2.Zero;
            Vector2 flareVector = screenCenter - lightPosition;

            foreach (Flare flare in flares)
            {
                SpriteDrawer.SetVisible(flare.id, true);

                // Compute the position of this flare sprite.
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;
                Color4 c = new SlimDX.Color4(flare.Color);
                c *= new Color4(SkyDome.SunColor);
                c *= occlusionAlpha;

                SpriteDrawer.AddInstance(flare.id, flare.Texture, flarePosition, c, flare.Scale / 2, SpriteBlendMode.Additive);
            }
        }

        #endregion
    }
}
