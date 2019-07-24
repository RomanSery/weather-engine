using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using WorldEditor.Rendering;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3D10 = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

using WeatherGame.Framework;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Weather;
using WeatherGame.RenderLoop;
using WorldEditor.Global;

using WeatherGame;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.Scripting;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Objects;

namespace WorldEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : CustomWindow
    {                  
        public static string ExecptionString;

        #region Init
        public MainEditorWindow()
        {
            //try
            //{
                InitializeComponent();
            //}
            //catch(Exception ex) 
            //{
              //  string s = ex.ToString();
            //}
            
            App.DoUpdate += new EventHandler(App_DoUpdate);
            mainMenu.SaveWorld += new Controls.MainMenu.SaveLoadCellHandler(mainMenu_SaveWorld);
            mainMenu.LoadWorld += new Controls.MainMenu.SaveLoadCellHandler(mainMenu_LoadWorld);
        }

        void mainMenu_LoadWorld(string filename)
        {
                 
        }

        void mainMenu_SaveWorld(string filename)
        {
       
        }
        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wh = new WindowInteropHelper(this);
            wh.EnsureHandle();
            IntPtr hwnd = wh.Handle;

            KeyboardInput.CreateDevice(hwnd);
            MouseInput.CreateDevice(hwnd);

            WorldEditor.Controls.ObjectWindow w = new Controls.ObjectWindow();
            Window owner = Window.GetWindow(this);
            w.Owner = owner;
            w.ShowInTaskbar = false;
            w.Width = 400;
            w.Height = 500;
            w.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            w.Left = owner.Width - 400;            
            w.Top = owner.Top + 60;
            w.Title = "Object window";            
            w.Show();

            WorldEditor.Controls.CellView w2 = new Controls.CellView();
            w2.Owner = owner;
            w2.ShowInTaskbar = false;
            w2.Width = 450;
            w2.Height = 300;
            w2.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            w2.Left = owner.Width - 500;
            w2.Top = owner.Top + 700;
            w2.Title = "Cell view";
            w2.Show();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            MainPlayer.Initialize();
            float aspectRatio = (float)view1.Width / (float)view1.Height;
            Camera.CurrentBehavior = Behavior.Spectator;
            //Camera.Perspective((float)Math.PI / 4.0f, aspectRatio, 1.0f, 1000000.0f);
            Camera.Perspective((float)Math.PI / 4.0f, aspectRatio, 1.0f, 4000.0f);
            Camera.Position = new Vector3(0, 0, 0);
            Camera.Velocity = new Vector3(40.0f, 40.0f, 40.0f);
            Camera.RotationSpeed = 0.25f;
            WorldTime.Speed = 0;             
           
            //compile shaders
            List<BaseGameObject> shaders = WorldData.GetObjectsByType(typeof(Shader));
            foreach (Shader s in shaders)
            {
                s.Recompile();
            }


           
           SpriteDrawer.Initialize();                                            
           DeferredRenderer.Initialize((int)view1.Width, (int)view1.Height);
           DeferredRenderer.RenderHeight = (int)view1.Height;
           DeferredRenderer.RenderWidth = (int)view1.Width;
           SkyDome.Initialize();           
           Rendering.Grid.Initialize();
           
           view1.OnRenderView += new EventHandler(view1_OnRender);
           view1.D3DScene = new CellViewer((int)view1.Width, (int)view1.Height);
           view1.OnRenderView += new EventHandler(view1_OnRender);

           this.WindowState = System.Windows.WindowState.Maximized;


           GameData.LoadGameData();
           PhysicsEngine.Initialize();
           ScriptManager.Initialize();
        }
       
        private void Window_Closing(object sender, CancelEventArgs e)
        {            
            try
            {
                Game.Device.ClearState();
            }
            catch { }
            Rendering.Grid.Dispose();            
            KeyboardInput.ReleaseDevice();
            MouseInput.ReleaseDevice();                       
            SpriteDrawer.Dispose();
            DeferredRenderer.Dispose();
            SkyDome.Dispose();
            TextureLoader.Dispose();
            PhysicsEngine.Dispose();
            GameData.Dispose();
            WorldData.Dispose();
            
            if (view1.D3DScene != null)
                view1.D3DScene.Dispose();
            view1.Dispose();

            ShaderHelper.Dispose();
            
        }
        #endregion        

        void view1_OnRender(object sender, EventArgs e)
        {
            if (!MainPlayer.Controller.LockCameraView)
            {
                Point windowPnt = new Point(this.Left, this.Top);
                Point relativePoint = view1.TransformToAncestor(this).Transform(new Point(0, 0));
                Point p = new Point(windowPnt.X + relativePoint.X + view1.ActualWidth / 2, windowPnt.Y + relativePoint.Y + view1.ActualHeight / 2);
                WeatherGame.RenderLoop.NativeMethods.SetCursorPos((int)p.X, (int)p.Y);
                Mouse.SetCursor(Cursors.None);

                MainPlayer.Update();
            }

            if (Global.GlobalSettings.RenderGrid)
            {                
                SlimDX.Matrix mGridWorld = WorldSpace.GetRealWorldMatrixGrid(Rendering.Grid.Position, GlobalSettings.CurrentCell);
                Rendering.Grid.Draw(mGridWorld);

                SlimDX.Matrix mWorld = WorldSpace.GetRealWorldMatrix(Vector3.Zero, GlobalSettings.CurrentCell);
                Gizmo.DrawOrigin(mWorld);
            }

            if (GlobalSettings.RenderLights && GlobalSettings.RenderVolumes && GlobalSettings.CurrentObjRef != null && GlobalSettings.CurrentObjRef.BaseGameObject is Light)
            {
                Light l = GlobalSettings.CurrentObjRef.BaseGameObject as Light;
                l.RenderDebugLightVolume(GlobalSettings.CurrentObjRef, GlobalSettings.CurrentObjRef.CellContainer, Global.GlobalSettings.GridColor);
            }


            App.Tick();
        }
        void App_DoUpdate(object sender, EventArgs e)
        {
            PhysicsEngine.Update();
            
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {
                ws.Update();
            }   

            ObjectPicker.Update();
            DeferredRenderer.Update();
            SkyDome.Update();  
            

            string s = string.Empty;
            s = "TotalGameTime: " + App.gameTime.TotalGameTime.ToString() + "\t";
            s += "TotalRealTime: " + App.gameTime.TotalRealTime.ToString() + "\t";
            s += "FramesPerSecond: " + App.gameTime.FramesPerSecond.ToString() + "\t";
            s += "ElapsedGameTime: " + App.gameTime.ElapsedGameTime.ToString() + "\t";
            s += "ElapsedRealTime: " + App.gameTime.ElapsedRealTime.ToString() + "\n";            
          

            //s += "Cc pos: " + PlayerInput.characterController.Body.Position.ToString();
            
        }        



        #region Mouse events
             

        #endregion


    }
}
