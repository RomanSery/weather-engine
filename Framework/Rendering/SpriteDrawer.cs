using System;
using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.RenderLoop;

namespace WeatherGame.Framework.Rendering
{
    public enum SpriteBlendMode
    {
        AlphaBlend,
        Additive
    }

    public class CustomSpriteInstance : SpriteInstance
    {
        public bool enabled = true;
        public SpriteBlendMode blendMode = SpriteBlendMode.AlphaBlend;

        public CustomSpriteInstance(ShaderResourceView srv, Vector2 c, Vector2 d) : base(srv, c, d)
        {

        }
    }

    public static class SpriteDrawer
    {
        static Sprite spriteDrawer;        
        static Dictionary<string, CustomSpriteInstance> instances;
        static Vector2 coordiantes, dimensions;
        static RasterizerState rs;
        static BlendState bsAlphaBlend, bsAdditive;

        public static void Initialize()
        {
            spriteDrawer = new Sprite(Game.Device, 0);
            instances = new Dictionary<string, CustomSpriteInstance>();
            coordiantes = new Vector2(0.0f, 0.0f);
            dimensions = new Vector2(1.0f, 1.0f);

            RasterizerStateDescription rsd = new RasterizerStateDescription();
            rsd.CullMode = CullMode.None;
            rsd.FillMode = FillMode.Solid;
            rs = RasterizerState.FromDescription(Game.Device, rsd);

            BlendStateDescription bsd = new BlendStateDescription();
            bsd.SetBlendEnable(0, true);
            bsd.BlendOperation = BlendOperation.Add;
            bsd.SourceBlend = BlendOption.SourceAlpha;
            bsd.DestinationBlend = BlendOption.InverseSourceAlpha;
            bsd.SourceAlphaBlend = BlendOption.SourceAlpha;
            bsd.DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
            bsd.AlphaBlendOperation = BlendOperation.Add;
            bsAlphaBlend = BlendState.FromDescription(Game.Device, bsd);

            bsd = new BlendStateDescription();
            bsd.SetBlendEnable(0, true);
            bsd.BlendOperation = BlendOperation.Add;
            bsd.SourceBlend = BlendOption.One;
            bsd.DestinationBlend = BlendOption.One;
            bsd.SourceAlphaBlend = BlendOption.One;
            bsd.DestinationAlphaBlend = BlendOption.One;
            bsd.AlphaBlendOperation = BlendOperation.Add;
            bsAdditive = BlendState.FromDescription(Game.Device, bsd);
        }

        public static void AddInstance(string instanceName, ShaderResourceView srv, Vector2 pos, Color4 c, float s, SpriteBlendMode mode)
        {
            Matrix m = Matrix.Scaling(s,s,s) * Matrix.Translation(pos.X, pos.Y, 0);

            if (instances.ContainsKey(instanceName))
            {
                instances[instanceName].Color = c;
                instances[instanceName].Transform = m;
                instances[instanceName].blendMode = mode;
            }
            else
            {
                CustomSpriteInstance si = new CustomSpriteInstance(srv, coordiantes, dimensions);               
                si.Color = c;
                si.Transform = m;
                si.blendMode = mode;
                instances.Add(instanceName, si);                
            }
        }
        public static void SetVisible(string instanceName, bool b)
        {
            if (instances.ContainsKey(instanceName))
            {
                instances[instanceName].enabled = b;                
            }
        }

        public static void Draw()
        {
            if (instances == null || instances.Count == 0) return;
            
            Game.Device.Rasterizer.State = rs;            
            spriteDrawer.Begin(SpriteFlags.SaveState);

            foreach (KeyValuePair<String, CustomSpriteInstance> entry in instances)
            {
                CustomSpriteInstance i = instances[entry.Key];
                if (i.enabled == false) continue;

                if(i.blendMode == SpriteBlendMode.Additive)
                    Game.Device.OutputMerger.BlendState = bsAdditive;
                else
                    Game.Device.OutputMerger.BlendState = bsAlphaBlend;
                spriteDrawer.DrawImmediate(new CustomSpriteInstance[]{ i });                
            }            
           
            spriteDrawer.End();
        }

        public static void Dispose()
        {
            if(spriteDrawer != null && !spriteDrawer.Disposed)
                spriteDrawer.Dispose();

            if (rs != null && !rs.Disposed) rs.Dispose();
            if (bsAlphaBlend != null && !bsAlphaBlend.Disposed) bsAlphaBlend.Dispose();
            if (bsAdditive != null && !bsAdditive.Disposed) bsAdditive.Dispose();
        }
    }
}
