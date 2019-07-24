using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D10.Buffer;
using SlimDX.D3DCompiler;

using WeatherGame.Framework;
using WeatherGame.Framework.Rendering;
using WeatherGame.RenderLoop;
using WorldEditor.Global;


namespace WorldEditor.Rendering
{
    class CellViewer : SlimDXScene
    {
        public CellViewer(int w, int h)
            : base(w, h)
        {         
            Initialize();
        }

        public override void Initialize()
        {                   
            App.D3DDevice.Flush();
        }

        public override void Render()
        {
            DeferredRenderer.DrawWpf(RenderView, DepthView, DepthStencilSRV, this.Width, this.Height, GlobalSettings.RenderLights);            
        }

       

        public override void Dispose()
        {                      
            base.Dispose();
        }       
    }
}
