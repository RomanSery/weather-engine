using System;
using SlimDX;
using SlimDX.DirectInput;

namespace WeatherGame.Framework.Input
{
    public static class KeyboardInput
    {
        static Keyboard keyboardDevice = null;
        static KeyboardState currState = null;
        static bool[] oldState = new bool[256];
        static DirectInput dinput = null;

        public static bool IsPressed(Key k)
        {
            if (currState != null)
                return currState.IsPressed(k);
            return false;
        }

        public static bool IsKeyDownOnce(Key k)
        {
            if (currState == null) return false;
            byte n = Convert.ToByte(k);
            if (oldState[n] == false && currState.IsPressed(k))
            {
                oldState[n] = true;
                return true;
            }
            if (oldState[n] && !currState.IsPressed(k))
            {
                oldState[n] = false;
                return false;
            }

            return false;
        }


        public static void Update()
        {
            currState = null;
            if (keyboardDevice == null) return;
            if (keyboardDevice.Acquire().IsFailure) return;
            if (keyboardDevice.Poll().IsFailure) return;

            KeyboardState state = keyboardDevice.GetCurrentState();
            if (Result.Last.IsFailure) return;


            currState = state;
        }

        public static void CreateDevice(System.Windows.Forms.Form gw)
        {
            dinput = new DirectInput();
            CooperativeLevel cooperativeLevel = CooperativeLevel.Exclusive | CooperativeLevel.Foreground;

            try
            {
                keyboardDevice = new Keyboard(dinput);
                keyboardDevice.SetCooperativeLevel(gw, cooperativeLevel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // since we want to use buffered data, we need to tell DirectInput
            // to set up a buffer for the data
            //keyboardDevice.Properties.BufferSize = 8;

            // acquire the device
            keyboardDevice.Acquire();
        }
        public static void CreateDevice(IntPtr handle)
        {
            dinput = new DirectInput();
            CooperativeLevel cooperativeLevel = CooperativeLevel.Nonexclusive | CooperativeLevel.Background;

            try
            {
                keyboardDevice = new Keyboard(dinput);
                keyboardDevice.SetCooperativeLevel(handle, cooperativeLevel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // since we want to use buffered data, we need to tell DirectInput
            // to set up a buffer for the data
            //keyboardDevice.Properties.BufferSize = 8;

            // acquire the device
            keyboardDevice.Acquire();
        }
        public static void ReleaseDevice()
        {
            if (keyboardDevice != null)
            {
                keyboardDevice.Unacquire();
                keyboardDevice.Dispose();
            }
            dinput.Dispose();
            keyboardDevice = null;
        }

        public static KeyboardState CurrentState { get { return currState; } }
    }
}
