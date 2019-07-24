using System.Collections.Generic;
using SlimDX;
using WeatherGame.RenderLoop;

namespace WeatherGame.Framework.Weather
{
    public struct WindValue
    {
        public Vector3 windAmount;
        public int time;

        public WindValue(Vector3 wA, int t)
        {
            windAmount = wA;
            time = t;
        }
    };

    public static class Wind
    {
        public static float g_WindAmount = 1;
        public static Vector3 interpolatedWind;

        static float time = 0;
        static List<WindValue> WindAnimation;

        public static void Initalize()
        {
            //populate the wind animation
            WindAnimation = new List<WindValue>();
            int time = 0; //time in seconds between each key
            WindAnimation.Add(new WindValue(new Vector3(-0.1f, -0.5f, 0.0f), time));
            WindAnimation.Add(new WindValue(new Vector3(-0.4f, -0.5f, 0.04f), time += 10));
            WindAnimation.Add(new WindValue(new Vector3(-0.2f, -0.5f, -0.4f), time += 5));
            WindAnimation.Add(new WindValue(new Vector3(0.0f, -0.5f, -0.02f), time += 10));
            WindAnimation.Add(new WindValue(new Vector3(0.0f, -0.5f, -0.02f), time += 10));
            WindAnimation.Add(new WindValue(new Vector3(0.1f, -0.5f, 0.4f), time += 6));
            WindAnimation.Add(new WindValue(new Vector3(-0.1f, -0.5f, 0.0f), time += 5));
        }

        public static void Update()
        {
            //estimate a value of wind at the given time frame
            //we use piecewise linear interpolation between an array of key frames of wind values
            if (WindAnimation == null) return;

            int lastTime = WindAnimation[WindAnimation.Count - 1].time;
            int upperFrame = 1;

            while (time > WindAnimation[upperFrame].time)
                upperFrame++;

            int lowerFrame = upperFrame - 1;
            float amount = (float)(time - WindAnimation[lowerFrame].time) /
                (WindAnimation[upperFrame].time - WindAnimation[lowerFrame].time);
            interpolatedWind = WindAnimation[lowerFrame].windAmount + amount * (WindAnimation[upperFrame].windAmount - WindAnimation[lowerFrame].windAmount);

            //adjust the wind based on the current frame rate; the values were estimated for 40FPS
            interpolatedWind *= 40.0f / Game.Time.FramesPerSecond;
            //lerp between the wind vector and just the vector pointing down depending on the amount of user chosen wind
            interpolatedWind = g_WindAmount * interpolatedWind + (1 - g_WindAmount) * (new Vector3(0.0f, -0.5f, 0.0f));

            //g_pTotalVelShaderVariable->SetFloatVector((float*)interpolatedWind);
            time += 1.0f / Game.Time.FramesPerSecond;
            if (time > lastTime)
                time = 0;

        }
    }
}
