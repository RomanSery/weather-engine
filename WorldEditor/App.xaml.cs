using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D10.Buffer;
using SlimDX.D3DCompiler;

using WeatherGame.Framework;
using WeatherGame.Framework.Player;
using WeatherGame.RenderLoop;

namespace WorldEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SlimDX.Direct3D10.Device D3DDevice;


        public static GameClock clock = null;
        public static GameTime gameTime = null;
        static TimeSpan maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
        static TimeSpan totalGameTime;
        static TimeSpan accumulatedElapsedGameTime;
        static TimeSpan lastFrameElapsedGameTime;
        static TimeSpan lastFrameElapsedRealTime;
        static TimeSpan targetElapsedTime = TimeSpan.FromTicks(166667);
        static TimeSpan inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        static int updatesSinceRunningSlowly1 = int.MaxValue;
        static int updatesSinceRunningSlowly2 = int.MaxValue;
        static bool forceElapsedTimeToZero;
        static bool drawRunningSlowly;
        static long lastUpdateFrame;
        static float lastUpdateTime;

        public static event EventHandler DoUpdate;

        static App()
        {


            //DeviceCreationFlags.Debug
            D3DDevice = new SlimDX.Direct3D10.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            

            Game.Device = D3DDevice;
            MainPlayer.Controller.LockCameraView = true;

            clock = new GameClock();
            gameTime = new GameTime();
            gameTime.ElapsedGameTime = 0;
            gameTime.ElapsedRealTime = 0;
            gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
            gameTime.TotalRealTime = (float)clock.CurrentTime.TotalSeconds;

            Game.Time = gameTime;
            clock.Resume();
        }


        public static void Tick()
        {
            clock.Step();

            gameTime.TotalRealTime = (float)clock.CurrentTime.TotalSeconds;
            gameTime.ElapsedRealTime = (float)clock.ElapsedTime.TotalSeconds;
            lastFrameElapsedRealTime += clock.ElapsedTime;
            TimeSpan elapsedAdjustedTime = clock.ElapsedAdjustedTime;
            if (elapsedAdjustedTime < TimeSpan.Zero)
                elapsedAdjustedTime = TimeSpan.Zero;

            if (forceElapsedTimeToZero)
            {
                gameTime.ElapsedRealTime = 0;
                lastFrameElapsedRealTime = elapsedAdjustedTime = TimeSpan.Zero;
                forceElapsedTimeToZero = false;
            }

            // cap the adjusted time
            if (elapsedAdjustedTime > maximumElapsedTime)
                elapsedAdjustedTime = maximumElapsedTime;

            
            accumulatedElapsedGameTime += elapsedAdjustedTime;
            long ratio = accumulatedElapsedGameTime.Ticks / TargetElapsedTime.Ticks;
            accumulatedElapsedGameTime = TimeSpan.FromTicks(accumulatedElapsedGameTime.Ticks % TargetElapsedTime.Ticks);
            lastFrameElapsedGameTime = TimeSpan.Zero;
            if (ratio == 0)
                return;
            TimeSpan targetElapsedTime = TargetElapsedTime;

            if (ratio > 1)
            {
                updatesSinceRunningSlowly2 = updatesSinceRunningSlowly1;
                updatesSinceRunningSlowly1 = 0;
            }
            else
            {
                if (updatesSinceRunningSlowly1 < int.MaxValue)
                    updatesSinceRunningSlowly1++;
                if (updatesSinceRunningSlowly2 < int.MaxValue)
                    updatesSinceRunningSlowly2++;
            }

            drawRunningSlowly = updatesSinceRunningSlowly2 < 20;

            // update until it's time to draw the next frame
            while (ratio > 0)
            {
                ratio -= 1;

                try
                {
                    gameTime.ElapsedGameTime = (float)targetElapsedTime.TotalSeconds;
                    gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
                    gameTime.IsRunningSlowly = drawRunningSlowly;
                    Game.Time = gameTime;

                    if (DoUpdate != null) DoUpdate(null, EventArgs.Empty);
                }
                finally
                {
                    lastFrameElapsedGameTime += targetElapsedTime;
                    totalGameTime += targetElapsedTime;
                }
            }

            // refresh the FPS counter once per second
            lastUpdateFrame++;
            if ((float)clock.CurrentTime.TotalSeconds - lastUpdateTime > 1.0f)
            {
                gameTime.FramesPerSecond = (float)lastUpdateFrame / (float)(clock.CurrentTime.TotalSeconds - lastUpdateTime);
                lastUpdateTime = (float)clock.CurrentTime.TotalSeconds;
                lastUpdateFrame = 0;
            }
        }        


        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (D3DDevice != null)
            {
                D3DDevice.Dispose();
                D3DDevice = null;
            }
            
            base.OnExit(e);
        }


        public static TimeSpan TargetElapsedTime
        {
            get { return targetElapsedTime; }
            set
            {
                // error checking
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "Target elapsed time must be greater than zero.");
                targetElapsedTime = value;
            }
        }
    }
}
