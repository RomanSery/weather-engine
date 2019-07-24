using System;
using SlimDX;
using SlimDX.DirectInput;

namespace WeatherGame.Framework.Input
{
    public static class MouseInput
    {
        static Mouse mouseDevice = null;
        static MouseState currentMouseState = null;
        static MouseState previousMouseState = null;
        static DirectInput dinput = null;

        /// <summary>
        /// Determines which way the mouse wheel has been rolled.
        /// The returned value is in the range [-1,1].
        /// </summary>
        /// <returns>
        /// A positive value indicates that the mouse wheel has been rolled
        /// towards the player. A negative value indicates that the mouse
        /// wheel has been rolled away from the player.
        /// </returns>
        public static float GetMouseWheelDirection()
        {
            int currentWheelValue = currentMouseState.Z;
            int previousWheelValue = previousMouseState.Z;

            if (currentWheelValue > previousWheelValue)
                return -1.0f;
            else if (currentWheelValue < previousWheelValue)
                return 1.0f;
            else
                return 0.0f;
        }

        public static MouseState CurrentState
        {
            get
            {
                return currentMouseState;
            }
        }


        public static void Update()
        {
            if (mouseDevice == null) return;
            if (mouseDevice.Acquire().IsFailure) return;
            if (mouseDevice.Poll().IsFailure) return;

            MouseState state = mouseDevice.GetCurrentState();
            if (Result.Last.IsFailure) return;

            previousMouseState = currentMouseState;
            currentMouseState = state;
        }

        public static void CreateDevice(System.Windows.Forms.Form gw)
        {
            dinput = new DirectInput();
            CooperativeLevel cooperativeLevel = CooperativeLevel.Exclusive | CooperativeLevel.Foreground;

            try
            {
                mouseDevice = new Mouse(dinput);
                mouseDevice.SetCooperativeLevel(gw, cooperativeLevel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // since we want to use buffered data, we need to tell DirectInput
            // to set up a buffer for the data
            //keyboardDevice.Properties.BufferSize = 8;

            // acquire the device
            mouseDevice.Acquire();
        }
        public static void CreateDevice(IntPtr hwnd)
        {
            dinput = new DirectInput();
            CooperativeLevel cooperativeLevel = CooperativeLevel.Nonexclusive | CooperativeLevel.Background;

            try
            {
                mouseDevice = new Mouse(dinput);
                mouseDevice.SetCooperativeLevel(hwnd, cooperativeLevel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            mouseDevice.Acquire();
        }
        public static void ReleaseDevice()
        {
            if (mouseDevice != null)
            {
                mouseDevice.Unacquire();
                mouseDevice.Dispose();
            }
            dinput.Dispose();
            mouseDevice = null;
        }
    }
}
