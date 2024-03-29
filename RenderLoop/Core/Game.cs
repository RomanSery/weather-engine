﻿/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using System.Collections.ObjectModel;

namespace WeatherGame.RenderLoop
{
    /// <summary>
    /// Presents an easy to use wrapper for making games and samples.
    /// </summary>
    public class Game : IDisposable
    {
        #region Fields
        public static SlimDX.Direct3D10.Device Device;
        public static GameTime Time;

        GameClock clock = new GameClock();
        GameTime gameTime = new GameTime();
        TimeSpan maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
        TimeSpan totalGameTime;
        TimeSpan accumulatedElapsedGameTime;
        TimeSpan lastFrameElapsedGameTime;
        TimeSpan lastFrameElapsedRealTime;
        TimeSpan targetElapsedTime = TimeSpan.FromTicks(166667);
        TimeSpan inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        int updatesSinceRunningSlowly1 = int.MaxValue;
        int updatesSinceRunningSlowly2 = int.MaxValue;
        bool forceElapsedTimeToZero;
        bool drawRunningSlowly;
        long lastUpdateFrame;
        float lastUpdateTime;

        
        public event EventHandler Disposed;        
        public event EventHandler Activated;        
        public event EventHandler Deactivated;
        public event EventHandler Exiting;
        public event CancelEventHandler FrameStart;
        public event EventHandler FrameEnd;
        #endregion
        


        static Game()
        {           
            Configuration.ThrowOnError = true;
            Configuration.AddResultWatch(SlimDX.Direct3D9.ResultCode.DeviceLost, ResultWatchFlags.AlwaysIgnore);            
            Configuration.AddResultWatch(SlimDX.DXGI.ResultCode.WasStillDrawing, ResultWatchFlags.AlwaysIgnore);            
            Configuration.AddResultWatch(SlimDX.DXGI.ResultCode.Failure, ResultWatchFlags.Throw);
            Configuration.AddResultWatch(SlimDX.DXGI.ResultCode.InvalidCall, ResultWatchFlags.Throw);
            Configuration.AddResultWatch(SlimDX.DXGI.ResultCode.DeviceHung, ResultWatchFlags.Throw);            

#if DEBUG
            Configuration.DetectDoubleDispose = true;
            Configuration.EnableObjectTracking = true;                        
#else
            Configuration.DetectDoubleDispose = false;
            Configuration.EnableObjectTracking = false;
#endif

            // setup the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }      
        public Game()
        {
            IsFixedTimeStep = true;            

            Window = new GameWindow();
            Window.ApplicationActivated += Window_ApplicationActivated;
            Window.ApplicationDeactivated += Window_ApplicationDeactivated;
            Window.Suspend += Window_Suspend;
            Window.Resume += Window_Resume;
            Window.Paint += Window_Paint;

            GraphicsDeviceManager = new GraphicsDeviceManager(this);            
        }


        public void Run()
        {
            IsRunning = true;

            try
            {
                gameTime.ElapsedGameTime = 0;
                gameTime.ElapsedRealTime = 0;
                gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
                gameTime.TotalRealTime = (float)clock.CurrentTime.TotalSeconds;
                gameTime.IsRunningSlowly = false;
                Time = gameTime;

                Update();

                Application.Idle += Application_Idle;
                Application.Run(Window);
            }
            finally
            {
                Application.Idle -= Application_Idle;
                IsRunning = false;
                OnExiting(EventArgs.Empty);
            }
        }

        public void Tick()
        {
            // if we are exiting, do nothing
            if (IsExiting)
                return;

            // if we are inactive, sleep for a bit
            if (!IsActive)
                Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);

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

            // check if we are using a fixed or variable time step
            if (IsFixedTimeStep)
            {
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
                while (ratio > 0 && !IsExiting)
                {
                    ratio -= 1;

                    try
                    {
                        gameTime.ElapsedGameTime = (float)targetElapsedTime.TotalSeconds;
                        gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
                        gameTime.IsRunningSlowly = drawRunningSlowly;
                        Time = gameTime;

                        Update();
                    }
                    finally
                    {
                        lastFrameElapsedGameTime += targetElapsedTime;
                        totalGameTime += targetElapsedTime;
                    }
                }
            }
            else
            {
                drawRunningSlowly = false;
                updatesSinceRunningSlowly1 = int.MaxValue;
                updatesSinceRunningSlowly2 = int.MaxValue;

                // make sure we shouldn't be exiting
                if (!IsExiting)
                {
                    try
                    {
                        gameTime.ElapsedGameTime = 0;
                        lastFrameElapsedGameTime = elapsedAdjustedTime;
                        gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
                        gameTime.IsRunningSlowly = false;
                        Time = gameTime;

                        Update();
                    }
                    finally
                    {
                        totalGameTime += elapsedAdjustedTime;
                    }
                }
            }

            DrawFrame();

            // refresh the FPS counter once per second
            lastUpdateFrame++;
            if ((float)clock.CurrentTime.TotalSeconds - lastUpdateTime > 1.0f)
            {
                gameTime.FramesPerSecond = (float)lastUpdateFrame / (float)(clock.CurrentTime.TotalSeconds - lastUpdateTime);
                lastUpdateTime = (float)clock.CurrentTime.TotalSeconds;
                lastUpdateFrame = 0;
            }
        }
        void DrawFrame()
        {
            try
            {
                if (!IsExiting && !Window.IsMinimized)
                {
                    CancelEventArgs e = new CancelEventArgs(false);
                    OnFrameStart(e);
                    if (!e.Cancel)
                    {
                        gameTime.TotalRealTime = (float)clock.CurrentTime.TotalSeconds;
                        gameTime.ElapsedRealTime = (float)lastFrameElapsedRealTime.TotalSeconds;
                        gameTime.TotalGameTime = (float)totalGameTime.TotalSeconds;
                        gameTime.ElapsedGameTime = (float)lastFrameElapsedGameTime.TotalSeconds;
                        gameTime.IsRunningSlowly = drawRunningSlowly;
                        Time = gameTime;
                        Draw();

                        OnFrameEnd(EventArgs.Empty);
                    }
                }
            }
            finally
            {
                lastFrameElapsedGameTime = TimeSpan.Zero;
                lastFrameElapsedRealTime = TimeSpan.Zero;
            }
        }


