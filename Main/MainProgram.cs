using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using WeatherGame.RenderLoop;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using WeatherGame.Framework;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Player;
using SlimDX.DirectInput;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.Weather;

namespace WeatherGame.Main
{
    

    class MainProgram : Game
    {
        const int InitialWidth = 1600;
        const int InitialHeight = 900;

        public MainProgram()
        {            
            Window.ClientSize = new Size(InitialWidth, InitialHeight);
            Window.Text = "WeatherGame";
            Window.StartPosition = FormStartPosition.WindowsDefaultLocation;      

            DeviceSettings desiredSettings = new DeviceSettings();
            desiredSettings.DeviceVersion = DeviceVersion.Direct3D10;
            desiredSettings.Windowed = true;
            desiredSettings.BackBufferWidth = InitialWidth;
            desiredSettings.BackBufferHeight = InitialHeight;
            //DXGI_FORMAT_R10G10B10A2_UNORM DXGI_FORMAT_R16G16B16A16_FLOAT
            desiredSettings.BackBufferFormat = Format.R10G10B10A2_UNorm;
            desiredSettings.EnableVSync = true;
            desiredSettings.MultisampleCount = 1;
            desiredSettings.MultisampleQuality = 4;
            desiredSettings.DeviceType = DriverType.Hardware;
            desiredSettings.DepthStencilFormat = Format.R32_Typeless;                 
            GraphicsDeviceManager.ChangeDevice(desiredSettings);

            DeferredRenderer.RenderHeight = InitialHeight;
            DeferredRenderer.RenderWidth = InitialWidth;
        }       
        

        
        protected override void Initialize()
        {
            //GameWorld.Initialize(this.Window, GraphicsDeviceManager.ScreenWidth, GraphicsDeviceManager.ScreenHeight);
            //GameWorld.DeSerializeWorld(@"..\..\Content\export\World\physicsTest.world");
            
            PlayerInput.Initialize();
            float aspectRatio = (float)GraphicsDeviceManager.ScreenWidth / (float)GraphicsDeviceManager.ScreenHeight;
            Camera.CurrentBehavior = Behavior.Spectator;
            Camera.Perspective((float)Math.PI / 4.0f, aspectRatio, 1.0f, 1000000.0f);
            Camera.Position = new Vector3(0.0f, 10.0f, 0.0f);
            Camera.Velocity = new Vector3(40.0f, 40.0f, 40.0f);


            KeyboardInput.CreateDevice(this.Window);
            MouseInput.CreateDevice(this.Window);
            PrintText.Initialize();
            EffectManager.Initialize();               
            SpriteDrawer.Initialize();
            DeferredRenderer.Initialize(GraphicsDeviceManager.ScreenWidth, GraphicsDeviceManager.ScreenHeight);

            SkyDome.Initialize();
            PhysicsEngine.Initialize();
            
        }
      

        protected override void Dispose(bool disposing)
        {
            //GameWorld.Dispose(disposing);            

            try
            {
                Game.Device.ClearState();
            }
            catch { }
            KeyboardInput.ReleaseDevice();
            MouseInput.ReleaseDevice();
            PrintText.Dispose();
            EffectManager.Dispose();
            SpriteDrawer.Dispose();
            DeferredRenderer.Dispose();
            SkyDome.Dispose();
            TextureLoader.Dispose();
            PhysicsEngine.Dispose();
        }

        protected override void Update()
        {

            if (KeyboardInput.IsKeyDownOnce(Key.F1))
            {
                //TextureLoader.Dispose();
                GraphicsDeviceManager.ToggleFullScreen();
            }
            if (KeyboardInput.IsPressed(Key.Escape))
                Exit();

            PlayerInput.Update();
            //GameWorld.Update();
        }

        protected override void Draw()
        {            
            //GameWorld.Draw(GraphicsDeviceManager.Direct3D10.RenderTarget, GraphicsDeviceManager.Direct3D10.DepthStencilView);
        }          
    }


    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (MainProgram game = new MainProgram())
                game.Run();
        }
    }
}
