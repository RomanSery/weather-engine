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

namespace WorldEditor.Rendering
{
    public abstract class SlimDXScene : IDisposable
    {
        public RenderTargetView RenderView;
        public Texture2D SharedTexture;

        public Texture2D DepthStencilTexture;
        public DepthStencilView DepthView;
        public ShaderResourceView DepthStencilSRV;   

        
        public int Width, Height;


        public SlimDXScene(int w, int h)
        {
            Width = w;
            Height = h;

            InitD3D();            
        }

        private void InitD3D()
        {
            Texture2DDescription colordesc = new Texture2DDescription();
            colordesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            colordesc.Format = Format.B8G8R8A8_UNorm;
            colordesc.Width = Width;
            colordesc.Height = Height;
            colordesc.MipLevels = 1;
            colordesc.ArraySize = 1;
            colordesc.SampleDescription = new SampleDescription(1, 0);
            colordesc.Usage = ResourceUsage.Default;
            colordesc.OptionFlags = ResourceOptionFlags.Shared;
            colordesc.CpuAccessFlags = CpuAccessFlags.None;
            SharedTexture = new Texture2D(App.D3DDevice, colordesc);

            RenderTargetViewDescription rtDesc = new RenderTargetViewDescription();
            rtDesc.Format = colordesc.Format;
            rtDesc.Dimension = RenderTargetViewDimension.Texture2D;//RenderTargetViewDimension.Texture2DMultisampled;
            rtDesc.MipSlice = 0;
            RenderView = new RenderTargetView(App.D3DDevice, SharedTexture, rtDesc);




            Texture2DDescription depthStencilTexDesc = new Texture2DDescription();            
            depthStencilTexDesc.Width = Width;
            depthStencilTexDesc.Height = Height;
            depthStencilTexDesc.MipLevels = 1;
            depthStencilTexDesc.ArraySize = 1;
            depthStencilTexDesc.Format = Format.R24G8_Typeless;
            depthStencilTexDesc.Usage = ResourceUsage.Default;
            depthStencilTexDesc.SampleDescription = new SampleDescription(1, 0);            
            depthStencilTexDesc.BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource;            
            depthStencilTexDesc.OptionFlags = ResourceOptionFlags.None;
            depthStencilTexDesc.CpuAccessFlags = CpuAccessFlags.None;            
            DepthStencilTexture = new Texture2D(App.D3DDevice, depthStencilTexDesc);


            DepthStencilViewDescription depthDesc = new DepthStencilViewDescription();
            depthDesc.Format = Format.D24_UNorm_S8_UInt;          
            depthDesc.Dimension = DepthStencilViewDimension.Texture2D;
            depthDesc.MipSlice = 0;
            DepthView = new DepthStencilView(App.D3DDevice, DepthStencilTexture, depthDesc);





            ShaderResourceViewDescription resDesc = new ShaderResourceViewDescription();
            resDesc.Format = Format.R24_UNorm_X8_Typeless;
            resDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            resDesc.MostDetailedMip = 0;
            resDesc.MipLevels = 1;
            DepthStencilSRV = new ShaderResourceView(App.D3DDevice, DepthStencilTexture, resDesc);

            DepthStencilTexture.Dispose();
        }

        public virtual void Dispose()
        {
            if (RenderView != null)
            {
                RenderView.Dispose();
                RenderView = null;
            }
            if (DepthView != null)
            {
                DepthView.Dispose();
                DepthView = null;
            }
            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }
            if (DepthStencilTexture != null && DepthStencilTexture.Disposed == false)
            {
                DepthStencilTexture.Dispose();
                DepthStencilTexture = null;
            }
        }
        public abstract void Initialize();
        public abstract void Render();
       
    }
}