        #region Helpers
        public void Dispose()
        {
            // GraphicsDeviceManager.Dispose will come around and call the Dispose(bool)
            // overload, so we don't need to do it here. It's convoluted, but it works well.
            if (GraphicsDeviceManager != null) GraphicsDeviceManager.Dispose();
            GraphicsDeviceManager = null;

            if (Disposed != null) Disposed(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }
        public void Exit()
        {
            IsExiting = true; // request the game to terminate
        }
        public void ResetElapsedTime()
        {
            forceElapsedTimeToZero = true;
            updatesSinceRunningSlowly1 = int.MaxValue;
            updatesSinceRunningSlowly2 = int.MaxValue;
        }
        #endregion


        #region Virtuals
        protected virtual void Update() { }        
        protected virtual void Draw(){}        
        protected internal virtual void Initialize(){}               
        protected internal virtual void LoadContent(){}               
        protected internal virtual void UnloadContent(){}        
        protected internal virtual void Dispose(bool disposing){}
        #endregion

        #region Events
        protected virtual void OnActivated(EventArgs e)
        {
            if (Activated != null)
                Activated(this, e);
        }        
        protected virtual void OnDeactivated(EventArgs e)
        {
            if (Deactivated != null)
                Deactivated(this, e);
        }        
        protected virtual void OnExiting(EventArgs e)
        {
            if (Exiting != null)
                Exiting(this, e);
        }        
        protected virtual void OnFrameStart(CancelEventArgs e)
        {
            if (FrameStart != null)
                FrameStart(this, e);
        }
        protected virtual void OnFrameEnd(EventArgs e)
        {
            if (FrameEnd != null)
                FrameEnd(this, e);
        }
        void Application_Idle(object sender, EventArgs e)
        {
            NativeMessage message;
            while (!NativeMethods.PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                if (IsExiting)
                    Window.Close();
                else
                    Tick();
            }
        }
        void Window_ApplicationDeactivated(object sender, EventArgs e)
        {
            if (IsActive)
            {
                IsActive = false;
                OnDeactivated(EventArgs.Empty);
            }
        }
        void Window_ApplicationActivated(object sender, EventArgs e)
        {
            if (!IsActive)
            {
                IsActive = true;
                OnActivated(EventArgs.Empty);
            }
        }
        void Window_Paint(object sender, PaintEventArgs e)
        {
            DrawFrame();
        }
        void Window_Resume(object sender, EventArgs e)
        {
            clock.Resume();
        }
        void Window_Suspend(object sender, EventArgs e)
        {
            clock.Suspend();
        }
        #endregion


        #region Poperties
        public TimeSpan InactiveSleepTime
        {
            get { return inactiveSleepTime; }
            set
            {
                // error checking
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "Inactive sleep time cannot be less than zero.");
                inactiveSleepTime = value;
            }
        }
        public TimeSpan TargetElapsedTime
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
        public bool IsFixedTimeStep
        {
            get;
            set;
        }
        public bool IsExiting
        {
            get;
            private set;
        }
        public bool IsRunning
        {
            get;
            private set;
        }
        public GameWindow Window
        {
            get;
            private set;
        }
        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get;
            private set;
        }
        public bool IsActive
        {
            get;
            private set;
        }
        #endregion       

        
    }
}
