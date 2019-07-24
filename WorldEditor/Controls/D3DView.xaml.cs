using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using WorldEditor.Rendering;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Player;

namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for D3DView.xaml
    /// </summary>
    public partial class D3DView : UserControl, IDisposable
    {
        D3DImageSlimDX D3DImageContainer;        
        private SlimDXScene _d3DScene;
        Stopwatch Timer;
        public event EventHandler OnRenderView;

        public D3DView()
        {
            Timer = new Stopwatch();           
            Loaded += Window_Loaded;            
            try
            {
                InitializeComponent();
            }
            catch { }
        }

        public SlimDXScene D3DScene
        {
            set 
            {
                _d3DScene = value;               
            }
            get { return _d3DScene; }
        }


        public IntPtr Handle
        {
            get
            {
                if (D3DImageContainer != null)
                    return D3DImageContainer.GetSharedHandle();
                return IntPtr.Zero;
            }
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            SlimDX.Direct3D10.Texture2D Texture = null;
            D3DImageContainer = new D3DImageSlimDX();
            D3DImageContainer.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
            SlimDXImage.Source = D3DImageContainer;

            if (_d3DScene != null)
                Texture = _d3DScene.SharedTexture;

            if(Texture != null)
                D3DImageContainer.SetBackBufferSlimDX(Texture);
            BeginRenderingScene();
        }

        public void Dispose()
        {
            if (D3DImageContainer != null)
            {
                D3DImageContainer.Dispose();
                D3DImageContainer = null;
            }

            if (_d3DScene != null)
            {
                _d3DScene.Dispose();
                _d3DScene = null;
            }
        }
        
        protected void OnRendering(object sender, EventArgs e)
        {
            if(_d3DScene != null) _d3DScene.Render();
            if (OnRenderView != null)
            {
                OnRenderView(sender, e);
            }
            WeatherGame.RenderLoop.Game.Device.Flush();
            if(D3DImageContainer != null) D3DImageContainer.InvalidateD3DImage();
            
        }

        protected void BeginRenderingScene()
        {
            if (_d3DScene == null) return;

            if (D3DImageContainer.IsFrontBufferAvailable)
            {
                SlimDX.Direct3D10.Texture2D Texture = _d3DScene.SharedTexture;
                D3DImageContainer.SetBackBufferSlimDX(Texture);
                CompositionTarget.Rendering += OnRendering;                
                Timer.Start();         
            }
        }

        protected void StopRenderingScene()
        {            
            Timer.Stop();
            CompositionTarget.Rendering -= OnRendering;
        }

        protected void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // This fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (D3DImageContainer.IsFrontBufferAvailable)
            {
                BeginRenderingScene();
            }
            else
            {
                StopRenderingScene();
            }
        }
    }
}
